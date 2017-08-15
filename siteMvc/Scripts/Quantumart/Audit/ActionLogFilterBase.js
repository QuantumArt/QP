Quantumart.QP8.ActionLogFilterBase = function (filterContainer) {
  this.$container = jQuery(filterContainer);
};

Quantumart.QP8.ActionLogFilterBase.prototype = {
  $container: null,

  initialize () { },
  onOpen () { },
  get_filterDetails () {
    return '?';
  },
  get_value () { },
  dispose () {
    this.$container = null;
  }
};

Quantumart.QP8.ActionLogFilterBase.registerClass('Quantumart.QP8.ActionLogFilterBase');
