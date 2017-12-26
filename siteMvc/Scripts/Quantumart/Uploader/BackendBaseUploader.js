window.EVENT_TYPE_LIBRARY_FILE_UPLOADED = 'OnLibraryFileUploaded';
window.EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED = 'OnLibraryAllFilesUploaded';

class BackendBaseUploader extends Quantumart.QP8.Observable {
  /** @abstract */
  initialize() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @returns {string}
   */
  getFolderPath() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @param {string} _value
   */
  setFolderPath(_value) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @abstract */
  dispose() {
    throw new Error($l.Common.methodNotImplemented);
  }
}

Quantumart.QP8.BackendBaseUploader = BackendBaseUploader;
