import { ActionLogFilterBase } from './ActionLogFilterBase';
import { $q } from '../Utils';

export class ActionLogTextFilter extends ActionLogFilterBase {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor(filterContainer) {
    super(filterContainer);
  }

  initialize() {
    this.$container.append('<div class="row"><input type="text" class="textbox" value="" /></div>');
    this.$container.find('input.textbox').focus();
  }

  onOpen() {
    this.$container.find('input.textbox').focus();
  }

  getValue() {
    return this.$container.find('input.textbox').val();
  }

  getFilterDetails() {
    const val = this.getValue();
    if (val) {
      return `"${$q.cutShort(val, 8)}"`;
    }
    return '""';
  }
}


Quantumart.QP8.ActionLogTextFilter = ActionLogTextFilter;
