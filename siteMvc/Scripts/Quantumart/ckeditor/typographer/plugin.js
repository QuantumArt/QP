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
    var fixToMdashFn;
    var result = str;
    var apostropheHtmlCode = '&#146;';
    var numericHtmlCode = '&#8470;';
    var extLeft = shouldUseEng ? '&ldquo;' : '&laquo;';
    var extRight = shouldUseEng ? '&rdquo;' : '&raquo;';
    var intLeft = shouldUseEng ? '&lsquo;' : '&bdquo;';
    var intRight = shouldUseEng ? '&rsquo;' : '&ldquo;';

    var quotesToReplace = [
      '&quot;',
      '&ldquo;',
      '&rdquo;',
      '&bdquo;',
      '&laquo;',
      '&raquo;',
      '&lsquo;',
      '&rsquo;',
      '«',
      '»',
      '“',
      '”'
    ];

    quotesToReplace.forEach(function foreach(quote) {
      result = result.replace(new RegExp(quote, 'g'), '"');
    });

    // wordfix
    result = result.replace(/…/g, '...');

    // &#8211; &ndash;
    result = result.replace(/–/g, '-');

    // &#8212; &mdash;
    result = result.replace(/—/g, '-');

    // advanced quotes
    // eslint-disable-next-line no-control-regex
    result = result.replace(/([\x01-(\s"])(")([^"]{1,})([^\s"(])(")/g, '$1«$3$4»');

    // quotes in quotes
    if (/"/.test(result)) {
      // eslint-disable-next-line no-control-regex
      result = result.replace(/([\x01(\s"])(")([^"]{1,})([^\s"(])(")/g, '$1«$3$4»');
      while (/(«)([^»]*)(«)/.test(result)) {
        result = result.replace(/(«)([^»]*)(«)([^»]*)(»)/g, '$1$2' + intLeft + '$4' + intRight);
      }
    }

    fixToMdashFn = function(result, symbol) {
      var tempResult = result;
      tempResult = tempResult.replace(new RegExp(' (' + symbol + '){1,2} ', 'g'), '&nbsp;&mdash; ');
      tempResult = tempResult.replace(new RegExp('([>|\\s])' + symbol + ' ', 'g'), '$1&mdash; ');
      tempResult = tempResult.replace(new RegExp('^' + symbol + ' ', 'g'), '&mdash; ');
      return tempResult;
    };

    result = result.replace(/«/g, extLeft);
    result = result.replace(/»/g, extRight);
    result = result.replace(/ +/g, ' ');
    result = result.replace(/\.{3}/g, '&hellip;');

    result = fixToMdashFn(result, '-');
    result = fixToMdashFn(result, '–');
    result = fixToMdashFn(result, '&ndash;');

    result = result.replace(/(\d)-(\d)/g, '$1&ndash;$2');
    result = result.replace(/(\S+)-(\S+)/g, function replacer(match, p1, p2) {
      if (p1.length <= 3 || p2.length <= 3) {
        return '<nobr>' + p1 + '-' + p2 + '</nobr>';
      }

      return match;
    });

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
      /([А-ЯA-Z])([. ]{1})[ ]{0,1}([А-ЯA-Z])([. ]{1})[ ]{0,1}([А-ЯA-Z][А-Яа-яA-Za-z]*)/g,
      '$1.&nbsp;$3.&nbsp;$5'
    );

    result = result.replace(
      /([А-ЯA-Z][А-Яа-яA-Za-z]*) ([А-ЯA-Z])[. ]{1}[ ]{0,1}([А-ЯA-Z])\.([, )]{1})/g,
      '$1&nbsp;$2.&nbsp;$3.$4'
    );

    // сокращения (напр. ул.)
    result = result.replace(/(\s[а-я]{1,2})\.\s/g, '$1.&nbsp;');

    // trade marks
    result = result.replace(/'/g, apostropheHtmlCode);
    result = result.replace(/\(c\)/gi, '&copy;');
    result = result.replace(/\(r\)/gi, '&reg;');
    result = result.replace(/\(tm\)/gi, '&trade;');
    result = result.replace(/№ /gi, numericHtmlCode + '&nbsp;');

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
