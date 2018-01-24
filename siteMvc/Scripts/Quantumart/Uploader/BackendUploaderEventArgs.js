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
