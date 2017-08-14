Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBlock = function (fieldSearchBlockElement, parentEntityId) {
  this._fieldSearchBlockElement = fieldSearchBlockElement;
  this._parentEntityId = parentEntityId;
  this._elementIdPrefix = Quantumart.QP8.BackendSearchBlockBase.generateElementPrefix();

  this._fieldSearchContainerList = {};

  this._onAddFieldClickHandler = jQuery.proxy(this._onAddFieldClick, this);
  this._onFieldSearchContainerCloseHandler = jQuery.proxy(this._onFieldSearchContainerClose, this);
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBlock.prototype = {
  _fieldSearchBlockElement: null,
  _fieldSearchListElement: null,
  _fieldsComboElement: null,
  _addFieldSearchButtonElement: null,

  _parentEntityId: 0,
  _elementIdPrefix: "",

  _fieldSearchContainerList: null,

  _onAddFieldClickHandler: null,
  _onFieldSearchContainerCloseHandler: null,

  initialize: function () {
    var serverContent;
    $q.getJsonFromUrl(
      "GET",
      window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK + "FieldSearchBlock",
      {
        parentEntityId: this._parentEntityId,
        elementIdPrefix: this._elementIdPrefix
      },
      false,
      false,
      function (data, textStatus, jqXHR) {
        if (data.success) {
 serverContent = data.view;
} else {
 $q.alertFail(data.message);
}
      },
      function (jqXHR, textStatus, errorThrown) {
        serverContent = null;
        $q.processGenericAjaxError(jqXHR);
      }
    );
    if (!$q.isNullOrWhiteSpace(serverContent)) {
      var $fieldSearchBlockElement = jQuery(this._fieldSearchBlockElement);
      $fieldSearchBlockElement.html(serverContent);

      this._fieldSearchListElement = $fieldSearchBlockElement.find("#" + this._elementIdPrefix + "_FieldSearchList").get(0);
      this._fieldsComboElement = $fieldSearchBlockElement.find("#" + this._elementIdPrefix + "_FieldsCombo").get(0);
      this._addFieldSearchButtonElement = $fieldSearchBlockElement.find("#" + this._elementIdPrefix + "_AddFieldSearchButton").get(0);
      this._attachFieldSearchBlockEventHandlers();

      $fieldSearchBlockElement = null;
    }
  },

  get_searchQuery: function () {
    var result = [];
    for (var fieldID in this._fieldSearchContainerList) {
      if (fieldID && this._fieldSearchContainerList[fieldID]) {
        var fscsq = this._fieldSearchContainerList[fieldID].get_searchQuery();
        if (fscsq) {
 result.push(fscsq);
}
      }
    }

    if (result.length > 0) {
 return result;
}
 return null;

  },

  get_blockState: function () {
    var r = jQuery.grep(
      jQuery.map(this._fieldSearchContainerList, function (fsc) {
        return fsc ? fsc.get_blockState() : null;
      }),
      function (fsc) {
 return fsc;
}
    );
    if (r && r.length > 0) {
      return r;
    }
  },

  restore_blockState: function (state) {
    if (state) {
      var that = this;
      var $options = jQuery("option", this._fieldsComboElement);
      jQuery.each(state, function (index, s) {
        if (s.fieldID && !that._fieldSearchContainerList[s.fieldID]) {
          var is = $options.is(function () {
            var $option = jQuery(this);
            return s.fieldID == $option.data("field_id")
                 && s.fieldName == $option.text()
                 && s.searchType == $option.data("search_type")
                 && s.fieldColumn == $option.data("field_column");
          });
          if (is) {
              var newContainer = that._createFieldSearchContainerInner(s.fieldID, s.contentID, s.searchType, s.fieldName, s.fieldColumn, s.fieldGroup, s.referenceFieldID);
            if (s.data) {
              newContainer.restore_blockState(s.data);
            }
          }
        }
      });
    }
  },

  clear: function () {
    if (this._fieldsComboElement) {
 jQuery(this._fieldsComboElement).find("option:first").prop("selected", true);
}

    this._destroyAllFieldSearchContainers();
  },

  _attachFieldSearchBlockEventHandlers: function () {
    if (this._fieldsComboElement) {
      var $combo = jQuery(this._fieldsComboElement);
      $combo.bind("change", this._onAddFieldClickHandler);
      $combo = null;
    }
  },

  _detachFieldSearchBlockEventHandlers: function () {
    if (this._fieldsComboElement) {
      var $combo = jQuery(this._fieldsComboElement);
      $combo.unbind("change", this._onAddFieldClickHandler);
      $combo = null;
    }
  },

  _createFieldSearchContainer: function () {
    var $combo = jQuery(this._fieldsComboElement);
    var $selectedField = $combo.find("option:selected");

    if ($selectedField) {
      var fieldID = $selectedField.data("field_id");

      if (fieldID && !this._fieldSearchContainerList[fieldID]) {
        var contentID = $selectedField.data("content_id");
        var fieldName = $selectedField.text();
        var fieldSearchType = $selectedField.data("search_type");
        var fieldColumn = $selectedField.data("field_column");
        var fieldGroup = $selectedField.data("field_group");
        var referenceFieldID = $selectedField.data("reference_field_id");
        this._createFieldSearchContainerInner(fieldID, contentID, fieldSearchType, fieldName, fieldColumn, fieldGroup, referenceFieldID);
      }

      $combo.val('');
    }
  },

  _createFieldSearchContainerInner: function (fieldID, contentID, fieldSearchType, fieldName, fieldColumn, fieldGroup, referenceFieldID) {
    var $fieldSearchContainerElement = jQuery("<div />", { class: "fieldSearchContainer" });
    jQuery(this._fieldSearchListElement).append($fieldSearchContainerElement);
    var newFieldSearchContainer = new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer($fieldSearchContainerElement.get(0), this._parentEntityId, fieldID, contentID, fieldName, fieldSearchType, fieldColumn, fieldGroup, referenceFieldID);
    newFieldSearchContainer.initialize();
    newFieldSearchContainer.attachObserver(window.EVENT_TYPE_CONRETE_FIELD_SEARCH_CONTAINER_CLOSE, this._onFieldSearchContainerCloseHandler);
    this._fieldSearchContainerList[fieldID] = newFieldSearchContainer;
    $fieldSearchContainerElement = null;
    return newFieldSearchContainer;
  },

  _destroyFieldSearchContainer: function (fieldID) {
    if (this._fieldSearchContainerList[fieldID]) {
      var fieldSearchContainer = this._fieldSearchContainerList[fieldID];
      var $fsContainer = jQuery(fieldSearchContainer.get_ContainerElement());
      fieldSearchContainer.detachObserver(window.EVENT_TYPE_CONRETE_FIELD_SEARCH_CONTAINER_CLOSE, this._onFieldSearchContainerCloseHandler);
      fieldSearchContainer.dispose();
      $q.removeProperty(this._fieldSearchContainerList, fieldID);
      $fsContainer.empty().remove();
      $fsContainer = null;
      fieldSearchContainer = null;
    }
  },

  _destroyAllFieldSearchContainers: function () {
    for (var fieldID in this._fieldSearchContainerList) {
 this._destroyFieldSearchContainer(fieldID);
}
  },

  _onAddFieldClick: function () {
    this._createFieldSearchContainer();
  },

  _onFieldSearchContainerClose: function (eventType, sender, args) {
    this._destroyFieldSearchContainer(args.fieldID);
  },

  dispose: function () {
    this._detachFieldSearchBlockEventHandlers();
    this._destroyAllFieldSearchContainers();
    this._fieldSearchContainerList = null;
    this._fieldSearchBlockElement = null;
    this._fieldSearchListElement = null;
    this._fieldsComboElement = null;
    this._addFieldSearchButtonElement = null;
    this._onAddFieldClickHandler = null;
    this._onFieldSearchContainerCloseHandler = null;
  }
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBlock.registerClass("Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBlock", null, Sys.IDisposable);
