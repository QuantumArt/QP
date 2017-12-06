Quantumart.QP8.MultistepActionImportSettings = function (options) {
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
  _fileNameId: 'FileName',
  _noHeadersId: 'NoHeaders',

  _initFileUploader(context, uploadPath) {
    this._fileWrapperElementId = `${context._popupWindowId}_upload_pl_cont_import`;
    this._fileWrapperElement = document.getElementById(this._fileWrapperElementId);
    $(this._fileWrapperElement).closest('.documentWrapper').addClass('ImportWrapper');
    if (this._uploaderType === Quantumart.QP8.Enums.UploaderType.Html) {
      this._uploaderComponent = new Quantumart.QP8.BackendHtmlUploader(this._fileWrapperElement, {
        extensions: '',
        resolveName: this._renameMatch
      });
    } else {
      this._uploaderComponent = new Quantumart.QP8.BackendPlUploader(this._fileWrapperElement, {
        extensions: '',
        resolveName: this._renameMatch,
        useSiteLibrary: this._useSiteLibrary
      });
    }

    this._uploaderComponent.initialize();
    this._uploaderComponent.setFolderPath(uploadPath);
    this._uploaderComponent.attachObserver(
      window.EVENT_TYPE_LIBRARY_FILE_UPLOADED,
      $.proxy(this._onFileUploadedHandler, this
      ));
  },

  _onFileUploadedHandler(eventType, sender, eventArgs) {
    [this.fileName] = eventArgs.getFileNames();

    $(`#${this.options.popupWindowId}_${this._fileNameId}`).val(this.fileName);
    $(`#${this._fileNameId}`).val(this.fileName);
    $(`#${this.options.popupWindowId}_${this._noHeadersId}`).prop('disabled', true);

    this._loadFileFields();
  },

  _initValidation() {
    const that = this;
    const id = this.options.popupWindowId;

    this._$importAction = $(`#${id}_ImportAction`);
    this._$uniqueFieldToUpdate = $(`#${id}_UniqueFieldToUpdate`);
    this._$uniqueContentFieldId = $(`#${id}_UniqueContentFieldId`);
    this._$fieldGroup = $(`#${id}_mapping_fields_selects`);
    this._$fields = $("select[data-required='True']");
    this._$identifiers = $("select[data-identifier='True']");
    this._uniqueFieldToUpdatePredicate = function _uniqueFieldToUpdatePredicate() {
      return +that._$importAction.val() > 0;
    };

    this._fieldsPredicate = function _fieldsPredicate($select) {
      let $identifier;
      let identifierSelected = true;
      if ($select) {
        $identifier = $select
          .closest('.import-field-group-container')
          .find('select:first')
          .filter("[data-identifier='True']");

        if ($identifier.length === 1) {
          identifierSelected = $identifier.val() !== '-1';
        }
      }

      return identifierSelected && $.inArray(+that._$importAction.val(), [0, 1, 2]) > -1;
    };

    this._identifiersPredicate = function _identifiersPredicate($select) {
      return $select
        .closest('.import-field-group-container')
        .find('[data-identifier="False"]select[value!="-1"]')
        .length > 0;
    };

    this._$uniqueFieldToUpdate.on('change', this._validateSelect(this._uniqueFieldToUpdatePredicate));
    this._$fieldGroup.on('change', "select[data-identifier='True']", function onChange() {
      that._validateSelect(that._identifiersPredicate).call(this);
      const $fields = $(this).closest('.import-field-group-container').find("select[data-required='True']");

      that._showValidationSigns($fields, that._fieldsPredicate);
      $fields.each(function each() {
        that._validateSelect(that._fieldsPredicate).call(this);
      });
    });

    this._$fieldGroup.on('change', "select[data-required='True']", this._validateSelect(this._fieldsPredicate));
    this._$fieldGroup.on('change', 'select[data-required]', () => {
      const $identifiers = that._$identifiers;
      that._showValidationSigns($identifiers, that._identifiersPredicate);
      $identifiers.each(function each() {
        that._validateSelect(that._identifiersPredicate).call(this);
      });
    });

    if (this._$uniqueContentFieldId.length === 1) {
      this._$uniqueContentFieldId.on('change', () => {
        that._updateMappingOptions();
      });

      this._$uniqueFieldToUpdate.on('change', () => {
        that._updateMappingOptions();
      });
    }

    this._$importAction.on('change', () => {
      that._showValidationSigns(that._$fields, that._fieldsPredicate);
      that._$fields.trigger('change');
      that._$uniqueFieldToUpdate.trigger('change');
      that._showValidationSigns($('.star:first'), that._uniqueFieldToUpdatePredicate);
      that._toggleFieldsRequiring(that._$fields, that._fieldsPredicate);
    });
  },

  _updateMappingOptions() {
    const that = this;
    let $select, $fieldDescription;

    this._$uniqueContentFieldId.removeClass('input-validation-error');
    $('.mapped').removeClass('mapped');
    $('option:hidden').show();

    if (this._$uniqueFieldToUpdate.val() !== '-1' && this._$uniqueContentFieldId.val() !== '') {
      $(this._$uniqueFieldToUpdate).addClass('mapped');
      $(this._$uniqueContentFieldId).addClass('mapped');

      $fieldDescription = $('.select-block-container').filter(function filter() {
        return $(this).text() === that._$uniqueContentFieldId.find('option:selected').text();
      });

      $fieldDescription.addClass('mapped');
      $select = $fieldDescription.prev().prev();
      $select.addClass('mapped');
      $select.find(`option[value!='-1']option[value!='${this._$uniqueFieldToUpdate.val()}']`).hide();

      if (this._$uniqueFieldToUpdate.val() !== $select.val()) {
        $select.val('-1');
      }
    }
  },

  _validate() {
    this._$importAction.trigger('change');
  },

  _disposeValidation() {
    if (this._$fieldGroup) {
      this._$fieldGroup.off('change');
    }

    if (this._$identifiers) {
      this._$identifiers.off('change');
    }

    if (this._$importAction) {
      this._$importAction.off('change');
    }

    if (this._$uniqueFieldToUpdate) {
      this._$uniqueFieldToUpdate.off('change');
    }

    if (this._$uniqueContentFieldId) {
      this._$uniqueContentFieldId.off('change');
    }
  },

  _validateSelect(predicate) {
    return function validateSelect() {
      if (predicate($(this)) && +$(this).val() === -1) {
        $(this).addClass('input-validation-error');
      } else {
        $(this).removeClass('input-validation-error');
      }
    };
  },

  _showValidationSigns(fields, predicate) {
    fields.each(function each() {
      let star = $(this);
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

  _toggleFieldsRequiring(fields, predicate) {
    fields.each(function each() {
      if (predicate($(this))) {
        this.removeAttribute('data-excludeValid');
      } else {
        this.setAttribute('data-excludeValid', 'True');
      }
    });
  },

  _addMessageLine(message) {
    let result = message;
    if (message.length > 0) {
      result = `${message}\n`;
    }

    return result;
  },

  initActions(object, options) {
    this._initFileUploader(object, options.UploadPath);
    this._initValidation();
  },

  validate() {
    let content;
    const that = this;
    let errorMessage = '';
    if (this.fileName === '') {
      errorMessage = $l.MultistepAction.NoDownloadedFile;
    }

    if (this._uniqueFieldToUpdatePredicate() > 0 && +this._$uniqueFieldToUpdate.val() === -1) {
      errorMessage = $l.MultistepAction.UniqueFieldToUpdate;
    }

    $('select[data-identifier="True"]select[value="-1"]').each(function eachSelect() {
      $(this).closest('.import-field-group-container').find("select[data-required='True']").each(function each() {
        if (that._identifiersPredicate($(this))) {
          content = $(this).parents('fieldset:first').find('legend').text();
          errorMessage = that._addMessageLine(errorMessage);
          errorMessage += `${$l.MultistepAction.UniqueExtensionFieldToUpdate} ${content}`;
        }
      });
    });

    this._$fields.each(function each() {
      if (that._fieldsPredicate($(this)) && +$(this).val() === -1) {
        errorMessage = that._addMessageLine(errorMessage);
        errorMessage += $(this).nextAll('.select-block-container').html() + $l.MultistepAction.RequiredField;
      }
    });

    const $requiredSelects = $('select[data-aggregated="False"]select[data-required="True"]');
    if (($requiredSelects.length === 0 || !this._fieldsPredicate(null))
        && ($('select[data-aggregated="False"]select[value!="-1"]').length === 0
        && $('select[data-aggregated="False"]select([data-excludeValid])').length === 0)) {
      errorMessage = that._addMessageLine(errorMessage);
      errorMessage += $l.MultistepAction.AnyFieldRequired;
    }

    return errorMessage;
  },

  _loadFileFields() {
    const id = `#${this.options.wrapperElementId} `;
    const delim = $(`${id}input[name="Delimiter"]:radio:checked`).val();
    const that = this;

    $(`${id}input[name="Delimiter"]`).on('click', function onClick() {
      that.loadFromFile(that.options, this.value);
    });

    $(`#${options.popupWindowId}_Encoding, #${options.popupWindowId}_LineSeparator`)
      .on('change', () => {
        that.loadFromFile(that.options);
      }
      );

    if (!isNaN(delim)) {
      that.loadFromFile(that.options);
    }
  },

  serializeForm() {
    return $(this.getParamsSelector()).serialize();
  },

  getParamsSelector() {
    const id = this.options.wrapperElementId;
    return `#${id} form input, #${id} form select`;
  },

  loadFromFile() {
    const that = this;

    // eslint-disable-next-line new-cap
    const act = new Quantumart.QP8.BackendActionExecutor.getBackendActionByCode(this.options.actionCode);
    let getFieldsUrl = String.format(
      '{0}FileFields/0/{1}/{2}/',
      act.ControllerActionUrl,
      this.options.parentEntityId,
      this.options.contentId
    );

    getFieldsUrl = getFieldsUrl.replace(/^~\//, window.APPLICATION_ROOT_URL);
    $.ajax({
      url: getFieldsUrl,
      data: this.serializeForm(),
      type: 'POST',
      success(fieldsFromFile) {
        if (fieldsFromFile.Type === 'Error') {
          $(`#${that.options.popupWindowId}_mapping_fields_selects`).hide();
          $q.alert(fieldsFromFile.Text);
        } else if (fieldsFromFile) {
          that._fillOptionsFromFile(fieldsFromFile);
          that._validate();
        }
      }
    });
  },

  _fillOptionsFromFile(fields) {
    const fieldsClass = ' .dropDownList.dataList.fields';
    const documentId = `#${this.options.wrapperElementId} `;
    $.each($(documentId + fieldsClass), (index, item) => {
      $(item).html('');
      $(item).append($('<option>', {
        value: -1,
        text: ''
      }));

      $.each(fields, (i, entry) => {
        $(item).append($('<option>', {
          value: entry,
          text: entry,
          selected: $(item).next().html() === entry
        }));
      });
    });

    this._$uniqueFieldToUpdate.val('CONTENT_ITEM_ID');
    this._$fieldGroup.show();
  },

  dispose() {
    this._disposeValidation();
    if (this._uploaderComponent) {
      this._uploaderComponent.detachObserver(window.EVENT_TYPE_LIBRARY_FILE_UPLOADED);
      this._uploaderComponent.dispose();
    }
  }
};

Quantumart.QP8.MultistepActionImportSettings.addButtons = function (dataItems) {
  const importButton = {
    Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
    Value: 'Import',
    Text: $l.MultistepAction.importTitle,
    Tooltip: $l.MultistepAction.importTitle,
    AlwaysEnabled: false,
    Icon: 'action.gif'
  };

  return dataItems.concat(importButton);
}


