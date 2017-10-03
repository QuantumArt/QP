/* eslint camelcase: 0 */
Quantumart.QP8.BackendPreviousAction = function (options) {
  if ($q.isObject(options)) {
    if (options.entityTypeCode) {
      this._entityTypeCode = options.entityTypeCode;
    }

    if (options.actionTypeCode) {
      this._actionTypeCode = options.actionTypeCode;
    }

    if (options.actionCode) {
      this._actionCode = options.actionCode;
    }

    if (!$q.isNull(options.isSuccessfullyExecuted)) {
      this._isSuccessfullyExecuted = options.isSuccessfullyExecuted;
    }
  }
};

Quantumart.QP8.BackendPreviousAction.prototype = {
  _entityTypeCode: '',
  _actionTypeCode: '',
  _actionCode: '',
  _isSuccessfullyExecuted: false,

  get_entityTypeCode() {
    return this._entityTypeCode;
  },

  set_entityTypeCode(value) {
    this._entityTypeCode = value;
  },

  get_actionTypeCode() {
    return this._actionTypeCode;
  },

  set_actionTypeCode(value) {
    this._actionTypeCode = value;
  },

  get_actionCode() {
    return this._actionCode;
  },

  set_actionCode(value) {
    this._actionCode = value;
  },

  get_isSuccessfullyExecuted() {
    return this._isSuccessfullyExecuted;
  },

  set_isSuccessfullyExecuted(value) {
    this._isSuccessfullyExecuted = value;
  }
};

Quantumart.QP8.BackendPreviousAction.registerClass('Quantumart.QP8.BackendPreviousAction');
