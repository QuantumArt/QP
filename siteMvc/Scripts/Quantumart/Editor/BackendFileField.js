// eslint-disable-next-line max-statements
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
  _uploaderType: Quantumart.QP8.Enums.UploaderType.Html,
  _selectPopupWindowComponent: null,
  _uploaderComponent: null,
  _uploaderSubFolder: '',

  PREVIEW_BUTTON_CLASS_NAME: 'previewButton',
  BROWSE_BUTTON_CLASS_NAME: 'browseButton',
  DOWNLOAD_BUTTON_CLASS_NAME: 'downloadButton',
  LIBRARY_BUTTON_CLASS_NAME: 'libraryButton',

  getFileFieldElementId() {
    return this._fileFieldElementId;
  },

  setFileFieldElementId(value) {
    this._fileFieldElementId = value;
  },

  getFileFieldElement() {
    return this._fileFieldElement;
  },

  getFileWrapperElementId() {
    return this._fileWrapperElementId;
  },

  setFileWrapperElementId(value) {
    this._fileWrapperElementId = value;
  },

  getFileWrapperElement() {
    return this._fileWrapperElement;
  },

  get_entityId() { // eslint-disable-line camelcase
    return this._entityId;
  },

  set_entityId(value) { // eslint-disable-line camelcase
    this._entityId = value;
  },

  getAllowFileUpload() {
    return this._allowFileUpload;
  },

  setAllowFileUpload(value) {
    this._allowFileUpload = value;
  },

  getLibraryPath() {
    return this._libraryPath;
  },

  setLibraryPath(value) {
    this._libraryPath = value;
  },

  getRenameMatched() {
    return this._renameMatched;
  },

  setRenameMatched(value) {
    this._renameMatched = value;
  },

  getIsImage() {
    return this._isImage;
  },

  setIsImage(value) {
    this._isImage = value;
  },

  updateUploader(value) {
    this._uploaderSubFolder = value;
    const $fileField = $(this._fileFieldElement);

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
      this._uploaderComponent.setFolderPath(path);
    }
  },

  _getFileFieldSubFolder() {
    let subFolder = this._uploaderSubFolder;

    if (subFolder && !subFolder.match(/\/$/)) {
      subFolder += '/';
    }

    return subFolder;
  },

  _librarySelectedHandler(eventType, sender, args) {
    this._closeLibrary();
    if (args) {
      const { entities } = args;
      if (entities.length > 0) {
        let url = args.context;
        if (url === '\\') {
          url = '';
        }

        url = url.replace(`${this._subFolder}\\`, '').replace(/\\/g, '/');
        $(this._fileFieldElement).val(url + entities[0].Name).trigger('change');
      }
    }
  },

  _libraryClosedHandler() {
    this._closeLibrary();
  },

  _onPreviewButtonClickHandler: null,
  _onDownloadButtonClickHandler: null,
  _onLibraryButtonClickHandler: null,

  _showOrHidePreviewButton(filename, $previewButton) {
    const it = Quantumart.QP8.Enums.LibraryFileType.Image;
    if (this._isImage || window.LIBRARY_FILE_EXTENSIONS_DICTIONARY[it].split(';').filter(
      ext => filename.toLowerCase().endsWith(ext.toLowerCase())
    ).length > 0) {
      $previewButton.show();
    } else {
      $previewButton.hide();
    }
  },
  initialize() {
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

  _initFileUploader() {
    const it = Quantumart.QP8.Enums.LibraryFileType.Image;
    const extensions = this._isImage ? window.LIBRARY_FILE_EXTENSIONS_DICTIONARY[it] : '';

    if (this._uploaderType === Quantumart.QP8.Enums.UploaderType.Html) {
      this._uploaderComponent = new Quantumart.QP8.BackendHtmlUploader(this._fileWrapperElement, {
        extensions,
        resolveName: this.getRenameMatched()
      });
    } else {
      this._uploaderComponent = new Quantumart.QP8.BackendPlUploader(this._fileWrapperElement, {
        extensions,
        resolveName: this.getRenameMatched(),
        useSiteLibrary: this._useSiteLibrary,
        getFormScriptOptions: this._getFormScriptOptions.bind(this)
      });
    }

    this._uploaderComponent.initialize();
    this.updateUploader();
    this._uploaderComponent.attachObserver(
      window.EVENT_TYPE_LIBRARY_FILE_UPLOADED, $.proxy(this._onFileUploadedHandler, this)
    );
  },

  _getFormScriptOptions() {
    return {
      // @ts-ignore FIXME
      imgFilterResolution: this.imgFilterResolution
    };
  },

  _previewImage() {
    const $fileField = $(this._fileFieldElement);

    if (!$q.isNullOrEmpty($fileField)) {
      const fieldName = $fileField.attr('name');
      const fieldValue = $fileField.val();

      if (!$q.isNullOrWhiteSpace(fieldValue)) {
        $c.destroyPopupWindow(this._previewWindowComponent);
        const urlParams = {
          id: fieldName,
          fileName: encodeURIComponent(fieldValue),
          isVersion: this._isVersion,
          entityId: this._entityId,
          parentEntityId: this._libraryParentEntityId
        };
        const testUrl = Quantumart.QP8.BackendLibrary.generateActionUrl('GetImageProperties', urlParams);

        this._previewWindowComponent = $c.preview(testUrl);
      }
    }
  },

  _downloadFieldFile() {
    const $fileField = $(this._fileFieldElement);

    if (!$q.isNullOrEmpty($fileField)) {
      const fieldName = $fileField.attr('name');
      const fieldValue = $fileField.val();

      if (!$q.isNullOrWhiteSpace(fieldValue)) {
        const urlParams = {
          id: fieldName,
          fileName: encodeURIComponent(fieldValue),
          isVersion: this._isVersion,
          entityId: this._entityId
        };
        const url = Quantumart.QP8.BackendLibrary.generateActionUrl('TestFieldValueDownload', urlParams);

        $c.downloadFileWithChecking(url, fieldValue);
      }
    }
  },

  getUrl() {
    return this._libraryUrl + $(this._fileFieldElement).val();
  },

  pasteUrl(url) {
    $(this._fileFieldElement).val(url.replace(this._libraryUrl, ''));
  },

  _openLibrary() {
    const filterFileTypeId = this._isImage ? Quantumart.QP8.Enums.LibraryFileType.Image : '';
    let eventArgs = new Quantumart.QP8.BackendEventArgs();

    eventArgs.set_entityId(this._libraryEntityId);
    eventArgs.set_parentEntityId(this._libraryParentEntityId);
    eventArgs.set_entityName('');
    eventArgs.set_entityTypeCode(this._useSiteLibrary ? window.ENTITY_TYPE_CODE_SITE : window.ENTITY_TYPE_CODE_CONTENT);
    eventArgs.set_actionCode(
      this._useSiteLibrary ? window.ACTION_CODE_POPUP_SITE_LIBRARY : window.ACTION_CODE_POPUP_CONTENT_LIBRARY
    );
    let options = {
      isMultiOpen: true,
      additionalUrlParameters: { filterFileTypeId, subFolder: this._subFolder, allowUpload: this._allowFileUpload }
    };

    if (!this._selectPopupWindowComponent) {
      this._selectPopupWindowComponent = new Quantumart.QP8.BackendSelectPopupWindow(eventArgs, options);
      this._selectPopupWindowComponent.attachObserver(
        window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED, $.proxy(this._librarySelectedHandler, this)
      );
      this._selectPopupWindowComponent.attachObserver(
        window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED, $.proxy(this._libraryClosedHandler, this)
      );
    }

    this._selectPopupWindowComponent.openWindow();
    eventArgs = null;
    options = null;
  },

  _closeLibrary() {
    this._selectPopupWindowComponent.closeWindow();
  },

  _destroyLibrary() {
    if (this._selectPopupWindowComponent) {
      this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
      this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
      this._selectPopupWindowComponent.dispose();
      this._selectPopupWindowComponent = null;
    }
  },

  _onFileUploadedHandler(eventType, sender, eventArgs) {
    if (eventArgs.getFileNames().length > 0) {
      $(this._fileFieldElement)
        .val(this._getFileFieldSubFolder() + eventArgs.getFileNames()[0])
        .trigger('change');
    }
  },

  _onPreviewButtonClick() {
    this._previewImage();
  },

  _onDownloadButtonClick() {
    this._downloadFieldFile();
  },

  _onLibraryButtonClick() {
    this._openLibrary();
  },

  // eslint-disable-next-line max-statements
  dispose() {
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
