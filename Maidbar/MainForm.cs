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
        string port;
        string tuner;     //選択中チューナ
        string driver;    //選択中ドライバ
        List<Service> services = new List<Service>();
        PairList define;
        Timer timer;

        class Event
        {
            public int EId = -1;
            public string Title = "";
            public string Time = "";
        }

        class Service
        {
            public long Fsid = -1;
            public string Name = "";
            public Event Event = new Event();
        }

        public MainForm()
        {
            InitializeComponent();

            EnableDoubleBuffer(serviceView);

            try
            {
                LoadDefine();
                InitTunerView();
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
            PropertyInfo prop = c.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(c, true, null);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                define["port"] = port;

                define["window"] = ((int)WindowState).ToString();
                WindowState = FormWindowState.Normal;

                define["splitter"] = splitter.SplitterDistance.ToString();

                define["font"] = Font.FontFamily.Name;
                define["fontsize"] = Font.SizeInPoints.ToString();

                define["left"] = Left.ToString();
                define["top"] = Top.ToString();
                define["width"] = Width.ToString();
                define["height"] = Height.ToString();

                define["name.width"] = nameHeader.Width.ToString();
                define["time.width"] = timeHeader.Width.ToString();
                define["event.width"] = eventHeader.Width.ToString();

                define.Save();
            }
            catch { }
        }

        void LoadDefine()
        {
            var path = Util.GetBasePath("Maidbar.def");

            define = new PairList(path);

            if (File.Exists(path) == false)
            {
                port = "20001";
                return;
            }

            define.Load();

            port = define["port"];

            var fam = new System.Drawing.FontFamily(define["font"]);
            Font = new System.Drawing.Font(fam, define.GetFloat("fontsize"));

            Left = define.GetInt("left");
            Top = define.GetInt("top");
            Width = define.GetInt("width");
            Height = define.GetInt("height");

            splitter.SplitterDistance = define.GetInt("splitter");

            nameHeader.Width = define.GetInt("name.width");
            timeHeader.Width = define.GetInt("time.width");
            eventHeader.Width = define.GetInt("event.width");

            WindowState = (FormWindowState)define.GetInt("window");
        }

        async void InitTunerView()
        {
            try
            {
                var client = new WebClient();

                client.Encoding = Encoding.UTF8;

                var sql = "select name, driver from tuner order by id";

                sql = System.Web.HttpUtility.UrlEncode(sql, Encoding.UTF8);

                var url = "http://localhost:" + port + "/webapi/GetTable?sql=" + sql;
                var data = await client.DownloadStringTaskAsync(url);
                var ret = DynamicJson.Parse(data);

                if (ret.code != 0)
                    throw new Exception(ret.message);

                var list = (dynamic[])ret.data1;

                foreach (var tuner in list)
                {
                    var node = new TreeNode(tuner[0]);

                    node.Tag = tuner[1];
                    tunerView.Nodes.Add(node);
                }

                statusText.Text = "ok";
            }
            catch (Exception ex)
            {
                statusText.Text = ex.Message;
                MessageBox.Show(ex.Message);
            }
        }

        private void serviceView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            try
            {
                Service service;

                //リストにないときでも、e.Itemをセットせずにコールバックから出てはいけない
                if (e.ItemIndex >= services.Count)
                    service = new Service();
                else
                    service = services[e.ItemIndex];

                var item = new ListViewItem();

                item.Text = service.Name;

                var time = new ListViewItem.ListViewSubItem(item, service.Event.Time);
                item.SubItems.Add(time);

                var title = new ListViewItem.ListViewSubItem(item, service.Event.Title);
                item.SubItems.Add(title);

                e.Item = item;
            }
            catch { }
        }

        static string SqlEncode(string text)
        {
            return text.Replace("'", "''");
        }

        async void Reload(object sender, EventArgs e)
        {
            if (tuner == null)
                return;

            try
            {
                var client = new WebClient();

                client.Encoding = Encoding.UTF8;

                var sql = "select service.fsid, name, start, end, title, eid from service left join"
                    + " (select fsid, eid, title, start, end from event where start < {0} and end > {0}) as _event".Formatex(DateTime.Now.Ticks)
                    + " on service.fsid = _event.fsid"
                    + " where driver = '{0}'".Formatex(SqlEncode(driver))
                    + " order by service.id";
                
                sql = System.Web.HttpUtility.UrlEncode(sql, Encoding.UTF8);

                var url = "http://localhost:" + port + "/webapi/GetTable?sql=" + sql;
                var data = await client.DownloadStringTaskAsync(url);
                var ret = DynamicJson.Parse(data);

                lock (services)
                {
                    if (ret.code != 0)
                        throw new Exception(ret.message);

                    services.Clear();

                    var list = (dynamic[])ret.data1;

                    foreach (var service in list)
                    {
                        var s = new Service();

                        s.Fsid = (long)service[0];
                        s.Name = service[1];

                        if (service[5] != null)
                        {
                            var start = new DateTime((long)service[2]);
                            var end = new DateTime((long)service[3]);
                            s.Event.Time = start.ToString("HH:mm-") + end.ToString("HH:mm");

                            s.Event.Title = service[4];
                            s.Event.EId = (int)service[5];
                        }

                        services.Add(s);
                    }
                }

                serviceView.VirtualListSize = services.Count;
                serviceView.Invalidate();

                statusText.Text = "ok";
            }
            catch (Exception ex)
            {
                serviceView.VirtualListSize = 0;
                serviceView.Invalidate();

                statusText.Text = ex.Message;
            }
        }

        //チューナ選択
        private void tunerView_AfterSelect(object sender, TreeViewEventArgs arg)
        {
            tuner = arg.Node.Text;
            driver = (string)arg.Node.Tag;

            Reload(null, null);
        }

        //サービス選択(タブルクリック)
        private void serviceView_ItemActivate(object sender, EventArgs arg)
        {
            ShowTv();
        }

        private void showTvMenuItem_Click(object sender, EventArgs e)
        {
            ShowTv();
        }

        async void ShowTv()
        {
            if (tuner == null)
                return;

            try
            {
                var client = new WebClient();
                var fsid = services[serviceView.FocusedItem.Index].Fsid;

                var url = "http://localhost:" + port + "/webapi/ShowServer?tuner={0}&fsid={1}".Formatex(tuner, fsid);
                var data = await client.DownloadStringTaskAsync(url);
                var ret = DynamicJson.Parse(data);

                if (ret.code != 0)
                    throw new Exception(ret.message);
            }
            catch (Exception ex)
            {
                statusText.Text = ex.Message;
                MessageBox.Show(ex.Message);
            }
        }

        async void closeTvMenuItem_Click(object sender, EventArgs e)
        {
            if (tuner == null)
                return;

            try
            {
                var client = new WebClient();
                var fsid = services[serviceView.FocusedItem.Index].Fsid;

                var url = "http://localhost:" + port + "/webapi/CloseServer?tuner={0}".Formatex(tuner);
                var data = await client.DownloadStringTaskAsync(url);
                var ret = DynamicJson.Parse(data);

                if (ret.code != 0)
                    throw new Exception(ret.message);
            }
            catch (Exception ex)
            {
                statusText.Text = ex.Message;
                MessageBox.Show(ex.Message);
            }
        }

        async void recordAddMenuItem_Click(object sender, EventArgs arg)
        {
            if (tuner == null)
                return;

            try
            {
                var service = services[serviceView.FocusedItem.Index];

                if (service.Event.EId == -1)
                {
                    var msg = "番組情報が無いため予約できません。TVTestの録画機能で録画してください。TVTestを開きますか？";
                    var ret = MessageBox.Show(msg, "", MessageBoxButtons.OKCancel);

                    if (ret == DialogResult.OK)
                        ShowTv();
                }
                else
                {
                    var client = new WebClient();

                    var url = "http://localhost:" + port + "/webapi/AddReserve?fsid={0}&eid={1}".Formatex(service.Fsid, service.Event.EId);
                    var data = await client.DownloadStringTaskAsync(url);
                    var ret = DynamicJson.Parse(data);

                    if (ret.code == 0)
                        MessageBox.Show("予約しました。");
                    else
                        throw new Exception("予約に失敗しました。" + ret.message);
                }
            }
            catch (Exception ex)
            {
                statusText.Text = ex.Message;
                MessageBox.Show(ex.Message);
            }
        }

        private void fontChangeMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new FontDialog();

            dialog.Font = tunerView.Font;

            if (dialog.ShowDialog() != DialogResult.Cancel)
                Font = dialog.Font;
        }
    }
}
