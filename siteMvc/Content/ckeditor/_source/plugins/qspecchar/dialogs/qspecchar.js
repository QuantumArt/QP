/*
Copyright (c) 2003-2011, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

CKEDITOR.dialog.add('qspecchar', function (editor) {
	/**
	* Simulate "this" of a dialog for non-dialog events.
	* @type {CKEDITOR.dialog}
	*/
	var dialog,
		lang = editor.lang.qspecchar;

	var charContainerHtml =
	'<div class="qspecchar">' +
		'<div class="charsTable"></div>' +
		'<div class="charDetail">' +
			'<div class="sample">&nbsp;</div>' +
			'<div><span class="blockHead">HTML-code:</span></div><div style="height: 30px;" class="code">&nbsp;</div>' +
			'<div><span class="blockHead">NUM-code:</span></div><div style="height: 30px;" class="num">&nbsp;</div>' +
		'</div>' +
	'</div>';

	return {
		title: lang.title,
		minWidth: 703,
		minHeight: 320,
		resizable: CKEDITOR.DIALOG_RESIZE_NONE,
		buttons: [CKEDITOR.dialog.cancelButton],
		onLoad: function () {
			var cols = 20;
			var i = 0;
			var aChars = CKEDITOR.config.qspecchar.chars;
			var $charContainer = jQuery(this.getContentElement('info', 'charContainer').getElement().$);
			var html = new $.telerik.stringBuilder();

			// сформировать таблицу символов
			html.cat('<table cellpadding="2" cellspacing="1" align="center" border="0">');
			while (i < aChars.length) {

				html.cat('<TR>');
				for (var j = 0; j < cols; j++) {
					if (aChars[i])
						html
							.cat('<TD align="center" class="hand" data-index="').cat(i).cat('">')
								.cat('<div class="entity">')
									.cat(aChars[i].entity)
								.cat('</div>')
							.cat('</TD>');
					else
						html.cat('<TD><div class="emptyCell">&nbsp;</div></TD>');
					i++;
				}
				html.cat('</TR>');
			}
			html.cat('</table>');

			var $charsTable = jQuery('.charsTable', $charContainer);
			$charsTable.html(html.string());
			jQuery('.hand', $charsTable).each(function (index, elem) {
				var $hand = $(elem);
				var index = $hand.data("index");
				var $entity = jQuery(".entity", $hand);
				$entity.data("entity", aChars[index].entity);
				$entity.data("decimal", aChars[index].decimal);
				$hand = null;
				$entity = null;
			});

			jQuery('.entity', $charsTable)
				.click(function () {
					var span = editor.document.createElement('span');
					span.setHtml(jQuery(this).text());
					editor.insertText(span.getText());
					CKEDITOR.dialog.getCurrent().hide()
				})
				.mouseover(function () {
					var $entity = jQuery(this);
					var $charDetail = jQuery('.charDetail', $charContainer);
					jQuery('.sample', $charDetail).html($entity.html());
					jQuery('.code', $charDetail).text($entity.data('entity'));
					jQuery('.num', $charDetail).text($entity.data('decimal'));
					$entity = null;
					$charDetail = null;
				})
				.mouseout(function () {					
					var $charDetail = jQuery('.charDetail', $charContainer);
					jQuery('.sample', $charDetail).html("&nbsp;");
					jQuery('.code', $charDetail).html("&nbsp;");
					jQuery('.num', $charDetail).html("&nbsp;");
					$charDetail = null;
				});


			$charsTable = null;
			aChars = null;
			html = null;
			$charContainer = null;

		},
		contents: [
			{
				id: 'info',
				label: editor.lang.common.generalTab,
				title: editor.lang.common.generalTab,
				padding: 0,
				align: 'top',
				elements: [
					{
						type: 'hbox',
						align: 'top',
						widths: ['703px'],
						children:
						[
							{
								type: 'html',
								id: 'charContainer',
								html: charContainerHtml,
								onLoad: function (event) {
									dialog = event.sender;
								}
							}
						]
					}
				]
			}
		]
	};
});
