"use strict"

//javascript汎用関数

//文字列フォーマット
//@author phi
//
//添字引数版
//var str = "{0} : {1} + {2} = {3}".format("足し算", 8, 0.5, 8+0.5);
//
//オブジェクト版
//str = "名前 : {name}, 年齢 : {age}".format( { "name":"山田", "age":128 } );
//
if (String.prototype.format == undefined)
{
    String.prototype.format = function(arg)
    {
        // 置換ファンク
        var rep_fn = undefined;
        
        // オブジェクトの場合
        if (typeof arg == "object")
        {
            rep_fn = function(m, k) { return arg[k]; }
        }
        // 複数引数だった場合
        else
        {
            var args = arguments;
            rep_fn = function(m, k) { return args[ parseInt(k) ]; }
        }
        
        return this.replace( /\{(\w+)\}/g, rep_fn );
    }
}

//htmlエスケープ
String.prototype.escapeHTML = function()
{
  return this.replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
}

//htmlエスケープを戻す
String.prototype.unescapeHTML = function()
{
  var div = document.createElement("div");
  div.innerHTML = this.replace(/</g,"&lt;")
                     .replace(/>/g,"&gt;")
                     .replace(/ /g, "&nbsp;")
                     .replace(/\r/g, "&#13;")
                     .replace(/\n/g, "&#10;");
  return div.textContent || div.innerText;
}

//改行が含まれているテキスト
String.prototype.htmlText = function ()
{
    return this.escapeHTML().replace(/\r?\n/g, "<br>");
}

String.prototype.htmlText2 = function ()
{
    return this.escapeHTML().replace(/\n/g, "<br>");
}

Array.prototype.find = function (callback)
{
    for (var i = 0; i < this.length; i++)
    {
        if (callback(this[i]))
            return this[i]
    }
}

function debounce(fn, wait)
{
    var timer

    return function ()
    {
        clearTimeout(timer)

        timer = setTimeout(function ()
        {
            fn()
        }, wait)
    }
}

function setFullscreen(el, obj)
{
    if (!document.fullscreenElement && !document.mozFullScreenElement && !document.webkitFullscreenElement && !document.msFullscreenElement)
    {
        if (el.requestFullscreen)
            el.requestFullscreen();
        else if (el.msRequestFullscreen)
            el.msRequestFullscreen();
        else if (el.mozRequestFullScreen)
            el.mozRequestFullScreen();
        else if (el.webkitRequestFullscreen)
            el.webkitRequestFullscreen();
    }
    else
    {
        if (document.exitFullscreen)
            document.exitFullscreen();
        else if (document.msExitFullscreen)
            document.msExitFullscreen();
        else if (document.mozCancelFullScreen)
            document.mozCancelFullScreen();
        else if (document.webkitExitFullscreen)
            document.webkitExitFullscreen();
    }
}
