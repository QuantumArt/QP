// ****************************************************************************
// *** Компонент "Блок поиска"                        ***
// ****************************************************************************

// #region event types of search block
// === Типы событий блока поиска ===
var EVENT_TYPE_SEARCH_BLOCK_FIND_START = "OnSearchBlockFindStart";
var EVENT_TYPE_CONTEXT_BLOCK_FIND_START = "OnContextBlockFindStart";
var EVENT_TYPE_SEARCH_BLOCK_RESET_START = "OnSearchBlockResetStart";
var EVENT_TYPE_SEARCH_BLOCK_RESIZED = "OnSearchBlockResized";
// #endregion

// #region event types of conrete field
// === Типы событий контейнера для блока поиска по конкрентому полю ===
var EVENT_TYPE_CONRETE_FIELD_SEARCH_CONTAINER_CLOSE = "OnFieldSearchContainerClose";
// #endregion

// #region class BackendSearchBlockBase
// === Класс "Блок поиска" ===
Quantumart.QP8.BackendSearchBlockBase = function (searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options) {
  Quantumart.QP8.BackendSearchBlockBase.initializeBase(this);

  this._searchBlockGroupCode = searchBlockGroupCode;
  this._searchBlockElementId = searchBlockElementId;
  this._entityTypeCode = entityTypeCode;
  this._parentEntityId = parentEntityId;
  if ($q.isObject(options)) {
    if (options.searchBlockContainerElementId) {
      this._searchBlockContainerElementId = options.searchBlockContainerElementId;
    }

    if (options.tabId) {
      this._tabId = options.tabId;
      this._hostId = options.tabId;
    }

    if (options.popupWindowId) {
      this._popupWindowId = options.popupWindowId;
      this._hostId = options.popupWindowId;
    }

    if (options.minSearchBlockHeight) {
      this._minSearchBlockHeight = options.minSearchBlockHeight;
    }

    if (options.maxSearchBlockHeight) {
      this._maxSearchBlockHeight = options.maxSearchBlockHeight;
    }

    if (options.hideButtons) {
      this._hideButtons = options.hideButtons;
    }

    if (options.actionCode) {
      this._actionCode = options.actionCode;
    }

    if (options.searchBlockState) {
      this._searchBlockState = options.searchBlockState;
    }
  }

  this._onSearchBlockResizedHandler = jQuery.proxy(this._onSearchBlockResized, this);
  this._onFindButtonClickHandler = jQuery.proxy(this._onFindButtonClick, this);
  this._onResetButtonClickHandler = jQuery.proxy(this._onResetButtonClick, this);
  this._onSearchFormSubmittedHandler = jQuery.proxy(this._onSearchFormSubmitted, this);
};

Quantumart.QP8.BackendSearchBlockBase.prototype = {
  _searchBlockGroupCode: "", // код группы, в которую входит блок поиска
  _searchBlockElementId: "", // клиентский идентификатор блока поиска
  _searchBlockElement: null, // DOM-элемент, образующий блок поиска
  _searchBlockContainerElementId: "", // клиентский идентификатор контейнера, в котором располагается блок поиска
  _tabId: "", // ID таба
  _popupWindowId: "", // ID всплывающего окна
  _hostId: "", // ID хоста
  _actionCode: null,
  _concreteSearchBlockElement: null, // DOM-элемент, образующий блок конкретного поиска
  _buttonsWrapperElement: null, // DOM-элемент, контейнер кнопок
  _findButtonElement: null, // DOM-элемент, образующий кнопку "Найти"
  _resetButtonElement: null, // DOM-элемент, образующий кнопку "Очистить"
  _entityTypeCode: "", // код типа сущности
  _parentEntityId: 0, // идентификатор родительской сущности;
  _minSearchBlockHeight: 180, // минимальная высота блока поиска
  _maxSearchBlockHeight: 500, // максимальная высота блока поиска
  _lastSearchBlockHeight: 500, // последнее значение высоты блока поиска
  _isVisible: false, // признак того, что блок поиск находится в режиме отображения
  _isRendered: false, // был ли выполнен рендеринг
  _verticalResizerComponent: null, // компонент, реализующий вертикальное масштабирование
  _hideButtons: false, // прятать ли стандартные кнопки поиска
  _searchBlockState: null, // сохраняемое состояние блока поиска

  _onSearchBlockResizedHandler: null,
  _onFindButtonClickHandler: null,
  _onResetButtonClickHandler: null,

  get_searchBlockGroupCode: function () {
    return this._searchBlockGroupCode;
  },

  set_searchBlockGroupCode: function (value) {
    this._searchBlockGroupCode = value;
  },

  get_searchBlockElementId: function () {
    return this._searchBlockElementId;
  },

  set_searchBlockElementId: function (value) {
    this._searchBlockElementId = value;
  },

  get_searchBlockElement: function () {
    return this._searchBlockElement;
  },

  get_searchBlockContainerElementId: function () {
    return this._searchBlockContainerElementId;
  },

  set_searchBlockContainerElementId: function (value) {
    this._searchBlockContainerElementId = value;
  },

  get_entityTypeCode: function () {
    return this._entityTypeCode;
  },

  set_entityTypeCode: function (value) {
    this._entityTypeCode = value;
  },

  get_parentEntityId: function () {
    return this._parentEntityId;
  },

  set_parentEntityId: function (value) {
    this._parentEntityId = value;
  },

  get_minSearchBlockHeight: function () {
    return this._minSearchBlockHeight;
  },

  set_minSearchBlockHeight: function (value) {
    this._minSearchBlockHeight = value;
  },

  get_maxSearchBlockHeight: function () {
    return this._maxSearchBlockHeight;
  },

  set_maxSearchBlockHeight: function (value) {
    this._maxSearchBlockHeight = value;
  },

  get_isRendered: function () {
    return this._isRendered;
  },

  set_isRendered: function (value) {
    this._isRendered = value;
  },

  initialize: function () {
    var $searchBlock = $('#' + this._searchBlockElementId);
    var searchBlockExist = !$q.isNullOrEmpty($searchBlock);
    var searchFormExist = !$q.isNullOrEmpty($searchBlock.find('form'));

    if (!searchBlockExist) {
      $searchBlock = $('<div />', { id: this._searchBlockElementId, class: 'searchBlock', css: { display: 'none'} });
    }

    if (!searchFormExist) {
      var $searchForm = $('<form />', { class: 'formLayout' });
      $searchBlock.append($searchForm);

      var $concreteSearchBlock = $('<div />');
      $searchForm.append($concreteSearchBlock);

      var $buttonsWrapper = $('<div />');
      if (this._hideButtons) {
        $buttonsWrapper.hide();
      }

      $searchForm.append($buttonsWrapper);

      var $findButton = $('<input />', { type: 'button', value: $l.SearchBlock.findButtonText, class: 'button' });
      var $resetButton = $('<input />', { type: 'button', value: $l.SearchBlock.resetButtonText, class: 'button' });

      $buttonsWrapper.append($findButton);
      $buttonsWrapper.append('&nbsp;');
      $buttonsWrapper.append($resetButton);

      this._concreteSearchBlockElement = $concreteSearchBlock.get(0);
      this._findButtonElement = $findButton.get(0);
      this._resetButtonElement = $resetButton.get(0);
      this._searchForm = $searchForm.get(0);
      this._buttonsWrapperElement = $buttonsWrapper.get(0);
    }

    if (!searchBlockExist) {
      if (!$q.isNullOrWhiteSpace(this._searchBlockContainerElementId)) {
        $('#' + this._searchBlockContainerElementId).append($searchBlock);
      } else {
        $('body:first').append($searchBlock);
      }
    }

    $searchBlock.verticalResizer({ bottomHandleCssClassName: 'searchBottomHandle', minPanelHeight: this._minSearchBlockHeight, maxPanelHeight: this._maxSearchBlockHeight });
    this._verticalResizerComponent = $searchBlock.data('vertical_resizer');

    this._searchBlockElement = $searchBlock.get(0);
    this._lastSearchBlockHeight = this._minSearchBlockHeight;

    if (!searchFormExist) {
      this._attachSearchBlockEventHandlers();
    }
  },

  _attachSearchBlockEventHandlers: function () {
    $(this._searchBlockElement).bind('resize', this._onSearchBlockResizedHandler);
    $(this._findButtonElement).bind('click', this._onFindButtonClickHandler);
    $(this._resetButtonElement).bind('click', this._onResetButtonClickHandler);
    $(this._searchForm).bind('submit', this._onSearchFormSubmittedHandler);
  },

  _detachSearchBlockEventHandlers: function () {
    $(this._searchBlockElement).unbind('resize', this._onSearchBlockResizedHandler);
    $(this._findButtonElement).unbind('click', this._onFindButtonClickHandler);
    $(this._resetButtonElement).unbind('click', this._onResetButtonClickHandler);
    $(this._searchForm).unbind('submit');
  },

  showSearchBlock: function () {
    this._isVisible = true;
    var $searchBlock = $(this._searchBlockElement);
    if ($searchBlock.is(':hidden')) {
      var verticalResizerComponent = this._verticalResizerComponent;
      if (verticalResizerComponent) {
        verticalResizerComponent.showPanelWrapper();
        verticalResizerComponent.showBottomHandle();
      }

      $searchBlock.show().height(this._lastSearchBlockHeight).trigger('resize');
    }
  },

  hideSearchBlock: function () {
    this._isVisible = false;
    var $searchBlock = $(this._searchBlockElement);
    if ($searchBlock.is(':visible')) {
      var verticalResizerComponent = this._verticalResizerComponent;
      if (verticalResizerComponent) {
        verticalResizerComponent.hideBottomHandle();
      }

      this._lastSearchBlockHeight = parseInt($searchBlock.height(), 10);
      $searchBlock.height(0).hide().trigger('resize');

      if (verticalResizerComponent) {
        verticalResizerComponent.hidePanelWrapper();
      }
    }
  },

  renderSearchBlock: function () {
    this.set_isRendered(true);
  },

  _onSearchBlockResized: function () {
    var $searchBlock = jQuery(this._searchBlockElement);
    var $bottomHandle = null;
    if (this._verticalResizerComponent) {
      $bottomHandle = jQuery(this._verticalResizerComponent.get_bottomHandleElement());
    }

    var searchBlockWidth = parseInt($searchBlock.width(), 10);
    var searchBlockHeight = 0;
    if (this._isVisible) {
      searchBlockHeight = parseInt($searchBlock.outerHeight(), 10);
      if (!$q.isNullOrEmpty($bottomHandle)) {
        searchBlockHeight += parseInt($bottomHandle.outerHeight(), 10);
      }
    } else {
      searchBlockHeight = parseInt($searchBlock.height(), 10);
    }

    $searchBlock = null;
    $bottomHandle = null;

    var eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, "");
    eventArgs.set_searchBlockWidth(searchBlockWidth);
    eventArgs.set_searchBlockHeight(searchBlockHeight);

    this.notify(EVENT_TYPE_SEARCH_BLOCK_RESIZED, eventArgs);
  },

  _onFindButtonClick: function () {
    var eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, "");
    this.notify(EVENT_TYPE_SEARCH_BLOCK_FIND_START, eventArgs);

    eventArgs = null;
  },

  _onResetButtonClick: function () {
    var eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, "");
    this.notify(EVENT_TYPE_SEARCH_BLOCK_RESET_START, eventArgs);

    eventArgs = null;
  },

  _onSearchFormSubmitted: function (e) {
    e.preventDefault();
    jQuery(this._findButtonElement).trigger("click");
    return false;
  },

  dispose: function () {
    Quantumart.QP8.BackendSearchBlockBase.callBaseMethod(this, "dispose");

    this._detachSearchBlockEventHandlers();

    this._verticalResizerComponent = null;
    this._concreteSearchBlockElement = null;
    this._findButtonElement = null;
    this._resetButtonElement = null;
    this._buttonsWrapperElement = null;
    if (this._searchBlockElement) {
      var $searchBlock = jQuery(this._searchBlockElement);
      $searchBlock
        .noVerticalResizer()
        .empty()
        .remove()
        ;

      $searchBlock = null;
      this._searchBlockElement = null;
    }
    this._onSearchBlockResizedHandler = null;
    this._onFindButtonClickHandler = null;
    this._onResetButtonClickHandler = null;
    this._onSearchFormSubmittedHandler = null;
  }
};

Quantumart.QP8.BackendSearchBlockBase.generateElementPrefix = function () {
    return "q" + $q.generateRandomString(6);
};

Quantumart.QP8.BackendSearchBlockBase.registerClass("Quantumart.QP8.BackendSearchBlockBase", Quantumart.QP8.Observable);
// #endregion

// #region class BackendSearchBlockEventArgs
// === Класс "Аргументы события, вызванного блоком поиска" ===
Quantumart.QP8.BackendSearchBlockEventArgs = function (searchBlockType, searchQuery) {
  Quantumart.QP8.BackendSearchBlockEventArgs.initializeBase(this);

  this._searchBlockType = searchBlockType;
  this._searchQuery = searchQuery;
};

Quantumart.QP8.BackendSearchBlockEventArgs.prototype = {
  _searchBlockType: 0, // тип поискового блока
  _searchQuery: null, // поисковый запрос
  _searchBlockWidth: 0, // ширина поискового блока
  _searchBlockHeight: 0, // высота поискового блока
  _searchBlockState: null, // сохраняемое состояние

  get_searchQuery: function () {
    return this._searchQuery;
  },

  set_searchQuery: function (value) {
    this._searchQuery = value;
  },

  get_searchBlockType: function () {
    return this._searchBlockType;
  },

  set_searchBlockType: function (value) {
    this._searchBlockType = value;
  },

  get_searchBlockWidth: function () {
    return this._searchBlockWidth;
  },

  set_searchBlockWidth: function (value) {
    this._searchBlockWidth = value;
  },

  get_searchBlockHeight: function () {
    return this._searchBlockHeight;
  },

  set_searchBlockHeight: function (value) {
    this._searchBlockHeight = value;
  },

  set_searchBlockState: function (value) {
    this._searchBlockState = value;
  },

  get_searchBlockState: function () {
    return this._searchBlockState;
  }
};

Quantumart.QP8.BackendSearchBlockEventArgs.registerClass("Quantumart.QP8.BackendSearchBlockEventArgs", Sys.EventArgs);
// #endregion

