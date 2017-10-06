Quantumart.QP8.ActionLogFilterBase = function (filterContainer) {
  this.$container = jQuery(filterContainer);
};

Quantumart.QP8.ActionLogFilterBase.prototype = {
  $container: null,

  initialize: $c.notImplemented,
  onOpen() {
    // default implementation
  },
  getFilterDetails() {
    return '?';
  },
  getValue: $c.notImplemented,
  dispose() {
    this.$container = null;
  }
};

Quantumart.QP8.ActionLogFilterBase.registerClass('Quantumart.QP8.ActionLogFilterBase');
