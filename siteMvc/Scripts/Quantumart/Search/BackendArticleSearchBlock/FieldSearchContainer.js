Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer = function (
  fieldSearchContainerElement,
  parentEntityId,
  fieldID,
  contentID,
  fieldName,
  fieldSearchType,
  fieldColumn,
  fieldGroup,
  referenceFieldId
) {
  Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer.initializeBase(this);

  this._containerElement = fieldSearchContainerElement;
  this._fieldID = fieldID;
  this._contentID = contentID;
  this._fieldName = fieldName;
  this._fieldSearchType = +fieldSearchType || 0;
  this._fieldColumn = fieldColumn;
  this._fieldGroup = fieldGroup;
  this._referenceFieldId = referenceFieldId;
  this._parentEntityId = parentEntityId;
  this._elementIdPrefix = Quantumart.QP8.BackendSearchBlockBase.generateElementPrefix();
  this._onCloseButtonClickHandler = $.proxy(this._onCloseButtonClick, this);
  this._onContentOpenWindowClickHandler = $.proxy(this._onContentOpenWindowClick, this);
  this._onCloseWndClickHandler = $.proxy(this._onCloseWndClick, this);
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer.prototype = {
  _containerElement: null,
  _fieldID: null,
  _contentID: null,
  _fieldName: null,
  _fieldSearchType: 0,
  _fieldColumn: null,
  _fieldGroup: null,
  _referenceFieldId: null,
  _parentEntityId: 0,

  _elementIdPrefix: '',

  _closeButtonElement: null,
  _closeAndApplyButton: null,
  _contentContainerElement: null,
  _contentWindowOpenLinkElement: null,
  $filterDetailsSpanElement: null,

  _fieldSearch: null,
  _popupWindowComponent: null,

  _onCloseButtonClickHandler: null,
  _onContentOpenWindowClickHandler: null,

  initialize() {
    // создать заголовок контейнера
    const containerHeaderHtml = new $.telerik.stringBuilder();
    containerHeaderHtml
      .cat('<div class="fieldSearchContainerHeader group">')
      .cat('<div class="title"><a href="javascript:void(0)" class="open-link js">')
      .cat(this._fieldGroup ? `${this._fieldGroup}: ${this._fieldName}` : this._fieldName)
      .cat('<span class="filter-details"></span>')
      .cat('</a></div>')
      .cat('<div ')
      .cat(` id="${$q.htmlEncode(`${this._elementIdPrefix}_CloseButton`)}"`)
      .cat(' class="closeButton"')
      .cat(` title="${$q.htmlEncode($l.SearchBlock.closeFieldSearchContainerButtonText)}"`)
      .cat('>')
      .cat('</div>')
      .cat('</div>');
    const $containerHeader = $(containerHeaderHtml.string());
    const $closeButton = $containerHeader.find(`#${this._elementIdPrefix}_CloseButton`);
    $closeButton.bind('click', this._onCloseButtonClickHandler);
    this._closeButtonElement = $closeButton.get(0);

    const $windowOpenLink = $('.open-link', $containerHeader);
    $windowOpenLink.click(this._onContentOpenWindowClickHandler);
    this._contentWindowOpenLinkElement = $windowOpenLink.get(0);
    this.$filterDetailsSpanElement = $('.filter-details', $containerHeader);

    $(this._containerElement).append($containerHeader);
  },

  // eslint-disable-next-line camelcase
  get_ContainerElement() {
    return this._containerElement;
  },

  // eslint-disable-next-line camelcase
  getSearchQuery() {
    if (this._fieldSearch) {
      return this._fieldSearch.getSearchQuery();
    }

    return null;
  },

  // eslint-disable-next-line camelcase
  getBlockState() {
    if (this._fieldSearch) {
      return this._fieldSearch.getBlockState();
    }

    return undefined;
  },

  // eslint-disable-next-line camelcase
  restoreBlockState(state) {
    let result;
    this._createFieldSearchComponent(false);
    this._savedWindowState = state;
    if (this._fieldSearch) {
      this._fieldSearch.setBlockState(state);
      this._fieldSearch.initialize();
      result = this._fieldSearch.restoreBlockState(state, false);
      this.$filterDetailsSpanElement.html(`: ${this._fieldSearch.getFilterDetails()}`);
      return result;
    }

    return undefined;
  },

  _createFieldSearchComponent(isOpened) {
    const inputCloseAndApplyHtml = `<input
      class="button closeAndApplyFilter" type="button" value="${$l.SearchBlock.closeAndApplyWndButtonText}">`;

    const html = new $.telerik.stringBuilder()
      .cat('<form class="formLayout">')
      .cat('<div class="fieldSearchContainerContent"></div>')
      .cat('<div>')
      .cat(inputCloseAndApplyHtml)
      .cat(`<input class="button closeFilter" type="button" value="${$l.SearchBlock.closeWndButtonText}">`)
      .cat('</form>')
      .string();

    let wndSize = this._getWindowSize();
    this._popupWindowComponent = $.telerik.window.create({
      title: $l.SearchBlock.filterSettings + this._fieldName,
      html,
      width: wndSize.width,
      height: wndSize.height,
      modal: true,
      resizable: false,
      draggable: true,
      visible: isOpened
    }).data('tWindow').center();
    wndSize = null;

    $(this._popupWindowComponent.element).bind('close', $.proxy(this._onPopupWindowClose, this));
    this._contentContainerElement = $('.fieldSearchContainerContent', this._popupWindowComponent.element).get(0);
    $('form', this._popupWindowComponent.element).submit($.proxy(this._onFilterFormSubmitted, this));

    let $closeWndButton = $('.closeFilter', this._popupWindowComponent.element);
    $closeWndButton.click(this._onCloseWndClickHandler);
    $closeWndButton = null;

    let $closeAndApplyWndButton = $('.closeAndApplyFilter', this._popupWindowComponent.element);
    this._closeAndApplyButton = $closeAndApplyWndButton.get(0);
    $closeAndApplyWndButton.click($.proxy(this._onCloseAndApplyWndClick, this));
    $closeAndApplyWndButton = null;

    this._createFieldSearch();
  },

  _createFieldSearch() {
    switch (this._fieldSearchType) {
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Identifier:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.IdentifierFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId
        );

        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Boolean:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.BooleanFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId
        );

        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.DateRange:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId,
          Quantumart.QP8.Enums.ArticleFieldSearchType.DateRange
        );

        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.M2MRelation:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId,
          Quantumart.QP8.Enums.ArticleFieldSearchType.M2MRelation
        );

        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.NumericRange:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId
        );

        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.O2MRelation:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId,
          Quantumart.QP8.Enums.ArticleFieldSearchType.O2MRelation
        );

        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.M2ORelation:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId,
          Quantumart.QP8.Enums.ArticleFieldSearchType.M2ORelation
        );

        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Text:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId
        );

        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.TimeRange:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId,
          Quantumart.QP8.Enums.ArticleFieldSearchType.TimeRange
        );

        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.DateTimeRange:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId,
          Quantumart.QP8.Enums.ArticleFieldSearchType.DateTimeRange
        );

        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Classifier:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.ClassifierFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId,
          Quantumart.QP8.Enums.ArticleFieldSearchType.Classifier
        );

        break;
      case Quantumart.QP8.Enums.ArticleFieldSearchType.StringEnum:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId
        );

        break;
      default:
        this._fieldSearch = new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase(
          this._contentContainerElement,
          this._parentEntityId,
          this._fieldID,
          this._contentID,
          this._fieldColumn,
          this._fieldName,
          this._fieldGroup,
          this._referenceFieldId
        );

        break;
    }
  },

  // создает объект класса который реализует сбор параметров поиска по данному полю
  _getWindowSize() {
    switch (this._fieldSearchType) {
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Identifier:
        return { width: 365, height: 150 };
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Text:
        return { width: 410, height: 150 };
      case Quantumart.QP8.Enums.ArticleFieldSearchType.StringEnum:
        return { width: 410, height: 130 };
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Boolean:
        return { width: 350, height: 100 };
      case Quantumart.QP8.Enums.ArticleFieldSearchType.DateRange:
      case Quantumart.QP8.Enums.ArticleFieldSearchType.NumericRange:
      case Quantumart.QP8.Enums.ArticleFieldSearchType.TimeRange:
      case Quantumart.QP8.Enums.ArticleFieldSearchType.DateTimeRange:
        return { width: 350, height: 200 };
      case Quantumart.QP8.Enums.ArticleFieldSearchType.O2MRelation:
      case Quantumart.QP8.Enums.ArticleFieldSearchType.M2MRelation:
      case Quantumart.QP8.Enums.ArticleFieldSearchType.M2ORelation:
        return { width: 510, height: 290 };
      case Quantumart.QP8.Enums.ArticleFieldSearchType.Classifier:
        return { width: 400, height: 90 };
      default:
        return { width: 350, height: 125 };
    }
  },

  _onCloseButtonClick() {
    this.notify(window.EVENT_TYPE_CONRETE_FIELD_SEARCH_CONTAINER_CLOSE, { fieldID: this._fieldID });
  },

  _onCloseWndClick() {
    this.$filterDetailsSpanElement.html(`: ${this._fieldSearch.getFilterDetails()}`);
    this._savedWindowState = this._fieldSearch.getBlockState();
    this._restoreWindowStateOnClosing = false;
    this._popupWindowComponent.close();
  },

  _onCloseAndApplyWndClick() {
    this._onCloseWndClick();
    $(this._containerElement).closest('form').trigger('submit');
  },

  _onFilterFormSubmitted(e) {
    e.preventDefault();
    $(this._closeAndApplyButton).trigger('click');
    return false;
  },

  _onContentOpenWindowClick() {
    if (this._popupWindowComponent) {
      this._popupWindowComponent.open();
      this._fieldSearch.onOpen();
    } else {
      this._createFieldSearchComponent(true);
      this._fieldSearch.initialize();
    }

    this._savedWindowState = this._fieldSearch.getBlockState();
    this._restoreWindowStateOnClosing = true;
  },

  _onPopupWindowClose() {
    if (this._restoreWindowStateOnClosing) {
      this._fieldSearch.restoreBlockState(this._savedWindowState.data, true);
    }
  },

  dispose() {
    Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer.callBaseMethod(this, 'dispose');

    this.$filterDetailsSpanElement = null;
    if (this._fieldSearch) {
      this._fieldSearch.dispose();
    }

    this._fieldSearch = null;

    $(this._closeButtonElement).unbind('click');
    $(this._contentWindowOpenLinkElement).unbind('click');

    if (this._popupWindowComponent) {
      $(this._popupWindowComponent.element).unbind('close');
      $('.closeFilter', this._popupWindowComponent.element).unbind('click');
      $('.closeAndApplyFilter', this._popupWindowComponent.element).unbind('click');
      $('form', this._popupWindowComponent.element).unbind('submit');
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

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer.registerClass(
  'Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer',
  Quantumart.QP8.Observable
);
