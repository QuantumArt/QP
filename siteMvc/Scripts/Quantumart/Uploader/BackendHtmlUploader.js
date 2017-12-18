class BackendHtmlUploader extends Quantumart.QP8.BackendBaseUploader {
  constructor(parentElement, options) {
    // @ts-ignore
    super();
    this._parentElement = parentElement;
    this._extensions = options.extensions || '';
    this._resolveName = options.resolveName || false;
    this._folderPath = '';
    this._uploadedFiles = [];
  }

  initialize() {
    const $mvcUpload = $(".t-upload input[type='file']", this._parentElement).data('tUpload');

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
  }

  dispose() {
    this._$telerikUpload.wrapper.unbind();
    $(".t-upload input[type='file']", this._parentElement).removeData('tUpload');
  }

  setFolderPath(value) {
    this._folderPath = value;
  }

  getFolderPath() {
    return this._folderPath;
  }

  _onUploadHandler(e) {
    // eslint-disable-next-line no-param-reassign
    e.data = {
      folderPath: this._folderPath,
      resolveFileName: $q.toString(this._resolveName, 'false')
    };
  }

  _onUploadSuccessHandler(e) {
    this._uploadedFiles = [];
    if (e.response) {
      if (e.response.proceed) {
        $.merge(this._uploadedFiles, e.response.fileNames);
      } else {
        $q.alertSuccess(e.response.msg);
      }
    }
  }

  _onUploadCompleteHandler() {
    if (this._uploadedFiles.length > 0) {
      const filenames = [];

      $.each(this._uploadedFiles, (i, val) => {
        filenames.push(val);
      });

      const eventArgs = new Quantumart.QP8.BackendUploaderEventArgs(filenames);

      this.notify(window.EVENT_TYPE_LIBRARY_FILE_UPLOADED, eventArgs);
      this._uploadedFiles = [];
    }
  }

  _onUploadSelectHandler(e) {
    let toPrevent = false;

    $.each(e.files, function () {
      if (this.size && this.size >= window.MAX_UPLOAD_SIZE_BYTES) {
        toPrevent = true;
        $q.alertFail(String.format(
          window.HTML_UPLOAD_MAX_SIZE_MESSAGE,
          this.name,
          Math.ceil(window.MAX_UPLOAD_SIZE_BYTES / 1024 / 1024 * 100) / 100)
        );
      }
    });

    if (toPrevent) {
      e.preventDefault();
      return false;
    }

    if (this._extensions.length > 0) {
      const extensions = this._extensions.split(';').map(val => val.toLowerCase());

      $.each(e.files, function () {
        if (Array.indexOf(extensions, this.extension.toLowerCase()) === -1) {
          toPrevent = true;
          $q.alertFail(String.format(window.UPLOAD_EXTENSION_MESSAGE, this.name, this.extension));
        }
      });
    }

    if (toPrevent) {
      e.preventDefault();
      return false;
    }

    const that = this;
    if (!this._resolveName) {
      $.each(e.files, function () {
        if (that._checkFileExistence(this.name)) {
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
    return true;
  }

  _onUploadErrorHandler(e) {
    $q.alertFail(`Uploading to ${this._folderPath} failed: ${window.HTML_UPLOAD_ERROR_MESSAGE}`);
    e.preventDefault();
    return false;
  }

  _checkFileExistence(fileName) {
    const url = `${window.APPLICATION_ROOT_URL}Library/FileExists/`;
    const obj = $q.getJsonSync(url, {
      path: this._folderPath,
      name: fileName
    });

    return obj.result;
  }
}

Quantumart.QP8.BackendHtmlUploader = BackendHtmlUploader;
