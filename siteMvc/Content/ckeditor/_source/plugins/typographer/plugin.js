/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

/**
 * @file Break plugin.
 */

(function () {
	var pluginName = 'typographer';

	var command =
	{
		canUndo: false,    // The undo snapshot will be handled by 'insertElement'.
		exec: function (editor) {
			if (editor.mode == 'source') {
				var html = editor.textarea.getValue();
				var proccessedHtml = processHtml(html, editor.config.useEnglishQuotes);
				editor.textarea.setValue(proccessedHtml);
			}
			else {
				var html = editor.getData();
				var proccessedHtml = processHtml(html, editor.config.useEnglishQuotes);
				editor.setData(proccessedHtml);
			}
		}
	};

	var processHtml = function (s, eng) {
		var i = 0;
		// put in array
		var a = s.match(/<([^>]*)>/g)
		var r = /<([^>]*)>/;
		while (r.test(s)) {
			i++;
			s = s.replace(r, "\x01" + i + "\x02");
		}

		//fix
		s = s.replace(/&quot;/g, "\"");

		//rekavychking
		s = s.replace(/&ldquo;/g, "\"");
		s = s.replace(/&rdquo;/g, "\"");
		s = s.replace(/&bdquo;/g, "\"");
		s = s.replace(/&laquo;/g, "\"");
		s = s.replace(/&raquo;/g, "\"");
		s = s.replace(/&lsquo;/g, "\"");
		s = s.replace(/&rsquo;/g, "\"");

		// wordfix
		s = s.replace(/«/g, "\"");
		s = s.replace(/»/g, "\"");
		s = s.replace(/“/g, "\"");
		s = s.replace(/”/g, "\"");
		s = s.replace(/…/g, "...");
		s = s.replace(/–/g, "-");
		s = s.replace(/—/g, "-");		
		var extLeft = (eng) ? '&ldquo;' : '&laquo;';
		var extRight = (eng) ? '&rdquo;' : '&raquo;';
		var intLeft = (eng) ? '&lsquo;' : '&bdquo;';
		var intRight = (eng) ? '&rsquo;' : '&ldquo;';

		// kavychking
		s = s.replace(/([\x01-(\s\"])(\")([^\"]{1,})([^\s\"(])(\")/g, "$1«$3$4»");
		
		// kavychking in kavychking
		if (/"/.test(s)) {
			s = s.replace(/([\x01(\s\"])(\")([^\"]{1,})([^\s\"(])(\")/g, "$1«$3$4»");
			while (/(«)([^»]*)(«)/.test(s))
				s = s.replace(/(«)([^»]*)(«)([^»]*)(»)/g, "$1$2" + intLeft + "$4" + intRight);
		}
		s = s.replace(/«/g, extLeft);
		s = s.replace(/»/g, extRight);
		s = s.replace(/ +/g, ' ');
		s = s.replace(/ -{1,2} /g, '&nbsp;&#151; ');
		s = s.replace(/\.{3}/g, '&hellip;');
		s = s.replace(/([>|\s])- /g, "$1&#151; ");
		s = s.replace(/^- /g, "&#151; ");
		s = s.replace(/(\d)-(\d)/g, "$1&#150;$2");
		s = s.replace(/(\S+)-(\S+)/g, "<nobr>$1-$2</nobr>");
		s = s.replace(/(\d)\s/g, "$1&nbsp;");
		s = s.replace(/([А-Яа-яA-Za-z]) (ли|ль|же|ж|бы|б|млн|млрд|руб)([^А-Яа-яA-Za-z])/gi, '$1&nbsp;$2$3');
		s = s.replace(/(\s)([А-Яа-я]{1})\s/g, '$1$2&nbsp;');
		s = s.replace(/(&nbsp;)([А-Яа-я]{1})\s/g, '$1$2&nbsp;');

		// инициалы
		// A.C. Пушкин
		s = s.replace(/([А-ЯA-Z])([\. ]{1})[ ]{0,1}([А-ЯA-Z])([\. ]{1})[ ]{0,1}([А-ЯA-Z][А-Яа-яA-Za-z]*)/g, '$1.&nbsp;$3.&nbsp;$5');
		// Пушкин А. С.
		s = s.replace(/([А-ЯA-Z][А-Яа-яA-Za-z]*) ([А-ЯA-Z])[\. ]{1}[ ]{0,1}([А-ЯA-Z])\.([,\ )]{1})/g, '$1&nbsp;$2.&nbsp;$3.$4');
		// сокращения типа ул.
		s = s.replace(/(\s[а-я]{1,2})\.\s/g, '$1.&nbsp;');
		// (А.С.Пушкин)
		// (Пушкин А.С)
		// (Пушкин А. С)
		s = s.replace(/'/g, "&#146;");
		s = s.replace(/\(c\)/gi, "&copy;");
		s = s.replace(/\(r\)/gi, "&reg;");
		s = s.replace(/\(tm\)/gi, "&trade;");
		s = s.replace(/№ /gi, "&#8470;&nbsp;");

		// out array
		i = 0;
		r = /\x01([0-9]*)\x02/;
		while (r.test(s)) {
			i++;
			s = s.replace(r, a[i - 1]);
		}
		return s; 
	}


	// Register a plugin named "linebreak".
	CKEDITOR.plugins.add(pluginName,
	{
		init: function (editor) {
			editor.addCommand(pluginName, command);

			editor.ui.addButton('Typographer',
				{
					label: editor.lang.typographer.toolbar,
					command: pluginName,
					icon: CKEDITOR.plugins.getPath('typographer') + "/images/typographer.gif"
				});

			editor.on('mode', function () {
				if (editor.readOnly == false)
					editor.getCommand(pluginName).enable();
			});
		}
	});
})();
