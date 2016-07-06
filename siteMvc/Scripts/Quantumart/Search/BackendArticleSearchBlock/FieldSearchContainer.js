//#region class BackendArticleSearchBlock.FieldSearchContainer
// === Класс контейнер для блока поиска по конкрентому полю ===
Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer = function (fieldSearchContainerElement, parentEntityId, fieldID, contentID, fieldName, fieldSearchType, fieldColumn, fieldGroup, referenceFieldId) {
  Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer.initializeBase(this);

  this._containerElement = fieldSearchContainerElement;

  this._fieldID = fieldID;
  this._contentID = contentID;
  this._fieldName = fieldName;
  this._fieldSearchType = $q.toInt(fieldSearchType, 0);
  this._fieldColumn = fieldColumn;
  this._fieldGroup = fieldGroup;
  this._referenceFieldId = referenceFieldId;
  this._parentEntityId = parentEntityId;

  this._elementIdPrefix = Quantumart.QP8.BackendSearchBlockBase.generateElementPrefix();

  this._onCloseButtonClickHandler = jQuery.proxy(this._onCloseButtonClick, this);
  this._onContentOpenWindowClickHandler = jQuery.proxy(this._onContentOpenWindowClick, this);
  this._onCloseWndClickHandler = jQuery.proxy(this._onCloseWndClick, this);
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer.prototype = {
  _containerElement: null, // dom-элемент контейнера
  _fieldID: null,
    _contentID: null,
  _fieldName: null,
  _fieldSearchType: 0,
  _fieldColumn: null,
  _fieldGroup: null,
    _referenceFieldId: null,
  _parentEntityId: 0, // идентификатор родительской сущности;

  _elementIdPrefix: "", //префикс идентификаторов dom-элементов

  _closeButtonElement: null, // dom-элемент кнопки для закрытия контейнера
    _closeAndApplyButton: null,
  _contentContainerElement: null, // dom-элемент контейнера который содержит специфичный контент для поиска по данному полю
  _contentWindowOpenLinkElement: null, // dom-элемент ссылки при клике по которой открываеться окно с фильтром
  $filterDetailsSpanElement: null, //span jquery-элемент для отображения параметров фильтра

  _fieldSearch: null, // объект класса который реализует сбор параметров поиска по данному полю
  _popupWindowComponent: null, // компонент окно

  _onCloseButtonClickHandler: null,
  _onContentOpenWindowClickHandler: null,

  initialize: function () {
    // создать заголовок контейнера
    var containerHeaderHtml = new $.telerik.stringBuilder();
    containerHeaderHtml
          .cat('<div class="fieldSearchContainerHeader group">')
              .cat('<div class="title"><a href="javascript:void(0)" class="open-link js">')
          .cat(this._fieldGroup ? this._fieldGroup + ": " + this._fieldName : this._fieldName)
          .cat('<span class="filter-details"></span>')
        .cat('</a></div>')
        .cat('<div ')
        .cat(' id="' + $q.htmlEncode(this._elementIdPrefix + "_CloseButton") + '"')
        .cat(' class="closeButton"')
        .cat(' title="' + $q.htmlEncode($l.SearchBlock.closeFieldSearchContainerButtonText) + '"')
        .cat('>')
        .cat('</div>')
          .cat("</div>");
    var $containerHeader = jQuery(containerHeaderHtml.string());
    var $closeButton = $containerHeader.find("#" + this._elementIdPrefix + "_CloseButton");
    $closeButton.bind("click", this._onCloseButtonClickHandler);
    this._closeButtonElement = $closeButton.get(0);

    var $windowOpenLink = jQuery(".open-link", $containerHeader);
    $windowOpenLink.click(this._onContentOpenWindowClickHandler);
    this._contentWindowOpenLinkElement = $windowOpenLink.get(0);

    this.$filterDetailsSpanElement = jQuery(".filter-details", $containerHeader);


    // добавить на страницу
    jQuery(this._containerElement)
          .append($containerHeader);

    $containerHeader = null;
    $closeButton = null;
    $contentContainer = null;
    $windowOpenLink = null;
  },

  get_ContainerElement: function () {
    return this._containerElement;
  },

  get_searchQuery: function () {
    if (this._fieldSearch)
      return this._fieldSearch.get_searchQuery();
    else
      return null;
  },

  get_blockState: function () {
    if (this._fieldSearch)
      return this._fieldSearch.get_blockState();
  },

  restore_blockState: function (state) {
    var result;
    this._createFieldSearchComponent(false);
    this._savedWindowState = state;
    if (this._fieldSearch) {
      this._fieldSearch.set_blockState(state);
      this._fieldSearch.initialize();
      result = this._fieldSearch.restore_blockState(state, false);
      this.$filterDetailsSpanElement.html(": " + this._fieldSearch.get_filterDetails());
      return result;
    }
  },

  _createFieldSearchComponent: function (isOpened) {
      var html = new $.telerik.stringBuilder()
      .cat('<form class="formLayout">')
      .cat('<div class="fieldSearchContainerContent"></div>')
            .cat('<div>')
      .cat('<input class="button closeAndApplyFilter" type="button" value="' + $l.SearchBlock.closeAndApplyWndButtonText + '">')
          .cat('<input class="button closeFilter" type="button" value="' + $l.SearchBlock.closeWndButtonText + '">')
      .cat('</form>')
      .string()

    var wndSize = this._getWindowSize();
    this._popupWindowComponent = $.telerik.window.create({
      title: $l.SearchBlock.filterSettings + this._fieldName,
      html: html,
      width: wndSize.w,
      height: wndSize.h,
      modal: true,
      resizable: false,
      draggable: false,
      visible: isOpened
    }).data("tWindow").center();
    wndSize = null;

    jQuery(this._popupWindowComponent.element).bind("close", jQuery.proxy(this._onPopupWindowClose, this));

    // получить контейнер для элементов поиска
    this._contentContainerElement = jQuery(".fieldSearchContainerContent", this._popupWindowComponent.element).get(0);

    jQuery("form", this._popupWindowComponent.element).submit(jQuery.proxy(this._onFilterFormSubmitted, this));

    var $closeWndButton = jQuery(".closeFilter", this._popupWindowComponent.element)
    $closeWndButton.click(this._onCloseWndClickHandler);
    $closeWndButton = null;

    var $closeAndApplyWndButton = jQuery(".closeAndApplyFilter", this._popupWindowComponent.element)
    this._closeAndApplyButton = $closeAndApplyWndButton.get(0);
    $closeAndApplyWndButton.click(jQuery.proxy(this._onCloseAndApplyWndClick, this));
    $closeAndApplyWndButton = null;

    this._createFieldSearch();
  },

  _createFieldSearch: function () {
    switch (this._fieldSearchType) {
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Identifier:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.IdentifierFieldSearch(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId);
        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Boolean:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.BooleanFieldSearch(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId);
        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.DateRange:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId, Quantumart.QP8.Enums.ArticleFieldSearchType.DateRange);
        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.M2MRelation:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId, Quantumart.QP8.Enums.ArticleFieldSearchType.M2MRelation);
        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.NumericRange:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId);
        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.O2MRelation:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId, Quantumart.QP8.Enums.ArticleFieldSearchType.O2MRelation);
        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.M2ORelation:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId, Quantumart.QP8.Enums.ArticleFieldSearchType.M2ORelation);
        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Text:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId);
        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.TimeRange:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId, Quantumart.QP8.Enums.ArticleFieldSearchType.TimeRange);
        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Classifier:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.ClassifierFieldSearch(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId, Quantumart.QP8.Enums.ArticleFieldSearchType.Classifier);
        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.StringEnum:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId);
        break;
      default:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase(this._contentContainerElement, this._parentEntityId, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldId);
        break;
    }
  },
  // создает объект класса который реализует сбор параметров поиска по данному полю
  _getWindowSize: function () {
    switch (this._fieldSearchType) {
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Identifier:
        return { w: 365, h: 150 };
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Text:
      case Quantumart.QP8.Enums.ArticleFieldSearchType.StringEnum:
        return { w: 410, h: 130 };
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Boolean:
        return { w: 350, h: 100 };
      case Quantumart.QP8.Enums.ArticleFieldSearchType.DateRange:
      case Quantumart.QP8.Enums.ArticleFieldSearchType.NumericRange:
      case Quantumart.QP8.Enums.ArticleFieldSearchType.TimeRange:
        return { w: 350, h: 200 };
      case Quantumart.QP8.Enums.ArticleFieldSearchType.O2MRelation:
      case Quantumart.QP8.Enums.ArticleFieldSearchType.M2MRelation:
      case Quantumart.QP8.Enums.ArticleFieldSearchType.M2ORelation:
        return { w: 510, h: 290 };
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Classifier:
        return { w: 400, h: 90 };
      default:
        return { w: 350, h: 125 };
    }
  },

  _onCloseButtonClick: function () {
    this.notify(EVENT_TYPE_CONRETE_FIELD_SEARCH_CONTAINER_CLOSE, { "fieldID": this._fieldID })
  },

  _onCloseWndClick: function () {
    this.$filterDetailsSpanElement.html(": " + this._fieldSearch.get_filterDetails());
    this._savedWindowState = this._fieldSearch.get_blockState();
    this._restoreWindowStateOnClosing = false;
    this._popupWindowComponent.close();
  },

  _onCloseAndApplyWndClick: function () {
    this._onCloseWndClick();
    $(this._containerElement).closest('form').trigger("submit");
  },

  _onFilterFormSubmitted: function (e) {
    e.preventDefault();
    $(this._closeAndApplyButton).trigger("click");
    return false;
  },

  _onContentOpenWindowClick: function () {
    if (this._popupWindowComponent) {
      this._popupWindowComponent.open();
      this._fieldSearch.onOpen();
    } else {
      this._createFieldSearchComponent(true);
      this._fieldSearch.initialize();
    }

    this._savedWindowState = this._fieldSearch.get_blockState();
    this._restoreWindowStateOnClosing = true;
  },

  _onPopupWindowClose: function () {
    if (this._restoreWindowStateOnClosing) {
      this._fieldSearch.restore_blockState(this._savedWindowState.data, true);
    }
  },

  dispose: function () {
    Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer.callBaseMethod(this, "dispose");

    this.$filterDetailsSpanElement = null;
    if (this._fieldSearch) {
      this._fieldSearch.dispose();
    }

    this._fieldSearch = null;

    $(this._closeButtonElement).unbind("click");
    $(this._contentWindowOpenLinkElement).unbind("click");

    if (this._popupWindowComponent) {
      $(this._popupWindowComponent.element).unbind("close");
      $(".closeFilter", this._popupWindowComponent.element).unbind("click");
      $(".closeAndApplyFilter", this._popupWindowComponent.element).unbind("click");
      $("form", this._popupWindowComponent.element).unbind("submit");
      $c.destroyPopupWindow(this._popupWindowComponent);
      this._popupWindowComponent = null;
    }

    this._contentContainerElement = null;
    this._closeButtonElement = null;
    this._closeAndApplyButton = null;
    this._containerElement = null;
    this._contentWindowOpenLinkElement = null;
    this._onCloseButtonClickHandler = null;
    this._onContentOpenWindowClickHandler = null;
    this._onCloseWndClickHandler = null;
  }
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer.registerClass("Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer", Quantumart.QP8.Observable);
//#endregion
