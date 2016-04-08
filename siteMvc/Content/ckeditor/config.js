CKEDITOR.editorConfig = function(config) {
  config = {
    language: 'ru',
    defaultLanguage: 'ru',
    contentsLanguage: 'ru',
    disableNativeSpellChecker: true,
    skin: 'bootstrapck',
    disallowedContent: 'script; *[on*]',
    allowedContent: {
      $1: {
        elements: CKEDITOR.dtd,
        attributes: true,
        styles: true,
        classes: true
      }
    },
    height: 340,
    resize_dir: 'both',
    resize_minWidth: 640,
    resize_maxHeight: 1024,
    fullPage: false,
    enterMode: 2,
    shiftEnterMode: 1,
    useEnglishQuotes: false
  };

  config.toolbar = [
    { name: 'document', items: ['Source', '-', 'Save', 'NewPage', 'Preview', 'Print', '-', 'Templates'] },
    { name: 'clipboard', items: ['Cut', 'Copy', 'Paste', 'PasteText', 'PasteFromWord', '-', 'Undo', 'Redo'] },
    { name: 'editing', items: ['Find', 'Replace', '-', 'SelectAll', '-', 'Spellchecker', 'Typographer', 'Tag'] },
    { name: 'forms', items: ['Form', 'Checkbox', 'Radio', 'TextField', 'Textarea', 'Select', 'Button', 'ImageButton', 'HiddenField'] },
    '/',
    { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'Strike', 'Subscript', 'Superscript', '-', 'RemoveFormat'] },
    { name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote', 'CreateDiv', '-', 'JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock', '-', 'BidiLtr', 'BidiRtl', 'Language'] },
    { name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
    { name: 'insert', items: ['Image', 'Flash', 'Table', 'HorizontalRule', 'Smiley', 'SpecialChar', 'PageBreak', 'Iframe'] },
    '/',
    { name: 'styles', items: ['Styles', 'Format', 'Font', 'FontSize'] },
    { name: 'colors', items: ['TextColor', 'BGColor'] },
    { name: 'tools', items: ['Maximize', 'ShowBlocks'] },
    { name: 'about', items: ['About'] }
  ];

  return config;
};
