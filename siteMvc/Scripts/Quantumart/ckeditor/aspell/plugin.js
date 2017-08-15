/* global CKEDITOR */

// eslint-disable-next-line no-extra-semi
; (function init() {
  CKEDITOR.aspellSettings = {
    pluginName: 'Spellchecker',
    pluginDesc: 'Проверка орфографии',
    spellCheckScriptPath: '/Backend/VisualEditorConfig/AspellCheck',
    spellcheckerHtmlPath: '/Backend/Scripts/Quantumart/ckeditor/aspell/spellerpages/spellchecker.html',
    spellcheckerJsPath: '/Backend/Scripts/Quantumart/ckeditor/aspell/spellerpages/spellChecker.js',
    spellcheckerDialogJsPath: '/Backend/Scripts/Quantumart/ckeditor/aspell/dialogs/aspell.js'
  };

  CKEDITOR.plugins.add(CKEDITOR.aspellSettings.pluginName, {
    init (editor) {
      if (editor.contextMenu) {
        editor.addMenuGroup('qp8', 10);
        editor.addMenuItem(CKEDITOR.aspellSettings.pluginName, {
          label: CKEDITOR.aspellSettings.pluginDesc,
          command: CKEDITOR.aspellSettings.pluginName,
          group: 'qp8'
        });

        editor.contextMenu.addListener(() => {
          return { typographer: CKEDITOR.TRISTATE_OFF };
        });
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
