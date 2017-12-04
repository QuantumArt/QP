window.EVENT_TYPE_LIBRARY_FILE_UPLOADED = 'OnLibraryFileUploaded';
window.EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED = 'OnLibraryAllFilesUploaded';

class BackendBaseUploader extends Quantumart.QP8.Observable {
  constructor() {
    super();
    $q.defineAbstractMethods(this, [
      'initialize',
      'setFolderPath',
      'getFolderPath',
      'dispose'
    ]);
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
