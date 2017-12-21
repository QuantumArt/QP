class BackendEntityEditorActionEventArgs extends Quantumart.QP8.BackendEventArgs {
  constructor() {
    // @ts-ignore
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

// for MicrosoftAjax Type.isInstanceOfType
BackendEntityEditorActionEventArgs.registerClass(
  'Quantumart.QP8.BackendEntityEditorActionEventArgs', Quantumart.QP8.BackendEventArgs
);

Quantumart.QP8.BackendEntityEditorActionEventArgs = BackendEntityEditorActionEventArgs;
