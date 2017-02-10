/* global CKEDITOR */

// eslint-disable-next-line no-extra-semi
; (function init() {
  var settings = {
    pluginName: 'Typographer',
    pluginDesc: 'Типографер',
    iconPath: '/Backend/Scripts/Quantumart/ckeditor/typographer/images/typographer.gif'
  };

  // eslint-disable-next-line max-statements
  var fixWithRegexps = function fixWithRegexps(str, shouldUseEng) {
    var result = str;
    var extLeft = shouldUseEng ? '&ldquo;' : '&laquo;';
    var extRight = shouldUseEng ? '&rdquo;' : '&raquo;';
    var intLeft = shouldUseEng ? '&lsquo;' : '&bdquo;';
    var intRight = shouldUseEng ? '&rsquo;' : '&ldquo;';

    // fix
    result = result.replace(/&quot;/g, '"');

    // rekavychking
    result = result.replace(/&ldquo;/g, '"');
    result = result.replace(/&rdquo;/g, '"');
    result = result.replace(/&bdquo;/g, '"');
    result = result.replace(/&laquo;/g, '"');
    result = result.replace(/&raquo;/g, '"');
    result = result.replace(/&lsquo;/g, '"');
    result = result.replace(/&rsquo;/g, '"');

    // wordfix
    result = result.replace(/«/g, '"');
    result = result.replace(/»/g, '"');
    result = result.replace(/“/g, '"');
    result = result.replace(/”/g, '"');
    result = result.replace(/…/g, '...');
    result = result.replace(/–/g, '-');
    result = result.replace(/—/g, '-');

    // kavychking
    // eslint-disable-next-line no-control-regex
    result = result.replace(/([\x01-(\s\"])(\")([^\"]{1,})([^\s\"(])(\")/g, '$1«$3$4»');

    // kavychking in kavychking
    if (/"/.test(result)) {
      // eslint-disable-next-line no-control-regex
      result = result.replace(/([\x01(\s\"])(\")([^\"]{1,})([^\s\"(])(\")/g, '$1«$3$4»');
      while (/(«)([^»]*)(«)/.test(result)) {
        result = result.replace(/(«)([^»]*)(«)([^»]*)(»)/g, '$1$2' + intLeft + '$4' + intRight);
      }
    }

    result = result.replace(/«/g, extLeft);
    result = result.replace(/»/g, extRight);
    result = result.replace(/ +/g, ' ');
    result = result.replace(/ -{1,2} /g, '&nbsp;&#151; ');
    result = result.replace(/\.{3}/g, '&hellip;');
    result = result.replace(/([>|\s])- /g, '$1&#151; ');
    result = result.replace(/^- /g, '&#151; ');
    result = result.replace(/(\d)-(\d)/g, '$1&#150;$2');
    result = result.replace(/(\S+)-(\S+)/g, '<nobr>$1-$2</nobr>');
    result = result.replace(/(\d)\s/g, '$1&nbsp;');
    result = result.replace(/([А-Яа-яA-Za-z]) (ли|ль|же|ж|бы|б|млн|млрд|руб)([^А-Яа-яA-Za-z])/gi, '$1&nbsp;$2$3');
    result = result.replace(/(\s)([А-Яа-я]{1})\s/g, '$1$2&nbsp;');
    result = result.replace(/(&nbsp;)([А-Яа-я]{1})\s/g, '$1$2&nbsp;');

    // инициалы
    // (А.С.Пушкин)
    // (Пушкин А.С)
    // (Пушкин А. С)
    // а.C. Пушкин
    // пушкин А. С.
    result = result.replace(
      /([А-ЯA-Z])([\. ]{1})[ ]{0,1}([А-ЯA-Z])([\. ]{1})[ ]{0,1}([А-ЯA-Z][А-Яа-яA-Za-z]*)/g,
      '$1.&nbsp;$3.&nbsp;$5'
    );

    result = result.replace(
      /([А-ЯA-Z][А-Яа-яA-Za-z]*) ([А-ЯA-Z])[\. ]{1}[ ]{0,1}([А-ЯA-Z])\.([,\ )]{1})/g,
      '$1&nbsp;$2.&nbsp;$3.$4'
    );

    // сокращения (напр. ул.)
    result = result.replace(/(\s[а-я]{1,2})\.\s/g, '$1.&nbsp;');

    // trade marks
    result = result.replace(/'/g, '&#146;');
    result = result.replace(/\(c\)/gi, '&copy;');
    result = result.replace(/\(r\)/gi, '&reg;');
    result = result.replace(/\(tm\)/gi, '&trade;');
    result = result.replace(/№ /gi, '&#8470;&nbsp;');

    return result;
  };

  var processHtml = function processHtml(str, shouldUseEng) {
    var i = 0;
    var result = str;
    var regexp = /<([^>]*)>/;
    var matches = result.match(/<([^>]*)>/g);

    while (regexp.test(result)) {
      i += 1;
      result = result.replace(regexp, '\x01' + i + '\x02');
    }

    result = fixWithRegexps(result, shouldUseEng);
    i = 0;

    // eslint-disable-next-line no-control-regex
    regexp = /\x01([0-9]*)\x02/;
    while (regexp.test(result)) {
      i += 1;
      result = result.replace(regexp, matches[i - 1]);
    }

    return result;
  };

  CKEDITOR.plugins.add(settings.pluginName, {
    init: function onInit(editor) {
      if (editor.contextMenu) {
        editor.addMenuGroup('qp8', 10);
        editor.addMenuItem(settings.pluginName, {
          label: settings.pluginDesc,
          command: settings.pluginName,
          group: 'qp8'
        });

        editor.contextMenu.addListener(function contextMenuListener() {
          return { typographer: CKEDITOR.TRISTATE_OFF };
        });
      }

      editor.addCommand(settings.pluginName, {
        exec: function onExec(execEditor) {
          var html, proccessedHtml;
          if (execEditor.mode === 'source' && execEditor.textarea) {
            html = execEditor.textarea.getValue();
            proccessedHtml = processHtml(html, execEditor.config.useEnglishQuotes);
            execEditor.textarea.setValue(proccessedHtml);
          } else {
            html = execEditor.getData();
            proccessedHtml = processHtml(html, execEditor.config.useEnglishQuotes);
            execEditor.setData(proccessedHtml);
          }
        }
      });

      editor.ui.addButton(settings.pluginName, {
        label: settings.pluginDesc,
        command: settings.pluginName,
        icon: settings.iconPath
      });

      editor.on('mode', function onMode() {
        if (editor.readOnly === false) {
          editor.getCommand(settings.pluginName).enable();
        }
      });
    }
  });
}());
