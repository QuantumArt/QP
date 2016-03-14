//#region class BackendHtmlUploader
// === Класс "HTML загрузчик" ===
Quantumart.QP8.BackendHtmlUploader = function(parentElement, options) {
  Quantumart.QP8.BackendHtmlUploader.initializeBase(this);

  this._parentElement = parentElement;

  if (!$q.isNull(options)) {
    if (!$q.isNull(options.extensions))
    this._extensions = options.extensions;
    if (!$q.isNull(options.resolveName))
    this._resolveName = options.resolveName;
  }
};

Quantumart.QP8.BackendHtmlUploader.prototype = {
  _parentElement: null,
  _$telerikUpload: null,
  _folderPath: '', // текущий путь к папке
  _extensions: '',
  _resolveName: false,
  _uploadedFiles: [],

  initialize: function() {
    var $mvcUpload = jQuery(".t-upload input[type='file']", this._parentElement).data('tUpload');

    $mvcUpload.wrapper.unbind('upload');
    $mvcUpload.wrapper.unbind('success');
    $mvcUpload.wrapper.unbind('complete');
    $mvcUpload.wrapper.unbind('select');
    $mvcUpload.wrapper.unbind('error');
    $mvcUpload.wrapper.bind('upload', jQuery.proxy(this._onUploadHandler, this));
    $mvcUpload.wrapper.bind('success', jQuery.proxy(this._onUploadSuccessHandler, this));
    $mvcUpload.wrapper.bind('complete', jQuery.proxy(this._onUploadCompleteHandler, this));
    $mvcUpload.wrapper.bind('select', jQuery.proxy(this._onUploadSelectHandler, this));
    $mvcUpload.wrapper.bind('error', jQuery.proxy(this._onUploadErrorHandler, this));

    this._$telerikUpload = $mvcUpload;
    $mvcUpload = null;
  },

  dispose: function() {
    this._$telerikUpload.wrapper.unbind();
    jQuery(".t-upload input[type='file']", this._parentElement).removeData('tUpload');
  },

  set_folderPath: function(value) {
    this._folderPath = value;
  },

  get_folderPath: function() {
    return this._folderPath;
  },

  _onUploadHandler: function(e) {
    e.data = {
      folderPath: this._folderPath,
      resolveFileName: $q.toString(this._resolveName, 'false')
    };
  },

  _onUploadSuccessHandler: function(e) {
    this._uploadedFiles = [];
    if (e.response) {
      if (!e.response.proceed) {
        alert(e.response.msg);
      } else {
        jQuery.merge(this._uploadedFiles, e.response.fileNames);
      }
    }
  },

  _onUploadCompleteHandler: function() {
    if (this._uploadedFiles.length > 0) {
      var filenames = [];

      jQuery.each(this._uploadedFiles, function(i, val) {
        filenames.push(val);
      });

      var eventArgs = new Quantumart.QP8.BackendUploaderEventArgs(filenames);

      this.notify(EVENT_TYPE_LIBRARY_FILE_UPLOADED, eventArgs);
      this._uploadedFiles = [];
    }
  },

  _onUploadSelectHandler: function(e) {
    var toPrevent = false;

    jQuery.each(e.files, function() {
      if (this.size && this.size >= MAX_UPLOAD_SIZE_BYTES) {
        toPrevent = true;
        alert(String.format(HTML_UPLOAD_MAX_SIZE_MESSAGE, this.name, Math.ceil(MAX_UPLOAD_SIZE_BYTES / 1024 / 1024 * 100) / 100));
      }
    });

    if (toPrevent) {
      e.preventDefault();
      return false;
    }

    // проверить на допустимость расширения
    if (this._extensions.length > 0) {
      var extensions = jQuery.map(this._extensions.split(';'), function(val) {
        return val.toLowerCase();
      });

      jQuery.each(e.files, function() {
        if (Array.indexOf(extensions, this.extension.toLowerCase()) == -1) {
          toPrevent = true;
          alert(String.format(UPLOAD_EXTENSION_MESSAGE, this.name, this.extension));
        }
      });
    }

    if (toPrevent) {
      e.preventDefault();
      return false;
    }

    // если не ресолвим файл, то проверим его существование
    var self = this;

    if (!this._resolveName) {
      jQuery.each(e.files, function() {
        if (self._checkFileExistence(self._folderPath, this.name)) {
          if (!confirm(String.format(UPLOAD_OVERWRITE_MESSAGE, this.name))) {
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

  _onUploadErrorHandler: function(e) {
    alert(HTML_UPLOAD_ERROR_MESSAGE);
    e.preventDefault();
    return false;
  },

  _checkFileExistence: function(folderPath, fileName) {
    var url = APPLICATION_ROOT_URL + 'Library/FileExists/';
    var obj = $q.getJsonSync(url, {
      path: folderPath,
      name: fileName
    });

    return obj.result;
  }
};

Quantumart.QP8.BackendHtmlUploader.registerClass('Quantumart.QP8.BackendHtmlUploader', Quantumart.QP8.Observable, Quantumart.QP8.IBackendUploader);

//#endregion
