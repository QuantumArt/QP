(function() {
   CKEDITOR.plugins.addExternal('thesaurus','http://thesaurus.rosnano.dev.quantumart.ru/Scripts/CKEditor/plugins/thesaurus/', 'plugin.js');
})();

CKEDITOR.editorConfig = function (config) {
    config.skin = 'qp8';
    config.browserContextMenuOnCtrl = false;
    config.disableNativeSpellChecker = true;
    config.disableNativeTableHandles = true;
    config.resize_dir = 'both';
    config.resize_minWidth = 640;
    config.resize_minHeight = 250;
    config.resize_maxHeight = 1024;
    config.extraPlugins = 'linebreak,cleaner,qspecchar,typographer,removeEmptyP';
    config.extraPlugins += ',thesaurus';
    config.toolbar =
		[
			['Source'],
			['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', 'Cleaner', 'Typographer', '-', 'SpellChecker', 'Scayt'],
			['Undo', 'Redo', '-', 'Find', 'Replace', '-', 'SelectAll', 'RemoveFormat'],
			'/',
			['Bold', 'Italic', 'Underline', 'Strike', '-', 'Subscript', 'Superscript'],
			['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent'],
			['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'],
			['Link', 'Unlink', 'Anchor'],
			['Image', 'Flash', '-', 'Table', 'QSpecChar', 'HorizontalRule', 'LineBreak', 'PageBreak'],
			'/',
			['Styles', 'Format', 'Font', 'FontSize'],
			['TextColor', 'BGColor']
		];
    config.toolbar.push(['Thesaurus', 'ThrsButton']);
};

//config.docType = '<!DOCTYPE html PUBLIC \"-\/\/W3C\/\/DTD XHTML 1.0 Transitional\/\/EN\" \"http:\/\/www.w3.org\/TR\/xhtml1\/DTD\/xhtml1-transitional.dtd\">';
//config.enterMode = CKEDITOR.ENTER_P;
//config.shiftEnterMode = CKEDITOR.ENTER_BR;
//config.useEnglishQuotes = true;
//config.language = 'en-us';
// config.fullPage = true;
//config.height = 340;