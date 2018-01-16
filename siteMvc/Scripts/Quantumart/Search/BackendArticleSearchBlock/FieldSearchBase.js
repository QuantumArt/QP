import { BackendSearchBlockBase } from '../BackendSearchBlockBase';
import { distinct } from '../../Utils/Filter';

export class FieldSearchBase {
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
    this._containerElement = containerElement;
    this._parentEntityId = parentEntityId;
    this._fieldID = fieldID;
    this._contentID = contentID;
    this._fieldColumn = fieldColumn;
    this._fieldName = fieldName;
    this._fieldGroup = fieldGroup;
    this._referenceFieldID = referenceFieldID;
    this._elementIdPrefix = BackendSearchBlockBase.generateElementPrefix();
  }

  _containerElement = null;
  _parentEntityId = 0;
  _fieldID = null;
  _contentID = null;
  _fieldColumn = null;
  _fieldName = null;
  _fieldGroup = null;
  _referenceFieldID = null;
  _elementIdPrefix = '';

  initialize() {
    $(this._containerElement).append('Не реализовано');
  }

  /**
   * @virtual
   * @returns {object}
   */
  getSearchQuery() {
    return null;
  }

  /**
   * @virtual
   * @returns {FieldSearchState}
   */
  getBlockState() {
    return null;
  }

  /**
   * @virtual
   * @param {object} _state
   */
  setBlockState(_state) {
    // default implementation
  }

  /**
   * @virtual
   * @param {object} _state
   */
  restoreBlockState(_state) {
    // default implementation
  }

  /**
   * @virtual
   * @returns {string}
   */
  getFilterDetails() {
    return '';
  }

  /** @virtual */
  onOpen() {
    // default implementation
  }

  dispose() {
    this._containerElement = null;
  }

  _getText(entities, callback) {
    const count = 3;
    const { length: len } = entities;
    let ids = entities.slice(0, count);
    if (callback) {
      ids = ids.map(callback);
    }

    let result = ids.join('; ');
    if (len > count) {
      result += `; ${$l.SearchBlock.etcText}, ${$l.SearchBlock.totalText}: ${len}`;
    }

    return result;
  }

  _getIds(/** @type {string} */ text) {
    return text
      .replace(/\r?\n|\r|;/g, ',')
      .split(',')
      .map(str => parseInt(str, 10))
      .filter(num => num)
      .filter(distinct());
  }
}


export class FieldSearchState {
  // eslint-disable-next-line max-params
  constructor(
    searchType,
    fieldID,
    contentID,
    fieldColumn,
    fieldName,
    fieldGroup,
    referenceFieldID,
    data
  ) {
    this.searchType = searchType;
    this.fieldID = fieldID;
    this.contentID = contentID;
    this.fieldColumn = fieldColumn;
    this.fieldName = fieldName;
    this.fieldGroup = fieldGroup;
    this.referenceFieldID = referenceFieldID;
    this.data = data;
  }

  searchType = null;
  fieldID = null;
  contentID = null;
  fieldColumn = null;
  fieldName = null;
  fieldGroup = null;
  referenceFieldID = null;
  data = null;
}


import('../BackendArticleSearchBlock').then(() => {
  Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase = FieldSearchBase;
  Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState = FieldSearchState;
});
