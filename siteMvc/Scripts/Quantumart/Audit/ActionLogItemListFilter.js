import { ActionLogFilterBase } from './ActionLogFilterBase';

export class ActionLogItemListFilter extends ActionLogFilterBase {
  constructor(filterContainer, items) {
    super(filterContainer);
    this._items = items;
  }

  _items = null;
  initialize() {
    const html = new $.telerik.stringBuilder();

    html
      .cat('<div class="row">')
      .cat('<select class="dropDownList">');

    for (let i = 0; i < this._items.length; i++) {
      const item = this._items[i];
      html.cat('<option value="').cat(item.value).cat('">')
        .cat(item.text)
        .cat('</option>');
    }

    html
      .cat('</select>')
      .cat('</div>');

    this.$container.append(html.string());

    this.$container.find('select.dropDownList').focus();
  }

  onOpen() {
    this.$container.find('select.dropDownList').focus();
  }

  getValue() {
    return this.$container.find('select.dropDownList option:selected').val();
  }

  getFilterDetails() {
    return this.$container.find('select.dropDownList option:selected').text();
  }
}


Quantumart.QP8.ActionLogItemListFilter = ActionLogItemListFilter;
