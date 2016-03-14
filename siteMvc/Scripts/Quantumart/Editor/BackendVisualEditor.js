Quantumart.QP8.BackendVisualEditor = function (componentElem) {
	var $componentElem = jQuery(componentElem);
	this._$containerElem = jQuery('.visualEditorContainer', $componentElem);
	this._componentElem = $componentElem.get(0);
	this._editorElem = jQuery('.visualEditor', $componentElem).get(0);
};

Quantumart.QP8.BackendVisualEditor.DATA_KEY_COMPONENT = "ve_data_key";

Quantumart.QP8.BackendVisualEditor.getComponent = function (editorElem) {
	if (editorElem){
		var $editor = jQuery(editorElem);
		return $editor.data(Quantumart.QP8.BackendVisualEditor.DATA_KEY_COMPONENT);
	}				
};

Quantumart.QP8.BackendVisualEditor.prototype = {
	_componentElem: null,
	_editorElem: null,
	_$containerElem: null,
	_onChangeVisualEditorDataInDesignModeHandlerProxy: null,
	_isInitialized: false,
	_$expandLink: null,
	_$collapseLink: null,
	_$textEditorLink: null,
	_$visualEditorLink: null,
	_siteId: null,
	_fieldId: null,
	_isExpanded: null,
    _isTextEditor: null,

	_storedTempValue: "", // сохраненное состояние
	_checkIntervalID: null,

	initialize: function () {
	    jQuery(this._componentElem).data(Quantumart.QP8.BackendVisualEditor.DATA_KEY_COMPONENT, this);
		this._onChangeVisualEditorDataInDesignModeHandlerProxy = jQuery.proxy(this._onChangeVisualEditorDataInDesignModeHandler, this);

		this._$expandLink = jQuery(".visualEditorToolbar LI.expand", this._componentElem);
		this._$collapseLink = jQuery(".visualEditorToolbar LI.collapse", this._componentElem);
		this._$textEditorLink = jQuery(".visualEditorToolbar LI.texteditor", this._componentElem);
		this._$visualEditorLink = jQuery(".visualEditorToolbar LI.visualeditor", this._componentElem);
		this._fieldId = jQuery(this._editorElem).data('field_id');
		this._siteId = jQuery(this._editorElem).data('library_entity_id');
		this._isExpanded = $q.toBoolean(jQuery(this._editorElem).data('is_expanded'));
		this._isTextEditor = $q.toBoolean(jQuery(this._editorElem).data('is_texteditor'));

		var that = this;

		 (this._isTextEditor ? this._$visualEditorLink : this._$expandLink)
		.click(function (e) {
			if (!that._isInitialized) {
				that._isInitialized = true;
				setTimeout(function () {				   
					CKEDITOR.replace(that._editorElem.id,
					{
						customConfig: "/Backend/VisualEditorConfig/LoadVeConfig?fieldId=" + that._fieldId + "&siteId=" + that._siteId + "&_=" + new Date().getTime(),
						on:
						{
							instanceReady: function (e) {
								this.setReadOnly(
									$q.toBoolean(jQuery(e.editor.element.$).data('is_readonly'), false)
								);
								var doc = this.document;
								if (doc) {
									doc.on("keyup", that._onChangeVisualEditorDataInDesignModeHandler, that);
									doc.on("paste", that._onChangeVisualEditorDataInDesignModeHandler, that);

									doc = null;
								}

								var dtd = CKEDITOR.dtd;
								for (var extd in CKEDITOR.tools.extend({}, dtd.$nonBodyContent, dtd.$block, dtd.$listItem, dtd.$tableContent)) {
									this.dataProcessor.writer.setRules(extd, {
										indent: false,
										breakAfterOpen: false
									});
								}

								this.dataProcessor.writer.setRules('br', {
									breakAfterOpen: false
								});
								that._onCKEEditorInitialized(this);
							},

							afterCommandExec: that._onChangeVisualEditorDataInDesignModeHandlerProxy,
							loadSnapshot: that._onChangeVisualEditorDataInDesignModeHandlerProxy,
							configLoaded: function (e) {
								var $editor = jQuery(e.editor.element.$);
								jQuery.extend(e.editor.config,
                                    {
                                        baseFloatZIndex: that.getZIndex()
                                    });
								$editor = null;
							}							
						}
					});
				}, 0);
			}
			e.preventDefault();
		});
	
		 if (this._isTextEditor) {
		     this._$textEditorLink
            .click(function (e) {
                that.disposeCKEditor(false);
                that._$containerElem.show();
                that._$visualEditorLink.show();
                that._$textEditorLink.hide();
                that._$collapseLink.hide();
                that._$expandLink.hide();
                that._isInitialized = false;
                e.preventDefault();
            });
	        this._$containerElem.show();
	    }
	    else if (this._isExpanded) {
			this._$expandLink.trigger('click');
		}		
	},

	getCKEditor: function () {
		var editorId = jQuery(this._editorElem).attr("id");
		var editor = CKEDITOR.instances[editorId];
		return editor;
	},

	saveVisualEditorData: function () {
		var editor = this.getCKEditor();
		if (editor) {
			editor.updateElement();
			editor = null;
		}
	},

	getZIndex: function() {
	    var $window = jQuery(this._componentElem).closest(".t-window");
	    var zIndex = ($window.length > 0) ? parseInt($window.css("zIndex"), 10) : 0;
	    return zIndex + 10000;
	},

	dispose: function () {
	    this.disposeCKEditor(false);

		this._onChangeVisualEditorDataInDesignModeHandlerProxy = null;
		this._editorElem = null;

		this._componentElem = null;

		this._$expandLink.off();
		this._$expandLink = null;
		this._$collapseLink.off();
		this._$collapseLink = null;

		this._$textEditorLink.off();
		this._$textEditorLink = null;
		this._$visualEditorLink.off();
		this._$visualEditorLink = null;

		this._$containerElem = null;
		this._siteId = null;
		this._fieldId = null;
	},

	disposeCKEditor: function (noUpdate) {
	    if (this._checkIntervalID) {
	        clearInterval(this._checkIntervalID);
	    }

	    var $editor = jQuery(this._editorElem);

	    this._destroyVisualEditorWindow($editor, "imageWindow");
	    this._destroyVisualEditorWindow($editor, "linkWindow");
	    this._destroyVisualEditorWindow($editor, "flashWindow");

	    var editor = this.getCKEditor();
	    if (editor) {
	        var textarea = editor.textarea;
	        if (textarea) {
	            textarea.removeAllListeners();
	            textarea = null;
	        }

	        var doc = editor.document;
	        if (doc) {
	            doc.removeAllListeners();
	            doc = null;
	        }

	        editor.removeListener("afterCommandExec", this._onChangeVisualEditorDataInDesignModeHandlerProxy);
	        editor.removeListener("loadSnapshot", this._onChangeVisualEditorDataInDesignModeHandlerProxy);
	        editor.destroy(noUpdate);

	        editor = null;
	    }

	    $editor.removeData();
	    $editor = null;
	},


	_onChangeVisualEditorDataInDesignModeHandler: function (e) {
		var editor = this.getCKEditor();
		if (editor) {
			var textarea = editor.textarea;
			if (textarea) {				
				if (!textarea.hasListeners("keyup")) {
					textarea.on("keyup", this._onChangeVisualEditorDataInSourceModeHandler, this);
				}
				if (!textarea.hasListeners("paste")) {
					textarea.on("paste", this._onChangeVisualEditorDataInSourceModeHandler, this);
				}
				textarea = null;
			}
			jQuery("#" + editor.name).addClass(CHANGED_FIELD_CLASS_NAME);
			editor = null;
		}
	},

	_onChangeVisualEditorDataInSourceModeHandler: function (e) {
		var editor = this.getCKEditor();
		if (editor) {			
			jQuery("#" + editor.name).addClass(CHANGED_FIELD_CLASS_NAME);
			editor = null;
		}
	},

	_onCheckChangesIntervalHandler: function () {
		var editor = this.getCKEditor();
		if (editor) {
			if (this._storedTempValue !== editor.getData()) {
				this._storedTempValue = editor.getData();
				if (editor.element && editor.element.$) {
					var $field = jQuery(editor.element.$);					
					$field.addClass(CHANGED_FIELD_CLASS_NAME);
					$field.trigger(JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, { "fieldName": $field.attr("name"), "value": editor.getData() });
					$field = null;
				}
			}
		}
		editor = null;
	},

	_destroyVisualEditorWindow: function ($editor, name) {
		if ($editor.data(name)) {
			$editor.data(name).dispose();
		}
	},

	_onCKEEditorInitialized: function (sender) {
		var that = this;
		
		if (sender) {
			this._storedTempValue = sender.getData();
		};
		this._checkIntervalID = setInterval(jQuery.proxy(this._onCheckChangesIntervalHandler, this), 10000);
		
		this._$containerElem.show();
		
		this._$expandLink
			.off("click")
			.click(function (e) {
				jQuery(this).hide();
				that._$collapseLink.show();
				that._$containerElem.show();
				e.preventDefault();
			})
			.hide();

		this._$collapseLink
			.click(function (e) {
				jQuery(this).hide();
				that._$expandLink.show();
				that._$containerElem.hide();
				e.preventDefault();
			})
			.show();

		if (this._isTextEditor) {
		    that._$visualEditorLink.hide();
		    that._$textEditorLink.show();
		    that._$collapseLink.show();
		    that._$expandLink.hide();
		}
	}
};

Quantumart.QP8.BackendVisualEditor.registerClass("Quantumart.QP8.BackendVisualEditor", null, Sys.IDisposable);

