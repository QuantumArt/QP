// jscs:disable disallowDanglingUnderscores

; (function() {
  'use strict';

  Quantumart.QP8.BackendVisualEditor = function(componentElem) {
    var $componentElem = $(componentElem);

    this._$containerElem = $('.visualEditorContainer', $componentElem);
    this._componentElem = $componentElem.get(0);
    this._editorElem = $('.visualEditor', $componentElem).get(0);
  };

  Quantumart.QP8.BackendVisualEditor.DATA_KEY_COMPONENT = 've_data_key';
  Quantumart.QP8.BackendVisualEditor.getComponent = function(editorElem) {
    if (editorElem) {
      return $(editorElem).data(Quantumart.QP8.BackendVisualEditor.DATA_KEY_COMPONENT);
    }
  };

  // jscs:disable requireCamelCaseOrUpperCaseIdentifiers, maximumLineLength
  function getCkEditorConfig(self, opts) {
    var defaultConfig = {
      language: 'ru',
      docType: '<!doctype html>',
      height: 340,
      enterMode: 2,
      shiftEnterMode: 1
    };

    var config = {
      language: opts.language || defaultConfig.language,
      defaultLanguage: opts.language || defaultConfig.language,
      contentsLanguage: opts.language || defaultConfig.language,
      disableNativeSpellChecker: true,
      skin: 'moonocolor',
      disallowedContent: 'script; *[on*]',
      allowedContent: {
        $1: {
          elements: CKEDITOR.dtd,
          attributes: true,
          styles: true,
          classes: true
        }
      },
      extraAllowedContent: 'replacement',
      height: opts.height || defaultConfig.height,
      resize_dir: 'both',
      resize_minWidth: 640,
      resize_maxHeight: 1024,
      bodyClass: opts.bodyClass,
      docType: opts.docType,
      fullPage: opts.fullPage,
      enterMode: opts.enterMode || defaultConfig.enterMode,
      shiftEnterMode: opts.shiftEnterMode || defaultConfig.shiftEnterMode,
      useEnglishQuotes: opts.useEnglishQuotes,
      contentsCss: opts.contentsCss,
      stylesSet: opts.stylesSet,
      toolbar: opts.toolbar,
      protectedSource: [/<a[^>]*><\/a>/g, /<i[^>]*><\/i>/g, /<b[^>]*><\/b>/g, /<span[^>]*><\/span>/g],
      extraPlugins: 'Spellchecker,Typographer,codemirror',
      removePlugins: 'save,newpage,scayt,spellchecker,forms,language,smiley,iframe,about',
      format_tags: opts.formatsSet.map(function(fs) {
        return fs.element;
      }).join(';')
    };

    opts.extraPlugins.forEach(function(pl) {
      config.extraPlugins += ',' + pl.name;
      if (pl.url) {
        CKEDITOR.plugins.addExternal(pl.name, pl.url, 'plugin.js');
      }
    });

    opts.formatsSet.forEach(function(fs) {
      config['format_' + fs.element] = fs;
    });

    config.on = {
      pluginsLoaded: function(ev) {
        ev.editor.dataProcessor.dataFilter.addRules({
          elements: {
            br: function(element) {
              if (element.name === 'br' && (!element.next || element.next._.isBlockLike)) {
                new CKEDITOR.htmlParser.text('&nbsp;').insertAfter(element);
              }
            }
          }
        });
      },
      instanceReady: function(ev) {
        ev.editor.filter.addElementCallback(function(el) {
          // disable attr <--> style transformations
          if (el.name === 'table' || el.name === 'img') {
            return CKEDITOR.FILTER_SKIP_TREE;
          }
        });

        Object.keys(CKEDITOR.tools.extend({},
          CKEDITOR.dtd.$nonBodyContent,
          CKEDITOR.dtd.$block,
          CKEDITOR.dtd.$listItem,
          CKEDITOR.dtd.$tableContent)).forEach(function(key) {
          // stop form@t$_#$*&^%... html source code
          this.dataProcessor.writer.setRules(key, {
            indent: false,
            breakBeforeOpen: true,
            breakAfterOpen: false,
            breakBeforeClose: false,
            breakAfterClose: false
          });
        }, this);

        this.dataProcessor.writer.setRules('br', {
          indent: false,
          breakBeforeOpen: false,
          breakAfterOpen: false,
          breakBeforeClose: false,
          breakAfterClose: false
        });

        this.setReadOnly($q.toBoolean($(ev.editor.element.$).data('is_readonly'), false));
        this.document.on('keyup', self._onChangeVisualEditorDataInDesignModeHandler, self);
        this.document.on('paste', self._onChangeVisualEditorDataInDesignModeHandler, self);

        self._onCKEEditorInitialized(this);
      },
      afterCommandExec: self._onChangeVisualEditorDataInDesignModeHandlerProxy,
      loadSnapshot: self._onChangeVisualEditorDataInDesignModeHandlerProxy,
      configLoaded: function(ev) {
        $.extend(ev.editor.config, {
          baseFloatZIndex: self.getZIndex()
        });
      }
    };

    return config;
  }

  // jscs:enable requireCamelCaseOrUpperCaseIdentifiers, maximumLineLength
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
    _storedTempValue: '',
    _checkIntervalID: null,

    initialize: function() {
      var self = this;
      var $editorLink;
      $(this._componentElem).data(Quantumart.QP8.BackendVisualEditor.DATA_KEY_COMPONENT, this);
      this._onChangeVisualEditorDataInDesignModeHandlerProxy =
        this._onChangeVisualEditorDataInDesignModeHandler.bind(this);

      this._$expandLink = $('.visualEditorToolbar LI.expand', this._componentElem);
      this._$collapseLink = $('.visualEditorToolbar LI.collapse', this._componentElem);
      this._$textEditorLink = $('.visualEditorToolbar LI.texteditor', this._componentElem);
      this._$visualEditorLink = $('.visualEditorToolbar LI.visualeditor', this._componentElem);
      this._fieldId = $(this._editorElem).data('field_id');
      this._siteId = $(this._editorElem).data('library_entity_id');
      this._isExpanded = $q.toBoolean($(this._editorElem).data('is_expanded'));
      this._isTextEditor = $q.toBoolean($(this._editorElem).data('is_texteditor'));

      $editorLink = this._isTextEditor ? this._$visualEditorLink : this._$expandLink;
      $editorLink.off('click').on('click', function(e) {
        if (!self._isInitialized) {
          self._isInitialized = true;
          $q.getAjax('/Backend/VisualEditorConfig/LoadVeConfig', {
            siteId: self._siteId,
            fieldId: self._fieldId
          }, function(data) {
            CKEDITOR.replace(self._editorElem.id, getCkEditorConfig(self, data));
          });
        }

        e.preventDefault();
      });

      if (this._isTextEditor) {
        this._$textEditorLink.off('click').on('click', function(e) {
        self.disposeCKEditor(false);
        self._$containerElem.show();
        self._$visualEditorLink.show();
        self._$textEditorLink.hide();
        self._$collapseLink.hide();
        self._$expandLink.hide();
        self._isInitialized = false;
        e.preventDefault();
      });

        this._$containerElem.show();
      } else if (this._isExpanded) {
        this._$expandLink.trigger('click');
      }
    },

    getCkEditor: function() {
      return CKEDITOR.instances[this._editorElem.id];
    },

    saveVisualEditorData: function() {
      var editor = this.getCkEditor();

      if (editor) {
        editor.updateElement();
      }
    },

    getZIndex: function() {
      var $window = $(this._componentElem).closest('.t-window');
      var zIndex = ($window.length > 0) ? parseInt($window.css('zIndex'), 10) : 0;

      return zIndex + 10000;
    },

    dispose: function() {
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

    disposeCKEditor: function(noUpdate) {
      var editor, $editor, windowsToDipose, listenersToRemove;

      if (this._checkIntervalID) {
        clearInterval(this._checkIntervalID);
      }

      $editor = $(this._editorElem);
      windowsToDipose = ['imageWindow', 'linkWindow', 'flashWindow'];
      listenersToRemove = ['afterCommandExec', 'loadSnapshot'];

      this._destroyVisualEditorWindow($editor, windowsToDipose[0]);
      this._destroyVisualEditorWindow($editor, windowsToDipose[1]);
      this._destroyVisualEditorWindow($editor, windowsToDipose[2]);

      editor = this.getCkEditor();
      if (editor) {
        if (editor.textarea) {
          editor.textarea.removeAllListeners();
        }

        if (editor.document) {
          editor.document.removeAllListeners();
        }

        editor.removeListener(listenersToRemove[0], this._onChangeVisualEditorDataInDesignModeHandlerProxy);
        editor.removeListener(listenersToRemove[1], this._onChangeVisualEditorDataInDesignModeHandlerProxy);
        editor.destroy(noUpdate);
      }

      $editor.removeData();
      $editor = editor = null;
    },

    _onChangeVisualEditorDataInDesignModeHandler: function() {
      var editor = this.getCkEditor();

      if (editor) {
        if (editor.textarea) {
          editor.textarea.on('keyup', this._onChangeVisualEditorDataInSourceModeHandler, this);
          editor.textarea.on('paste', this._onChangeVisualEditorDataInSourceModeHandler, this);
        }

        $('#' + editor.name).addClass(window.CHANGED_FIELD_CLASS_NAME);
      }
    },

    _onChangeVisualEditorDataInSourceModeHandler: function() {
      $('#' + this.getCkEditor().name).addClass(window.CHANGED_FIELD_CLASS_NAME);
    },

    _onCheckChangesIntervalHandler: function() {
      var $field;
      var editor = this.getCkEditor();

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

    _destroyVisualEditorWindow: function($editor, name) {
      if ($editor.data(name)) {
        $editor.data(name).dispose();
      }
    },

    _onCKEEditorInitialized: function(sender) {
      var self = this;

      if (sender) {
        this._storedTempValue = sender.getData();
      }

      this._checkIntervalID = setInterval(this._onCheckChangesIntervalHandler.bind(this), 10000);
      this._$containerElem.show();
      this._$expandLink.off('click').on('click', function(e) {
        $(this).hide();
        self._$collapseLink.show();
        self._$containerElem.show();
        e.preventDefault();
      }).hide();

      this._$collapseLink.off('click').on('click', function(e) {
        $(this).hide();
        self._$expandLink.show();
        self._$containerElem.hide();
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

  Quantumart.QP8.BackendVisualEditor.registerClass('Quantumart.QP8.BackendVisualEditor', null, window.Sys.IDisposable);
})();
