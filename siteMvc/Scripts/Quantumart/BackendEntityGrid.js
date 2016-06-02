var EVENT_TYPE_ENTITY_GRID_DATA_BINDING = 'OnEntityGridDataBinding';
var EVENT_TYPE_ENTITY_GRID_DATA_BOUND = 'OnEntityGridDataBound';
var EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING = 'OnEntityGridActionExecuting';
var EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED = 'OnEntityGridEntitySelected';
var EVENT_TYPE_ENTITY_GRID_TITLE_LINK_CLICK = 'OnEntityGridEntityTitleLinkClick';

//#region class BackendEntityGrid
// === Класс "Список сущностей" ===
Quantumart.QP8.BackendEntityGrid = function(gridGroupCodes, gridElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions) {
  Quantumart.QP8.BackendEntityGrid.initializeBase(this);

  this._gridGroupCodes = gridGroupCodes;
  this._gridElementId = gridElementId;
  this._entityTypeCode = entityTypeCode;
  this._parentEntityId = parentEntityId;
  this._actionCode = actionCode;
  if ($q.isObject(options)) {
    if (options.selectedEntitiesIDs) {
      this._selectedEntitiesIDs = options.selectedEntitiesIDs;
    }

    if (options.actionCodeForLink) {
      this._actionCodeForLink = options.actionCodeForLink;
    }

    if (options.contextMenuCode) {
      this._contextMenuCode = options.contextMenuCode;
    }

    if (!$q.isNull(options.allowMultipleRowSelection)) {
      this._allowMultipleRowSelection = options.allowMultipleRowSelection;
    }

    if (!$q.isNull(options.allowGlobalSelection)) {
      this._allowGlobalSelection = options.allowGlobalSelection;
    }

    if (options.keyColumnName) {
      this._keyColumnName = options.keyColumnName;
    }

    if (options.titleColumnName) {
      this._titleColumnName = options.titleColumnName;
    }

    if (options.parentKeyColumnName) {
      this._parentKeyColumnName = options.parentKeyColumnName;
    }

    if (!$q.isNull(options.linkOpenNewTab)) {
      this._linkOpenNewTab = options.linkOpenNewTab;
    }

    if (!$q.isNull(options.allowFilterSelectedEntities)) {
      this._allowFilterSelectedEntities = options.allowFilterSelectedEntities;
    }

    if (options.autoGenerateLink) {
      this._autoGenerateLink = options.autoGenerateLink;
    }

    if (options.generateLinkOnTitle) {
      this._generateLinkOnTitle = options.generateLinkOnTitle;
    }

    if (!$q.isNull(options.autoLoad)) {
      this._autoLoad = options.autoLoad;
    }

    if (!$q.isNull(options.delayAutoLoad)) {
      this._delayAutoLoad = options.delayAutoLoad;
    }

    if (options.filter) {
      this._filter = options.filter;
    }

    if (options.isWindow) {
      this._hostIsWindow = options.isWindow;
    }

    if (options.treeFieldId) {
      this._treeFieldId = options.treeFieldId;
    }

    if (options.selectAllId) {
      this._selectAllId = options.selectAllId;
    }

    if (options.unselectId) {
      this._deselectAllId = options.unselectId;
    }

    if (options.zIndex) {
      this._zIndex = options.zIndex;
    }
  }

  if (!$q.isNull(hostOptions)) {
    if (hostOptions.searchQuery) {
      this._searchQuery = hostOptions.searchQuery;
    }

    if (hostOptions.contextQuery) {
      this._contextQuery = hostOptions.contextQuery;
    }

    if (hostOptions.filter) {
      this._filter = hostOptions.filter;
    }

    if (hostOptions.currentPage) {
      this._currentPage = hostOptions.currentPage;
    }

    if (hostOptions.orderBy) {
      this._orderBy = hostOptions.orderBy;
    }

    this._isBindToExternal = $q.toBoolean(hostOptions.isBindToExternal, false);
  }

  Quantumart.QP8.Utils.bindProxies.call(this, [
    '_onDataBinding',
    '_onDataBound',
    '_onRowDataBound',
    '_onDataLoadError',
    '_onHeaderCheckboxClick',
    '_onTitleLinkClick',
    '_onRowCellClick',
    '_onRowCheckboxCellClick',
    '_onContextMenu',
    '_onRowContextMenuShowing',
    '_onRowContextMenuItemClicking',
    '_onRowContextMenuHidden',
    '_onSelectAllClick',
    '_onDeselectAllClick'
  ]);
};

Quantumart.QP8.BackendEntityGrid.prototype = {
  _gridGroupCodes: null, // код групп, в которую входит грид
  _gridElementId: '', // клиентский идентификатор грида
  _gridElement: null, // DOM-элемент, образующий грид
  _gridComponent: null, // компонент "Грид"
  _currentRowId: -1, // идентификатор текущей строки
  _allowMultipleRowSelection: false, // признак, разрешающий множественный выбор строк
  _allowGlobalSelection: false, // признак разрешающий глобальный выбор элементов списка
  _entityTypeCode: '', // код типа сущности
  _parentEntityId: 0, // идентификатор родительской сущности
  _actionCode: '', // код действия
  _actionCodeForLink: '', // код действия, которое запускается при щелчке на гиперссылке
  _contextMenuCode: '', // код контекстного меню
  _keyColumnName: 'Id', // название столбца, содержащего первичный ключ сущности
  _titleColumnName: 'Name', // название столбца, содержащего название сущности
  _parentKeyColumnName: 'ParentId', // название столбца, содержащего id родителя
  _linkOpenNewTab: false, // открывать ли новый таб при клике на Link
  _startingEntitiesIDs: [], // идентификаторы выбранных сущностей, которые задаются при инициализации компонента
  _selectedEntitiesIDs: [], // идентификаторы выбранных сущностей
  _allowFilterSelectedEntities: false, //признак, разрешающий обрабатывать только те выбранные сущности, которые прошли фильтрацию
  _removedIds: [],
  _allowSaveRowsSelection: true, // признак, разрешающий сохранять выделение строк грида при загрузке в него новых данных
  _stopDeferredOperations: false, // признак, отвечающий за остановку все отложенных операций
  _isDataLoaded: false, // признак того, что данные загружены в грид
  _searchQuery: null, // поисковый запрос
  _contextQuery: null, // запрос для переключения контекста
  _autoGenerateLink: false, // признак, разрешающий автоматическую генерацию ссылок
  _generateLinkOnTitle: false, // признак, показывающий, где генерировать ссылку: на Title или на ID
  _contextMenuComponent: null, // компонент "Контекстное меню"
  _gridManagerComponent: null, // менеджер списков сущностей
  _autoLoad: true, // признак, отвечающий за загрузку данных в Grid при его создании
  _delayAutoLoad: false, // признак, отвечающий за откладывание загрузки Grid до вызова onLoad
  _contextMenuActionCode: '',
  _filter: '', // фильтр сущностей
  _hostIsWindow: false,
  _treeFieldId: 0, //Идентификатор поля по которому создаются дочерние элементы
  _isBindToExternal: false,
  _deselectAllId: '',
  _selectAllId: '',
  _zIndex: 0,

  GRID_BUSY_CLASS_NAME: 'busy',
  ROW_SELECTED_CLASS_NAME: 't-state-selected',
  TITLE_CELL_CLASS_NAME: 'title',
  ID_CELL_CLASS_NAME: 'id',
  ROW_CLICKABLE_SELECTORS: 'tr:not(.t-grouping-row,.t-detail-row,.t-no-data,:has(>.t-edit-container))',
  CHECKBOX_HEADER_SELECTORS: '.t-select-header INPUT:checkbox',
  CHECKBOX_CELL_SELECTORS: '.t-select-cell INPUT:checkbox',

  get_gridGroupCodes: function() {
    return this._gridGroupCodes;
  },
  get_gridElementId: function() {
    return this._gridElementId;
  },
  set_gridElementId: function(value) {
    this._gridElementId = value;
  },
  get_gridElement: function() {
    return this._gridElement;
  },
  set_gridElement: function(value) {
    this._gridElement = value;
  },
  get_gridComponent: function() {
    return this._gridComponent;
  },
  get_entityTypeCode: function() {
    return this._entityTypeCode;
  },
  set_entityTypeFromRow: function(rowElem) {
    var $row = this.getRow(rowElem);
    var dataItem = this.getDataItem($row);

    if (dataItem['EntityTypeCode']) {
      this._entityTypeCode = dataItem['EntityTypeCode'];
    }
  },
  set_actionCodeFromRow: function(rowElem) {
    var $row = this.getRow(rowElem);
    var dataItem = this.getDataItem($row);

    if (dataItem['ActionCode']) {
      this._actionCodeForLink = dataItem['ActionCode'];
    }
  },
  set_entityTypeCode: function(value) {
    this._entityTypeCode = value;
  },
  get_parentEntityId: function() {
    return this._parentEntityId;
  },
  set_parentEntityId: function(value) {
    this._parentEntityId = value;
  },
  get_actionCode: function() {
    return this._actionCode;
  },
  set_actionCode: function(value) {
    this._actionCode = value;
  },
  get_actionCodeForLink: function() {
    return this._actionCodeForLink;
  },
  set_actionCodeForLink: function(value) {
    this._actionCodeForLink = value;
  },
  get_allowMultipleRowSelection: function() {
    return this._allowMultipleRowSelection;
  },
  set_allowMultipleRowSelection: function(value) {
    this._allowMultipleRowSelection = value;
  },
  get_allowGlobalSelection: function() {
    return this._allowGlobalSelection;
  },
  set_allowGlobalSelection: function(value) {
    this._allowGlobalSelection = value;
  },
  get_selectedEntitiesIDs: function() {
    return this._selectedEntitiesIDs;
  },
  set_selectedEntitiesIDs: function(value) {
    this._selectedEntitiesIDs = value;
  },
  get_contextMenuCode: function() {
    return this._contextMenuCode;
  },
  set_contextMenuCode: function(value) {
    this._contextMenuCode = value;
  },
  get_keyColumnName: function() {
    return this._keyColumnName;
  },
  set_keyColumnName: function(value) {
    this._keyColumnName = value;
  },
  get_titleColumnName: function() {
    return this._titleColumnName;
  },
  set_titleColumnName: function(value) {
    this._titleColumnName = value;
  },
  get_autoGenerateLink: function() {
    return this._autoGenerateLink;
  },
  set_autoGenerateLink: function(value) {
    this._autoGenerateLink = value;
  },
  get_gridManager: function() {
    return this._gridManagerComponent;
  },
  set_gridManager: function(value) {
    this._gridManagerComponent = value;
  },
  get_treeFieldId: function() {
    return this._treeFieldId;
  },
  set_treeFieldId: function(value) {
    this._treeFieldId = value;
  },
  get_currentPage: function() {
    if (this._gridComponent) {
      return this._gridComponent.currentPage;
    }
  },
  get_orderBy: function() {
    if (this._gridComponent) {
      return this._gridComponent.orderBy;
    }
  },

  _onDataBindingHandler: null,
  _onDataBoundHandler: null,
  _onRowDataBoundHandler: null,
  _onHeaderCheckboxClickHandler: null,
  _onTitleLinkClickHandler: null,
  _onRowCellClickHandler: null,
  _onRowCheckboxCellClickHandler: null,
  _onContextMenuHandler: null,
  _onRowContextMenuShowingHandler: null,
  _onRowContextMenuItemClickingHandler: null,
  _onRowContextMenuHiddenHandler: null,
  _onSelectAllClickHandler: null,
  _onDeselectAllClickHandler: null,

  initialize: function() {
    $('.fullTextBlock label').addClass('hidden');

    var $grid = $('#' + this._gridElementId);
    var gridComponent = $grid.data('tGrid');

    if (this._currentPage) {
      gridComponent.currentPage = this._currentPage;
    }

    if (this._orderBy) {
      gridComponent.orderBy = this._orderBy;
    }

    this._gridElement = $grid.get(0);
    this._gridComponent = gridComponent;
    this._startingEntitiesIDs = Array.clone(this._selectedEntitiesIDs);

    $grid
      .unbind('dataBinding', gridComponent.onDataBinding)
      .unbind('dataBound', gridComponent.onDataBound)
      .unbind('rowDataBound', gridComponent.onRowDataBound)
      .bind('dataBinding', this._onDataBindingHandler)
      .bind('dataBound', this._onDataBoundHandler)
      .bind('rowDataBound', this._onRowDataBoundHandler);

    if (this._autoLoad && !this._delayAutoLoad) {
      gridComponent.ajaxRequest();
    }

    if (gridComponent.selectable) {
      var $headerCheckbox = this._getHeaderCheckbox();

      if (!$q.isNullOrEmpty($headerCheckbox)) {
        if (this._allowMultipleRowSelection) {
          $headerCheckbox.bind('click', this._onHeaderCheckboxClickHandler);
        } else {
          $headerCheckbox.css('display', 'none');
        }
      }

      $headerCheckbox = null;

      gridComponent.$tbody
        .undelegate(this.ROW_CLICKABLE_SELECTORS, 'click')
        .delegate(this.ROW_CLICKABLE_SELECTORS + ' td', 'click', this._onRowCellClickHandler)
        .delegate(this.ROW_CLICKABLE_SELECTORS + ' ' + this.CHECKBOX_CELL_SELECTORS, 'click', this._onRowCheckboxCellClickHandler);
    }

    if (!$q.isNullOrWhiteSpace(this._contextMenuCode)) {
      var contextMenuComponent = new Quantumart.QP8.BackendContextMenu(this._contextMenuCode, String.format('{0}_ContextMenu', this._gridElementId), {
        targetElements: this._gridElement,
        allowManualShowing: 'true',
        isBindToExternal: this._isBindToExternal,
        zIndex: this._zIndex
      });

      contextMenuComponent.initialize();
      contextMenuComponent.addMenuItemsToMenu(true);

      var contextMenuEventType = contextMenuComponent.getContextMenuEventType();

      $grid.delegate(this.ROW_CLICKABLE_SELECTORS, contextMenuEventType, this._onContextMenuHandler);

      contextMenuComponent.attachObserver(EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onRowContextMenuShowingHandler);
      contextMenuComponent.attachObserver(EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onRowContextMenuItemClickingHandler);
      contextMenuComponent.attachObserver(EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onRowContextMenuHiddenHandler);

      this._contextMenuComponent = contextMenuComponent;
    }

    if (this._deselectAllId) {
      $('#' + this._deselectAllId).bind('click', this._onDeselectAllClickHandler);
    }

    if (this._selectAllId) {
      $('#' + this._selectAllId).bind('click', this._onSelectAllClickHandler);
    }

    gridComponent = null;
    $grid = null;
  },

  _getCurrentAction: function() {
    return $a.getBackendActionByCode(this._actionCode);
  },

  _getTitleLinks: function() {
    return this.getRows().find(this._getLinkCellSelector() + ' > A');
  },

  _addLinkToCell: function(rowElem) {
    var $row = $q.toJQuery(rowElem);
    var $titleCell = $row.find(this._getLinkCellSelector());

    if (!$q.isNullOrEmpty($titleCell)) {
      $titleCell.wrapInner('<a href="javascript:void(0)" />');
      $titleCell.find('A')
        .addClass((this._linkOpenNewTab) ? 'js' : 'html')
        .bind('click', this._onTitleLinkClickHandler)
        .bind('mouseup', this._onTitleLinkClickHandler);
    }

    $titleCell = null;
    $row = null;
  },

  _getLinkCellSelector: function() {
    var result = 'TD.' + this.ID_CELL_CLASS_NAME;
    if (this._generateLinkOnTitle) {
      result += ', TD.' + this.TITLE_CELL_CLASS_NAME;
    }

    return result;
  },

  _removeLinksFromCells: function() {
    this._getTitleLinks()
      .unbind('click', this._onTitleLinkClickHandler)
      .unbind('mouseup', this._onTitleLinkClickHandler)
      .empty()
      .remove();
  },

  getRows: function() {
    var $rows = [];
    if (this._gridComponent) {
      $rows = this._gridComponent.$rows();
    }

    return $rows;
  },

  getSelectedRows: function() {
    return this.getRows().filter('.' + this.ROW_SELECTED_CLASS_NAME);
  },

  getRow: function(row) {
    var $row = null;

    if ($q.isObject(row)) {
      $row = $q.toJQuery(row);
    } else if ($q.isInt(row)) {
      var rowIndex = parseInt(row);
      $row = this.getRows().eq(rowIndex);
    }

    return $row;
  },

  getRowByEntityId: function(entityId) {
    return [].find.call(this.getRows(), function(rowEl) {
      return this.getEntityId(rowEl) === entityId;
    }, this);
  },

  getRowsByEntityIds: function(entityIds) {
    return [].filter.call(this.getRows(), function(rowEl) {
      return entityIds.includes(this.getEntityId(rowEl), this);
    }, this);
  },

  selectRow: function(rowElem, saveOtherRowsSelection) {
    if (!saveOtherRowsSelection) {
      this._changeRowsState(this.getRows(), false);
      if (!this._allowMultipleRowSelection && this._allowGlobalSelection) {
        this._resetRowSelectionState();
      }
    }

    var $row = this.getRow(rowElem);
    this._changeRowsState($row, !this.isRowSelected($row));
    this._saveRowSelectionState();
    this._raiseSelectEvent();
  },

  selectRows: function(rowElems) {
    this._changeRowsState(this.getRows(), false);

    if (!$q.isNullOrEmpty(rowElems)) {
      this._changeRowsState($q.toJQuery(rowElems), true);
    }

    this._saveRowSelectionState();
    this._raiseSelectEvent();
  },

  selectAllRows: function() {
    this._changeRowsState(this.getRows(), true);
    this._saveRowAllSelectionState();
    this._raiseSelectEvent();
  },

  deselectAllRows: function() {
    this._changeRowsState(this.getRows(), false);
    this._selectedEntitiesIDs = [];
    this._raiseSelectEvent();
  },

  toggleDirectChildRows: function(parentArticleId, rowState) {
    this._selectedEntitiesIDs = this.getChildEntityIds(parentArticleId);
    var $rows = $(this.getRowsByEntityIds(this._selectedEntitiesIDs));
    this._changeRowsState($rows, rowState);
    $.each(childEntities, function(i, entry) {
      Quantumart.QP8.Utils.addRemoveToArrUniq(this._selectedEntitiesIDs, entry, rowState);
    }.bind(this));
    this._saveRowSelectionState();
    this._raiseSelectEvent();
  },

  selectPageRows: function(value) {
    this._changeRowsState(this.getRows(), value);
    this._saveRowSelectionState();
    this._raiseSelectEvent();
  },

  isRowSelected: function(rowElem) {
    return this.getRow(rowElem).hasClass(this.ROW_SELECTED_CLASS_NAME);
  },

  _changeRowsState: function($rows, state) {
    $rows[state ? 'addClass' : 'removeClass'](this.ROW_SELECTED_CLASS_NAME);
    $rows.find(this.CHECKBOX_CELL_SELECTORS).prop('checked', state);
  },

  _getHeaderCheckbox: function() {
    return $(this.CHECKBOX_HEADER_SELECTORS, this._gridComponent.$header);
  },

  _refreshHeaderCheckbox: function() {
    this._getHeaderCheckbox().prop('checked', !!this._isAllRowsSelectedInCurrentPage());
  },

  _refreshCancelSelection: function() {
    if (this._deselectAllId) {
      var $linkButton = $('#' + this._deselectAllId);
      var actionLink = $linkButton.data('action_link_component');

      if (actionLink) {
        if (this._selectedEntitiesIDs.length > 0) {
          actionLink.enableActionLink();
        } else {
          actionLink.disableActionLink();
        }
      }
    }
  },

  getChildEntityIds: function(parentArticleId) {
    var url = window.CONTROLLER_URL_ARTICLE + "GetChildArticleIds"
    var params = {
      filter: this._filter,
      contentId: this._parentEntityId,
      parentArticleId: parentArticleId
    }

    return Quantumart.QP8.Utils.getJsonSync(url, params);
  },

  getDataItem: function(rowElem) {
    var dataItem = null;
    var $row = this.getRow(rowElem);

    if (!$q.isNullOrEmpty($row)) {
      dataItem = this._gridComponent.dataItem($row.get(0));
    }

    return dataItem;
  },

  getEntityId: function(rowElem) {
    var entityId = 0;
    var $row = this.getRow(rowElem);
    var dataItem = this.getDataItem($row);

    if (dataItem && dataItem[this._keyColumnName]) {
      entityId = dataItem[this._keyColumnName];
    }

    return entityId;
  },

  getEntityName: function(rowElem) {
    var entityName = '';
    var $row = this.getRow(rowElem);
    var dataItem = this.getDataItem($row);

    if (dataItem && dataItem[this._titleColumnName]) {
      entityName = dataItem[this._titleColumnName];
    }

    return entityName;
  },

  getParentEntityId: function(rowElem) {
    var entityId = this._parentEntityId;
    var $row = this.getRow(rowElem);
    var dataItem = this.getDataItem($row);

    if (dataItem && dataItem[this._parentKeyColumnName]) {
      entityId = dataItem[this._parentKeyColumnName];
    }

    return entityId;
  },

  checkExistEntityInCurrentPage: function(entityId) {
    var result = false;
    var $row = $(this.getRowByEntityId(entityId));

    if (!$q.isNullOrEmpty($row)) {
      result = true;
    }

    return result;
  },

  checkExistEntitiesInCurrentPage: function(entities) {
    var self = this;
    var result = false;

    $.each(entities, function(index, value) {
      if (self.checkExistEntityInCurrentPage(value.Id)) {
        result = true;
        return false;
      }
    });

    return result;
  },

  getEntitiesFromRows: function(rows) {
    var entities = [];
    var rowCount = rows.length;

    for (var rowIndex = 0; rowIndex < rowCount; rowIndex++) {
      var entityId = 0;
      var entityName = '';
      var dataItem = this.getDataItem(rows[rowIndex]);

      if (dataItem) {
        if (dataItem[this._keyColumnName]) {
          entityId = dataItem[this._keyColumnName];
        }

        if (dataItem[this._titleColumnName]) {
          entityName = dataItem[this._titleColumnName] + '';
        } else {
          entityName = entityId;
        }

        if (entityId) {
          Array.add(entities, { Id: entityId, Name: entityName });
        }
      }
    }

    return entities;
  },

  getSelectedEntities: function() {
    var selectedEntities = [];

    if (this._allowGlobalSelection) {
      var rows = this.getRowsByEntityIds(this._selectedEntitiesIDs);
      var self = this;

      selectedEntities = $.map(rows, function(row) {
        return { Id: self.getEntityId(row), Name: self.getEntityName(row) };
      }
    );

      var notFoundIds = $.grep(this._selectedEntitiesIDs, function(elem) {
        for (var i = 0; i < selectedEntities.length; i++) {
          if (selectedEntities[i].Id == elem) {
            return false;
          }
        }

        return true;
      });

      $.each(notFoundIds, function(index, elem) {
        selectedEntities.push({ Id: elem, Name: '' });
      });
    } else {
      selectedEntities = this.getEntitiesFromRows(this.getSelectedRows());
    }

    return selectedEntities;
  },

  resetGrid: function(options) {
    if ($q.isObject(options)) {
      if (!$q.isNull(options.searchQuery)) {
        this._searchQuery = options.searchQuery;
      }

      if (!$q.isNull(options.contextQuery)) {
        this._contextQuery = options.contextQuery;
      }
    }

    var gridComponent = this._gridComponent;
    gridComponent.currentPage = 1;

    if (gridComponent) {
      gridComponent.ajaxRequest();
    }

    gridComponent = null;
  },

  refreshGrid: function(options) {
    if ($q.isObject(options)) {
      if (!$q.isNull(options.saveRowsSelection)) {
        this._allowSaveRowsSelection = options.saveRowsSelection;
      }

      if (!$q.isNull(options.searchQuery)) {
        this._searchQuery = options.searchQuery;
      }

      if (!$q.isNull(options.contextQuery)) {
        this._contextQuery = options.contextQuery;
      }

      if (!$q.isNull(options.removedIds)) {
        this._removedIds = options.removedIds;
      }
    }

    var gridComponent = this._gridComponent;

    if (gridComponent) {
      gridComponent.ajaxRequest();
    }
  },

  markGridAsBusy: function() {
    $(this._gridElement).addClass(this.GRID_BUSY_CLASS_NAME);
  },

  unmarkGridAsBusy: function() {
    $(this._gridElement).removeClass(this.GRID_BUSY_CLASS_NAME);
  },

  isGridBusy: function() {
    return $(this._gridElement).hasClass(this.GRID_BUSY_CLASS_NAME);
  },

  executeAction: function(row, actionCode, followLink, ctrlKey) {
    var $row = this.getRow(row);

    if (!$q.isNullOrEmpty($row)) {
      var action = $a.getBackendActionByCode(actionCode);
      if (!action) {
        window.alert($l.Common.ajaxDataReceivingErrorMessage);
      } else {
        var entityId = this.getEntityId($row);
        var context = { ctrlKey: ctrlKey };

        if (actionCode == ACTION_CODE_ADD_NEW_CHILD_ARTICLE) {
          context.additionalUrlParameters = {
            fieldId: this._treeFieldId,
            articleId: entityId,
            isChild: true
          };
        }

        if (actionCode === window.ACTION_TYPE_SELECT_CHILD_ARTICLES) {
          this.toggleDirectChildRows(entityId, true)
        }

        if (actionCode === window.ACTION_TYPE_UNSELECT_CHILD_ARTICLES) {
          this.toggleDirectChildRows(entityId, false)
        }

        var entityName = this.getEntityName($row);
        var params = new Quantumart.QP8.BackendActionParameters({
          entityTypeCode: this._entityTypeCode,
          entityId: entityId,
          entityName: entityName,
          entities: (action.ActionType.IsMultiple) ? [{ Id: entityId, Name: entityName }] : null,
          parentEntityId: this.getParentEntityId($row),
          context: context
        });

        params.correct(action);

        var eventArgs = $a.getEventArgsFromActionWithParams(action, params);

        eventArgs.set_startedByExternal(this._isBindToExternal);
        var message = Quantumart.QP8.Backend.getInstance().checkOpenDocumentByEventArgs(eventArgs);

        if (this._hostIsWindow) {
          if (message) {
            window.alert(message);
          } else {
            eventArgs.set_isWindow(true);
            this.notify(EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING, eventArgs);
          }
        } else {
          if (followLink && !this._linkOpenNewTab && !message) {
            this.notify(EVENT_TYPE_ENTITY_GRID_TITLE_LINK_CLICK, eventArgs);
          } else {
            this.notify(EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING, eventArgs);
          }
        }

        params = null;
        eventArgs = null;
      }

      action = null;
    }
  },

  _raiseSelectEvent: function() {
    var action = this._getCurrentAction();
    var eventArgs = $a.getEventArgsFromAction(action);

    eventArgs.set_isMultipleEntities(true);
    eventArgs.set_entityTypeCode(this._entityTypeCode);
    eventArgs.set_entities(this.getSelectedEntities());
    eventArgs.set_parentEntityId(this._parentEntityId);

    if (this._allowFilterSelectedEntities && !this._hostIsWindow) {
      eventArgs.set_context({
        dataQueryParams: this._createDataQueryParams(),
        url: this._gridComponent.url('selectUrl')
      });
    }

    this.notify(EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED, eventArgs);
    eventArgs = null;

    this._refreshHeaderCheckbox();
    this._refreshCancelSelection();
  },

  _isAllRowsSelectedInCurrentPage: function() {
    var $rows = this.getRows();
    var $selectedRows = this.getSelectedRows();

    return ($rows.length == $selectedRows.length);
  },

  _saveRowSelectionState: function() {
    var $rows = this.getRows();
    var rowCount = $rows.length;

    for (var rowIndex = 0; rowIndex < rowCount; rowIndex++) {
      var $row = $rows.eq(rowIndex);
      Quantumart.QP8.Utils.addRemoveToArrUniq(
        this._selectedEntitiesIDs,
        this.getEntityId($row),
        this.isRowSelected($row));
    }

    $rows = null;
  },

  _saveRowAllSelectionState: function() {
    var rowsOnPage = this.getRows().length;
    var totalRows = this._gridComponent.total;
    var onlyOnePage = !(rowsOnPage < totalRows);

    if (onlyOnePage) {
      this._saveRowSelectionState();
    } else {
      var url = this._gridComponent.url('selectUrl');
      var queryData = $.extend({ page: 1, size: 0, onlyIds: true }, this._createDataQueryParams());
      var rowsData = null;
      var eventArgs = null;

      var action = this._getCurrentAction();

      if (action) {
        eventArgs = $a.getEventArgsFromAction(action);
        eventArgs.set_isMultipleEntities(true);
        eventArgs.set_entityTypeCode(this._entityTypeCode);
        eventArgs.set_entities(this.getSelectedEntities());
        eventArgs.set_parentEntityId(this._parentEntityId);
      }

      if (action) {
        this.notify(EVENT_TYPE_ENTITY_GRID_DATA_BINDING, eventArgs);
      }

      $q.postDataToUrl(url, queryData, false, function(data) {
        rowsData = data;
      }, function(jqXHR) {
        $q.processGenericAjaxError(jqXHR);
      });

      if (action) {
        this.notify(EVENT_TYPE_ENTITY_GRID_DATA_BOUND, eventArgs);
      }

      var that = this;

      if (rowsData) {
        this._selectedEntitiesIDs = $.map(rowsData.data, function(dataItem) {
          return dataItem[that._keyColumnName];
        });
      }
    }
  },

  _restoreRowSelectionState: function() {
    var self = this;
    var selectedEntitiesIDs = this._selectedEntitiesIDs;
    var selectedRowElems = [];

    this.getRows().each(
      function(rowIndex, rowElem) {
        var $row = $(rowElem);
        var entityId = self.getEntityId($row);

        if (Array.contains(selectedEntitiesIDs, entityId)) {
          Array.add(selectedRowElems, rowElem);
        }
      }
    );

    this.selectRows(selectedRowElems);
  },

  _resetRowSelectionState: function() {
    $q.clearArray(this._selectedEntitiesIDs);
  },

  _fixGridWidth: function() {
    var gridElement = this.get_gridElement();
    var tableElement = $('table', gridElement).get(0);

    if (tableElement.offsetWidth > gridElement.offsetWidth) {
      $(gridElement).css('width', tableElement.offsetWidth + 'px');
    }
  },

  _createDataQueryParams: function() {
    var params = { gridParentId: this._parentEntityId };

    if (!$q.isNullOrEmpty(this._startingEntitiesIDs)) {
      params.IDs = this._startingEntitiesIDs.join(',');
    }

    if (!$q.isNull(this._searchQuery)) {
      params.searchQuery = this._searchQuery;
    }

    if (!$q.isNull(this._contextQuery)) {
      params.contextQuery = this._contextQuery;
    }

    if (!$q.isNull(this._filter)) {
      params.customFilter = this._filter;
    }

    return params;
  },

  _onDataBinding: function(e) {
    var params = this._createDataQueryParams();

    e.data = $.extend(e.data, params);
    if (this._isDataLoaded) {
      var action = this._getCurrentAction();

      if (action) {
        var eventArgs = $a.getEventArgsFromAction(action);

        eventArgs.set_isMultipleEntities(true);
        eventArgs.set_entityTypeCode(this._entityTypeCode);
        eventArgs.set_entities(this.getSelectedEntities());
        eventArgs.set_parentEntityId(this._parentEntityId);

        this.notify(EVENT_TYPE_ENTITY_GRID_DATA_BINDING, eventArgs);
      }

      if (this._allowSaveRowsSelection) {
        this._saveRowSelectionState();
      } else {
        this._resetRowSelectionState();
      }

      var contextMenuComponent = this._contextMenuComponent;
      if (contextMenuComponent) {
        contextMenuComponent.hideMenu();
      }
    }
  },

  _onDataBound: function() {
    var grid = this._gridComponent;
    if (grid.currentPage > 1 && !grid.total) {
      grid.pageTo(grid.currentPage - 1);
    }

    this._isDataLoaded = true;
    this._fixGridWidth();

    var self = this;
    this._selectedEntitiesIDs = $.grep(this._selectedEntitiesIDs, function(item) {
      return !Array.contains(self._removedIds, item);
    });

    this._removedIds = [];
    var action = this._getCurrentAction();

    if (action) {
      var eventArgs = $a.getEventArgsFromAction(action);

      eventArgs.set_isMultipleEntities(true);
      eventArgs.set_entityTypeCode(this._entityTypeCode);
      eventArgs.set_entities(this.getSelectedEntities());
      eventArgs.set_parentEntityId(this._parentEntityId);

      this.notify(EVENT_TYPE_ENTITY_GRID_DATA_BOUND, eventArgs);
    }

    if (this._allowSaveRowsSelection) {
      this._restoreRowSelectionState();
    }
  },

  _onRowDataBound: function(e) {
    this._applyStatusColor(e.row, e.dataItem);
    if (this._autoGenerateLink) {
      this._addLinkToCell(e.row);
    }
  },

  _onDataLoadError: function(e) {
    $q.processGenericAjaxError(e.XMLHttpRequest);
    e.preventDefault();
  },

  _onHeaderCheckboxClick: function(e) {
    this.selectPageRows($(e.currentTarget).is(':checked'));
  },

  _onTitleLinkClick: function(e) {
    e.preventDefault();
    var isMiddleClick = e.type == 'mouseup' && e.which == 2;
    var isLeftClick = e.type == 'click' && (e.which == 1 || e.which == 0);
    var isRightClick = !isLeftClick && !isMiddleClick;

    if (this.isGridBusy() || isRightClick) {
      return false;
    }

    var $row = $(e.currentTarget).parent().parent();

    this.set_actionCodeFromRow($row);
    this.set_entityTypeFromRow($row);

    var actionCodeForLink = this._actionCodeForLink;
    if (!$q.isNullOrWhiteSpace(actionCodeForLink)) {
      this.executeAction($row, actionCodeForLink, !(e.ctrlKey || e.shiftKey || isMiddleClick), e.ctrlKey || isMiddleClick);
    } else {
      window.alert('Вы не задали код действия, которое открывает форму редактирования сущности!');
    }
  },

  _onRowCellClick: function(e) {
    var $target = $(e.target);

    if (!$target.is(':button, A, :input, A > .t-icon')) {
      e.stopPropagation();
      this.selectRow($target.closest('TR'), this._allowMultipleRowSelection && e.ctrlKey);
    }

    if (this._contextMenuComponent) {
      this._contextMenuComponent.hideMenu();
    }

    $target = null;
  },

  _onRowCheckboxCellClick: function(e) {
    var $checkbox = $(e.target).closest('INPUT:checkbox');
    if (!$q.isNullOrEmpty($checkbox)) {
      var $row = $checkbox.parent().parent();
      this.selectRow($row, this._allowMultipleRowSelection);
    }
  },

  _onContextMenu: function(e) {
    var $element = $(e.currentTarget);
    this._currentRowId = this.getEntityId($($element.closest('TR')[0]));
    if (this._contextMenuComponent) {
      this._contextMenuComponent.showMenu(e, $element.get(0));
    }

    $element = null;
    e.preventDefault();
  },

  _onRowContextMenuShowing: function(eventType, sender, args) {
    var menuComponent = args.get_menu();
    if (!$q.isNullOrEmpty($(args.get_targetElement())) && !$q.isNullOrEmpty(menuComponent)) {
      var entityTypeCode = this._entityTypeCode;
      var entityId = this._currentRowId;
      var parentEntityId = this._parentEntityId;
      menuComponent.tuneMenuItems(entityId, parentEntityId);
    }
  },

  _onRowContextMenuItemClicking: function(eventType, sender, args) {
    var $menuItem = $(args.get_menuItem());
    if (!$q.isNullOrEmpty($menuItem)) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }

    $menuItem = null;
  },

  _onRowContextMenuHidden: function(eventType, sender, args) {
    if (!$q.isNullOrEmpty(this._contextMenuActionCode)) {
      this.executeAction($(args.get_targetElement()), this._contextMenuActionCode, false, false);
      this._contextMenuActionCode = null;
    }
  },

  _onSelectAllClick: function() {
    this.selectAllRows();
  },

  _onDeselectAllClick: function() {
    this.deselectAllRows();
  },

  _applyStatusColor: function(row, item) {
    var id = item['STATUS_TYPE_COLOR'];

    if (id) {
      var $row = $(row);
      var isAlt = $row.hasClass('t-alt');
      var className = (isAlt) ? 't-alt-' + id : 't-' + id;
      $row.removeClass('t-alt').addClass(className);
    }
  },

  onLoad: function() {
    if (this._autoLoad && this._delayAutoLoad) {
      this.refreshGrid();
    }
  },

  dispose: function() {
    this._stopDeferredOperations = true;
    Quantumart.QP8.BackendEntityGrid.callBaseMethod(this, 'dispose');

    if (this._autoGenerateLink) {
      this._removeLinksFromCells();
    }

    this._resetRowSelectionState();
    Quantumart.QP8.Utils.dispose.call(this, [
      '_searchQuery',
      '_contextQuery',
      '_startingEntitiesIDs',
      '_selectedEntitiesIDs'
    ]);

    if (this._gridManagerComponent) {
      var gridElementId = this._gridElementId;

      if (!$q.isNullOrWhiteSpace(gridElementId)) {
        this._gridManagerComponent.removeGrid(gridElementId);
      }

      this._gridManagerComponent = null;
    }

    var $grid = $(this._gridElement);
    var gridComponent = this._gridComponent;

    if (gridComponent) {
      if (this._contextMenuComponent) {
        if (gridComponent.selectable) {
          var $headerCheckbox = this._getHeaderCheckbox();

          if (!$q.isNullOrEmpty($headerCheckbox)) {
            if (this._allowMultipleRowSelection) {
              $headerCheckbox.unbind('click', this._onHeaderCheckboxClickHandler);
            }
          }

          $headerCheckbox = null;
          gridComponent.$tbody
            .undelegate(this.ROW_CLICKABLE_SELECTORS + ' td', 'click', this._onRowCellClickHandler)
            .undelegate(this.ROW_CLICKABLE_SELECTORS + ' ' + this.CHECKBOX_CELL_SELECTORS, 'click', this._onRowCheckboxCellClickHandler);
        }

        var contextMenuEventType = this._contextMenuComponent.getContextMenuEventType();

        $grid
          .unbind('dataBinding')
          .unbind('dataBound')
          .unbind('rowDataBound')
          .undelegate(this.ROW_CLICKABLE_SELECTORS, contextMenuEventType, this._onContextMenuHandler);

        this._contextMenuComponent.detachObserver(EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onRowContextMenuShowingHandler);
        this._contextMenuComponent.detachObserver(EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onRowContextMenuItemClickingHandler);
        this._contextMenuComponent.detachObserver(EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onRowContextMenuHiddenHandler);

        this._contextMenuComponent.dispose();
        this._contextMenuComponent = null;
      }

      gridComponent.element = null;
      gridComponent = null;

      this._gridComponent = null;
    }

    $grid.removeData('tGrid').empty();
    $grid = null;

    if (this._deselectAllId) {
      $('#' + this._deselectAllId).unbind('click', this._onDeselectAllClickHandler);
    }

    if (this._selectAllId) {
      $('#' + this._selectAllId).unbind('click', this._onSelectAllClickHandler);
    }

    Quantumart.QP8.Utils.dispose.call(this, [
      '_gridElement',
      '_onDataBindingHandler',
      '_onDataBoundHandler',
      '_onRowDataBoundHandler',
      '_onHeaderCheckboxClickHandler',
      '_onTitleLinkClickHandler',
      '_onRowCellClickHandler',
      '_onRowCheckboxCellClickHandler',
      '_onContextMenuHandler',
      '_onRowContextMenuShowingHandler',
      '_onRowContextMenuItemClickingHandler',
      '_onRowContextMenuHiddenHandler',
      '_onSelectAllClickHandler',
      '_onDeselectAllClickHandler',
      '_onDataBindingHandler',
      '_onDataBindingHandler'
    ]);

    Quantumart.QP8.Utils.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendEntityGrid.registerClass('Quantumart.QP8.BackendEntityGrid', Quantumart.QP8.Observable);

//#endregion
