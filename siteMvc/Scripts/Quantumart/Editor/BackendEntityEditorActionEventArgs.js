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

  // eslint-disable-next-line camelcase
  get_data() {
    return this.data;
  }

  // eslint-disable-next-line camelcase
  set_data(newData) {
    this.data = newData;
  }
}

Quantumart.QP8.BackendEntityEditorActionEventArgs = BackendEntityEditorActionEventArgs;
