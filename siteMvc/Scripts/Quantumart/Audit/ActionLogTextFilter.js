// eslint-disable-next-line no-useless-constructor, FIXME
Quantumart.QP8.ActionLogTextFilter = function (filterContainer) {
  Quantumart.QP8.ActionLogTextFilter.initializeBase(this, [filterContainer]);
};

Quantumart.QP8.ActionLogTextFilter.prototype = {
  initialize() {
    this.$container.append('<div class="row"><input type="text" class="textbox" value="" /></div>');
    this.$container.find('input.textbox').focus();
  },

  onOpen() {
    this.$container.find('input.textbox').focus();
  },

  getValue() {
    return this.$container.find('input.textbox').val();
  },

  getFilterDetails() {
    const val = this.getValue();
    if (val) {
      return `"${$q.cutShort(val, 8)}"`;
    }
    return '""';
  }
};

Quantumart.QP8.ActionLogTextFilter.registerClass(
  'Quantumart.QP8.ActionLogTextFilter', Quantumart.QP8.ActionLogFilterBase
);
