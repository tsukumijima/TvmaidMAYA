using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Tvmaid
{
    class TvServer: TvServerApi
    {
        const int timeout = 30 * 1000;

        //参照カウンタ
        static RefCounter refCounter = new RefCounter();

        public TvServer(Tuner tuner) : base (tuner) { }
        
        public new void Open(bool show = false)
        {
            using (new MutexEx("/tvmaid/mutex/tvserver/start/" + tuner.DriverId, timeout))
            {
                if (IsOpen())
                    return;

                base.Open(show);

                //TVTestのウインドウがアイドルになっても、プラグインの初期化が終わっているとは限らない
                //プラグインの通信用ウインドウが作成されるのを待つ
                var interval = 100;

                for (int i = 0; i < timeout / interval; i++)
                {
                    if (IsOpen())
                        return;
                    else
                        Thread.Sleep(interval);
                }

                throw new Exception("TVTestの初期化が時間内に終了しませんでした。[原因]TVTestが初期化中にエラーになったか、PCの負荷が高過ぎる等が考えられます。");
            }
        }

        public new void Close()
        {
            if (IsOpen() == false)
                return;

            //参照カウント確認
            if (refCounter.Count(tuner.Name) > 0 || IsRecording)
                return;

            using (new MutexEx("/tvmaid/mutex/tvserver/start/" + tuner.DriverId, timeout))
            {
                if (IsOpen())
                {
                    try
                    {
                        base.Close();
                    }
                    catch { }
                }

                //閉じるのを待つ
                //待たないと、すぐに次の予約があるとき録画が失敗する
                var interval = 100;

                for (int i = 0; i < timeout / interval; i++)
                {
                    if (IsOpen() == false)
                        return;
                    else
                        Thread.Sleep(interval);
                }

                Log.Error("TVTestが時間内に終了しませんでした。");
            }
        }

        public void AddRef()
        {
            refCounter.Inc(tuner.Name);
        }

        public void RemoveRef()
        {
            refCounter.Dec(tuner.Name);
        }

        //状態を取得
        public new State GetState()
        {
            try
            {
                if (IsOpen())
                    return (State)base.GetState();
                else
                    return State.Stoped;
            }
            catch
            {
                return State.Unknown;
            }
        }

        //サービス切り替え
        public new void SetService(Service service)
        {
            try
            {
                if (IsRecording)
                    return;

                base.SetService(service);
            }
            catch (TvServerExceotion ex)
            {
                if (ex.Code == (int)ErrorCode.SetService)
                {
                    //リトライ
                    Thread.Sleep(1000);
                    base.SetService(service);
                }
                else
                    throw;
            }
        }

        public bool IsRecording
        {
            get
            {
                var state = GetState();
                return (state == State.Recoding || state == State.RecPaused);
            }
        }        
    }

    //参照カウンタ
    class RefCounter
    {
        Dictionary<string, int> counter = new Dictionary<string, int>();

        public void Inc(string name)
        {
            lock (counter)
            {
                if (counter.ContainsKey(name) == false)
                    counter.Add(name, 1);
                else
                    counter[name]++;
            }
            Debug.WriteLine("TvServer ref count {0}: {1}".Formatex(name, counter[name]));
        }

        public void Dec(string name)
        {
            lock (counter)
            {
                if (counter.ContainsKey(name) == false)
                    return;
                else
                    counter[name]--;
            }
            Debug.WriteLine("TvServer ref count {0}: {1}".Formatex(name, counter[name]));
        }

        public int Count(string name)
        {
            lock (counter)
            {
                if (counter.ContainsKey(name))
                    return counter[name];
                else
                    return 0;
            }
        }
    }
}
