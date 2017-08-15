Quantumart.QP8.BackendHtmlUploader = function (parentElement, options) {
  Quantumart.QP8.BackendHtmlUploader.initializeBase(this);

  this._parentElement = parentElement;

  if (!$q.isNull(options)) {
    if (!$q.isNull(options.extensions)) {
      this._extensions = options.extensions;
    }
    if (!$q.isNull(options.resolveName)) {
      this._resolveName = options.resolveName;
    }
  }
};

Quantumart.QP8.BackendHtmlUploader.prototype = {
  _parentElement: null,
  _$telerikUpload: null,
  _folderPath: '',
  _extensions: '',
  _resolveName: false,
  _uploadedFiles: [],

  initialize: function () {
    let $mvcUpload = $(".t-upload input[type='file']", this._parentElement).data('tUpload');

    $mvcUpload.wrapper.unbind('upload');
    $mvcUpload.wrapper.unbind('success');
    $mvcUpload.wrapper.unbind('complete');
    $mvcUpload.wrapper.unbind('select');
    $mvcUpload.wrapper.unbind('error');
    $mvcUpload.wrapper.bind('upload', $.proxy(this._onUploadHandler, this));
    $mvcUpload.wrapper.bind('success', $.proxy(this._onUploadSuccessHandler, this));
    $mvcUpload.wrapper.bind('complete', $.proxy(this._onUploadCompleteHandler, this));
    $mvcUpload.wrapper.bind('select', $.proxy(this._onUploadSelectHandler, this));
    $mvcUpload.wrapper.bind('error', $.proxy(this._onUploadErrorHandler, this));

    this._$telerikUpload = $mvcUpload;
    $mvcUpload = null;
  },

  dispose: function () {
    this._$telerikUpload.wrapper.unbind();
    $(".t-upload input[type='file']", this._parentElement).removeData('tUpload');
  },

  set_folderPath: function (value) {
    this._folderPath = value;
  },

  get_folderPath: function () {
    return this._folderPath;
  },

  _onUploadHandler: function (e) {
    e.data = {
      folderPath: this._folderPath,
      resolveFileName: $q.toString(this._resolveName, 'false')
    };
  },

  _onUploadSuccessHandler: function (e) {
    this._uploadedFiles = [];
    if (e.response) {
      if (!e.response.proceed) {
        $q.alertSuccess(e.response.msg);
      } else {
        $.merge(this._uploadedFiles, e.response.fileNames);
      }
    }
  },

  _onUploadCompleteHandler: function () {
    if (this._uploadedFiles.length > 0) {
      const filenames = [];

      $.each(this._uploadedFiles, (i, val) => {
        filenames.push(val);
      });

      const eventArgs = new Quantumart.QP8.BackendUploaderEventArgs(filenames);

      this.notify(window.EVENT_TYPE_LIBRARY_FILE_UPLOADED, eventArgs);
      this._uploadedFiles = [];
    }
  },

  _onUploadSelectHandler: function (e) {
    let toPrevent = false;

    $.each(e.files, function () {
      if (this.size && this.size >= window.MAX_UPLOAD_SIZE_BYTES) {
        toPrevent = true;
        $q.alertFail(String.format(window.HTML_UPLOAD_MAX_SIZE_MESSAGE, this.name, Math.ceil(window.MAX_UPLOAD_SIZE_BYTES / 1024 / 1024 * 100) / 100));
      }
    });

    if (toPrevent) {
      e.preventDefault();
      return false;
    }

    if (this._extensions.length > 0) {
      const extensions = $.map(this._extensions.split(';'), val => val.toLowerCase());

      $.each(e.files, function () {
        if (Array.indexOf(extensions, this.extension.toLowerCase()) == -1) {
          toPrevent = true;
          $q.alertFail(String.format(window.UPLOAD_EXTENSION_MESSAGE, this.name, this.extension));
        }
      });
    }

    if (toPrevent) {
      e.preventDefault();
      return false;
    }

    const self = this;
    if (!this._resolveName) {
      $.each(e.files, function () {
        if (self._checkFileExistence(self._folderPath, this.name)) {
          if (!$q.confirmMessage(String.format(window.UPLOAD_OVERWRITE_MESSAGE, this.name))) {
            toPrevent = true;
          }
        }
      });
    }

    if (toPrevent) {
      e.preventDefault();
      return false;
    }
  },

  _onUploadErrorHandler: function (e) {
    $q.alertFail(window.HTML_UPLOAD_ERROR_MESSAGE);
    e.preventDefault();
    return false;
  },

  _checkFileExistence: function (folderPath, fileName) {
    const url = `${window.APPLICATION_ROOT_URL}Library/FileExists/`;
    const obj = $q.getJsonSync(url, {
      path: folderPath,
      name: fileName
    });

    return obj.result;
  }
};

Quantumart.QP8.BackendHtmlUploader.registerClass('Quantumart.QP8.BackendHtmlUploader', Quantumart.QP8.Observable, Quantumart.QP8.IBackendUploader);
