"use strict"

var Webapi = {

    exec: function (func, arg, callback)
    {
        $.ajaxSetup({ cache: false })  //edge対策

        $.getJSON("/webapi/" + func, arg, callback)
        .fail(function ()
        {
            Dialog.alert("通信に失敗しました。デバイスの通信環境やTvmaidが起動中かどうか確認してください。")
        })
    },

    getTable: function (sql, callback)
    {
        this.exec("GetTable", { sql: sql }, callback)
    },

    //SQL文で、文字列を指定する場合のエスケープ
    sqlEncode: function (text)
    {
        return text.split("'").join("''")
    },

    //SQLのlike文のエスケープ
    //like文を使う場合は「escape '^'」を追加してください
    sqlLikeEncode: function (text)
    {
        text = this.sqlEncode(text)

        text = text.split("^").join("^^")
        text = text.split("_").join("^_")
        text = text.split("%").join("^%")

        return text;
    },

    //DateTimeをXDateに変換
    convertXDate: function (time)
    {
        time = time / 10000 - 62135596800000	//ナノ秒をミリ秒にして、1970年分引く
        var timezone = (new Date()).getTimezoneOffset() * 60 * 1000	//タイムゾーン
        return new XDate(time + timezone)
    },

    //XDateをDateTimeに変換
    convertDateTime: function (xdate)
    {
        var timezone = (new Date()).getTimezoneOffset() * 60 * 1000
        return (xdate.getTime() - timezone + 62135596800000) * 10000
    }
}

var ReserveState = {

    isEnable: function (status)
    {
        return (status & 1) > 0;
    },

    isEventMode: function (status)
    {
        return (status & 2) > 0;
    },
    
    isTunerLock: function (status)
    {
        return (status & 4) > 0;
    },
    
    isOverlay: function (status)
    {
        return (status & 32) > 0;
    },

    isRecording: function (status)
    {
        return (status & 64) > 0;
    },

    isRecordEnd: function (status)
    {
        return (status & 128) > 0;
    }
}
