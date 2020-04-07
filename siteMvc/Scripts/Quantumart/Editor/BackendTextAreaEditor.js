import { BackendEventArgs } from '../Common/BackendEventArgs';
import { BackendSelectPopupWindow } from '../List/BackendSelectPopupWindow';
import { BackendBrowserHistoryManager } from '../Managers/BackendBrowserHistoryManager';
import { $q } from '../Utils';

/* global CodeMirror, JSONEditor */

export class BackendHighlightedTextArea {
  _backendBrowserHistoryManager = BackendBrowserHistoryManager.getInstance();

  constructor(componentElem) {
    this._componentElem = componentElem;
  }

  _selectPopupWindowComponent = null;
  _libraryButton = null;
  _restoreButton = null;
  _insertButton = null;
  _insertPopUp = null;
  _onLibraryButtonClickHandler = null;
  _onRestoreButtonClickHandler = null;
  _onInsertButtonClickHandler = null;
  _onInsertCallHandler = null;

  _libraryEntityId = 0;
  _libraryParentEntityId = 0;
  _templateId = null;
  _formatId = null;
  _netLanguageId = null;
  _presentationOrCodeBehind = false;

  _insertWindowInitialized = false;
  _insertWindowHtml = null;
  _insertWindowComponent = null;
  _storedTempValue = '';
  _checkIntervalID = null;

  _editorWidth = null;
  _editorHeight = null;

  _minJsonEditorHeight = 190;

  _openLibrary() {
    let eventArgs = new BackendEventArgs();
    let options = { isMultiOpen: true, additionalUrlParameters: { filterFileTypeId: '', allowUpload: true } };
    eventArgs.set_entityId(this._libraryEntityId);
    eventArgs.set_parentEntityId(this._libraryParentEntityId);
    eventArgs.set_entityName('');
    eventArgs.set_entityTypeCode(window.ENTITY_TYPE_CODE_SITE);
    eventArgs.set_actionCode(window.ACTION_CODE_POPUP_SITE_LIBRARY);
    if (!this._selectPopupWindowComponent) {
      this._selectPopupWindowComponent = new BackendSelectPopupWindow(eventArgs, options);
      this._selectPopupWindowComponent.attachObserver(
        window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED,
        $.proxy(this._librarySelectedHandler, this)
      );

      this._selectPopupWindowComponent.attachObserver(
        window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED,
        $.proxy(this._libraryClosedHandler, this)
      );
    }

    this._selectPopupWindowComponent.openWindow();
    eventArgs = null;
    options = null;
  }

  _closeLibrary() {
    this._selectPopupWindowComponent.closeWindow();
  }

  _destroyLibrary() {
    if (this._selectPopupWindowComponent) {
      this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
      this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
      this._selectPopupWindowComponent.dispose();
      this._selectPopupWindowComponent = null;
    }
  }

  _libraryClosedHandler() {
    this._closeLibrary();
  }

  _librarySelectedHandler(eventType, sender, args) {
    let url, entities;
    this._closeLibrary();
    if (args) {
      ({ entities } = args);
      if (entities.length > 0) {
        url = $(`#${this._selectPopupWindowComponent._popupWindowId}_Library`).find('.l-virtual-path').text();
        url += entities[0].Name;
        this._insertLibraryTag(url);
      }
    }
  }

  _onCheckChangesIntervalHandler() {
    let curVal;
    if (this._componentElem.data('codeMirror')) {
      curVal = this._componentElem.data('codeMirror').getValue();
      if (this._storedTempValue !== curVal) {
        this._storedTempValue = curVal;
        this._componentElem.addClass(window.CHANGED_FIELD_CLASS_NAME);
        this._componentElem.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, {
          fieldName: this._componentElem.attr('name'),
          value: this._componentElem.data('codeMirror').getValue(),
          contentFieldName: this._componentElem.closest('dl').data('field_name')
        });
      }
    } else if (this._componentElem.data('jsonEditor')) {
      curVal = this._componentElem.data('jsonEditor').getText();
      if (this._storedTempValue !== curVal) {
        this._storedTempValue = curVal;
        this._componentElem.addClass(window.CHANGED_FIELD_CLASS_NAME);
        this._componentElem.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, {
          fieldName: this._componentElem.attr('name'),
          value: this._componentElem.data('jsonEditor').getText(),
          contentFieldName: this._componentElem.closest('dl').data('field_name')
        });
      }
    }
  }

  _insertLibraryTag(url) {
    const sCurs = this._componentElem.data('codeMirror').getCursor('start');
    const eCurs = this._componentElem.data('codeMirror').getCursor('end');
    if ((eCurs.line === sCurs.line) && (eCurs.ch === sCurs.ch)) {
      this._componentElem.data('codeMirror').replaceRange(this._generateTag(url), sCurs);
    } else {
      this._componentElem.data('codeMirror').replaceRange(this._generateTag(url), sCurs, eCurs);
    }

    this._componentElem.addClass(window.CHANGED_FIELD_CLASS_NAME);
  }

  _insertCallText(callText) {
    const sCurs = this._componentElem.data('codeMirror').getCursor('start');
    const eCurs = this._componentElem.data('codeMirror').getCursor('end');
    if ((eCurs.line === sCurs.line) && (eCurs.ch === sCurs.ch)) {
      this._componentElem.data('codeMirror').replaceRange(callText, sCurs);
    } else {
      this._componentElem.data('codeMirror').replaceRange(callText, sCurs, eCurs);
    }

    this._insertWindowComponent.close();
  }

  _generateTag(url) {
    if (url.endsWith('.gif')
      || url.endsWith('.jpg')
      || url.endsWith('.jpeg')
      || url.endsWith('.png')
      || url.endsWith('.bmp')
      || url.endsWith('.svg')) {
      return $.telerik.formatString('<img src="{0}"/>', url);
    } else if (url.endsWith('.js')) {
      return $.telerik.formatString('<script language="JavaScript" src="{0}" type="text/javascript"></script>', url);
    } else if (url.endsWith('.swf')) {
      // eslint-disable-next-line max-len
      return $.telerik.formatString('<EMBED src="{0}" loop="false" menu="false" quality="high" TYPE="application/x-shockwave-flash" PLUGINSPAGE="http://www.macromedia.com/shockwave/download/index.cgi?P1_Prod_Version=ShockwaveFlash"></EMBED>', url);
    } else if (url.endsWith('.css')) {
      return $.telerik.formatString('<link rel="stylesheet" type="text/css" href="{0}">', url);
    }

    return $.telerik.formatString('<a  href="{0}">', url);
  }

  initialize() {
    const tArea = this._componentElem;
    this._presentationOrCodeBehind = tArea.data('is_presentation') === 'True';
    this._templateId = tArea.data('template_id');
    this._formatId = tArea.data('format_id');
    this._netLanguageId = String(tArea.data('net_language'));
    this._libraryEntityId = tArea.data('site_id');
    if (tArea.hasClass('hta-JsonTextArea')) {
      if ($q.isNull(tArea.data('jsonEditor'))) {
        this.initJsonEditor(tArea);
      }
    } else if ($q.isNull(tArea.data('codeMirror'))) {
      this.initCodeMirrorTArea(tArea);
    }

    this._checkIntervalID = setInterval($.proxy(this._onCheckChangesIntervalHandler, this), 10000);
  }

  initCodeMirrorTArea(tArea) {
    let cm;
    tArea.wrap('<div class="CodemirrorContainer">');
    cm = CodeMirror.fromTextArea(tArea.get(0), {
      lineNumbers: $q.toBoolean(tArea.data('hta_lineNumbers'), true),
      matchBrackets: $q.toBoolean(tArea.data('hta_matchBrackets'), true),
      lineWrapping: $q.toBoolean(tArea.data('hta_lineWrapping'), true),
      mode: this.getMode(),
      readOnly: tArea.is('[readOnly]'),
      tabMode: 'indent'
    });

    this.initTemplateToolbar();
    cm.setSize(this._editorWidth, this._editorHeight);

    if (!$q.isNull(tArea.data('height'))) {
      cm.setSize(null, tArea.data('height'));
    }

    if (!$q.isNull(tArea.data('width'))) {
      cm.setSize(tArea.data('width'));
    }

    this._storedTempValue = cm.getValue();
    this._storedTempValue = 'qweqwe';
    tArea.data('codeMirror', cm);
    cm = null;
  }

  initJsonEditor(tArea) {
    let json;
    tArea.hide();
    tArea.wrap('<div id="jsonEditor">');

    const options = {
      mode: 'code',
      modes: ['text', 'code', 'tree'],
      onError() {
        $q.alertError($l.TextArea.forbiddenJsonMode);
      }
    };

    if (tArea.is('[readOnly]')) {
      options.onEditable = function (node) {
        if (!node.path) {
          return false;
        }
        return undefined;
      };
    }

    const height = parseInt(tArea.css('height'), 10);
    this._editorHeight = !height || height < this._minJsonEditorHeight ? this._minJsonEditorHeight : height;

    tArea.parent().css('height', this._editorHeight);

    const je = new JSONEditor(tArea.parent().get(0), options);
    json = '';
    if (tArea.val()) {
      json = tArea.val();
      je.setText(json);
    }

    this._storedTempValue = je.getText();
    tArea.data('jsonEditor', je);
  }

  _onLibraryButtonClick() {
    this._openLibrary();
  }

  _onRestoreButtonClick() {
    const actionName = this._presentationOrCodeBehind ? 'GetDefaultPresentation' : 'GetDefaultCode';
    $q.getJsonFromUrl('POST', window.CONTROLLER_URL_PAGE_TEMPLATE + actionName, {
      formatId: this._formatId
    }, true, false).done($.proxy(function ajaxDone(data) {
      if ($q.confirmMessage(window.HTA_DEFAULT_CONFIRM)) {
        this._componentElem.data('codeMirror').replaceRange(data.code,
          {
            line: 0,
            ch: 0
          },
          {
            line: this._componentElem.data('codeMirror').lastLine() + 1,
            ch: 0
          });

        this._componentElem.addClass(window.CHANGED_FIELD_CLASS_NAME);
      }
    }, this));
  }

  _onInsertButtonClick() {
    if (this._insertWindowInitialized) {
      this._insertWindowComponent.open();
    } else {
      this.createInsertPopupWindow();
      this._insertWindowInitialized = true;
    }
  }

  _onInsertCall() {
    const valToInsert = this._insertPopUp.data('valToInsert');

    if (valToInsert !== undefined && valToInsert !== null && valToInsert !== '') {
      const valType = this._insertPopUp.data('valType');

      if (valType === 'object') {
        this._insertObjectFunc(
          valToInsert,
          this._netLanguageId,
          !this._presentationOrCodeBehind
        );
      } else if (valType === 'function') {
        this._insertFunc(
          valToInsert,
          this._netLanguageId,
          !this._presentationOrCodeBehind
        );
      } else if (valType === 'field') {
        this._insertFieldFunc(
          valToInsert,
          this._netLanguageId,
          !this._presentationOrCodeBehind
        );
      }

      this._componentElem.addClass(window.CHANGED_FIELD_CLASS_NAME);
    }
  }

  _insertObjectFunc(objectName, netLanguage, isCodeBehind) {
    let strIns;
    if (netLanguage === '') {
      strIns = `<%Object("${objectName}")%>`;
    } else if (isCodeBehind) {
      strIns = netLanguage === '2'
        ? `ShowObject("${objectName}", Me)`
        : `ShowObject("${objectName}", this);`;
    } else {
      strIns = `<qp:placeholder calls="${objectName}" runat="server"  />`;
    }

    this._insertCallText(strIns);
  }

  _insertFunc(fieldName, netLanguage, isCodeBehind) {
    let strIns;
    if (netLanguage === '') {
      strIns = `<%=${fieldName}%>`;
    } else if (isCodeBehind === '0') {
      strIns = `<%# ${fieldName}%>`;
    } else if (netLanguage === '1') {
      strIns = fieldName;
    }

    this._insertCallText(strIns);
  }

  _insertFieldFunc(fieldName, netLanguage, isCodeBehind) {
    let strIns = `Field("${fieldName}")`;
    if (netLanguage === '1') {
      if (isCodeBehind === '0') {
        strIns = `Field(((DataRowView)(Container.DataItem)), "${fieldName}")`;
      } else {
        strIns = `Field(Data.Rows[e.Item.ItemIndex], "${fieldName}")`;
      }
    }
    if (netLanguage === '2') {
      if (isCodeBehind === '0') {
        strIns = `Field(CType(Container.DataItem, DataRowView), "${fieldName}")`;
      } else {
        strIns = `Field(Data.Rows(e.Item.ItemIndex), "${fieldName}")`;
      }
    }
    this._insertFunc(strIns, netLanguage, isCodeBehind);
  }

  createInsertPopupWindow() {
    this._insertWindowHtml = '';
    $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_PAGE_TEMPLATE}GetInsertPopUpMarkUp`, {
      presentationOrCodeBehind: this._presentationOrCodeBehind,
      formatId: this._formatId,
      templateId: this._templateId
    }, true, false).done($.proxy(function ajaxDone(data) {
      this._insertWindowHtml = data.html;
      this._insertWindowComponent = $.telerik.window.create({
        title: window.HTA_INSERT_CALL,
        html: this._insertWindowHtml,
        modal: true,
        width: 700,
        resizable: false,
        draggable: false,
        actions: ['Close'],
        effects: {
          list: [{ name: 'toggle' }, {
            name: 'property', properties: ['opacity']
          }],
          openDuration: 'fast',
          closeDuration: 'fast'
        },
        onOpen: this._backendBrowserHistoryManager.handleModalWindowOpen,
        onClose: this._backendBrowserHistoryManager.handleModalWindowClose
      }).data('tWindow').center();

      this._insertPopUp = $(this._insertWindowComponent.element).addClass('popupWindow');
      this._onInsertCallHandler = $.proxy(this._onInsertCall, this);
      this._insertPopUp.find('.insertButton').parent('span').bind('click', this._onInsertCallHandler);
      this._insertPopUp.find('select').change($.proxy(function onChange(sender) {
        $(sender.target)
          .closest('.row')
          .siblings()
          .find('select option:first-child')
          .attr('selected', 'selected');

        this._insertPopUp.data('valToInsert', $(sender.target).val());
        this._insertPopUp.data('valType', this.computeInsertType($(sender.target)));
      }, this));
    }, this));
  }

  computeInsertType(elem) {
    const $elem = $(elem);
    if ($elem.hasClass('ht-toolbar-container-selector') || $elem.hasClass('ht-toolbar-function-selector')) {
      return 'function';
    } else if ($elem.hasClass('ht-toolbar-template-obj-selector') || $elem.hasClass('ht-toolbar-page-obj-selector')) {
      return 'object';
    } else if ($elem.hasClass('ht-toolbar-field-selector')) {
      return 'field';
    }

    return undefined;
  }

  saveData() {
    let jsonEditor;
    const codeMirror = this._componentElem.data('codeMirror');
    if (codeMirror) {
      if (this._componentElem.val() !== codeMirror.getValue()) {
        this._componentElem.change();
      }

      codeMirror.save();
    } else {
      jsonEditor = this._componentElem.data('jsonEditor');
      if (jsonEditor) {
        if (this._componentElem.val() !== jsonEditor.getText()) {
          this._componentElem.val(jsonEditor.getText());
          this._componentElem.change();
        }
      }
    }
  }

  getMode() {
    const tArea = this._componentElem;
    if (tArea.hasClass('hta-htmlTextArea')) {
      return 'application/x-aspx';
    } else if (tArea.hasClass('hta-cSharpTextArea')) {
      return 'text/x-csharp';
    } else if (tArea.hasClass('hta-VBSTextArea')) {
      return 'text/vbscript';
    } else if (tArea.hasClass('hta-VBTextArea')) {
      return 'text/x-vb';
    } else if (tArea.hasClass('hta-XmlTextArea')) {
      return 'application/xml';
    } else if (tArea.hasClass('hta-JsTextArea')) {
      return 'text/javascript';
    } else if (tArea.hasClass('hta-SqlTextArea')) {
      return 'text/x-sql';
    }

    return null;
  }

  initTemplateToolbar() {
    $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_PAGE_TEMPLATE}GetHtaToolbarMarkUp`, {
      presentationOrCodeBehind: this._presentationOrCodeBehind,
      formatId: this._formatId,
      templateId: this._templateId
    }, true, false).done($.proxy(function ajaxDone(data) {
      this._componentElem.parent().prepend(data.html);
      this._libraryButton = this._componentElem.siblings('.codeMirrorToolbar').find('.libraryButton');
      this._restoreButton = this._componentElem.siblings('.codeMirrorToolbar').find('.restoreButton');
      this._insertButton = this._componentElem.siblings('.codeMirrorToolbar').find('.insertButton');
      if (this._libraryButton) {
        this._onLibraryButtonClickHandler = $.proxy(this._onLibraryButtonClick, this);
        this._libraryButton.bind('click', this._onLibraryButtonClickHandler);
      }

      if (this._restoreButton) {
        this._onRestoreButtonClickHandler = $.proxy(this._onRestoreButtonClick, this);
        this._restoreButton.bind('click', this._onRestoreButtonClickHandler);
      }

      if (this._insertButton) {
        this._onInsertButtonClickHandler = $.proxy(this._onInsertButtonClick, this);
        this._insertButton.bind('click', this._onInsertButtonClickHandler);
      }
    }, this));
  }

  destroy() {
    if (this._checkIntervalID) {
      clearInterval(this._checkIntervalID);
    }

    this._destroyLibrary();
    this._componentElem = null;
  }
}


Quantumart.QP8.BackendHighlightedTextArea = BackendHighlightedTextArea;
