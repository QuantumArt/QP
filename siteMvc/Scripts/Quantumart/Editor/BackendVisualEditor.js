/* global CKEDITOR */
/* eslint camelcase: ["off", { properties: "never"}] */

// eslint-disable-next-line no-extra-semi
const getCkEditorConfig = function getCkEditorConfig(obj, opts) {
  const defaultConfig = {
    language: 'ru',
    docType: '<!doctype html>',
    height: 340,
    enterMode: 2,
    shiftEnterMode: 1
  };

  const config = {
    language: opts.language || defaultConfig.language,
    defaultLanguage: opts.language || defaultConfig.language,
    contentsLanguage: opts.language || defaultConfig.language,
    disableNativeSpellChecker: true,
    skin: 'moonocolor',
    allowedContent: true,
    codemirror: {
      tabSize: 2,
      indentUnit: 2,
      smartIndent: false,
      indentWithTabs: false,
      autoCloseTags: false
    },
    fillEmptyBlocks: false,
    forcePasteAsPlainText: true,
    height: opts.height || defaultConfig.height,
    resize_dir: 'both',
    resize_minWidth: 640,
    resize_maxHeight: 1024,
    bodyClass: opts.bodyClass,
    docType: opts.docType || defaultConfig.docType,
    fullPage: opts.fullPage,
    enterMode: opts.enterMode || defaultConfig.enterMode,
    shiftEnterMode: opts.shiftEnterMode || defaultConfig.shiftEnterMode,
    useEnglishQuotes: opts.useEnglishQuotes,
    disableListAutoWrap: opts.disableListAutoWrap,
    contentsCss: opts.contentsCss,
    stylesSet: opts.stylesSet,
    toolbar: opts.toolbar,
    specialChars: window.CKEDITOR.config.specialChars.concat([
      ['&#36;', 'Доллар США'],
      ['&#8364;', 'Евро'],
      ['&#8381;', 'Российский рубль'],
      ['&#8372;', 'Украинская гривна'],
      ['Br', 'Белорусский рубль'],
      ['&#8382;', 'Грузинский лари']
    ]),
    protectedSource: [/<a[^>]*><\/a>/g, /<i[^>]*><\/i>/g, /<b[^>]*><\/b>/g, /<span[^>]*><\/span>/g],
    extraPlugins: 'Spellchecker,Typographer,codemirror',
    removePlugins: 'save,newpage,scayt,spellchecker,forms,language,smiley,iframe,about',
    format_tags: opts.formatsSet.map(fs => fs.element).join(';')
  };

  opts.extraPlugins.forEach(pl => {
    config.extraPlugins += `,${pl.name}`;
    if (pl.url) {
      window.CKEDITOR.plugins.addExternal(pl.name, pl.url, 'plugin.js');
    }
  });

  opts.formatsSet.forEach(fs => {
    config[`format_${fs.element}`] = fs;
  });

  config.listItems = Object.assign({}, window.CKEDITOR.dtd.$listItem, {
    dd: 1,
    dt: 1,
    li: 1
  });

  CKEDITOR.on('instanceCreated', () => {
    Object.keys(Object.assign({},
      config.listItems
    )).forEach(key => {
      if (config.disableListAutoWrap) {
        delete CKEDITOR.dtd.$listItem[key];
        delete CKEDITOR.dtd.$intermediate[key];
      } else {
        CKEDITOR.dtd.$listItem[key] = 1;
        window.CKEDITOR.dtd.$intermediate[key] = 1;
      }
    });
  });

  config.on = {
    instanceReady(ev) {
      ev.editor.filter.addElementCallback(el => {
        if (el.name === 'table' || el.name === 'img') {
          return window.CKEDITOR.FILTER_SKIP_TREE;
        }

        return undefined;
      });

      Object.keys(Object.assign({},
        window.CKEDITOR.dtd.$nonBodyContent,
        window.CKEDITOR.dtd.$block,
        window.CKEDITOR.dtd.$tableContent,
        config.listItems
      )).forEach(function setIndenting(key) {
        this.dataProcessor.writer.setRules(key, {
          indent: false,
          breakBeforeOpen: true,
          breakAfterOpen: false,
          breakBeforeClose: false,
          breakAfterClose: false
        });
      }, this);

      this.dataProcessor.writer.setRules('html', {
        indent: true,
        breakBeforeOpen: true,
        breakAfterOpen: true,
        breakBeforeClose: true,
        breakAfterClose: true
      });

      this.dataProcessor.writer.setRules('head', {
        indent: true,
        breakBeforeOpen: true,
        breakAfterOpen: true,
        breakBeforeClose: true,
        breakAfterClose: true
      });

      this.dataProcessor.writer.setRules('body', {
        indent: true,
        breakBeforeOpen: true,
        breakAfterOpen: true,
        breakBeforeClose: true,
        breakAfterClose: true
      });

      this.dataProcessor.writer.setRules('br', {
        indent: false,
        breakBeforeOpen: false,
        breakAfterOpen: false,
        breakBeforeClose: false,
        breakAfterClose: false
      });

      this.setReadOnly($q.toBoolean($(ev.editor.element.$).data('is_readonly'), false));
      this.document.on('keyup', obj._onChangeDataInDesignModeHandler, obj);
      this.document.on('paste', obj._onChangeDataInDesignModeHandler, obj);

      obj._onCKEEditorInitialized(this);
    },
    afterCommandExec: obj._onChangeDataInDesignModeHandlerProxy,
    loadSnapshot: obj._onChangeDataInDesignModeHandlerProxy,
    configLoaded(ev) {
      Object.assign(ev.editor.config, {
        baseFloatZIndex: obj.getZIndex()
      });
    }
  };

  return config;
};

Quantumart.QP8.BackendVisualEditor = function BackendVisualEditor(componentElem) {
  const $componentElem = $(componentElem);
  this._$containerElem = $('.visualEditorContainer', $componentElem);
  this._componentElem = $componentElem.get(0);
  this._editorElem = $('.visualEditor', $componentElem).get(0);
};

Quantumart.QP8.BackendVisualEditor.DATA_KEY_COMPONENT = 've_data_key';
Quantumart.QP8.BackendVisualEditor.getComponent = function getComponent(editorElem) {
  if (editorElem) {
    return $(editorElem).data(Quantumart.QP8.BackendVisualEditor.DATA_KEY_COMPONENT);
  }

  return undefined;
};

Quantumart.QP8.BackendVisualEditor.prototype = {
  _componentElem: null,
  _editorElem: null,
  _$containerElem: null,
  _onChangeDataInDesignModeHandlerProxy: null,
  _isInitialized: false,
  _$expandLink: null,
  _$collapseLink: null,
  _$textEditorLink: null,
  _$visualEditorLink: null,
  _siteId: null,
  _fieldId: null,
  _isExpanded: null,
  _isTextEditor: null,
  _storedTempValue: '',
  _checkIntervalID: null,

  initialize() {
    const that = this;
    $(this._componentElem).data(Quantumart.QP8.BackendVisualEditor.DATA_KEY_COMPONENT, this);
    this._onChangeDataInDesignModeHandlerProxy = this._onChangeDataInDesignModeHandler.bind(this);

    this._$expandLink = $('.visualEditorToolbar LI.expand', this._componentElem);
    this._$collapseLink = $('.visualEditorToolbar LI.collapse', this._componentElem);
    this._$textEditorLink = $('.visualEditorToolbar LI.texteditor', this._componentElem);
    this._$visualEditorLink = $('.visualEditorToolbar LI.visualeditor', this._componentElem);
    this._fieldId = $(this._editorElem).data('field_id');
    this._siteId = $(this._editorElem).data('library_entity_id');
    this._isExpanded = $q.toBoolean($(this._editorElem).data('is_expanded'));
    this._isTextEditor = $q.toBoolean($(this._editorElem).data('is_texteditor'));

    const $editorLink = this._isTextEditor ? this._$visualEditorLink : this._$expandLink;
    $editorLink.off('click').on('click', e => {
      if (!that._isInitialized) {
        that._isInitialized = true;
        $q.getAjax('/Backend/VisualEditorConfig/LoadVeConfig', {
          siteId: that._siteId,
          fieldId: that._fieldId
        }, data => {
          const instance = that.getCkEditor();
          if (instance) {
            that.disposeCKEditor(false);
          }
          window.CKEDITOR.replace(that._editorElem.id, getCkEditorConfig(that, data));
        });
      }

      e.preventDefault();
    });

    if (this._isTextEditor) {
      this._$textEditorLink.off('click').on('click', e => {
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
    } else if (this._isExpanded) {
      this._$expandLink.trigger('click');
    }
  },

  getCkEditor() {
    return window.CKEDITOR.instances[this._editorElem.id];
  },

  saveVisualEditorData() {
    const editor = this.getCkEditor();

    if (editor) {
      editor.updateElement();
    }
  },

  getZIndex() {
    const $window = $(this._componentElem).closest('.t-window');
    const zIndex = $window.length > 0 ? parseInt($window.css('zIndex'), 10) : 0;
    return zIndex + 10000;
  },

  dispose() {
    this.disposeCKEditor(false);
    this._$expandLink.off();
    this._$collapseLink.off();
    this._$textEditorLink.off();
    this._$visualEditorLink.off();

    $q.dispose.call(this, [
      '_siteId',
      '_fieldId',
      '_editorElem',
      '_componentElem',
      '_onChangeDataInDesignModeHandlerProxy',
      '_$expandLink',
      '_$collapseLink',
      '_$textEditorLink',
      '_$visualEditorLink',
      '_$containerElem'
    ]);
  },

  disposeCKEditor(noUpdate) {
    if (this._checkIntervalID) {
      clearInterval(this._checkIntervalID);
    }

    const $editor = $(this._editorElem);
    const windowsToDipose = ['imageWindow', 'linkWindow', 'flashWindow'];
    const listenersToRemove = ['afterCommandExec', 'loadSnapshot'];

    this._destroyVisualEditorWindow($editor, windowsToDipose[0]);
    this._destroyVisualEditorWindow($editor, windowsToDipose[1]);
    this._destroyVisualEditorWindow($editor, windowsToDipose[2]);

    const editor = this.getCkEditor();
    if (editor) {
      if (editor.textarea) {
        editor.textarea.removeAllListeners();
      }

      if (editor.document) {
        editor.document.removeAllListeners();
      }

      editor.removeListener(listenersToRemove[0], this._onChangeDataInDesignModeHandlerProxy);
      editor.removeListener(listenersToRemove[1], this._onChangeDataInDesignModeHandlerProxy);
      editor.destroy(noUpdate);
    }

    $editor.removeData();
  },

  _onCheckChangesIntervalHandler() {
    let $field;
    const editor = this.getCkEditor();

    if (editor) {
      if (this._storedTempValue !== editor.getData()) {
        this._storedTempValue = editor.getData();
        if (editor.element && editor.element.$) {
          $field = $(editor.element.$);
          $field.addClass(window.CHANGED_FIELD_CLASS_NAME);
          $field.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, {
            fieldName: $field.attr('name'),
            value: editor.getData()
          });
        }
      }
    }
  },

  _onChangeDataInDesignModeHandler() {
    const editor = this.getCkEditor();
    if (editor) {
      if (editor.textarea) {
        editor.textarea.off('keyup').on('keyup', this._onChangeDataInSourceModeHandler, this);
        editor.textarea.off('paste').on('paste', this._onChangeDataInSourceModeHandler, this);
      }

      $(`#${editor.name}`).addClass(window.CHANGED_FIELD_CLASS_NAME);
    }
  },

  _onChangeDataInSourceModeHandler() {
    $(`#${this.getCkEditor().name}`).addClass(window.CHANGED_FIELD_CLASS_NAME);
  },

  _destroyVisualEditorWindow($editor, dataName) {
    if ($editor.data(dataName)) {
      $editor.data(dataName).dispose();
    }
  },

  _onCKEEditorInitialized(sender) {
    const that = this;
    if (sender) {
      this._storedTempValue = sender.getData();
    }

    this._checkIntervalID = setInterval(this._onCheckChangesIntervalHandler.bind(this), 10000);
    this._$containerElem.show();
    this._$expandLink.off('click').on('click', function onClick(e) {
      $(this).hide();
      that._$collapseLink.show();
      that._$containerElem.show();
      e.preventDefault();
    }).hide();

    this._$collapseLink.off('click').on('click', function onClick(e) {
      $(this).hide();
      that._$expandLink.show();
      that._$containerElem.hide();
      e.preventDefault();
    }).show();

    if (this._isTextEditor) {
      this._$visualEditorLink.hide();
      this._$textEditorLink.show();
      this._$collapseLink.show();
      this._$expandLink.hide();
    }
  }
};

Quantumart.QP8.BackendVisualEditor.registerClass('Quantumart.QP8.BackendVisualEditor', null, Sys.IDisposable);
