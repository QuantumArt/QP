; (function () {


  var interval;
  var number = CKEDITOR.tools.getNextNumber();
  var iframeId = 'cke_frame_' + number;
  var textareaId = 'cke_data_' + number;
  var spellHTML = '<textarea name="' + textareaId + '" id="' + textareaId + '" style="display: none;"></textarea>' + '<iframe src="" style="width:485px; height:380px"' + ' frameborder="0" name="' + iframeId + '" id="' + iframeId + '"' + ' allowtransparency="1"></iframe>';

  function oSpeller_OnFinished(dialog, numberOCorrections, editor) {
    if (numberOCorrections > 0) {
      editor.focus();
      editor.fire('saveSnapshot');
      dialog.getParentEditor().setData(document.getElementById(textareaId).value);
      editor.fire('saveSnapshot');
    }

    dialog.hide();
  }

  function spellTime(dialog, editor) {
    var i = 0;

    return function () {
      if (typeof window.spellChecker === 'function') {
        if (typeof interval !== 'undefined') {
          window.clearInterval(interval);
        }

        var oSpeller = new window.spellChecker(document.getElementById(textareaId));

        oSpeller.spellCheckScript = CKEDITOR.aspellSettings.spellCheckScriptPath;
        oSpeller.OnFinished = function (numChanges) {
          oSpeller_OnFinished(dialog, numChanges, editor);
        };

        oSpeller.popUpUrl = CKEDITOR.aspellSettings.spellcheckerHtmlPath;
        oSpeller.popUpName = iframeId;
        oSpeller.popUpProps = null;

        oSpeller.openChecker();
      } else if (i++ === 180) {
        alert('Плагин "Проверка синтаксиса" - не доступен');
        dialog.hide();
      }
    };
  }

  CKEDITOR.dialog.add(CKEDITOR.aspellSettings.pluginName, function (editor) {
    return {
      title: CKEDITOR.aspellSettings.pluginDesc,
      minWidth: document.all ? 510 : 485,
      minHeight: document.all ? 405 : 380,
      buttons: [CKEDITOR.dialog.cancelButton],
      resizable: CKEDITOR.DIALOG_RESIZE_NONE,
      contents: [{
        id: 'general',
        label: CKEDITOR.aspellSettings.pluginDesc,
        padding: 0,
        elements: [{
          type: 'html',
          id: 'content',
          style: 'width:485;height:380px',
          html: '<div></div>'
        }]
      }],
      onShow: function () {
        var contentArea = this.getContentElement('general', 'content').getElement();

        contentArea.setHtml(spellHTML);
        CKEDITOR.document.getHead().append(CKEDITOR.document.createElement('script', {
          attributes: {
            type: 'text/javascript',
            src: CKEDITOR.aspellSettings.spellcheckerJsPath
          }
        }));

        CKEDITOR.document.getById(textareaId).setValue(editor.getData());
        interval = window.setInterval(spellTime(this, editor), 250);
      },
      onHide: function () {
        window.ooo = undefined;
        window.framesetLoaded = undefined;
        window.int_framsetLoaded = undefined;
        window.is_window_opened = false;
      }
    };
  });
}());
