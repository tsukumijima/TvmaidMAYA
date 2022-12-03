using System;
using System.Net;
using System.Reflection;

namespace Tvmaid
{
    abstract class WebApiBase : WebTask
    {
        protected WebApiData ret = new WebApiData();

        protected WebApiBase(HttpListenerContext con) : base(con) { }

        public override void Run()
        {
            var uri = con.Request.Url.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            Execute(uri[1]);
        }

        //メソッドを検索して呼び出す
        void Execute(string func)
        {
            try
            {
                //メソッド呼び出し
                this.GetType().InvokeMember(
                    func,
                    BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    this,
                    null);
            }
            catch (TargetInvocationException tie)
            {
                //呼び出した関数で例外が発生したとき
                //Exceptionだと、InvokeMemberのエラーしか取得できない
                ret.SetCode(1, tie.InnerException.Message);
            }
            catch (MissingMethodException)
            {
                ret.SetCode(1, "指定されたWeb Apiはありません。" + func);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                ret.SetCode(1, ex.Message);
            }

            try
            {
                //クロスドメインを許可
                con.Response.Headers["Access-Control-Allow-Origin"] = "*";

                //JSON変換
                var json = Codeplex.Data.DynamicJson.Serialize(ret);
                var data = System.Text.Encoding.UTF8.GetBytes(json);

                con.Response.SendChunked = false;
                con.Response.ContentLength64 = data.Length;
                con.Response.ContentType = "application/json";
                con.Response.StatusCode = (int)HttpStatusCode.OK;
                con.Response.OutputStream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }

    //jsonに変換してクライアントへ返す
    class WebApiData
    {
        public int code { get; set; }       //リターンコード。0:成功、1:失敗
        public string message { get; set; }
        public object data1 { get; set; }

        public WebApiData()
        {
            SetCode(0, "");
        }

        public void SetCode(int code, string message)
        {
            this.code = code;
            this.message = message;
        }
    }
}
