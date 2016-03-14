(function () {
	
	function checkup(dialog, checkboxId)
	{
		return dialog.getContentElement('main', checkboxId).getValue();
	}

	function CleanHtmlTags(html,isHTML,isCSS,isFont,isSpan,isWord,isWidth) {
	if (isHTML) {
		html = html.replace(/\<[^>]*\>/ig, "");
		html = html.replace(/\&nbsp;/ig," ");
	}
	else 
	{    
		if (isSpan) {
			html = html.replace(/\<span[^\>]*\>/ig, "");
			html = html.replace(/\<\/span[^\>]*\>/ig, "");
		}
		if (isFont){
			html = html.replace(/\<font[^\>]*\>/ig, "");
			html = html.replace(/\<\/font[^\>]*\>/ig, "");
		}
		if (isWord) {
			html = html.replace(/<o:p>\s*<\/o:p>/g, '') ;
			html = html.replace(/<o:p>[\s\S]*?<\/o:p>/g, '&nbsp;') ;

			// Remove mso-xxx styles.
			html = html.replace( /\s*mso-[^:]+:[^;"]+;?/gi, '' ) ;
			
			// Remove MsoXXX classes.
			html = html.replace(/\s*class[\s]*=[\s]*Mso[^\s\>]+/ig, "");
			html = html.replace(/\s*class[\s]*=[\s]*\"Mso[^\"]*\"/ig, "");
			html = html.replace(/\s*class[\s]*=[\s]*\'Mso[^\']*\'/ig, "");
			
			// Remove style, meta and link tags
			html = html.replace( /<STYLE[^>]*>[\s\S]*?<\/STYLE[^>]*>/gi, '' ) ;
			html = html.replace( /<(?:META|LINK)[^>]*>\s*/gi, '' ) ;
			
			// Remove Lang attributes
			html = html.replace(/<(\w[^>]*) lang=([^ |>]*)([^>]*)/gi, "<$1$3") ;
			
			// Remove XML elements and declarations
			html = html.replace(/<\\?\?xml[^>]*>/gi, '' ) ;

			// Remove w: tags with contents.
			html = html.replace( /<w:[^>]*>[\s\S]*?<\/w:[^>]*>/gi, '' ) ;

			// Remove Tags with XML namespace declarations: <o:p><\/o:p>
			html = html.replace(/<\/?\w+:[^>]*>/gi, '' ) ;

			// Remove comments [SF BUG-1481861].
			html = html.replace(/<\!--[\s\S]*?-->/g, '' ) ;
			
			// Remove language tags
			html = html.replace( /<(\w[^>]*) language=([^ |>]*)([^>]*)/gi, "<$1$3") ;

			// Remove onmouseover and onmouseout events (from MS Word comments effect)
			html = html.replace( /<(\w[^>]*) onmouseover="([^\"]*)"([^>]*)/gi, "<$1$3") ;
			html = html.replace( /<(\w[^>]*) onmouseout="([^\"]*)"([^>]*)/gi, "<$1$3") ;
		}
		if (isCSS) {
			html = html.replace(/\s*style[\s]*=[\s]*\"[^\"]*\"/ig, "");
			html = html.replace(/\s*style[\s]*=[\s]*\'[^\']*\'/ig, "");
			html = html.replace(/\s*style[\s]*=[\s]*[^\s\>]+/ig, "");
			html = html.replace(/\s*class[\s]*=[\s]*[^\s\>]+/ig, "");
			html = html.replace(/\s*class[\s]*=[\s]*\"[^\"]*\"/ig, "");
			html = html.replace(/\s*class[\s]*=[\s]*\'[^\']*\'/ig, "");
			//html = html.replace(/(\&nbsp;){2,}/ig, "");
			//html = html.replace(/\<pre[^\>]*\>/ig, "");
			//html = html.replace(/\<\/pre[^\>]*\>/ig, "");
		}
		if (isWidth) {
			html = html.replace(/(<(?:table|td|tr).*?[\s])width[\s]?=[\s]?(\"[^\"]*\"|[^\s]+|\'[^\']*\')([^>]*>)/ig, "$1$3");
			html = html.replace(/(<(?:table|td|tr).*?[\s]style[\s]?=[\s]?)\"([^\"]*?)width[\s]*(?:=|:)[\s]*([^;\"]+)(?:;([^\"]*)|[\s]*)\"([^>]*>)/ig, "$1\"$2$4\"$5");
			html = html.replace(/(<(?:table|td|tr).*?[\s]style[\s]?=[\s]?)\'([^\']*?)width[\s]*(?:=|:)[\s]*([^;\']+)(?:;([^\']*)|[\s]*)\"([^>]*>)/ig, "$1\"$2$4\"$5");
			html = html.replace(/(<(?:table|td|tr).*?[\s]style[\s]?=[\s]?)(.*?)width(?:=|:)([^;\s]+)(?:;([^\s]*)|[\s]?)([^>]*>)/ig, "$1\"$2$4\"$5");
		}
		
		// Remove empty styles.
		html = html.replace( /\s*style="\s*"/gi, '' ) ;
		
		// Replace space-tags
		html = html.replace( /<([^\s>]+)(\s[^>]*)?>\s+<\/\1>/g, ' ' ) ;
		
		// Remove empty tags (three times, just to be sure).
		// This also removes any empty anchor
		html = html.replace( /<([^\s>]+)(\s[^>]*)?>\s*<\/\1>/g, '' ) ;
		html = html.replace( /<([^\s>]+)(\s[^>]*)?>\s*<\/\1>/g, '' ) ;
		html = html.replace( /<([^\s>]+)(\s[^>]*)?>\s*<\/\1>/g, '' ) ;
	}
	
	return html;
}


	var cleanerDialog = function (editor) {
		return {
			title: editor.lang.cleaner.buttonLabel,
			minWidth: 420,
			minHeight: 360,
			resizable: CKEDITOR.DIALOG_RESIZE_NONE,
			buttons: [CKEDITOR.dialog.okButton, CKEDITOR.dialog.cancelButton],
			contents: [{
				id: 'main',  /* not CSS ID attribute! */
				label: '',
				elements: [
					{
						type: 'checkbox',
						id: 'word',
						label: editor.lang.cleaner.wordClearCheckbox,
						'default': 'checked',						
					},
					{
						type: 'checkbox',
						id: 'css',
						label: editor.lang.cleaner.cssClearCheckbox,
						'default': 'checked',						
					},
					{
						type: 'checkbox',
						id: 'span',
						label: editor.lang.cleaner.spanClearCheckbox,
						'default': 'checked',						
					},
					{
						type: 'checkbox',
						id: 'font',
						label: editor.lang.cleaner.fontClearCheckbox,
						'default': 'checked',						
					},
					{
						type: 'checkbox',
						id: 'table',
						label: editor.lang.cleaner.tableClearCheckbox						
					},
					{
						type: 'checkbox',
						id: 'tags',
						label: editor.lang.cleaner.tagsClearCheckbox						
					}
				]
			}],
			onOk: function(){
				var dialog = CKEDITOR.dialog.getCurrent();
				var area = null;

				if (editor.mode == 'source'){
					var html = editor.textarea.getValue();
					var proccessedHtml = CleanHtmlTags(html, 
						checkup(dialog, 'tags'),
						checkup(dialog, 'css'),
						checkup(dialog, 'font'),
						checkup(dialog, 'span'),
						checkup(dialog, 'word'),
						checkup(dialog, 'table'));				
					editor.textarea.setValue(proccessedHtml);				
				}
				else{
					var html = editor.getData();
					var proccessedHtml = CleanHtmlTags(html, 
						checkup(dialog, 'tags'),
						checkup(dialog, 'css'),
						checkup(dialog, 'font'),
						checkup(dialog, 'span'),
						checkup(dialog, 'word'),
						checkup(dialog, 'table'));				
					editor.setData(proccessedHtml);				
				}
			}	
		}		
	}

	CKEDITOR.dialog.add('cleaner', function (editor) {
		return cleanerDialog(editor);
	});

})();