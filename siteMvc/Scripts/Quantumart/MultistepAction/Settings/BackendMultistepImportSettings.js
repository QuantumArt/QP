Quantumart.QP8.MultistepActionImportSettings = function(options) {
  this.options = options;
};

Quantumart.QP8.MultistepActionImportSettings.prototype = {
  IMPORT_BUTTON: 'Import',

  _fileWrapperElement: null,
  _fileWrapperElementId: '',
  fileName: '',

  _$importAction: null,
  _$uniqueFieldToUpdate: null,
  _$uniqueContentFieldId: null,
  _$fieldGroup: null,
  _$fields: null,
  _$identifiers: null,
  _fieldsPredicate: null,
  _uniqueFieldToUpdatePredicate: null,
  _identifiersPredicate: null,

  _renameMatch: true,
  _useSiteLibrary: false,
  _uploaderType: Quantumart.QP8.Enums.UploaderType.PlUpload,
  _onFileUploadedHandler: '',
  _fileNameId: 'FileName',
  _noHeadersId: 'NoHeaders',
  options: null,

  _initFileUploader: function(context, uploadPath) {
    this._fileWrapperElementId = context._popupWindowId + '_upload_pl_cont_import';
    this._fileWrapperElement = document.getElementById(this._fileWrapperElementId);
    $(this._fileWrapperElement).closest('.documentWrapper').addClass('ImportWrapper');
    if (this._uploaderType == Quantumart.QP8.Enums.UploaderType.Silverlight) {
      this._uploaderComponent = new Quantumart.QP8.BackendSilverlightUploader(this._fileWrapperElement, {
        background: '#ffffff',
        extensions: '',
        resolveName: this._renameMatch
      });
    } else if (this._uploaderType == Quantumart.QP8.Enums.UploaderType.Html) {
      this._uploaderComponent = new Quantumart.QP8.BackendHtmlUploader(this._fileWrapperElement, { extensions: '', resolveName: this._renameMatch });
    } else if (this._uploaderType == Quantumart.QP8.Enums.UploaderType.PlUpload) {
      this._uploaderComponent = new Quantumart.QP8.BackendPlUploader(this._fileWrapperElement, { extensions: '', resolveName: this._renameMatch, useSiteLibrary: this._useSiteLibrary });
    }

    this._uploaderComponent.initialize();
    this._uploaderComponent.set_folderPath(uploadPath);
    this._uploaderComponent.attachObserver(EVENT_TYPE_LIBRARY_FILE_UPLOADED, $.proxy(this._onFileUploadedHandler, this));
  },

  _onFileUploadedHandler: function(eventType, sender, eventArgs) {
    this.fileName = eventArgs.get_fileNames()[0];

    $('#' + this.options._popupWindowId + '_' + this._fileNameId).val(this.fileName);
    $('#' + this._fileNameId).val(this.fileName);
    $('#' + this.options._popupWindowId + '_' + this._noHeadersId).prop('disabled', true);
    this._loadFileFields(this.options);
  },

  _initValidation: function() {
    var that = this;
    var id = this.options._popupWindowId;

    this._$importAction = $('#' + id + '_ImportAction');
    this._$uniqueFieldToUpdate = $('#' + id + '_UniqueFieldToUpdate');
    this._$uniqueContentFieldId = $('#' + id + '_UniqueContentFieldId');
    this._$fieldGroup = $('#' + this.options._popupWindowId + '_mapping_fields_selects');
    this._$fields = $("select[data-required='True']");
    this._$identifiers = $("select[data-identifier='True']");
    this._uniqueFieldToUpdatePredicate = function() {
      return +that._$importAction.val() > 0;
    };

    this._fieldsPredicate = function($select) {
      var identifierSelected = true;
      if ($select) {
        var $identifier = $select.closest('.import-field-group-container').find('select:first').filter("[data-identifier='True']");
        if ($identifier.length == 1) {
          identifierSelected = $identifier.val() != '-1';
        }
      }

      return identifierSelected && $.inArray(+that._$importAction.val(), [0, 1, 2]) > -1;
    };

    this._identifiersPredicate = function($select) {
      return $select.closest('.import-field-group-container').find('[data-identifier="False"]select[value!="-1"]').length > 0;
    };

    this._$uniqueFieldToUpdate.on('change', this._validateSelect(this._uniqueFieldToUpdatePredicate));
    this._$fieldGroup
      .on('change', "select[data-identifier='True']", function() {
        that._validateSelect(that._identifiersPredicate).call(this);
        var $fields = $(this).closest('.import-field-group-container').find("select[data-required='True']");

        that._showValidationSigns($fields, that._fieldsPredicate);
        $fields.each(function() {
          that._validateSelect(that._fieldsPredicate).call(this);
        });
      })
      .on('change', "select[data-required='True']", this._validateSelect(this._fieldsPredicate))
      .on('change', 'select[data-required]', function() {
        var $identifiers = that._$identifiers;

        that._showValidationSigns($identifiers, that._identifiersPredicate);
        $identifiers.each(function() {
          that._validateSelect(that._identifiersPredicate).call(this);
        });
      });

    if (this._$uniqueContentFieldId.length == 1) {
      this._$uniqueContentFieldId.on('change', function() {
        that._updateMappingOptions.call(that);
      });

      this._$uniqueFieldToUpdate.on('change', function() {
        that._updateMappingOptions.call(that);
      });
    }

    this._$importAction.on('change', function() {
      that._showValidationSigns(that._$fields, that._fieldsPredicate);
      that._$fields.trigger('change');
      that._$uniqueFieldToUpdate.trigger('change');
      that._showValidationSigns($('.star:first'), that._uniqueFieldToUpdatePredicate);
    });
  },

  _updateMappingOptions: function() {
    var that = this;

    this._$uniqueContentFieldId.removeClass('input-validation-error');
    $('.mapped').removeClass('mapped');
    $('option:hidden').show();

    if (this._$uniqueFieldToUpdate.val() != '-1' && this._$uniqueContentFieldId.val() != '') {
      $(this._$uniqueFieldToUpdate).addClass('mapped');
      $(this._$uniqueContentFieldId).addClass('mapped');

      var $fieldDescription = $('.select-block-container').filter(function() {
        return $(this).text() === that._$uniqueContentFieldId.find('option:selected').text();
      });

      $fieldDescription.addClass('mapped');
      var $select = $fieldDescription.prev().prev();
      $select.addClass('mapped');
      $select.find("option[value!='-1']option[value!='" + this._$uniqueFieldToUpdate.val() + "']").hide();

      if (this._$uniqueFieldToUpdate.val() != $select.val()) {
        $select.val('-1');
      }
    }
  },

  _validate: function() {
    this._$importAction.trigger('change');
  },

  _disposeValidation: function() {
    if (this._$fieldGroup) {
      this._$fieldGroup.off('change');
      this._$fieldGroup = null;
    }

    if (this._$identifiers) {
      this._$identifiers.off('change');
      this._$identifiers = null;
    }

    if (this._$importAction) {
      this._$importAction.off('change');
      this._$importAction = null;
    }

    if (this._$uniqueFieldToUpdate) {
      this._$uniqueFieldToUpdate.off('change');
      this._$uniqueFieldToUpdate = null;
    }

    if (this._$uniqueContentFieldId) {
      this._$uniqueContentFieldId.off('change');
      this._$uniqueContentFieldId = null;
    }

    this._$fields = null;
  },

  _validateSelect: function(predicate) {
    return function() {
      if (predicate($(this)) && +$(this).val() == -1) {
        $(this).addClass('input-validation-error');
      } else {
        $(this).removeClass('input-validation-error');
      }
    };
  },

  _showValidationSigns: function(fields, predicate) {
    fields.each(function() {
      var star = $(this);

      if (!star.hasClass('star')) {
        star = star.nextAll().filter('.star:first');
      }

      if (predicate($(this))) {
        star.show();
      } else {
        star.hide();
      }
    });
  },

  _addMessageLine: function(message) {
    if (message.length > 0) {
      return message += '\n';
    } else {
      return message;
    }
  },

  AddButtons: function(dataItems) {
    var importButton = {
      Type: TOOLBAR_ITEM_TYPE_BUTTON,
      Value: this.IMPORT_BUTTON,
      Text: $l.MultistepAction.importTitle,
      Tooltip: $l.MultistepAction.importTitle,
      AlwaysEnabled: false,
      Icon: 'action.gif'
    };

    Array.add(dataItems, importButton);
    return dataItems;
  },

  InitActions: function(object, options) {
    this._initFileUploader(object, options.UploadPath);
    this._initValidation();
  },

  Validate: function() {
    var that = this;
    var errorMessage = '';
    if (this.fileName == '') {
      errorMessage = $l.MultistepAction.NoDownloadedFile;
    }

    if (this._uniqueFieldToUpdatePredicate() > 0 && +this._$uniqueFieldToUpdate.val() == -1) {
      errorMessage = $l.MultistepAction.UniqueFieldToUpdate;
    }

    var $select = $('.select-block-container').filter(function() {
      return $(this).text() === that._$uniqueContentFieldId.find('option:selected').text();
    });

    $('select[data-identifier="True"]select[value="-1"]').each(function() {
      $(this).closest('.import-field-group-container').find("select[data-required='True']").each(function() {
        if (that._identifiersPredicate($(this))) {
          var content = $(this).parents('fieldset:first').find('legend').text();
          errorMessage = that._addMessageLine(errorMessage);
          errorMessage += $l.MultistepAction.UniqueExtensionFieldToUpdate + ' ' + content;
        }
      });
    });

    this._$fields.each(function() {
      if (that._fieldsPredicate($(this)) && +$(this).val() == -1) {
        errorMessage = that._addMessageLine(errorMessage);
        errorMessage += $(this).nextAll('.select-block-container').html() + $l.MultistepAction.RequiredField;
      }
    });

    if (($('select[data-aggregated="False"]select[data-required="True"]').length == 0 || !this._fieldsPredicate(null)) && $('select[data-aggregated="False"]select[value!="-1"]').length == 0) {
      errorMessage = that._addMessageLine(errorMessage);
      errorMessage += $l.MultistepAction.AnyFieldRequired;
    }

    return errorMessage;
  },

  _loadFileFields: function(options) {
    var documentId = '#' + options._popupWindowComponent._documentWrapperElementId + ' ';
    var delim = $(documentId + 'input[name="Delimiter"]:radio:checked').val();
    var that = this;

    $(documentId + 'input[name="Delimiter"]').on('click', function() {
      that.loadFromFile(options, this.value);
    });

    $('#' + options._popupWindowId + '_Encoding, #' + options._popupWindowId + '_LineSeparator').on('change', function() {
      that.loadFromFile(options);
    });

    if (!isNaN(delim)) {
      that.loadFromFile(options);
    }
  },

  loadFromFile: function(options) {
    var that = this;
    var prms = $('#' + options._popupWindowComponent._documentWrapperElementId + ' form input, #' + options._popupWindowComponent._documentWrapperElementId + ' form select').serialize();
    var act = new Quantumart.QP8.BackendActionExecutor.getBackendActionByCode(options._actionCode);
    var getFieldsUrl = String.format('{0}FileFields/0/{1}/{2}/', act.ControllerActionUrl, options._parentEntityId, options._contentId);
    getFieldsUrl = getFieldsUrl.replace(/^~\//, APPLICATION_ROOT_URL);
    $.ajax({
      url: getFieldsUrl,
      data: prms,
      type: 'POST',
      success: function(fieldsFromFile) {
        if (fieldsFromFile.Type === 'Error') {
          $('#' + that.options._popupWindowId + '_mapping_fields_selects').hide();
          window.alert(fieldsFromFile.Text);
        } else {
          if (fieldsFromFile) {
            that._fillOptionsFromFile(fieldsFromFile);
            that._validate();
          }
        }
      }
    });
  },

  _fillOptionsFromFile: function(fields) {
    var fieldsClass = ' .dropDownList.dataList.fields';
    var documentId = '#' + this.options._popupWindowComponent._documentWrapperElementId + ' ';
    $.each($(documentId + fieldsClass), function(index, item) {
      $(item).html('');
      $(item).append($('<option>', {
        value: -1,
        text: ''
      }));

      $.each(fields, function(i, it) {
        $(item).append($('<option>', {
          value: it,
          text: it,
          selected: $(item).next().html() == it
        }));
      });
    });

    this._$uniqueFieldToUpdate.val('CONTENT_ITEM_ID');
    this._$fieldGroup.show();
  },

  dispose: function() {
    this._disposeValidation();
    if (this._uploaderComponent) {
      this._uploaderComponent.detachObserver(EVENT_TYPE_LIBRARY_FILE_UPLOADED);
      this._uploaderComponent.dispose();
      this._uploaderComponent = null;
    }
  }
};
