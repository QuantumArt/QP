/* global CKEDITOR */

// eslint-disable prefer-template
// eslint-disable-next-line no-extra-semi
; (function init() {
  CKEDITOR.aspellSettings = {
    pluginName: 'Spellchecker',
    pluginDesc: 'Проверка орфографии',
    /* eslint-disable prefer-template */
    spellCheckScriptPath: window.APPLICATION_ROOT_URL + 'VisualEditorConfig/AspellCheck',
    spellcheckerHtmlPath: window.APPLICATION_ROOT_URL
      + 'Scripts/Quantumart/ckeditor/aspell/spellerpages/spellchecker.html',
    spellcheckerJsPath: window.APPLICATION_ROOT_URL + 'Scripts/Quantumart/ckeditor/aspell/spellerpages/spellChecker.js',
    spellcheckerDialogJsPath: window.APPLICATION_ROOT_URL + 'Scripts/Quantumart/ckeditor/aspell/dialogs/aspell.js'
    /* eslint-enable prefer-template */
  };

  CKEDITOR.plugins.add(CKEDITOR.aspellSettings.pluginName, {
    init(editor) {
      if (editor.contextMenu) {
        editor.addMenuGroup('qp8', 10);
        editor.addMenuItem(CKEDITOR.aspellSettings.pluginName, {
          label: CKEDITOR.aspellSettings.pluginDesc,
          command: CKEDITOR.aspellSettings.pluginName,
          group: 'qp8'
        });

        editor.contextMenu.addListener(() => ({ typographer: CKEDITOR.TRISTATE_OFF }));
      }

      editor.addCommand(
        CKEDITOR.aspellSettings.pluginName,
        new CKEDITOR.dialogCommand(CKEDITOR.aspellSettings.pluginName)
      );

      editor.ui.addButton(CKEDITOR.aspellSettings.pluginName, {
        label: CKEDITOR.aspellSettings.pluginDesc,
        command: CKEDITOR.aspellSettings.pluginName
      });

      CKEDITOR.dialog.add(CKEDITOR.aspellSettings.pluginName, CKEDITOR.aspellSettings.spellcheckerDialogJsPath);
    }
  });
}());
