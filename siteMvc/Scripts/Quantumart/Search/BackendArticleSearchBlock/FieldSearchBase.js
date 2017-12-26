// eslint-disable-next-line max-params
Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase = function (
  containerElement,
  parentEntityId,
  fieldID,
  contentID,
  fieldColumn,
  fieldName,
  fieldGroup,
  referenceFieldID
) {
  Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase.initializeBase(this);
  this._containerElement = containerElement;
  this._parentEntityId = parentEntityId;
  this._fieldID = fieldID;
  this._contentID = contentID;
  this._fieldColumn = fieldColumn;
  this._fieldName = fieldName;
  this._fieldGroup = fieldGroup;
  this._referenceFieldID = referenceFieldID;
  this._elementIdPrefix = Quantumart.QP8.BackendSearchBlockBase.generateElementPrefix();
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase.prototype = {
  _containerElement: null,
  _parentEntityId: 0,
  _fieldID: null,
  _contentID: null,
  _fieldColumn: null,
  _fieldName: null,
  _fieldGroup: null,
  _referenceFieldID: null,
  _elementIdPrefix: '',

  initialize() {
    $(this._containerElement).append('Не реализовано');
  },

  /**
   * @virtual
   * @returns {object}
   */
  getSearchQuery() {
    return null;
  },

  /**
   * @virtual
   // @ts-ignore
   * @returns {Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState}
   */
  getBlockState() {
    return null;
  },

  /**
   * @virtual
   * @param {object} _state
   */
  setBlockState(_state) {
    // default implementation
  },

  /**
   * @virtual
   * @param {object} _state
   */
  restoreBlockState(_state) {
    // default implementation
  },

  /**
   * @virtual
   * @returns {string}
   */
  getFilterDetails() {
    return '';
  },

  /** @virtual */
  onOpen() {
    // default implementation
  },

  dispose() {
    this._containerElement = null;
  },

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
  },

  _getIds(/** @type {string} */ text) {
    const ids = text
      .replace(/\r?\n|\r|;/g, ',')
      .split(',')
      .map(str => parseInt(str, 10))
      .filter(num => num);

    return Array.distinct(ids);
  }
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase.registerClass(
  'Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase',
  null,
  Sys.IDisposable
);

// eslint-disable-next-line max-params
Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState = function (
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
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState.prototype = {
  searchType: null,
  fieldID: null,
  contentID: null,
  fieldColumn: null,
  fieldName: null,
  fieldGroup: null,
  referenceFieldID: null,
  data: null
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState.registerClass(
  'Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState'
);
