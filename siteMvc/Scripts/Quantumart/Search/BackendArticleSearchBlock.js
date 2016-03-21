// === Класс "Блок поиска статей" ===
Quantumart.QP8.BackendArticleSearchBlock = function(searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options) {
  Quantumart.QP8.BackendArticleSearchBlock.initializeBase(this, [searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options]);
  this.isVirtual = options.isVirtual;
  if (this.isVirtual) {
    this._minSearchBlockHeight = 125;
  } else {
    this._minSearchBlockHeight = 180;
  }
};

Quantumart.QP8.BackendArticleSearchBlock.prototype = {
  _fullTextBlockElement: null, // dom-элемент контейнер для блока полнотекстового поиска
  _fullTextBlock: null, // объект-блок для полнотекстового поиска
  _fieldSearchBlockElement: null, // dom-элемент контейнер для блока поиска по полям
  _fieldSearchBlock: null, // объект-блок для поиска по полям
  _isVirtualArticles: false,
  _$defaultFilterButton: null,
  _defaultFieldSearchBlockState: null,

  initialize: function() {
    Quantumart.QP8.BackendArticleSearchBlock.callBaseMethod(this, 'initialize');
    if (this._searchBlockState && this._searchBlockState.defaultFieldSearchBlockState && this._buttonsWrapperElement) {
      this._defaultFieldSearchBlockState = this._searchBlockState.defaultFieldSearchBlockState;

      var $buttonsWrapper = jQuery(this._buttonsWrapperElement);

      this._$defaultFilterButton = jQuery('<input />', { type: 'button', value: $l.SearchBlock.defaultFilterButtonText, class: 'button' });
      $buttonsWrapper.append('&nbsp;');
      $buttonsWrapper.append(this._$defaultFilterButton);
      $buttonsWrapper = null;
      this._$defaultFilterButton.click(jQuery.proxy(this._onDefaultFilterClicked, this));
    }
  },

  renderSearchBlock: function() {
    if (this.get_isRendered() !== true) {
      var $concreteSearchBlockElement = jQuery(this._concreteSearchBlockElement);

      if (!this.isVirtual) {
        // Создает контейнер для блока полнотекстового поиска
        var $fullTextBlockElement = jQuery('<div/>', { class: 'articleSearchRegion' });

        this._fullTextBlockElement = $fullTextBlockElement.get(0);

        // Cоздает объект-блок полнотекстового поиска и инициализирует его
        this._fullTextBlock = new Quantumart.QP8.BackendArticleSearchBlock.FullTextBlock(this._fullTextBlockElement, this.get_parentEntityId());
        this._fullTextBlock.initialize();

        // добавляет контейнер для блока полнотекстового поиска в блок поиска
        $concreteSearchBlockElement.append($fullTextBlockElement);
      }

      // Создаем контейнер для блока поиска по полям
      var $fieldSeachBlockElement = jQuery('<div/>', { class: 'articleSearchRegion' });

      this._fieldSearchBlockElement = $fieldSeachBlockElement.get(0);

      // Создаем объект-блок поиска по полям
      this._fieldSearchBlock = new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBlock(this._fieldSearchBlockElement, this.get_parentEntityId());
      this._fieldSearchBlock.initialize();

      // добавляет контейнер для блока поиска по полям в блок поиска
      $concreteSearchBlockElement.append($fieldSeachBlockElement);

      // Восстановить состояние
      this._restore_searchBlockState();

      this.set_isRendered(true);

      $concreteSearchBlockElement = null;
      $fullTextBlockElement = null;
      $fieldSeachBlockElement = null;
    }
  },

  _restore_searchBlockState: function() {
    if (this._searchBlockState) {
      if (this._searchBlockState.fullTextBlockState) {
        this._fullTextBlock.restore_blockState(this._searchBlockState.fullTextBlockState);
      }

      if (this._searchBlockState.fieldSearchBlockState) {
        this._fieldSearchBlock.restore_blockState(this._searchBlockState.fieldSearchBlockState);
      }
    }
  },

  // возвращает параметры поиска
  get_searchQuery: function() {
    var result = [];

    if (this._fullTextBlock) {
      var ftbsq = this._fullTextBlock.get_searchQuery();

      if (ftbsq) {
        result.push(ftbsq);
      }
    }

    if (this._fieldSearchBlock) {
      var fssq = this._fieldSearchBlock.get_searchQuery();

      if (fssq) {
        result = result.concat(fssq);
      }
    }

    return JSON.stringify(result);
  },

  // Возвращает состояние блока поиска
  get_searchBlockState: function() {
    var bs = null;

    this._searchBlockState = {};
    if (this._fullTextBlock) {
      bs = this._fullTextBlock.get_blockState();
      if (bs) {
        this._searchBlockState.fullTextBlockState = bs;
      }
    }

    if (this._fieldSearchBlock) {
      bs = this._fieldSearchBlock.get_blockState();
      if (bs) {
        this._searchBlockState.fieldSearchBlockState = bs;
      }
    }

    if (this._defaultFieldSearchBlockState) {
      this._searchBlockState.defaultFieldSearchBlockState = this._defaultFieldSearchBlockState;
    }

    if (jQuery.isEmptyObject(this._searchBlockState)) {
      this._searchBlockState = null;
    }

    return this._searchBlockState;
  },

  _onFindButtonClick: function() {
    var eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, this.get_searchQuery());

    eventArgs.set_searchBlockState(this.get_searchBlockState());
    this.notify(EVENT_TYPE_SEARCH_BLOCK_FIND_START, eventArgs);
    eventArgs = null;
  },

  _onResetButtonClick: function() {
    if (this._fullTextBlock) {
      this._fullTextBlock.clear();
    }

    if (this._fieldSearchBlock) {
      this._fieldSearchBlock.clear();
    }

    this._onFindButtonClick();
  },

  _onDefaultFilterClicked: function() {
    if (this._defaultFieldSearchBlockState) {
      if (this._fullTextBlock)
      this._fullTextBlock.clear();
      if (this._fieldSearchBlock)
      this._fieldSearchBlock.clear();

      this._searchBlockState = {
        fieldSearchBlockState: this._defaultFieldSearchBlockState,
        defaultFieldSearchBlockState: this._defaultFieldSearchBlockState
      };

      this._restore_searchBlockState();
      this._onFindButtonClick();
    }
  },

  dispose: function() {
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

// Создать параметр поискового запроса для конкретного поля
Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery = function(searchType, fieldID, fieldColumn, contentId, referenceFieldId) {
  return {
    SearchType: searchType,
    FieldID: fieldID,
    FieldColumn: fieldColumn,
    ContentId: contentId,
    ReferenceFieldId: referenceFieldId,
    QueryParams: Array.prototype.slice.call(arguments, 5)
  };
};

Quantumart.QP8.BackendArticleSearchBlock.registerClass('Quantumart.QP8.BackendArticleSearchBlock', Quantumart.QP8.BackendSearchBlockBase);
