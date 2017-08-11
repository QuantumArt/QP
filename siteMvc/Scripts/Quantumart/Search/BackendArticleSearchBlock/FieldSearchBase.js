Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID) {
  Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase.initializeBase(this);

  this._containerElement = containerElement;
  this._parentEntityId = parentEntityId;
  this._fieldID = fieldID;
  this._contentID =  contentID;
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
  _elementIdPrefix: "",

  initialize: function () {
    $(this._containerElement).append("Не реализовано");
  },

  get_searchQuery: function () {
    return null;
  },

  get_blockState: function () {
    return null;
  },

  set_blockState: function (state) {
  },

  restore_blockState: function (state) {
  },

  get_filterDetails: function(){
    return "";
  },

  onOpen: function () {
  },

  dispose: function () {
    this._containerElement = null;
  },

  _getText: function (entities, callback) {
    var count = 3;
    var length = entities.length;
    var ids = entities.slice(0, count);
    if (callback) {
      ids = ids.map(callback);
    }

    var result = ids.join("; ");
    if (length > count) {
      result += "; " + $l.SearchBlock.etcText + ", " + $l.SearchBlock.totalText + ": " + length;
    }

    return result;
  },

  _getIds: function (text) {
    var ids = text.replace(/\r?\n|\r|;/g, ",").split(",");
    ids = ids.map(function (e) { return parseInt(e); });
    ids = $.grep(ids, function (e) { return (e); });
    ids = _.uniq(ids);
    return ids;
  }
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase.registerClass("Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase", null, Sys.IDisposable);
Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState = function (searchType, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID, data) {
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

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState.registerClass("Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState");
