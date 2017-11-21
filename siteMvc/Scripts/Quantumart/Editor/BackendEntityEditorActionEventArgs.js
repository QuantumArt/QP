class BackendEntityEditorActionEventArgs extends Quantumart.QP8.BackendEventArgs {
  constructor() {
    super();
    this._data = null;
  }

  get data() {
    return this._data;
  }

  set data(newData) {
    this._data = newData;
  }
}

Quantumart.QP8.BackendEntityEditorActionEventArgs = BackendEntityEditorActionEventArgs;
