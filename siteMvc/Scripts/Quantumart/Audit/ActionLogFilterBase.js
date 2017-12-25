Quantumart.QP8.ActionLogFilterBase = function (filterContainer) {
  this.$container = $(filterContainer);
};

Quantumart.QP8.ActionLogFilterBase.prototype = {
  $container: null,

  /** @abstract */
  initialize() {
    throw new Error($l.Common.methodNotImplemented);
  },

  onOpen() {
    // default implementation
  },

  /**
   * @abstract
   * @returns {any}
   */
  getValue() {
    throw new Error($l.Common.methodNotImplemented);
  },

  getFilterDetails() {
    return '?';
  },

  dispose() {
    $q.dispose.call(this, [
      '$container'
    ]);
  }
};

Quantumart.QP8.ActionLogFilterBase.registerClass('Quantumart.QP8.ActionLogFilterBase');
