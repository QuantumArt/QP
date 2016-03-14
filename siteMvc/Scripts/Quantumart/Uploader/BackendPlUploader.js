'use strict';

Quantumart.QP8.BackendPlUploader = function($containerBlock, options) {
  Quantumart.QP8.BackendPlUploader.initializeBase(this);
  this._container = $containerBlock.find('.l-pl-uploader-container').first();
  this._pickupButton = $containerBlock.find('.pl_upload_button').first();
  this._pb_cont = $containerBlock.find('.lop-pbar-container').first();
  this._progressbar = $containerBlock.find('.lop-pbar').first();
  this._field = $containerBlock.find('input').first();
  this._folderId = $containerBlock.data('folder_id');

  this._extensions = options.extensions;
  this._resolveName = options.resolveName;
  this._useSiteLibrary = options.useSiteLibrary;
  this._getFormScriptOptions = options.getFormScriptOptions.bind(this);
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

  set_folderPath: function(value) {
    this._folderPath = value;
  },

  get_folderPath: function() {
    return this._folderPath;
  },

  _checkFileExistence: function(folderPath, fileName) {
    var url = window.APPLICATION_ROOT_URL + 'Library/FileExists/';
    var obj = $q.getJsonSync(url, {
      path: folderPath,
      name: fileName
    });

    return obj.result;
  },

  _resolveFileName: function(path, fileName) {
    var url = window.APPLICATION_ROOT_URL + 'Library/ResolveFileName/';
    var obj = $q.getJsonSync(url, { path: path, name: fileName });

    return obj.result;
  },

  _showProgress: function() {
    this._progress.value(0);
    this._progress.refresh();
    this._progress.setText('0%');
    this._pickupButton.css('display', 'none');
    this._pb_cont.css('display', 'inline-block');
  },

  _hideProgress: function() {
    this._pickupButton.css('display', 'inline-block');
    this._pb_cont.css('display', 'none');
  },

  _beforeUploadHandler: function(up) {
    up.settings.multipart_params = { destinationUrl: encodeURI(this.get_folderPath()) };
    up.refresh();
  },

  _uploadProgressHandler: function(up, file) {
    this._progress.value(file.percent);
    this._progress.refresh();
    this._progress.setText(file.name + ' ' + this._progress.value() + '%');
    up.refresh();
  },

  _fileUploadedHandler: function(up, file, data) {
    var response = JSON.parse(data.response);

    if (response.isError) {
      this._hideProgress();
      window.alert(response.message);
    } else {
      this._filesNames.push([file.name]);
      var eventArgs = new Quantumart.QP8.BackendUploaderEventArgs([file.name]);

      this.notify(window.EVENT_TYPE_LIBRARY_FILE_UPLOADED, eventArgs);

      if (up.total.uploaded === up.files.length) {
        var newEventArgs = new Quantumart.QP8.BackendEventArgs();

        newEventArgs.set_entityTypeCode((this._useSiteLibrary) ? window.ENTITY_TYPE_CODE_SITE_FILE : window.ENTITY_TYPE_CODE_CONTENT_FILE);
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

    up.refresh();
  },

  _filesAddedHandler: function(up, files) {
    var cancelledFiles = [];

    for (var i = 0; i < files.length; i++) {
      var file = files[i];

      if (file.size === 0) {
        up.removeFile(file);
        cancelledFiles.push(file);
        window.alert($.telerik.formatString(window.PL_UPLOAD_ZERO_SIZE_WARN, file.name));
      }

      if (this._resolveName) {
        file.name = this._resolveFileName(this._folderPath, file.name);
      } else if (this._checkFileExistence(this._folderPath, file.name) && !window.confirm(String.format(UPLOAD_OVERWRITE_MESSAGE, file.name))) {
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

  initialize: function() {
    var getFormOptsFn = this._getFormScriptOptions.bind(this);

    var options = {
      runtimes: 'html5,flash,html4,silverlight',
      browse_button: this._pickupButton.attr('id'),
      container: this._container.attr('id'),
      max_file_size: window.MAX_UPLOAD_SIZE_BYTES + 'b',
      chunk_size: '1mb',
      url: '/Backend/Upload/UploadChunk',
      flash_swf_url: '/Backend/Scripts/PlUpload/Moxie.swf',
      silverlight_xap_url: '/Backend/Scripts/PlUpload/Moxie.xap',
      filters: {
        max_img_resolution: {
          enabled: false,
          imageResolution: window.PL_IMAGE_RESOLUTION,
          prefilterAction: function() {
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

    this._uploader = new plupload.Uploader(options);
    this._uploader.bind('BeforeUpload', jQuery.proxy(this._beforeUploadHandler, this));
    this._uploader.init();

    this._uploader.bind('FilesAdded', jQuery.proxy(this._filesAddedHandler, this));
    this._uploader.bind('UploadProgress', jQuery.proxy(this._uploadProgressHandler, this));
    this._uploader.bind('FileUploaded', jQuery.proxy(this._fileUploadedHandler, this));
    this._uploader.bind('Error', function(up, err) {
      if (+err.code === -600) {
        window.alert(String.format(window.HTML_UPLOAD_MAX_SIZE_MESSAGE, err.file.name, Math.ceil(window.MAX_UPLOAD_SIZE_BYTES / 1024 / 1024 * 100) / 100));
      } else {
        window.alert(String.format(window.PL_UPLOAD_ERROR_REPORT, err.file.name, err.message, err.code));
      }

      up.refresh(); // Reposition Flash/Silverlight
    });

    this._progressbar.backendProgressBar();
    this._progress = this._progressbar.data('backendProgressBar');
    this._progress.total(100);
    this._uploader.refresh();

    this._container.data('qp_pl_uploader', this);
  },

  dispose: function() {
    this._container = null;
    this._fileList = null;
    this._pickupButton = null;
    this._progressbar = null;
    this._progress = null;
    this._extensions = null;
    this._filesLoadedDuringSession = null;
    this._pb_cont = null;
    this._folderId = null;
    this._uploader.destroy();
    this._uploader = null;
  }
};

Quantumart.QP8.BackendPlUploader.registerClass('Quantumart.QP8.BackendPlUploader', Quantumart.QP8.Observable, Quantumart.QP8.IBackendUploader);
