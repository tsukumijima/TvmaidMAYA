using System;
using System.Collections.Generic;

namespace Tvmaid
{
    //HLSプレイリスト基本クラス
    abstract class HlsPlaylist
    {
        protected class HlsSegment
        {
            public string Name;
            public double Duration;
        }

        protected List<HlsSegment> segments = new List<HlsSegment>();
        protected double length = 0;  //全体の長さ

        HlsSegment next = null;  //読み込み中のセグメント情報

        public abstract string GetList();

        //start: リストに書き込む最初の位置
        //max: リスト最大数
        //sequence: #EXT-X-MEDIA-SEQUENCE
        public string GetList(int start, int max, int sequence)
        {
            var header = "#EXTM3U\n"
                        + "#EXT-X-VERSION:3\n"
                        + "#EXT-X-ALLOW-CACHE:NO\n"
                        + "#EXT-X-MEDIA-SEQUENCE:{0}\n".Formatex(sequence);

            var list = "";
            var maxDuration = 0.0;

            for (var i = start; i < segments.Count && i < start + max; i++)
            {
                list += "#EXTINF:{0:0.000000},\n/hls/{1}\n".Formatex(segments[i].Duration, segments[i].Name);
                maxDuration = segments[i].Duration > maxDuration ? segments[i].Duration : maxDuration;
            }

            header += "#EXT-X-TARGETDURATION:{0}\n".Formatex(Math.Ceiling(maxDuration));

            return header + list;
        }

        public int Count
        {
            get
            {
                lock (segments)
                    return segments.Count;
            }
        }

        public void GetSegmentNames(Action<string> action)
        {
            lock (segments)
            {
                foreach (var seg in segments)
                    action(seg.Name);
            }
        }

        //プレイリストを1行ずつ読み込み
        public void Load(string line)
        {
            if (next == null && line.StartsWith("#EXTINF:"))
            {
                next = new HlsSegment();

                try
                {
                    next.Duration = line.Replace("#EXTINF:", "").Replace(",", "").ToDouble();
                }
                catch
                {
                    next = null;
                }
            }
            else if (next != null)
            {
                next.Name = line;

                lock (segments)
                {
                    segments.Add(next);
                    length += next.Duration;
                }

                next = null;
            }
        }
    }

    class HlsLivePlaylist : HlsPlaylist
    {
        public override string GetList()
        {
            lock (segments)
            {
                const int max = 30; //60秒保持する
                var start = segments.Count <= max ? 0 : segments.Count - max;

                return GetList(start, segments.Count, start);
            }
        }
    }

    class HlsRecordPlaylist : HlsPlaylist
    {
        bool reset = true;
        int start = 0;
        int count = 3;

        public bool Seekable(int pos)
        {
            return pos < length;
        }

        public void Seek(int pos)
        {
            var time = 0.0;

            for (var i = 0; i < segments.Count; i++)
            {
                time += segments[i].Duration;
                if (time > pos)
                {
                    start = i == 0 ? 0 : (i - 1);
                    break;
                }
            }
            reset = true;
        }

        public override string GetList()
        {
            lock (segments)
            {
                if (reset)
                    count = 3;
                else
                    count = count <= segments.Count ? count + 3 : segments.Count;

                reset = false;

                return GetList(start, count, 0);
            }
        }
    }
}
