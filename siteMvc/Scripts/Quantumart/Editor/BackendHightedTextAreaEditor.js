Quantumart.QP8.BackendHighlightedTextArea = function (componentElem) {
    this._componentElem = componentElem;
};


Quantumart.QP8.BackendHighlightedTextArea.prototype = {
    _selectPopupWindowComponent: null, // окно выбора файла в библиотеке
    _libraryButton: null,
    _restoreButton: null,
    _insertButton: null,
    _insertPopUp: null,
    _onLibraryButtonClickHandler: null,
    _onRestoreButtonClickHandler: null,
    _onInsertButtonClickHandler: null,
    _onInsertCallHandler: null,

    _libraryEntityId: 0,
    _libraryParentEntityId: 0,
    _templateId: null,
    _formatId: null,
    _netLanguageId: null,
    _presentationOrCodeBehind: false,

    _insertWindowInitialized: false,
    _insertWindowHtml: null,
    _insertWindowComponent: null,
    _storedTempValue: "",
    _checkIntervalID: null,

    _editorWidth: null,
    _editorHeight: null,

    _openLibrary: function () {
        var eventArgs = new Quantumart.QP8.BackendEventArgs();
        eventArgs.set_entityId(this._libraryEntityId);
        eventArgs.set_parentEntityId(this._libraryParentEntityId);
        eventArgs.set_entityName("");
        eventArgs.set_entityTypeCode(ENTITY_TYPE_CODE_SITE);
        eventArgs.set_actionCode(ACTION_CODE_POPUP_SITE_LIBRARY);
        if (!this._selectPopupWindowComponent) {
            var options = { isMultiOpen: true, additionalUrlParameters: { "filterFileTypeId": "", "allowUpload": true }};
            this._selectPopupWindowComponent = new Quantumart.QP8.BackendSelectPopupWindow(eventArgs, options);
            this._selectPopupWindowComponent.attachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED, jQuery.proxy(this._librarySelectedHandler, this));
            this._selectPopupWindowComponent.attachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED, jQuery.proxy(this._libraryClosedHandler, this));
        }
        this._selectPopupWindowComponent.openWindow();
        eventArgs = null;
        options = null;
    },

    _closeLibrary: function () {
        this._selectPopupWindowComponent.closeWindow();
    },

    _destroyLibrary: function () {
        if (this._selectPopupWindowComponent) {
            this._selectPopupWindowComponent.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
            this._selectPopupWindowComponent.detachObserver(EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
            this._selectPopupWindowComponent.dispose();
            this._selectPopupWindowComponent = null;
        }
    },

    _libraryClosedHandler: function (eventType, sender, args) {
        this._closeLibrary();
    },

    _librarySelectedHandler: function (eventType, sender, args) {
        this._closeLibrary();
        if (args) {
            var url = jQuery('#' + this._selectPopupWindowComponent._popupWindowId + '_Library').find('.l-virtual-path').text();
            var entities = args.entities;
            if (entities.length > 0) {
                url = url + entities[0].Name;
                jQuery.proxy(this._insertLibraryTag(url), this);
            }
        }
    },

    _onCheckChangesIntervalHandler: function () {
        if (!$q.isNull(this._componentElem.data('codeMirror')))
        {
            var curVal = this._componentElem.data('codeMirror').getValue();
            if (this._storedTempValue !== curVal) {
                this._storedTempValue = curVal;
                this._componentElem.addClass(CHANGED_FIELD_CLASS_NAME);
                this._componentElem.trigger(JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, { "fieldName": this._componentElem.attr("name"), "value": this._componentElem.data('codeMirror').getValue() });
            }
        }
    },

    _insertLibraryTag: function (url)
    {
        var sCurs = this._componentElem.data('codeMirror').getCursor('start');
        var eCurs = this._componentElem.data('codeMirror').getCursor('end');

        if ((eCurs.line == sCurs.line) && (eCurs.ch == sCurs.ch)) {
            this._componentElem.data('codeMirror').replaceRange(this._generateTag(url), sCurs);
        }
        else
            this._componentElem.data('codeMirror').replaceRange(this._generateTag(url), sCurs, eCurs);
        this._componentElem.addClass(CHANGED_FIELD_CLASS_NAME);
    },

    _insertCallText: function (callText)
    {
        var sCurs = this._componentElem.data('codeMirror').getCursor('start');
        var eCurs = this._componentElem.data('codeMirror').getCursor('end');

        if ((eCurs.line == sCurs.line) && (eCurs.ch == sCurs.ch)) {
        this._componentElem.data('codeMirror').replaceRange(callText, sCurs);
        }
        else
            this._componentElem.data('codeMirror').replaceRange(callText, sCurs, eCurs);
        this._insertWindowComponent.close();
    },

    _generateTag: function(url)
    {
        if (url.endsWith('.gif') || url.endsWith('.jpg') || url.endsWith('.jpeg') || url.endsWith('.png') || url.endsWith('.bmp')) {
            return $.telerik.formatString('<img src="{0}"/>', url);
        }

        else if (url.endsWith('.js')) {
            return $.telerik.formatString('<script language="JavaScript" src="{0}" type="text/javascript"></script>', url);
        }

        else if (url.endsWith('.swf')) {
            return $.telerik.formatString('<EMBED src="{0}" loop="false" menu="false" quality="high" TYPE="application/x-shockwave-flash" PLUGINSPAGE="http://www.macromedia.com/shockwave/download/index.cgi?P1_Prod_Version=ShockwaveFlash"></EMBED>',
                url);
        }

        else if (url.endsWith('.css')) {
            return $.telerik.formatString('<link rel="stylesheet" type="text/css" href="{0}">', url);
        }

        else {
            return $.telerik.formatString('<a  href="{0}">', url);
        }
    },

    initialize: function () {
        var tArea = this._componentElem;
        this._presentationOrCodeBehind = tArea.data('is_presentation') == "True";
        this._templateId = tArea.data('template_id');
        this._formatId = tArea.data('format_id');
        this._netLanguageId = tArea.data('net_language');
        this._libraryEntityId = tArea.data('site_id');
        if ($q.isNull(tArea.data('codeMirror'))) {
            this._componentElem.wrap('<div class="CodemirrorContainer">');
            var cm = CodeMirror.fromTextArea(this._componentElem.get(0), {
                lineNumbers: $q.toBoolean(tArea.data("hta_lineNumbers"), true),
                matchBrackets: $q.toBoolean(tArea.data("hta_matchBrackets"), true),
                lineWrapping: $q.toBoolean(tArea.data("hta_lineWrapping"), true),
                mode: this.getMode(tArea),
                readOnly: tArea.is('[disabled]'),
                tabMode: "indent"
            });

            this.initTemplateToolbar(cm);
            cm.setSize(this._editorWidth, this._editorHeight);

            if (!$q.isNull(tArea.data('height'))) {
                cm.setSize(null, tArea.data('height'));
            }

            if (!$q.isNull(tArea.data('width'))) {
                cm.setSize(tArea.data('width'));
            }

            this._storedTempValue = cm.getValue();
            tArea.data('codeMirror', cm);
            this._checkIntervalID = setInterval(jQuery.proxy(this._onCheckChangesIntervalHandler, this), 10000);
            cm = null;
        }
    },

    _onLibraryButtonClick: function () {
        this._openLibrary();
    },

    _onRestoreButtonClick: function () {
        $q.getJsonFromUrl('POST', CONTROLLER_URL_PAGE_TEMPLATE + (this._presentationOrCodeBehind ? "GetDefaultPresentation" : "GetDefaultCode"),
		{
		    formatId: this._formatId
		},
		true, false).done(jQuery.proxy(function (data) {
		    if (confirm(HTA_DEFAULT_CONFIRM)) {
		        this._componentElem.data('codeMirror').replaceRange(data.code, { line: 0, ch: 0 }, { line: this._componentElem.data('codeMirror').lastLine() + 1, ch: 0 });
		        this._componentElem.addClass(CHANGED_FIELD_CLASS_NAME);
		    }
		}, this));
    },

    _onInsertButtonClick: function () {
        if (!this._insertWindowInitialized) {
            this.createInsertPopupWindow();
            this._insertWindowInitialized = true;
        }
        else {
            this._insertWindowComponent.open();
        }
    },

    _onInsertCall: function () {
        if (!this._insertPopUp.data('valToInsert') == '') {
            if (this._insertPopUp.data('valType') == 'object') {
                this._insertObjectFunc(this._insertPopUp.data('valToInsert'), this._netLanguageId, !this._presentationOrCodeBehind)
            }
            else if (this._insertPopUp.data('valType') == 'function') {
                this._insertFunc(this._insertPopUp.data('valToInsert'), this._netLanguageId, !this._presentationOrCodeBehind);
            }

            else if (this._insertPopUp.data('valType') == 'field') {
                this._insertFieldFunc(this._insertPopUp.data('valToInsert'), this._netLanguageId, !this._presentationOrCodeBehind);
            }
            this._componentElem.addClass(CHANGED_FIELD_CLASS_NAME);
        }
    },

    _insertObjectFunc: function(objectName, netLanguage, isCodeBehind){
	    if (netLanguage != '')
        {
            if (isCodeBehind)
            { var strIns = (netLanguage == '2') ? 'ShowObject("'+objectName+'", Me)' : 'ShowObject("'+objectName+'", this);'; }
            else
            { var strIns = '<'+'qp:placeholder calls="'+objectName+'" runat="server"  /'+'>'; }
        }
        else
		        var strIns = '<'+'%Object("'+objectName+'")%'+'>'
	    this._insertCallText(strIns);
    },

    _insertFunc: function(fieldName, netLanguage, isCodeBehind)
    {
	    if (netLanguage == '')
            var strIns = '<'+'%=' + fieldName + '%'+'>';
        else
        {
		    if (isCodeBehind == '0')
                var strIns = '<'+'%# ' + fieldName + '%'+'>';
            else
            {
			    if (netLanguage == '1') //fieldName += ";";
                    strIns = fieldName;
            }
        }
	    this._insertCallText(strIns);
    },

    _insertFieldFunc: function(fieldName,  netLanguage, isCodeBehind)
    {
       var strIns = 'Field("' + fieldName  + '")';
        if (netLanguage == '1')
        {
            if (isCodeBehind == '0')
                strIns = 'Field(((DataRowView)(Container.DataItem)), "' + fieldName  + '")';
            else
                strIns = 'Field(Data.Rows[e.Item.ItemIndex], "' + fieldName  + '")';
        }
        if (netLanguage == '2')
        {
            if (isCodeBehind == '0')
                strIns = 'Field(CType(Container.DataItem, DataRowView), "' + fieldName  + '")';
            else
                strIns = 'Field(Data.Rows(e.Item.ItemIndex), "' + fieldName  + '")';
        }
        this._insertFunc(strIns, netLanguage, isCodeBehind);
    },

    createInsertPopupWindow: function () {
        this._insertWindowHtml = '';
        $q.getJsonFromUrl('POST', CONTROLLER_URL_PAGE_TEMPLATE + "GetInsertPopUpMarkUp",
		{
		    presentationOrCodeBehind: this._presentationOrCodeBehind,
		    formatId: this._formatId,
            templateId: this._templateId
		},
		true, false).done(jQuery.proxy(function (data) {
		    this._insertWindowHtml = data.html;
		    this._insertWindowComponent = $.telerik.window.create({
		        title: HTA_INSERT_CALL,
		        html: this._insertWindowHtml,
		        modal: true,
		        width: 700,
		        resizable: false,
		        draggable: false,
		        actions: ["Close"],
		        effects: { list: [{ name: "toggle" }, { name: "property", properties: ["opacity"] }], openDuration: "fast", closeDuration: "fast" }
		    }).data("tWindow").center();

		    this._insertPopUp = jQuery(this._insertWindowComponent.element).addClass('popupWindow');
		    this._onInsertCallHandler = jQuery.proxy(this._onInsertCall, this);
		    this._insertPopUp.find('.insertButton').parent('span').bind("click", this._onInsertCallHandler);

		    this._insertPopUp.find('select').change( jQuery.proxy (function (sender) {
		        jQuery(sender.target).closest('.row').siblings().find('select option:first-child').attr("selected", "selected");
		        this._insertPopUp.data('valToInsert', jQuery(sender.target).val());
		        this._insertPopUp.data('valType', this.computeInsertType(jQuery(sender.target)));
		    }, this));

		}, this));
    },

    computeInsertType: function(elem)
    {
        var $elem = jQuery(elem);
        if ($elem.hasClass('ht-toolbar-container-selector') || $elem.hasClass('ht-toolbar-function-selector')) {
            return 'function';
        }

        else if ($elem.hasClass('ht-toolbar-template-obj-selector') || $elem.hasClass('ht-toolbar-page-obj-selector')) {
            return 'object';
        }

        else if ($elem.hasClass('ht-toolbar-field-selector')) {
            return 'field';
        }
    },

    saveData: function () {
    	var codeMirror = this._componentElem.data('codeMirror');
    	if (codeMirror) {
    		if (this._componentElem.val() != codeMirror.getValue())
    			this._componentElem.change()
    		codeMirror.save();
        }
    },

    getMode: function () {
        var tArea = this._componentElem;
        if (tArea.hasClass("hta-htmlTextArea"))
            return "application/x-aspx";
        else if (tArea.hasClass("hta-cSharpTextArea"))
            return "text/x-csharp";
        else if (tArea.hasClass("hta-VBSTextArea"))
            return "text/vbscript";
        else if (tArea.hasClass("hta-VBTextArea"))
            return "text/x-vb";
        else if (tArea.hasClass("hta-XmlTextArea"))
        	return "application/xml";
        else if (tArea.hasClass("hta-JsTextArea"))
        	return "text/javascript";
        else if (tArea.hasClass("hta-SqlTextArea"))
            return "text/x-sql";
        else
            return null;
    },

    initTemplateToolbar: function (cm) {
        $q.getJsonFromUrl('POST', CONTROLLER_URL_PAGE_TEMPLATE + "GetHTAToolbarMarkUp",
		{
		    presentationOrCodeBehind: this._presentationOrCodeBehind,
		    formatId: this._formatId,
		    templateId: this._templateId
		},
		true, false).done(jQuery.proxy(function (data) {
		    this._componentElem.parent().prepend(data.html);
		    this._libraryButton = this._componentElem.siblings('.codeMirrorToolbar').find('.libraryButton');
		    this._restoreButton = this._componentElem.siblings('.codeMirrorToolbar').find('.restoreButton');
		    this._insertButton = this._componentElem.siblings('.codeMirrorToolbar').find('.insertButton');
		    if (this._libraryButton) {
		        this._onLibraryButtonClickHandler = jQuery.proxy(this._onLibraryButtonClick, this);
		        this._libraryButton.bind("click", this._onLibraryButtonClickHandler);
		    }

		    if (this._restoreButton) {
		        this._onRestoreButtonClickHandler = jQuery.proxy(this._onRestoreButtonClick, this);
		        this._restoreButton.bind("click", this._onRestoreButtonClickHandler);
		    }

		    if (this._insertButton) {
		        this._onInsertButtonClickHandler = jQuery.proxy(this._onInsertButtonClick, this);
		        this._insertButton.bind("click", this._onInsertButtonClickHandler);
		    }
		}, this));
    },

    destroy: function () {
        if (this._checkIntervalID) {
            clearInterval(this._checkIntervalID);
        }
        this._destroyLibrary();
        this._componentElem = null;
    }
};
