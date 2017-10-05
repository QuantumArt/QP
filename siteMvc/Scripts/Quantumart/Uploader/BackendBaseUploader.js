window.EVENT_TYPE_LIBRARY_FILE_UPLOADED = 'OnLibraryFileUploaded';
window.EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED = 'OnLibraryAllFilesUploaded';

class BackendBaseUploader extends Quantumart.QP8.Observable {
  // eslint-disable-next-line class-methods-use-this
  initialize() {
    $c.notImplemented();
  }
  // eslint-disable-next-line class-methods-use-this
  dispose() {
    $c.notImplemented();
  }
  // eslint-disable-next-line class-methods-use-this
  setFolderPath() {
    $c.notImplemented();
  }
  // eslint-disable-next-line class-methods-use-this
  getFolderPath() {
    $c.notImplemented();
  }
}
Quantumart.QP8.BackendBaseUploader = BackendBaseUploader;

class BackendUploaderEventArgs extends Sys.EventArgs {
  constructor(fileNames) {
    super();
    this._fileNames = fileNames || [];
  }

  getFileNames() {
    return this._fileNames;
  }
}
Quantumart.QP8.BackendUploaderEventArgs = BackendUploaderEventArgs;
