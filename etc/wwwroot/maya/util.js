"use strict"

var Util = {

    getWeekText: function (num)
    {
        var weekText = ["日", "月", "火", "水", "木", "金", "土"];
        return weekText[num];
    },

    getSearchText: function (text)
    {
        var regx = /\[.+?\]|「.+?」|【.+?】|<.+?>|#\d*(?!\d)|第\d*[話回]/g;
        return text.replace(regx, "");
    },

    getWebSearchLink: function (word)
    {
        word = this.getSearchText(word)
        return "http://www.google.co.jp/search?q={0}".format(encodeURIComponent(word));
    },

    getTimeString: function (start, end)
    {
        return "{0} ({1}) {2} ～ {3} ({4} 分)".format(
                start.toString("M/d"),
                this.getWeekText(start.getDay()),
                start.toString("HH:mm"),
                end.toString("HH:mm"),
                Math.floor((end - start) / 1000 / 60))
    }
}

var SearchOpt = function (keyword, fsid, week, hour)
{
    this.keyword = keyword
    this.fsid = fsid
    this.week = week
    this.hour = hour
}

SearchOpt.fromJson = function(json)
{
    var opt = JSON.parse(json)
    return new SearchOpt(opt.keyword, opt.fsid, opt.week, opt.hour)
}

SearchOpt.prototype = {

    toJson: function ()
    {
        return JSON.stringify(this)
    },

    //select id from event where 条件...の形式で返す
    toSql: function ()
    {
        var sql = "select id from event where " + this.keywordToSql(this.keyword)
        sql += this.arrToSql(this.fsid, "fsid")
        sql += this.arrToSql(this.week, "week")
        sql += this.arrToSql(this.hour, "(start / 36000000000 % 24)")

        return sql
    },

    arrToSql: function (arr, name)
    {
        var sql = ""

        if (arr != null && arr.length > 0)
        {
            var str = arr.join(",")
            sql = " and {0} in ({1})".format(name, str)
        }

        return sql
    },

    keywordToSql: function (keyword)
    {
        var sql = "";

        if (keyword == null)
            keyword = ""

        var words = keyword.split(/ |　/g);	//スペースで区切る(全角OK)

        words.forEach(function (word)
        {
            if (word == "" || word == "-" || word == "+")
                return

            var sw = word.charAt(0);

            if (sw == "-")
            {
                if (sql != "") sql += "and ";
                sql += "not ";
                word = word.substr(1, word.length - 1);
            }
            else if (sw == "+")
            {
                if (sql != "") sql += "or ";
                word = word.substr(1, word.length - 1);
            }
            else
                if (sql != "") sql += "and ";

            sql += "(title||desc||longdesc||genre_text) like '%{0}%' escape '^'".format(Webapi.sqlLikeEncode(word));
            sql = "(" + sql + ")";  //and、orの優先順位を無くす
        })

        return sql == "" ? "1" : sql;
    }
}

var Genre = {

    getFirstClass: function (genres)
    {
        var id = (genres & 0xf0) >> 4
        return this.getClass(id)
    },

    getClass: function (id)
    {
        var arr =
        {
            0x0: "news",
            0x1: "sports",
            0x2: "infomation",
            0x3: "dorama",
            0x4: "music",
            0x5: "variety",
            0x6: "movie",
            0x7: "anime",
            0x8: "documentary",
            0x9: "performance",
            0xA: "education",
            0xB: "welfare",
            0xC: "genre-etc",
            0xD: "genre-etc",
            0xE: "genre-etc",
            0xF: "genre-etc",
        };
        return arr[id]
    },

    getText: function (id)
    {
        var arr =
        {
            0x0: "ニュース／報道",
            0x1: "スポーツ",
            0x2: "情報／ワイドショー",
            0x3: "ドラマ",
            0x4: "音楽",
            0x5: "バラエティ",
            0x6: "映画",
            0x7: "アニメ／特撮",
            0x8: "ドキュメンタリー／教養",
            0x9: "劇場／公演",
            0xA: "趣味／教育",
            0xB: "福祉",
            0xC: "予備",
            0xD: "予備",
            0xE: "拡張",
            0xF: "その他"
        }
        return arr[id];
    }
}

var KeywordHistoy = {

    getList: function ()
    {
        var hist = localStorage.getItem("keyword-history")

        if (hist == null)
            return []
        else
            return hist.split("||")
    },

    save: function (keyword)
    {
        if (keyword == "")
            return

        var list = this.getList()

        if (list.length > 0 && list[0] == keyword)
            return

        list.unshift(keyword)

        if (list.length > 30)
            list.pop()

        localStorage.setItem("keyword-history", list.join("||"))
    }
}

var ChatWindow = function ()
{
    this.fontsize = 24
    this.fps = 30
    this.speed = 200    //画面を流れる速さ(px/秒)
    this.adjust = 3     //テキスト位置調整(Firefox対策 3px下げて表示)
    this.listMax = 500

    this.list = []
    this.pxPerSec
    this.rows
    this.context
    this.canvas
    this.timer
    this.paused 
}

ChatWindow.prototype = {

    init: function (canvas, listInit)
    {
        if (listInit)
            this.list = []

        canvas.width = canvas.parentNode.clientWidth
        canvas.height = canvas.parentNode.clientHeight - this.adjust
        this.canvas = canvas
        this.context = canvas.getContext('2d')
        this.context.textBaseline = 'top'
        this.context.fillStyle = 'white'
        this.context.font = "bold {0}px sans-serif".format(this.fontsize)
        this.context.shadowColor = "#555";
        this.context.shadowOffsetX = 2;
        this.context.shadowOffsetY = 1;

        this.setRate(1)

        var count = Math.floor(canvas.height / this.fontsize)
        this.rows = new Array(count == 0 ? 1 : count)

        this.paused = false

        this.stop()
        this.timer = setInterval(this.draw.bind(this), 1000 / this.fps)
    },

    pause: function ()
    {
        this.paused = true
    },

    resume: function ()
    {
        this.paused = false
    },

    setRate: function (rate)
    {
        this.pxPerSec = this.speed * rate
    },

    stop: function ()
    {
        if (this.timer != null)
            clearInterval(this.timer)

        this.list = []
        this.context.clearRect(0, 0, this.canvas.width, this.canvas.height)
    },

    draw: function ()
    {
        if (this.paused)
            return

        this.context.clearRect(0, 0, this.canvas.width, this.canvas.height)

        //コメント描画
        this.list.forEach(function (chat)
        {
            if (chat.dx + chat.width > 0 && chat.dx < this.canvas.width)
                this.context.fillText(chat.text, chat.dx, chat.row * this.fontsize + this.adjust)

            if (chat.dx + chat.width > 0)
                chat.dx -= this.pxPerSec / this.fps

        }.bind(this))
    },

    add: function (array)
    {
        //画面外のコメントを削除
        this.list = this.list.filter(function (chat)
        {
            return chat.dx + chat.width > 0
        })

        array.forEach(function (chat, i, arr)
        {
            //多すぎるときは追加しない
            if (this.list.length > this.listMax)
                return

            var basetime = arr[0].time
            var basedx = i == 0 ? this.canvas.width : arr[0].dx 

            chat.dx = (chat.time - basetime) * this.pxPerSec + basedx
            chat.width = this.context.measureText(chat.text).width

            chat.row = this.getRow(chat)
            this.rows[chat.row] = chat

            this.list.push(chat)

        }.bind(this))        
    },

    getRow: function (chat)
    {
        var i   //行の位置

        //空いている行を探す
        for (i = 0; i < this.rows.length; i++)
        {
            if (this.rows[i] == null || this.rows[i].dx + this.rows[i].width < chat.dx)
                return i
        }

        //空いている行がないときは、一番重ならない行を探す
        this.rows.reduce(function (prev, current, index)
        {
            if (prev.dx + prev.width >= current.dx + current.width)
                i = index

            return prev.dx + prev.width >= current.dx + current.width ? current : prev

        }, this.rows[0])

        chat.dx = this.rows[i].dx + this.rows[i].width   //重ならないようにずらす
        return i
    }
}
