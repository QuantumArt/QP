Quantumart.QP8.ActionLogTextFilter = function (filterContainer) {
  Quantumart.QP8.ActionLogTextFilter.initializeBase(this, [filterContainer]);
};

Quantumart.QP8.ActionLogTextFilter.prototype = {
  initialize () {
    this.$container.append('<div class="row"><input type="text" class="textbox" value="" /></div>');
    this.$container.find('input.textbox').focus();
  },

  onOpen () {
    this.$container.find('input.textbox').focus();
  },

  get_value () {
    return this.$container.find('input.textbox').val();
  },

  get_filterDetails () {
    const val = this.get_value();
    if (val) {
      return `"${$q.cutShort(val, 8)}"`;
    }
    return '""';

  }
};

Quantumart.QP8.ActionLogTextFilter.registerClass('Quantumart.QP8.ActionLogTextFilter', Quantumart.QP8.ActionLogFilterBase);
