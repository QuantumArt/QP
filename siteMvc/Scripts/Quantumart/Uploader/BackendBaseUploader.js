window.EVENT_TYPE_LIBRARY_FILE_UPLOADED = 'OnLibraryFileUploaded';
window.EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED = 'OnLibraryAllFilesUploaded';

class BackendBaseUploader extends Quantumart.QP8.Observable { }
$q.defineAbstractMethods.call(BackendBaseUploader.prototype, [
  'initialize',
  'setFolderPath',
  'getFolderPath',
  'dispose'
]);

Quantumart.QP8.BackendBaseUploader = BackendBaseUploader;
