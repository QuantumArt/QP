Quantumart.QP8.BackendFileField = function (fileFieldElementId, fileWrapperElementId, options) {
  Quantumart.QP8.BackendFileField.initializeBase(this);

  this._fileFieldElementId = fileFieldElementId;
  this._fileWrapperElementId = fileWrapperElementId;
  if (!$q.isNull(options)) {
    if (options.entityId) {
      this._entityId = options.entityId;
    }

    if (!$q.isNull(options.allowFileUpload)) {
      this._allowFileUpload = options.allowFileUpload;
    }

    if (options.libraryPath) {
      this._libraryPath = options.libraryPath;
    }

    if (options.libraryUrl) {
      this._libraryUrl = options.libraryUrl;
    }

    if (!$q.isNull(options.renameMatched)) {
      this._renameMatched = options.renameMatched;
    }

    if (!$q.isNull(options.isImage)) {
      this._isImage = options.isImage;
    }

    if (!$q.isNull(options.isVersion)) {
      this._isVersion = options.isVersion;
    }

    if (options.libraryEntityId) {
      this._libraryEntityId = options.libraryEntityId;
    }

    if (options.libraryParentEntityId) {
      this._libraryParentEntityId = options.libraryParentEntityId;
    }

    if (!$q.isNull(options.useSiteLibrary)) {
      this._useSiteLibrary = options.useSiteLibrary;
    }

    if (options.subFolder) {
      this._subFolder = options.subFolder;
    }

    if (options.uploaderType) {
      this._uploaderType = options.uploaderType;
    }
  }

  this._onPreviewButtonClickHandler = $.proxy(this._onPreviewButtonClick, this);
  this._onDownloadButtonClickHandler = $.proxy(this._onDownloadButtonClick, this);
  this._onLibraryButtonClickHandler = $.proxy(this._onLibraryButtonClick, this);
};

Quantumart.QP8.BackendFileField.prototype = {
  _fileFieldElementId: '',
  _fileFieldElement: null,
  _fileWrapperElementId: '',
  _fileWrapperElement: null,
  _browseButtonElement: null,
  _previewButtonElement: null,
  _libraryButtonElement: null,
  _previewWindowComponent: null,
  _downloadButtonElement: null,
  _entityId: 0,
  _allowFileUpload: false,
  _libraryPath: '',
  _libraryUrl: '',
  _renameMatched: false,
  _isImage: false,
  _isVersion: false,
  _subFolder: '',
  _useSiteLibrary: false,
  _libraryEntityId: 0,
  _libraryParentEntityId: 0,
  _uploaderType: Quantumart.QP8.Enums.UploaderType.Silverlight,
  _selectPopupWindowComponent: null,
  _uploaderComponent: null,
  _uploaderSubFolder: '',

  PREVIEW_BUTTON_CLASS_NAME: 'previewButton',
  BROWSE_BUTTON_CLASS_NAME: 'browseButton',
  DOWNLOAD_BUTTON_CLASS_NAME: 'downloadButton',
  LIBRARY_BUTTON_CLASS_NAME: 'libraryButton',

  get_fileFieldElementId: function () {
    return this._fileFieldElementId;
  },

  set_fileFieldElementId: function (value) {
    this._fileFieldElementId = value;
  },

  get_fileFieldElement: function () {
    return this._fileFieldElement;
  },

  get_fileWrapperElementId: function () {
    return this._fileWrapperElementId;
  },

  set_fileWrapperElementId: function (value) {
    this._fileWrapperElementId = value;
  },

  get_fileWrapperElement: function () {
    return this._fileWrapperElement;
  },

  get_entityId: function () {
    return this._entityId;
  },

  set_entityId: function (value) {
    this._entityId = value;
  },

  get_allowFileUpload: function () {
    return this._allowFileUpload;
  },

  set_allowFileUpload: function (value) {
    this._allowFileUpload = value;
  },

  get_libraryPath: function () {
    return this._libraryPath;
  },

  set_libraryPath: function (value) {
    this._libraryPath = value;
  },

  get_renameMatched: function () {
    return this._renameMatched;
  },

  set_renameMatched: function (value) {
    this._renameMatched = value;
  },

  get_isImage: function () {
    return this._isImage;
  },

  set_isImage: function (value) {
    this._isImage = value;
  },

  get_isVersion: function () {
    return this._isVersion;
  },

  set_isVersion: function (value) {
    this._isVersion = value;
  },

  updateUploader: function (value) {
    this._uploaderSubFolder = value;
    let $fileField = $(this._fileFieldElement);

    if (this._uploaderSubFolder) {
      $fileField.attr('placeholder', this._getFileFieldSubFolder());
    } else {
      $fileField.removeAttr('placeholder');
      this._uploaderSubFolder = '';
    }

    let path = this._libraryPath;

    if (this._uploaderSubFolder) {
      path += `\\${this._uploaderSubFolder}`;
    }

    if (this._uploaderComponent) {
      this._uploaderComponent.set_folderPath(path);
    }
  },

  _getFileFieldSubFolder: function () {
    let subFolder = this._uploaderSubFolder;

    if (subFolder && !subFolder.match(/\/$/)) {
      subFolder += '/';
    }

    return subFolder;
  },

  _librarySelectedHandler: function (eventType, sender, args) {
    this._closeLibrary();
    if (args) {
      let entities = args.entities;

      if (entities.length > 0) {
        let url = args.context;

        if (url === '\\') {
          url = '';
        }

        url = url.replace(`${this._subFolder}\\`, '').replace(/\\/g, '\/');
        $(this._fileFieldElement).val(url + entities[0].Name).trigger('change');
      }
    }
  },

  _libraryClosedHandler: function () {
    this._closeLibrary();
  },

  _onPreviewButtonClickHandler: null,
  _onDownloadButtonClickHandler: null,
  _onLibraryButtonClickHandler: null,

  _checkExt: function (filename, value) {
    return filename.toLowerCase().endsWith(value.toLowerCase());
  },

  _showOrHidePreviewButton: function (filename, $previewButton) {
    let _arrayOfExtensions = window.LIBRARY_FILE_EXTENSIONS_DICTIONARY[Quantumart.QP8.Enums.LibraryFileType.Image].split(';');
    let result = _arrayOfExtensions.filter(this._checkExt.bind(null, filename));
    if ((typeof result !== 'undefined' && result.length > 0) || this._isImage) {
      $previewButton.show();
    } else {
      $previewButton.hide();
    }
  },
  initialize: function () {
    let $fileField = $(`#${this._fileFieldElementId}`);

    $fileField.data('file_field', this);
    let $fileWrapper = $(`#${this._fileWrapperElementId}`);
    let $browseButton = $fileWrapper.find(`.${this.BROWSE_BUTTON_CLASS_NAME}:first`);
    let $previewButton = $fileWrapper.find(`.${this.PREVIEW_BUTTON_CLASS_NAME}:first`);

    $previewButton.bind('click', this._onPreviewButtonClickHandler);
    this._showOrHidePreviewButton($fileField.val(), $previewButton);
    let $libraryButton = $fileWrapper.find(`.${this.LIBRARY_BUTTON_CLASS_NAME}:first`);

    $libraryButton.bind('click', this._onLibraryButtonClickHandler);
    let $downloadButton = $fileWrapper.find(`.${this.DOWNLOAD_BUTTON_CLASS_NAME}:first`);

    $downloadButton.bind('click', this._onDownloadButtonClickHandler);

    this._fileFieldElement = $fileField.get(0);
    this._fileWrapperElement = $fileWrapper.get(0);
    this._browseButtonElement = $browseButton.get(0);
    this._previewButtonElement = $previewButton.get(0);
    this._libraryButtonElement = $libraryButton.get(0);
    this._downloadButtonElement = $downloadButton.get(0);

    if (this._allowFileUpload) {
      this._initFileUploader();
    }

    $fileField = null;
    $fileWrapper = null;
    $browseButton = null;
    $previewButton = null;
    $libraryButton = null;
    $downloadButton = null;
  },

  _initFileUploader: function () {
    let extensions = this._isImage ? window.LIBRARY_FILE_EXTENSIONS_DICTIONARY[Quantumart.QP8.Enums.LibraryFileType.Image] : '';

    if (this._uploaderType === Quantumart.QP8.Enums.UploaderType.Silverlight) {
      this._uploaderComponent = new Quantumart.QP8.BackendSilverlightUploader(this._fileWrapperElement, {
        background: '#ffffff',
        extensions: extensions,
        resolveName: this.get_renameMatched()
      });
    } else if (this._uploaderType === Quantumart.QP8.Enums.UploaderType.Html) {
      this._uploaderComponent = new Quantumart.QP8.BackendHtmlUploader(this._fileWrapperElement, {
        extensions: extensions,
        resolveName: this.get_renameMatched()
      });
    } else if (this._uploaderType === Quantumart.QP8.Enums.UploaderType.PlUpload) {
      this._uploaderComponent = new Quantumart.QP8.BackendPlUploader(this._fileWrapperElement, {
        extensions: extensions,
        resolveName: this.get_renameMatched(),
        useSiteLibrary: this._useSiteLibrary,
        getFormScriptOptions: this._getFormScriptOptions.bind(this)
      });
    }

    this._uploaderComponent.initialize();
    this.updateUploader();
    this._uploaderComponent.attachObserver(window.EVENT_TYPE_LIBRARY_FILE_UPLOADED, $.proxy(this._onFileUploadedHandler, this));
  },

  _getFormScriptOptions: function () {
    return {
      imgFilterResolution: this.imgFilterResolution
    };
  },

  _previewImage: function () {
    let $fileField = $(this._fileFieldElement);

    if (!$q.isNullOrEmpty($fileField)) {
      let fieldName = $fileField.attr('name');
      let fieldValue = $fileField.val();

      if (!$q.isNullOrWhiteSpace(fieldValue)) {
        $c.destroyPopupWindow(this._previewWindowComponent);
        let urlParams = { id: fieldName, fileName: encodeURIComponent(fieldValue), isVersion: this._isVersion, entityId: this._entityId, parentEntityId: this._libraryParentEntityId };
        let testUrl = Quantumart.QP8.BackendLibrary.generateActionUrl('GetImageProperties', urlParams);

        this._previewWindowComponent = $c.preview(testUrl);
      }
    }
  },

  _downloadFieldFile: function () {
    let $fileField = $(this._fileFieldElement);

    if (!$q.isNullOrEmpty($fileField)) {
      let fieldName = $fileField.attr('name');
      let fieldValue = $fileField.val();

      if (!$q.isNullOrWhiteSpace(fieldValue)) {
        let urlParams = { id: fieldName, fileName: encodeURIComponent(fieldValue), isVersion: this._isVersion, entityId: this._entityId };
        let url = Quantumart.QP8.BackendLibrary.generateActionUrl('TestFieldValueDownload', urlParams);

        $c.downloadFileWithChecking(url, fieldValue);
      }
    }
  },

  getUrl: function () {
    return this._libraryUrl + $(this._fileFieldElement).val();
  },

  pasteUrl: function (url) {
    $(this._fileFieldElement).val(url.replace(this._libraryUrl, ''));
  },

  _openLibrary: function () {
    let filterFileTypeId = this._isImage ? Quantumart.QP8.Enums.LibraryFileType.Image : '';
    let eventArgs = new Quantumart.QP8.BackendEventArgs();

    eventArgs.set_entityId(this._libraryEntityId);
    eventArgs.set_parentEntityId(this._libraryParentEntityId);
    eventArgs.set_entityName('');
    eventArgs.set_entityTypeCode(this._useSiteLibrary ? window.ENTITY_TYPE_CODE_SITE : window.ENTITY_TYPE_CODE_CONTENT);
    eventArgs.set_actionCode(this._useSiteLibrary ? window.ACTION_CODE_POPUP_SITE_LIBRARY : window.ACTION_CODE_POPUP_CONTENT_LIBRARY);
    let options = { isMultiOpen: true, additionalUrlParameters: { filterFileTypeId: filterFileTypeId, subFolder: this._subFolder, allowUpload: this._allowFileUpload } };

    if (!this._selectPopupWindowComponent) {
      this._selectPopupWindowComponent = new Quantumart.QP8.BackendSelectPopupWindow(eventArgs, options);
      this._selectPopupWindowComponent.attachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED, $.proxy(this._librarySelectedHandler, this));
      this._selectPopupWindowComponent.attachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED, $.proxy(this._libraryClosedHandler, this));
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
      this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
      this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
      this._selectPopupWindowComponent.dispose();
      this._selectPopupWindowComponent = null;
    }
  },

  _onFileUploadedHandler: function (eventType, sender, eventArgs) {
    if (eventArgs.get_fileNames().length > 0) {
      $(this._fileFieldElement)
        .val(this._getFileFieldSubFolder() + eventArgs.get_fileNames()[0])
        .trigger('change');
    }
  },

  _onPreviewButtonClick: function () {
    this._previewImage();
  },

  _onDownloadButtonClick: function () {
    this._downloadFieldFile();
  },

  _onLibraryButtonClick: function () {
    this._openLibrary();
  },

  dispose: function () {
    this._destroyLibrary();

    let $fileFieldElement = $(this._fileFieldElement);

    $fileFieldElement.removeData();
    $fileFieldElement = null;

    if (this._downloadButtonElement) {
      let $downloadButton = $(this._downloadButtonElement);

      $downloadButton.unbind('click', this._onDownloadButtonClickHandler);

      $downloadButton = null;
      this._downloadButtonElement = null;
    }

    if (this._previewWindowComponent) {
      $c.destroyPopupWindow(this._previewWindowComponent);
      this._previewWindowComponent = null;
    }

    if (this._previewButtonElement) {
      let $previewButton = $(this._previewButtonElement);

      $previewButton.unbind('click', this._onPreviewButtonClickHandler);

      $previewButton = null;
      this._previewButtonElement = null;
    }

    if (this._libraryButtonElement) {
      let $libraryButton = $(this._libraryButtonElement);

      $libraryButton.unbind('click', this._onLibraryButtonClickHandler);

      $libraryButton = null;
      this._libraryButtonElement = null;
    }

    if (this._uploaderComponent) {
      this._uploaderComponent.detachObserver(window.EVENT_TYPE_LIBRARY_FILE_UPLOADED);
      this._uploaderComponent.dispose();
      this._uploaderComponent = null;
    }

    this._browseButtonElement = null;
    this._fileWrapperElement = null;
    this._fileFieldElement = null;

    this._onPreviewButtonClickHandler = null;
    this._onDownloadButtonClickHandler = null;
    this._onLibraryButtonClickHandler = null;
  }
};

Quantumart.QP8.BackendFileField.registerClass('Quantumart.QP8.BackendFileField', null, Sys.IDisposable);
