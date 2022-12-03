using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Tvmaid
{
    public partial class MainForm : Form
    {
        string host;
        List<Reserve> reserves = new List<Reserve>();
        PairList define;
        Timer timer;

        class Reserve
        {
            public string Title = "";
            public string Service = "";
            public string Time = "";
            public int Status = 0;
            public string Tuner = "";
        }

        enum ReserveStatus
        {
            Enable = 1,
            EventMode = 2,
            Duplication = 32,
            Recoding = 64,
            Complete = 128
        };

        public MainForm()
        {
            InitializeComponent();

            EnableDoubleBuffer(listView);

            try
            {
                LoadDefine();
                this.Text = host + " - " + this.Text;
            }
            catch (Exception e)
            {
                MessageBox.Show("起動に失敗しました。[詳細] " + e.Message);
                return;
            }

            timer = new Timer();

            timer.Tick += Reload;
            timer.Interval = 2000;
            timer.Enabled = true;
        }

        //ダブルバッファ
        static void EnableDoubleBuffer(Control c)
        {
            var prop = c.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(c, true, null);
        }
        
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                define["host"] = host;

                define["font"] = Font.FontFamily.Name;
                define["fontsize"] = Font.SizeInPoints.ToString();

                define["left"] = this.Left.ToString();
                define["top"] = this.Top.ToString();
                define["width"] = this.Width.ToString();
                define["height"] = this.Height.ToString();

                define["title.width"] = this.titleHeader.Width.ToString();
                define["service.width"] = this.serviceHeader.Width.ToString();
                define["time.width"] = this.timeHeader.Width.ToString();
                define["status.width"] = this.statusHeader.Width.ToString();
                define["tuner.width"] = this.tunerHeader.Width.ToString();

                define.Save();
            }
            catch { }
        }

        void LoadDefine()
        {
            var path = Util.GetBasePath("Timekeeper.def");

            define = new PairList(path);

            if (File.Exists(path) == false)
            {
                host = "localhost:20001";
                return;
            }

            define.Load();

            host = define["host"];

            var fam = new System.Drawing.FontFamily(define["font"]);
            base.Font = new System.Drawing.Font(fam, define.GetFloat("fontsize"));

            Left = define.GetInt("left");
            Top = define.GetInt("top");
            Width = define.GetInt("width");
            Height = define.GetInt("height");

            titleHeader.Width = define.GetInt("title.width");
            serviceHeader.Width = define.GetInt("service.width");
            timeHeader.Width = define.GetInt("time.width");
            statusHeader.Width = define.GetInt("status.width");
            tunerHeader.Width = define.GetInt("tuner.width");
        }

        private void listView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            lock (reserves)
            {
                try
                {
                    Reserve res;

                    //リストにないときでも、e.Itemをセットせずにコールバックから出てはいけない
                    if (e.ItemIndex >= reserves.Count)
                        res = new Reserve();
                    else
                        res = reserves[e.ItemIndex];

                    var item = new ListViewItem();

                    item.Text = res.Title;
                    item.SubItems.Add(res.Service);
                    item.SubItems.Add(res.Time);

                    var status = "正常";

                    if ((res.Status & (int)ReserveStatus.Duplication) > 0)
                    {
                        status = "重複";
                        item.BackColor = System.Drawing.Color.Gold;
                    }

                    if ((res.Status & (int)ReserveStatus.Recoding) > 0)
                    {
                        status = "録画中";
                        item.BackColor = System.Drawing.Color.LightCoral;
                    }

                    item.SubItems.Add(status);
                    item.SubItems.Add(res.Tuner);

                    e.Item = item;
                }
                catch { }
            }
        }

        async void Reload(object sender, EventArgs e)
        {
            try
            {
                var client = new WebClient();

                client.Encoding = Encoding.UTF8;

                var sql = "select title, service.name, start, end, status, tuner from reserve"
                        + " left join service on reserve.fsid = service.fsid"
                        + " where status & 1 and end > {0} group by reserve.id order by start".Formatex(DateTime.Now.Ticks);

                sql = System.Web.HttpUtility.UrlEncode(sql, Encoding.UTF8);

                var url = "http://" + host + "/webapi/GetTable?sql=" + sql;
                var data = await client.DownloadStringTaskAsync(url);
                var ret = DynamicJson.Parse(data);
                var total = new TimeSpan();
                var count = 0;

                lock (reserves)
                {
                    if (ret.code != 0)
                        throw new Exception(ret.message);

                    reserves.Clear();

                    var list = (dynamic[])ret.data1;
                    count = list.Length;

                    foreach (var item in list)
                    {
                        var res = new Reserve();

                        res.Title = item[0];
                        res.Service = item[1];

                        var start = new DateTime((long)item[2]);
                        var end = new DateTime((long)item[3]);
                        res.Time = start.ToString("dd(ddd) HH:mm-") + end.ToString("HH:mm");
                        total += end - start;

                        res.Status = (int)item[4];
                        res.Tuner = item[5];

                        reserves.Add(res);
                    }
                }

                listView.VirtualListSize = reserves.Count;
                listView.Invalidate();

                url = "http://" + host + "/webapi/GetRecordFolderFree";
                data = await client.DownloadStringTaskAsync(url);
                ret = DynamicJson.Parse(data);

                if (ret.code == 0)
                {
                    var freeDisk = ret.data1 / 1024 / 1024 / 1024;
                    var freeTime = ret.data1 / 2048 / 1024 / 60;
                    statusText.Text = string.Format("ok, 予約 {2} ({3:0} 分), 空き {0:0.00} GB ({1:0} 分)", freeDisk, freeTime, count, total.TotalMinutes);
                }
                else
                    statusText.Text = "ok";
            }
            catch (Exception ex)
            {
                listView.VirtualListSize = 0;
                listView.Invalidate();

                statusText.Text = ex.Message;
            }
        }

        private void fontChangeMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new FontDialog();

            dialog.Font = listView.Font;

            if (dialog.ShowDialog() != DialogResult.Cancel)
                Font = dialog.Font;
        }
    }
}
