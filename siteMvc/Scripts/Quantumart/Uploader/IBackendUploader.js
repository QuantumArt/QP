// #region event types of IBackendUploader
var EVENT_TYPE_LIBRARY_FILE_UPLOADED = 'OnLibraryFileUploaded';
var EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED = 'OnLibraryAllFilesUploaded';

// #endregion

// #region interface IBackendUploader
Quantumart.QP8.IBackendUploader = function() { };

Quantumart.QP8.IBackendUploader.prototype = {
  initialize: function() {
    alert('initialize is not implemented');
  },
  dispose: function() {
    alert('dispose is not implemented');
  },
  set_folderPath: function() {
    alert('set_folderPath is not implemented');
  },
  get_folderPath: function() {
    alert('get_folderPath is not implemented');
  }
};

Quantumart.QP8.IBackendUploader.registerInterface('Quantumart.QP8.IBackendUploader');

// #endregion

// #region class BackendUploaderEventArgs
Quantumart.QP8.BackendUploaderEventArgs = function(fileNames) {
  Quantumart.QP8.BackendUploaderEventArgs.initializeBase(this);

  this._fileNames = fileNames;
};

Quantumart.QP8.BackendUploaderEventArgs.prototype = {
  _fileNames: [],

  get_fileNames: function() {
    return this._fileNames;
  }
};

Quantumart.QP8.BackendUploaderEventArgs.registerClass('Quantumart.QP8.BackendUploaderEventArgs', Sys.EventArgs);

// #endregion

