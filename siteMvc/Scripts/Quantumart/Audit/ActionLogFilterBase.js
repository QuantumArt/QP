Quantumart.QP8.ActionLogFilterBase = function (filterContainer) {
  this.$container = $(filterContainer);
};

Quantumart.QP8.ActionLogFilterBase.prototype = {
  $container: null,

  onOpen() {
    // default implementation
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

$q.defineAbstractMethods.call(Quantumart.QP8.ActionLogFilterBase.prototype, [
  'initialize',
  'getValue'
]);

Quantumart.QP8.ActionLogFilterBase.registerClass('Quantumart.QP8.ActionLogFilterBase');
