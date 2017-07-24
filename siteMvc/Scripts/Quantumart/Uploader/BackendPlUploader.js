/* eslint camelcase: ["off", { properties: "never"}] */
Quantumart.QP8.BackendPlUploader = function BackendPlUploader(containerBlock, options) {
  var $container;
  var getFormScriptOptions = function getFormScriptOptions() {
    return {
      imgFilterResolution: 0,
      extensions: options.extensions,
      resolveName: options.resolveName,
      useSiteLibrary: options.useSiteLibrary
    };
  };

  Quantumart.QP8.BackendPlUploader.initializeBase(this);

  $container = $(containerBlock);
  this._container = $container.find('.l-pl-uploader-container').first();
  this._pickupButton = $container.find('.pl_upload_button').first();
  this._pb_cont = $container.find('.lop-pbar-container').first();
  this._progressbar = $container.find('.lop-pbar').first();
  this._field = $container.find('input').first();
  this._folderId = $container.data('folder_id');
  this._extensions = options.extensions;
  this._resolveName = options.resolveName;
  this._useSiteLibrary = options.useSiteLibrary;
  this._getFormScriptOptions = options.getFormScriptOptions || getFormScriptOptions;
  this._previewButton = $container.find('.previewButton').first();
};

Quantumart.QP8.BackendPlUploader.prototype = {
  _fileList: null,
  _pickupButton: null,
  _folderPath: '',
  _container: null,
  _progressbar: null,
  _progress: null,
  _extensions: null,
  _filesNames: [],
  _resolveName: false,
  _useSiteLibrary: false,
  _libraryEntityId: null,
  _libraryParentEntityId: null,
  _uploader: null,

  set_folderPath: function (value) {
    this._folderPath = value;
  },

  get_folderPath: function () {
    return this._folderPath;
  },

  _checkFileExistence: function _checkFileExistence(folderPath, fileName) {
    var url = window.APPLICATION_ROOT_URL + 'Library/FileExists/';
    return $q.getJsonSync(url, { path: folderPath, name: fileName }).result;
  },

  _resolveFileName: function (path, fileName) {
    var url = window.APPLICATION_ROOT_URL + 'Library/ResolveFileName/';
    return $q.getJsonSync(url, { path: path, name: fileName }).result;
  },

  _showProgress: function () {
    this._progress.value(0);
    this._progress.refresh();
    this._progress.setText('0%');
    this._pickupButton.css('display', 'none');
    this._pb_cont.css('display', 'inline-block');
  },

  _hideProgress: function () {
    this._pickupButton.css('display', 'inline-block');
    this._pb_cont.css('display', 'none');
  },

  _beforeUploadHandler: function (up) {
    // eslint-disable-next-line no-param-reassign
    up.settings.multipart_params = { destinationUrl: encodeURI(this.get_folderPath()) };
    up.refresh();
  },

  _uploadProgressHandler: function (up, file) {
    this._progress.value(file.percent);
    this._progress.refresh();
    this._progress.setText(file.name + ' ' + this._progress.value() + '%');
    up.refresh();
  },

  _fileUploadedHandler: function (up, file, data) {
    var eventArgs, newEventArgs;
    var response = JSON.parse(data.response);
    if (response.isError) {
      this._hideProgress();
      $q.alertError(response.message);
    } else {
      this._filesNames.push([file.name]);

      eventArgs = new Quantumart.QP8.BackendUploaderEventArgs([file.name]);
      this.notify(window.EVENT_TYPE_LIBRARY_FILE_UPLOADED, eventArgs);

      if (up.total.uploaded === up.files.length) {
        newEventArgs = new Quantumart.QP8.BackendEventArgs();
        newEventArgs.set_entityTypeCode(this._useSiteLibrary
          ? window.ENTITY_TYPE_CODE_SITE_FILE
          : window.ENTITY_TYPE_CODE_CONTENT_FILE);

        newEventArgs.set_actionTypeCode(window.ACTION_TYPE_CODE_ALL_FILES_UPLOADED);
        if (this._useSiteLibrary) {
          newEventArgs.set_actionCode(window.ACTION_CODE_UPDATE_SITE_FILE);
        } else {
          newEventArgs.set_actionCode(window.ACTION_CODE_UPDATE_CONTENT_FILE);
        }

        newEventArgs.set_parentEntityId(this._folderId);
        this._hideProgress();
        this.notify(window.EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED, newEventArgs);
      }
    }
    this._showOrHidePreviewButton(file.name);
    up.refresh();
  },

  _checkExt: function (filename, value) {
    return filename.toLowerCase().endsWith(value.toLowerCase());
  },

  _showOrHidePreviewButton: function (filename, $previewButton) {
    var _arrayOfExtensions = LIBRARY_FILE_EXTENSIONS_DICTIONARY[Quantumart.QP8.Enums.LibraryFileType.Image].split(';');

    var result = _arrayOfExtensions.filter(this._checkExt.bind(null, filename)) ;
    if (typeof result !== 'undefined' && result.length > 0 || this._isImage) {
      this._previewButton.show();
    }
    else {
      this._previewButton.hide();
    }
  },

  _filesAddedHandler: function filesAddedHandler(up, files) {
    var i, file;
    var cancelledFiles = [];
    for (i = 0; i < files.length; i++) {
      file = files[i];
      if (file.size === 0) {
        up.removeFile(file);
        cancelledFiles.push(file);
        $q.alertWarn($.telerik.formatString(window.PL_UPLOAD_ZERO_SIZE_WARN, file.name));
      }

      if (this._resolveName) {
        file.name = this._resolveFileName(this._folderPath, file.name);
      } else if (this._checkFileExistence(this._folderPath, file.name)
        && !$q.confirmMessage(String.format(window.UPLOAD_OVERWRITE_MESSAGE, file.name))) {
        up.removeFile(file);
        cancelledFiles.push(file);
      }
    }

    if (cancelledFiles.length < files.length) {
      up.start();
      this._showProgress();
      up.refresh();
    }
  },

  initialize: function initialize() {
    var getFormOptsFn = this._getFormScriptOptions.bind(this);
    var options = {
      runtimes: 'html5,flash,html4,silverlight',
      browse_button: this._pickupButton.attr('id'),
      container: this._container.attr('id'),
      max_file_size: window.MAX_UPLOAD_SIZE_BYTES + 'b',
      chunk_size: '1mb',
      debug: false,
      url: '/Backend/Upload/UploadChunk',
      flash_swf_url: '/Backend/Scripts/PlUpload/Moxie.swf',
      silverlight_xap_url: '/Backend/Scripts/PlUpload/Moxie.xap',
      filters: {
        max_img_resolution: {
          enabled: false,
          imageResolution: window.PL_IMAGE_RESOLUTION,
          prefilterAction: function () {
            this.imageResolution = getFormOptsFn().imgFilterResolution;
            this.enabled = !!this.imageResolution;
          }
        }
      }
    };

    if (this._extensions) {
      options.filters.mime_types = [{
        title: 'Allowed files',
        extensions: this._extensions.split(';.').join(',').replace('.', '')
      }];
    }

    this._uploader = new window.plupload.Uploader(options);
    this._uploader.bind('BeforeUpload', $.proxy(this._beforeUploadHandler, this));
    this._uploader.init();

    this._uploader.bind('FilesAdded', $.proxy(this._filesAddedHandler, this));
    this._uploader.bind('UploadProgress', $.proxy(this._uploadProgressHandler, this));
    this._uploader.bind('FileUploaded', $.proxy(this._fileUploadedHandler, this));
    this._uploader.bind('Error', function errorHandler(up, err) {
      if (+err.code === -600) {
        $q.alertError(String.format(
          window.HTML_UPLOAD_MAX_SIZE_MESSAGE,
          err.file.name,
          Math.ceil(window.MAX_UPLOAD_SIZE_BYTES / 1024 / 1024 * 100) / 100
        ));
      } else {
        $q.alertFail(String.format(
          window.PL_UPLOAD_ERROR_REPORT,
          err.file.name,
          err.message,
          err.code
        ));
      }

      up.refresh();
    });

    this._progressbar.backendProgressBar();
    this._progress = this._progressbar.data('backendProgressBar');
    this._progress.total(100);
    this._uploader.refresh();

    this._container.data('qp_pl_uploader', this);
  },

  dispose: function () {
    this._uploader.destroy();
    $q.dispose.call(this, [
      '_container',
      '_fileList',
      '_pickupButton',
      '_progressbar',
      '_progress',
      '_extensions',
      '_filesLoadedDuringSession',
      '_pb_cont',
      '_folderId',
      '_uploader'
    ]);
  }
};

Quantumart.QP8.BackendPlUploader.registerClass(
  'Quantumart.QP8.BackendPlUploader',
  Quantumart.QP8.Observable,
  Quantumart.QP8.IBackendUploader
);
