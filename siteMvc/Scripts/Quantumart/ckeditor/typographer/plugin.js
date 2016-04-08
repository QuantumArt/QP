/* jshint -W071, -W074 */

; (function() {
  'use strict';

  var settings = {
    pluginName: 'Typographer',
    pluginDesc: 'Типографер',
    iconPath: '/Backend/Scripts/Quantumart/ckeditor/typographer/images/typographer.gif'
  };

  function fixWithRegexps(s, shouldUseEng) {
    var extLeft = shouldUseEng ? '&ldquo;' : '&laquo;';
    var extRight = shouldUseEng ? '&rdquo;' : '&raquo;';
    var intLeft = shouldUseEng ? '&lsquo;' : '&bdquo;';
    var intRight = shouldUseEng ? '&rsquo;' : '&ldquo;';

    // fix
    s = s.replace(/&quot;/g, '"');

    // rekavychking
    s = s.replace(/&ldquo;/g, '"');
    s = s.replace(/&rdquo;/g, '"');
    s = s.replace(/&bdquo;/g, '"');
    s = s.replace(/&laquo;/g, '"');
    s = s.replace(/&raquo;/g, '"');
    s = s.replace(/&lsquo;/g, '"');
    s = s.replace(/&rsquo;/g, '"');

    // wordfix
    s = s.replace(/«/g, '"');
    s = s.replace(/»/g, '"');
    s = s.replace(/“/g, '"');
    s = s.replace(/”/g, '"');
    s = s.replace(/…/g, '...');
    s = s.replace(/–/g, '-');
    s = s.replace(/—/g, '-');

    // kavychking
    s = s.replace(/([\x01-(\s\"])(\")([^\"]{1,})([^\s\"(])(\")/g, '$1«$3$4»');

    // kavychking in kavychking
    if (/"/.test(s)) {
      s = s.replace(/([\x01(\s\"])(\")([^\"]{1,})([^\s\"(])(\")/g, '$1«$3$4»');
      while (/(«)([^»]*)(«)/.test(s)) {
        s = s.replace(/(«)([^»]*)(«)([^»]*)(»)/g, '$1$2' + intLeft + '$4' + intRight);
      }
    }

    s = s.replace(/«/g, extLeft);
    s = s.replace(/»/g, extRight);
    s = s.replace(/ +/g, ' ');
    s = s.replace(/ -{1,2} /g, '&nbsp;&#151; ');
    s = s.replace(/\.{3}/g, '&hellip;');
    s = s.replace(/([>|\s])- /g, '$1&#151; ');
    s = s.replace(/^- /g, '&#151; ');
    s = s.replace(/(\d)-(\d)/g, '$1&#150;$2');
    s = s.replace(/(\S+)-(\S+)/g, '<nobr>$1-$2</nobr>');
    s = s.replace(/(\d)\s/g, '$1&nbsp;');
    s = s.replace(/([А-Яа-яA-Za-z]) (ли|ль|же|ж|бы|б|млн|млрд|руб)([^А-Яа-яA-Za-z])/gi, '$1&nbsp;$2$3');
    s = s.replace(/(\s)([А-Яа-я]{1})\s/g, '$1$2&nbsp;');
    s = s.replace(/(&nbsp;)([А-Яа-я]{1})\s/g, '$1$2&nbsp;');

    // инициалы
    // (А.С.Пушкин)
    // (Пушкин А.С)
    // (Пушкин А. С)
    // а.C. Пушкин
    // пушкин А. С.
    s = s.replace(/([А-ЯA-Z])([\. ]{1})[ ]{0,1}([А-ЯA-Z])([\. ]{1})[ ]{0,1}([А-ЯA-Z][А-Яа-яA-Za-z]*)/g, '$1.&nbsp;$3.&nbsp;$5');
    s = s.replace(/([А-ЯA-Z][А-Яа-яA-Za-z]*) ([А-ЯA-Z])[\. ]{1}[ ]{0,1}([А-ЯA-Z])\.([,\ )]{1})/g, '$1&nbsp;$2.&nbsp;$3.$4');

    // сокращения (напр. ул.)
    s = s.replace(/(\s[а-я]{1,2})\.\s/g, '$1.&nbsp;');

    // trade marks
    s = s.replace(/'/g, '&#146;');
    s = s.replace(/\(c\)/gi, '&copy;');
    s = s.replace(/\(r\)/gi, '&reg;');
    s = s.replace(/\(tm\)/gi, '&trade;');
    s = s.replace(/№ /gi, '&#8470;&nbsp;');

    return s;
  }

  function processHtml(s, shouldUseEng) {
    var i = 0;
    var a = s.match(/<([^>]*)>/g);
    var r = /<([^>]*)>/;

    while (r.test(s)) {
      i++;
      s = s.replace(r, '\x01' + i + '\x02');
    }

    s = fixWithRegexps(s, shouldUseEng);

    i = 0;
    r = /\x01([0-9]*)\x02/;
    while (r.test(s)) {
      i++;
      s = s.replace(r, a[i - 1]);
    }

    return s;
  }

  CKEDITOR.plugins.add(settings.pluginName, {
    init: function(editor) {
      if (editor.contextMenu) {
        editor.addMenuGroup('qp8', 10);
        editor.addMenuItem(settings.pluginName, {
          label: settings.pluginDesc,
          command: settings.pluginName,
          group: 'qp8'
        });

        editor.contextMenu.addListener(function() {
          return { typographer: CKEDITOR.TRISTATE_OFF };
        });
      }

      editor.addCommand(settings.pluginName, {
        exec: function(editor) {
          var html, proccessedHtml;

          if (editor.mode === 'source') {
            html = editor.textarea.getValue();
            proccessedHtml = processHtml(html, editor.config.useEnglishQuotes);
            editor.textarea.setValue(proccessedHtml);
          } else {
            html = editor.getData();
            proccessedHtml = processHtml(html, editor.config.useEnglishQuotes);
            editor.setData(proccessedHtml);
          }
        }
      });

      editor.ui.addButton(settings.pluginName, {
        label: settings.pluginDesc,
        command: settings.pluginName,
        icon: settings.iconPath
      });

      editor.on('mode', function() {
        if (editor.readOnly === false) {
          editor.getCommand(settings.pluginName).enable();
        }
      });
    }
  });
})();
