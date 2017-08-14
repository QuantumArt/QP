Quantumart.QP8.BackendSilverlightUploader = function (parentElement, options) {
  Quantumart.QP8.BackendSilverlightUploader.initializeBase(this);

  let $parentElement = $('.l-sl-uploader', parentElement);

  this._parentElement = $parentElement.get(0);
  this._parentElementId = $parentElement.attr('id');

  $parentElement = null;

  if (!$q.isNull(options)) {
    if (!$q.isNull(options.background)) {
      this._background = options.background;
    }
    if (!$q.isNull(options.extensions)) {
      this._extensions = options.extensions;
    }
    if (!$q.isNull(options.resolveName)) {
      this._resolveName = options.resolveName;
    }
  }
};

Quantumart.QP8.BackendSilverlightUploader.prototype = {
  _parentElementId: '',
  _parentElement: null,

  _folderPath: '',
  _background: '#EBF5FB',
  _extensions: '',
  _resolveName: false,

  initialize: function () {
    let $parentElement = $(this._parentElement);
    $parentElement.data('qp_sl_uploader', this);

    this._createFileUploader();

    $parentElement = null;
  },

  dispose: function () {
    let $parentElement = $(this._parentElement);
    $parentElement.removeData();

    $parentElement = null;
    this._parentElement = null;
  },


  _createFileUploader: function () {
    let functionPrefix = 'Quantumart$QP8$BackendSilverlightUploader$';
    let uploadUrl = `${window.APPLICATION_ROOT_URL}Upload/`;
    let resolveNameFuction = this._resolveName ? `${functionPrefix}resolveFileName` : '';
    let params = {
      path: '',
      f_callback: `${functionPrefix}uploadCallback`,
      f_check: `${functionPrefix}checkFileExistence`,
      f_resolve: resolveNameFuction,
      f_localize: `${functionPrefix}localizeUpload`,
      f_path: `${functionPrefix}returnFolderPath`,
      f_security: `${functionPrefix}checkSecurity`,
      return_id: this._parentElementId,
      extensions: this._extensions,
      url: `${uploadUrl}RadUploadHandler.ashx`
    };

    let objId = `${this._parentElementId}_object`;

    Silverlight.createObjectEx({
      source: `${uploadUrl}SilverlightUpload.xap`,
      parentElement: this._parentElement,
      id: objId,
      properties: {
        width: '250',
        height: '20',
        isWindowless: 'true',
        background: 'transparent',
        alt: '',
        version: '2.0.31005.0'
      },
      events: {
        onError: this._onSilverlightErrorHandler
      },
      initParams: $q.hashToString(params),
      context: 'context'
    });
  },


  _onSilverlightErrorHandler: function (sender, args) {
    let appSource = '';
    if (sender != null && sender != 0) {
      appSource = sender.getHost().Source;
    }

    let errorType = args.ErrorType;
    let iErrorCode = args.ErrorCode;

    let errMsg = `Unhandled Error in Silverlight 2 Application ${appSource}\n`;

    errMsg += `Code: ${iErrorCode}    \n`;
    errMsg += `Category: ${errorType}       \n`;
    errMsg += `Message: ${args.ErrorMessage}     \n`;

    if (errorType == 'ParserError') {
      errMsg += `File: ${args.xamlFile}     \n`;
      errMsg += `Line: ${args.lineNumber}     \n`;
      errMsg += `Position: ${args.charPosition}     \n`;
    } else if (errorType == 'RuntimeError') {
      if (args.lineNumber != 0) {
        errMsg += `Line: ${args.lineNumber}     \n`;
        errMsg += `Position: ${args.charPosition}     \n`;
      }
      errMsg += `MethodName: ${args.methodName}     \n`;
    }
  },

  set_folderPath: function (value) {
    this._folderPath = value;
  },

  get_folderPath: function () {
    return this._folderPath;
  }
};

function Quantumart$QP8$BackendSilverlightUploader$uploadCallback(id, fileName) {
  let $element = $(`#${id}`);
  if ($element) {
    let component = $element.data('qp_sl_uploader');
    if (component) {
      let eventArgs = new Quantumart.QP8.BackendUploaderEventArgs([fileName]);
      component.notify(window.EVENT_TYPE_LIBRARY_FILE_UPLOADED, eventArgs);
    }
  }
}

function Quantumart$QP8$BackendSilverlightUploader$checkFileExistence(longfileName) {
  let url = `${window.APPLICATION_ROOT_URL}Library/FullNameFileExists/`;
  let obj = $q.getJsonSync(url, { name: longfileName });
  return obj.result;
}

function Quantumart$QP8$BackendSilverlightUploader$resolveFileName(path, fileName) {
  let url = `${window.APPLICATION_ROOT_URL}Library/ResolveFileName/`;
  let obj = $q.getJsonSync(url, { path: path, name: fileName });
  return obj.result;
}

function Quantumart$QP8$BackendSilverlightUploader$checkSecurity(path) {
  let url = `${window.APPLICATION_ROOT_URL}Library/CheckSecurity/`;
  let obj = $q.getJsonSync(url, { path: path });
  return obj.result;
}


function Quantumart$QP8$BackendSilverlightUploader$returnFolderPath(id) {
  let $element = $(`#${id}`);
  let result = '';
  if ($element) {
    let component = $element.data('qp_sl_uploader');
    if (component) {
      result = component.get_folderPath();
    }
  }

  return result;
}

function Quantumart$QP8$BackendSilverlightUploader$localizeUpload() {
  let result = {
    UploadBrowse: window.UPLOAD_BROWSE_BUTTON_NAME,
    UploadTotal: window.UPLOAD_TOTAL_LABEL,
    ExtensionMessage: window.UPLOAD_EXTENSION_MESSAGE,
    OverwriteMessage: window.UPLOAD_OVERWRITE_MESSAGE,
    MaxSizeMessage: window.UPLOAD_MAX_SIZE_MESSAGE,
    SecurityMessage: window.UPLOAD_SECURITY_MESSAGE
  };

  return result;
}

Quantumart.QP8.BackendSilverlightUploader.registerClass('Quantumart.QP8.BackendSilverlightUploader', Quantumart.QP8.Observable, Quantumart.QP8.IBackendUploader);
