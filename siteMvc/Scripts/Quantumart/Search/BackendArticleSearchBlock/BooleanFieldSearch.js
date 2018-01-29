import { BackendArticleSearchBlock } from '../BackendArticleSearchBlock';
import { FieldSearchBase, FieldSearchState } from './FieldSearchBase';
import { $q } from '../../Utils';

export class BooleanFieldSearch extends FieldSearchBase {
  // eslint-disable-next-line max-params
  constructor(
    containerElement,
    parentEntityId,
    fieldID,
    contentID,
    fieldColumn,
    fieldName,
    fieldGroup,
    referenceFieldID
  ) {
    super(
      containerElement,
      parentEntityId,
      fieldID,
      contentID,
      fieldColumn,
      fieldName,
      fieldGroup,
      referenceFieldID
    );

    this._onIsNullCheckBoxChangeHandler = $.proxy(this._onIsNullCheckBoxChange, this);
  }

  initialize() {
    const isNullCheckBoxID = `${this._elementIdPrefix}_isNullCheckBox`;
    const radioGroupName = `${this._elementIdPrefix}_radioGroup`;
    const disablingContainerID = `${this._elementIdPrefix}_disablingContainer`;
    const radioTrueID = `${this._elementIdPrefix}_radioTrue`;
    const radioFalseID = `${this._elementIdPrefix}_radioFalse`;

    let html = new $.telerik.stringBuilder();
    html
      .cat('<div class="row">')
      .cat('  <div ')
      .cat(` id="${$q.htmlEncode(disablingContainerID)}"`)
      .cat(' class="radioButtonsList horizontalDirection"')
      .cat('>\n')
      .cat('    <ul>\n')
      .cat('      <li>\n')
      .cat('        <input type="radio" ')
      .cat(` id="${$q.htmlEncode(radioTrueID)}"`)
      .cat(` name="${$q.htmlEncode(radioGroupName)}"`)
      .cat(' value="true" checked="checked"')
      .cat(' />\n')
      .cat(`        <label for="${$q.htmlEncode(radioTrueID)}">${$l.SearchBlock.trueText}</label>\n`)
      .cat('      </li>\n')
      .cat('      <li>\n')
      .cat('        <input type="radio"')
      .cat(` id="${$q.htmlEncode(radioFalseID)}"`)
      .cat(` name="${$q.htmlEncode(radioGroupName)}"`)
      .cat(' />\n')
      .cat(`        <label for="${$q.htmlEncode(radioFalseID)}">${$l.SearchBlock.falseText}</label>`)
      .cat('      </li>\n')
      .cat('    </ul>\n')
      .cat('  </div>\n')
      .cat('</div>\n')
      .cat('<div class="row">')
      .cat('  <span class="checkbox">\n')
      .cat('    <input type="checkbox"')
      .cat(` id="${$q.htmlEncode(isNullCheckBoxID)}"`)
      .cat(' />\n')
      .cat(`    <label for="${$q.htmlEncode(isNullCheckBoxID)}">${$l.SearchBlock.isNullCheckBoxLabelText}</label>`)
      .cat('  </span>')
      .cat('</div>')
    ;

    // добавить разметку на страницу
    let $containerElement = $(this._containerElement);
    $containerElement.append(html.string());

    // назначить обработчик события change чекбоксу
    let $isNullCheckBoxElement = $containerElement.find(`#${isNullCheckBoxID}`);
    $isNullCheckBoxElement.bind('change', this._onIsNullCheckBoxChangeHandler);

    // запомнить ссылку на dom-элементы
    this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);

    $isNullCheckBoxElement = null;
    $containerElement = null;
    html = null;
  }

  getSearchQuery() {
    return BackendArticleSearchBlock.createFieldSearchQuery(
      Quantumart.QP8.Enums.ArticleFieldSearchType.Boolean,
      this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID,
      this.getIsNull(), this._getValue());
  }

  getBlockState() {
    return new FieldSearchState(
      Quantumart.QP8.Enums.ArticleFieldSearchType.Boolean,
      this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
      {
        isNull: this.getIsNull(),
        value: this._getValue()
      });
  }

  getFilterDetails() {
    const stateData = this.getBlockState().data;
    if (stateData.isNull) {
      return $l.SearchBlock.isNullCheckBoxLabelText;
    }
    return stateData.value ? $l.SearchBlock.trueText : $l.SearchBlock.falseText;
  }

  restoreBlockState(state) {
    if (state) {
      if (this._isNullCheckBoxElement) {
        let $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
        $isNullCheckBoxElement.prop('checked', state.isNull);
        $isNullCheckBoxElement.trigger('change');
        $isNullCheckBoxElement = null;
      }
      if (!$q.isNull(state.value)) {
        if (state.value) {
          $('input:radio[value=true]', this._containerElement).prop('checked', true);
        } else if (!state.value) {
          $('input:radio[value!=true]', this._containerElement).prop('checked', true);
        }
      }
    }
  }

  _getValue() {
    let result = null;
    const val = $(this._containerElement).find('input:radio:checked').val();
    if (val === 'true') {
      result = true;
    } else {
      result = false;
    }
    return result;
  }

  _onIsNullCheckBoxChange() {
    $(this._containerElement)
      .find(`#${this._elementIdPrefix}_disablingContainer *`)
      .prop('disabled', this.getIsNull());
  }

  dispose() {
    if (this._isNullCheckBoxElement) {
      let $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
      $isNullCheckBoxElement.unbind('change', this._onIsNullCheckBoxChangeHandler);
      $isNullCheckBoxElement = null;
    }

    this._isNullCheckBoxElement = null;
    this._onIsNullCheckBoxChangeHandler = null;

    super.dispose();
  }

  _onIsNullCheckBoxChangeHandler = null;
  getIsNull() {
    if (this._isNullCheckBoxElement) {
      return $(this._isNullCheckBoxElement).is(':checked');
    }
    return false;
  }

  _isNullCheckBoxElement = null;
}


import('../BackendArticleSearchBlock').then(() => {
  Quantumart.QP8.BackendArticleSearchBlock.BooleanFieldSearch = BooleanFieldSearch;
});
