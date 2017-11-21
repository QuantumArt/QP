window.EVENT_TYPE_SEARCH_BLOCK_FIND_START = 'OnSearchBlockFindStart';
window.EVENT_TYPE_CONTEXT_BLOCK_FIND_START = 'OnContextBlockFindStart';
window.EVENT_TYPE_SEARCH_BLOCK_RESET_START = 'OnSearchBlockResetStart';
window.EVENT_TYPE_SEARCH_BLOCK_RESIZED = 'OnSearchBlockResized';
window.EVENT_TYPE_CONRETE_FIELD_SEARCH_CONTAINER_CLOSE = 'OnFieldSearchContainerClose';

Quantumart.QP8.BackendSearchBlockBase = function (
  searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options) {
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

  this._onSearchBlockResizedHandler = $.proxy(this._onSearchBlockResized, this);
  this._onFindButtonClickHandler = $.proxy(this._onFindButtonClick, this);
  this._onResetButtonClickHandler = $.proxy(this._onResetButtonClick, this);
  this._onSearchFormSubmittedHandler = $.proxy(this._onSearchFormSubmitted, this);
};

Quantumart.QP8.BackendSearchBlockBase.prototype = {
  _searchBlockGroupCode: '',
  _searchBlockElementId: '',
  _searchBlockElement: null,
  _searchBlockContainerElementId: '',
  _tabId: '',
  _popupWindowId: '',
  _hostId: '',
  _actionCode: null,
  _concreteSearchBlockElement: null,
  _buttonsWrapperElement: null,
  _findButtonElement: null,
  _resetButtonElement: null,
  _entityTypeCode: '',
  _parentEntityId: 0,
  _minSearchBlockHeight: 180,
  _maxSearchBlockHeight: 500,
  _lastSearchBlockHeight: 500,
  _isVisible: false,
  _isRendered: false,
  _verticalResizerComponent: null,
  _hideButtons: false,
  _searchBlockState: null,

  _onSearchBlockResizedHandler: null,
  _onFindButtonClickHandler: null,
  _onResetButtonClickHandler: null,

  // eslint-disable-next-line camelcase
  get_searchBlockGroupCode() {
    return this._searchBlockGroupCode;
  },

  // eslint-disable-next-line camelcase
  set_searchBlockGroupCode(value) {
    this._searchBlockGroupCode = value;
  },

  // eslint-disable-next-line camelcase
  get_searchBlockElementId() {
    return this._searchBlockElementId;
  },

  // eslint-disable-next-line camelcase
  set_searchBlockElementId(value) {
    this._searchBlockElementId = value;
  },

  // eslint-disable-next-line camelcase
  get_searchBlockElement() {
    return this._searchBlockElement;
  },

  // eslint-disable-next-line camelcase
  get_searchBlockContainerElementId() {
    return this._searchBlockContainerElementId;
  },

  // eslint-disable-next-line camelcase
  set_searchBlockContainerElementId(value) {
    this._searchBlockContainerElementId = value;
  },

  // eslint-disable-next-line camelcase
  get_entityTypeCode() {
    return this._entityTypeCode;
  },

  // eslint-disable-next-line camelcase
  set_entityTypeCode(value) {
    this._entityTypeCode = value;
  },

  // eslint-disable-next-line camelcase
  get_parentEntityId() {
    return this._parentEntityId;
  },

  // eslint-disable-next-line camelcase
  set_parentEntityId(value) {
    this._parentEntityId = value;
  },

  // eslint-disable-next-line camelcase
  get_minSearchBlockHeight() {
    return this._minSearchBlockHeight;
  },

  // eslint-disable-next-line camelcase
  set_minSearchBlockHeight(value) {
    this._minSearchBlockHeight = value;
  },

  // eslint-disable-next-line camelcase
  get_maxSearchBlockHeight() {
    return this._maxSearchBlockHeight;
  },

  // eslint-disable-next-line camelcase
  set_maxSearchBlockHeight(value) {
    this._maxSearchBlockHeight = value;
  },

  // eslint-disable-next-line camelcase
  get_isRendered() {
    return this._isRendered;
  },

  // eslint-disable-next-line camelcase
  set_isRendered(value) {
    this._isRendered = value;
  },

  initialize() {
    let $searchBlock = $(`#${this._searchBlockElementId}`);
    const searchBlockExist = !$q.isNullOrEmpty($searchBlock);
    const searchFormExist = !$q.isNullOrEmpty($searchBlock.find('form'));

    if (!searchBlockExist) {
      $searchBlock = $('<div />', { id: this._searchBlockElementId, class: 'searchBlock', css: { display: 'none' } });
    }

    if (!searchFormExist) {
      const $searchForm = $('<form />', { class: 'formLayout' });
      $searchBlock.append($searchForm);

      const $concreteSearchBlock = $('<div />');
      $searchForm.append($concreteSearchBlock);

      const $buttonsWrapper = $('<div />');
      if (this._hideButtons) {
        $buttonsWrapper.hide();
      }

      $searchForm.append($buttonsWrapper);

      const $findButton = $('<input />', { type: 'button', value: $l.SearchBlock.findButtonText, class: 'button' });
      const $resetButton = $('<input />', { type: 'button', value: $l.SearchBlock.resetButtonText, class: 'button' });

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
      if ($q.isNullOrWhiteSpace(this._searchBlockContainerElementId)) {
        $('body:first').append($searchBlock);
      } else {
        $(`#${this._searchBlockContainerElementId}`).append($searchBlock);
      }
    }

    $searchBlock.verticalResizer({
      bottomHandleCssClassName: 'searchBottomHandle',
      minPanelHeight: this._minSearchBlockHeight,
      maxPanelHeight: this._maxSearchBlockHeight
    });

    this._verticalResizerComponent = $searchBlock.data('vertical_resizer');
    this._searchBlockElement = $searchBlock.get(0);
    this._lastSearchBlockHeight = this._minSearchBlockHeight;

    if (!searchFormExist) {
      this._attachSearchBlockEventHandlers();
    }
  },

  _attachSearchBlockEventHandlers() {
    $(this._searchBlockElement).bind('resize', this._onSearchBlockResizedHandler);
    $(this._findButtonElement).bind('click', this._onFindButtonClickHandler);
    $(this._resetButtonElement).bind('click', this._onResetButtonClickHandler);
    $(this._searchForm).bind('submit', this._onSearchFormSubmittedHandler);
  },

  _detachSearchBlockEventHandlers() {
    $(this._searchBlockElement).unbind('resize', this._onSearchBlockResizedHandler);
    $(this._findButtonElement).unbind('click', this._onFindButtonClickHandler);
    $(this._resetButtonElement).unbind('click', this._onResetButtonClickHandler);
    $(this._searchForm).unbind('submit');
  },

  showSearchBlock() {
    this._isVisible = true;
    const $searchBlock = $(this._searchBlockElement);
    if ($searchBlock.is(':hidden')) {
      const verticalResizerComponent = this._verticalResizerComponent;
      if (verticalResizerComponent) {
        verticalResizerComponent.showPanelWrapper();
        verticalResizerComponent.showBottomHandle();
      }

      $searchBlock.show().height(this._lastSearchBlockHeight).trigger('resize');
    }
  },

  hideSearchBlock() {
    this._isVisible = false;
    const $searchBlock = $(this._searchBlockElement);
    if ($searchBlock.is(':visible')) {
      const verticalResizerComponent = this._verticalResizerComponent;
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

  renderSearchBlock() {
    this.set_isRendered(true);
  },

  _onSearchBlockResized() {
    const $searchBlock = $(this._searchBlockElement);
    let $bottomHandle = null;
    if (this._verticalResizerComponent) {
      $bottomHandle = $(this._verticalResizerComponent.get_bottomHandleElement());
    }

    const searchBlockWidth = parseInt($searchBlock.width(), 10);
    let searchBlockHeight = 0;
    if (this._isVisible) {
      searchBlockHeight = parseInt($searchBlock.outerHeight(), 10);
      if (!$q.isNullOrEmpty($bottomHandle)) {
        searchBlockHeight += parseInt($bottomHandle.outerHeight(), 10);
      }
    } else {
      searchBlockHeight = parseInt($searchBlock.height(), 10);
    }

    const eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, '');
    eventArgs.setSearchBlockWidth(searchBlockWidth);
    eventArgs.setSearchBlockHeight(searchBlockHeight);
    this.notify(window.EVENT_TYPE_SEARCH_BLOCK_RESIZED, eventArgs);
  },

  _onFindButtonClick() {
    const eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, '');
    this.notify(window.EVENT_TYPE_SEARCH_BLOCK_FIND_START, eventArgs);
  },

  _onResetButtonClick() {
    const eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, '');
    this.notify(window.EVENT_TYPE_SEARCH_BLOCK_RESET_START, eventArgs);
  },

  _onSearchFormSubmitted(e) {
    e.preventDefault();
    $(this._findButtonElement).trigger('click');
    return false;
  },

  dispose() {
    Quantumart.QP8.BackendSearchBlockBase.callBaseMethod(this, 'dispose');

    this._detachSearchBlockEventHandlers();
    this._verticalResizerComponent = null;
    this._concreteSearchBlockElement = null;
    this._findButtonElement = null;
    this._resetButtonElement = null;
    this._buttonsWrapperElement = null;
    if (this._searchBlockElement) {
      const $searchBlock = $(this._searchBlockElement);
      $searchBlock
        .noVerticalResizer()
        .empty()
        .remove();

      this._searchBlockElement = null;
    }

    this._onSearchBlockResizedHandler = null;
    this._onFindButtonClickHandler = null;
    this._onResetButtonClickHandler = null;
    this._onSearchFormSubmittedHandler = null;
  }
};

Quantumart.QP8.BackendSearchBlockBase.generateElementPrefix = function () {
  return `q${$q.generateRandomString(6)}`;
};

Quantumart.QP8.BackendSearchBlockBase.registerClass('Quantumart.QP8.BackendSearchBlockBase', Quantumart.QP8.Observable);
Quantumart.QP8.BackendSearchBlockEventArgs = function (searchBlockType, searchQuery) {
  Quantumart.QP8.BackendSearchBlockEventArgs.initializeBase(this);
  this._searchBlockType = searchBlockType;
  this._searchQuery = searchQuery;
};

Quantumart.QP8.BackendSearchBlockEventArgs.prototype = {
  _searchBlockType: 0,
  _searchQuery: null,
  _searchBlockWidth: 0,
  _searchBlockHeight: 0,
  _searchBlockState: null,

  getSearchQuery() {
    return this._searchQuery;
  },

  setSearchQuery(value) {
    this._searchQuery = value;
  },

  getSearchBlockType() {
    return this._searchBlockType;
  },

  setSearchBlockType(value) {
    this._searchBlockType = value;
  },

  getSearchBlockWidth() {
    return this._searchBlockWidth;
  },

  setSearchBlockWidth(value) {
    this._searchBlockWidth = value;
  },

  getSearchBlockHeight() {
    return this._searchBlockHeight;
  },

  setSearchBlockHeight(value) {
    this._searchBlockHeight = value;
  },

  setSearchBlockState(value) {
    this._searchBlockState = value;
  },

  getSearchBlockState() {
    return this._searchBlockState;
  }
};

Quantumart.QP8.BackendSearchBlockEventArgs.registerClass('Quantumart.QP8.BackendSearchBlockEventArgs', Sys.EventArgs);
