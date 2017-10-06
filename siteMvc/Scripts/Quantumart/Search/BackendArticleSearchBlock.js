Quantumart.QP8.BackendArticleSearchBlock = function (
  searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options
) {
  Quantumart.QP8.BackendArticleSearchBlock.initializeBase(
    this, [searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options]
  );
  this.isVirtual = options.isVirtual;
  this._minSearchBlockHeight = this.isVirtual ? 125 : 180;
};

Quantumart.QP8.BackendArticleSearchBlock.prototype = {
  _fullTextBlockElement: null,
  _fullTextBlock: null,
  _fieldSearchBlockElement: null,
  _fieldSearchBlock: null,
  _isVirtualArticles: false,
  _$defaultFilterButton: null,
  _defaultFieldSearchBlockState: null,

  initialize() {
    Quantumart.QP8.BackendArticleSearchBlock.callBaseMethod(this, 'initialize');
    if (this._searchBlockState && this._searchBlockState.defaultFieldSearchBlockState && this._buttonsWrapperElement) {
      this._defaultFieldSearchBlockState = this._searchBlockState.defaultFieldSearchBlockState;

      let $buttonsWrapper = $(this._buttonsWrapperElement);
      this._$defaultFilterButton = $('<input />', {
        type: 'button',
        value: $l.SearchBlock.defaultFilterButtonText,
        class: 'button'
      });
      $buttonsWrapper.append('&nbsp;');
      $buttonsWrapper.append(this._$defaultFilterButton);
      $buttonsWrapper = null;

      this._$defaultFilterButton.click($.proxy(this._onDefaultFilterClicked, this));
    }
  },

  renderSearchBlock() {
    if (!this.get_isRendered()) {
      const $concreteSearchBlockElement = $(this._concreteSearchBlockElement);
      if (!this.isVirtual) {
        const $fullTextBlockElement = $('<div/>', { class: 'articleSearchRegion' });
        this._fullTextBlockElement = $fullTextBlockElement.get(0);
        this._fullTextBlock = new Quantumart.QP8.BackendArticleSearchBlock.FullTextBlock(
          this._fullTextBlockElement, this.get_parentEntityId()
        );
        this._fullTextBlock.initialize();

        $concreteSearchBlockElement.append($fullTextBlockElement);
      }

      const $fieldSeachBlockElement = $('<div/>', { class: 'articleSearchRegion' });
      this._fieldSearchBlockElement = $fieldSeachBlockElement.get(0);

      this._fieldSearchBlock = new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBlock(
        this._fieldSearchBlockElement, this.get_parentEntityId()
      );
      this._fieldSearchBlock.initialize();

      $concreteSearchBlockElement.append($fieldSeachBlockElement);

      this.restoreSearchBlockState();
      this.set_isRendered(true);
    }
  },

  restoreSearchBlockState() {
    if (this._searchBlockState) {
      if (this._searchBlockState.fullTextBlockState) {
        this._fullTextBlock.restoreBlockState(this._searchBlockState.fullTextBlockState);
      }

      if (this._searchBlockState.fieldSearchBlockState) {
        this._fieldSearchBlock.restoreBlockState(this._searchBlockState.fieldSearchBlockState);
      }
    }
  },

  getSearchQuery() {
    let result = [];
    if (this._fullTextBlock) {
      const ftbsq = this._fullTextBlock.getSearchQuery();
      if (ftbsq) {
        result.push(ftbsq);
      }
    }

    if (this._fieldSearchBlock) {
      const fssq = this._fieldSearchBlock.getSearchQuery();
      if (fssq) {
        result = result.concat(fssq);
      }
    }

    return JSON.stringify(result);
  },

  getSearchBlockState() {
    let bs = null;

    this._searchBlockState = {};
    if (this._fullTextBlock) {
      bs = this._fullTextBlock.getBlockState();
      if (bs) {
        this._searchBlockState.fullTextBlockState = bs;
      }
    }

    if (this._fieldSearchBlock) {
      bs = this._fieldSearchBlock.getBlockState();
      if (bs) {
        this._searchBlockState.fieldSearchBlockState = bs;
      }
    }

    if (this._defaultFieldSearchBlockState) {
      this._searchBlockState.defaultFieldSearchBlockState = this._defaultFieldSearchBlockState;
    }

    if ($.isEmptyObject(this._searchBlockState)) {
      this._searchBlockState = null;
    }

    return this._searchBlockState;
  },

  _onFindButtonClick() {
    let eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, this.getSearchQuery());

    eventArgs.setSearchBlockState(this.getSearchBlockState());
    this.notify(window.EVENT_TYPE_SEARCH_BLOCK_FIND_START, eventArgs);
    eventArgs = null;
  },

  _onResetButtonClick() {
    if (this._fullTextBlock) {
      this._fullTextBlock.clear();
    }

    if (this._fieldSearchBlock) {
      this._fieldSearchBlock.clear();
    }

    this._onFindButtonClick();
  },

  _onDefaultFilterClicked() {
    if (this._defaultFieldSearchBlockState) {
      if (this._fullTextBlock) {
        this._fullTextBlock.clear();
      }

      if (this._fieldSearchBlock) {
        this._fieldSearchBlock.clear();
      }

      this._searchBlockState = {
        fieldSearchBlockState: this._defaultFieldSearchBlockState,
        defaultFieldSearchBlockState: this._defaultFieldSearchBlockState
      };

      this.restoreSearchBlockState();
      this._onFindButtonClick();
    }
  },

  dispose() {
    Quantumart.QP8.BackendArticleSearchBlock.callBaseMethod(this, 'dispose');
    if (this._fullTextBlock) {
      this._fullTextBlock.dispose();
    }

    this._fullTextBlock = null;
    this._fullTextBlockElement = null;
    if (this._fieldSearchBlock) {
      this._fieldSearchBlock.dispose();
    }

    this._fieldSearchBlock = null;
    this._fieldSearchBlockElement = null;
    if (this._$defaultFilterButton) {
      this._$defaultFilterButton.off();
    }

    this._$defaultFilterButton = null;
  }
};

Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery = function (
  searchType, fieldID, fieldColumn, contentId, referenceFieldId, ...params
) {
  return {
    SearchType: searchType,
    FieldID: fieldID,
    FieldColumn: fieldColumn,
    ContentId: contentId,
    ReferenceFieldId: referenceFieldId,
    QueryParams: params
  };
};

Quantumart.QP8.BackendArticleSearchBlock.registerClass(
  'Quantumart.QP8.BackendArticleSearchBlock',
  Quantumart.QP8.BackendSearchBlockBase
);
