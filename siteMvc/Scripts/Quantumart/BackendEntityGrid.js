/* eslint max-lines: 'off' */

window.EVENT_TYPE_ENTITY_GRID_DATA_BINDING = 'OnEntityGridDataBinding';
window.EVENT_TYPE_ENTITY_GRID_DATA_BOUND = 'OnEntityGridDataBound';
window.EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING = 'OnEntityGridActionExecuting';
window.EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED = 'OnEntityGridEntitySelected';
window.EVENT_TYPE_ENTITY_GRID_TITLE_LINK_CLICK = 'OnEntityGridEntityTitleLinkClick';

class BackendEntityGrid extends Quantumart.QP8.Observable {
  static applyStatusColor(row, item) {
    if (item.STATUS_TYPE_COLOR) {
      const $row = $(row);
      const isAlt = $row.hasClass('t-alt');
      const className = isAlt ? `t-alt-${item.STATUS_TYPE_COLOR}` : `t-${item.STATUS_TYPE_COLOR}`;
      $row.removeClass('t-alt').addClass(className);
    }
  }

  // eslint-disable-next-line max-statements, complexity, max-params
  constructor(
    gridGroupCodes,
    gridElementId,
    entityTypeCode,
    parentEntityId,
    actionCode,
    options,
    hostOptions
  ) {
    super();

    this.GRID_BUSY_CLASS_NAME = 'busy';
    this.ROW_SELECTED_CLASS_NAME = 't-state-selected';
    this.TITLE_CELL_CLASS_NAME = 'title';
    this.ID_CELL_CLASS_NAME = 'id';
    this.ROW_CLICKABLE_SELECTORS = 'tbody tr:not(.t-grouping-row,.t-detail-row,.t-no-data,:has(> .t-edit-container))';
    this.CHECKBOX_HEADER_SELECTORS = '.t-select-header input:checkbox';
    this.CHECKBOX_CELL_SELECTORS = '.t-select-cell input:checkbox';

    this._currentRowId = -1;
    this._allowMultipleRowSelection = false;
    this._allowGlobalSelection = false;
    this._actionCodeForLink = '';
    this._contextMenuCode = '';
    this._keyColumnName = 'Id';
    this._titleColumnName = 'Name';
    this._parentKeyColumnName = 'ParentId';
    this._linkOpenNewTab = false;
    this._startingEntitiesIDs = [];
    this._selectedEntitiesIDs = [];
    this._allowFilterSelectedEntities = false;
    this._removedIds = [];
    this._allowSaveRowsSelection = true;
    this._stopDeferredOperations = false;
    this._isDataLoaded = false;
    this._autoGenerateLink = false;
    this._generateLinkOnTitle = false;
    this._autoLoad = true;
    this._delayAutoLoad = false;
    this._contextMenuActionCode = '';
    this._filter = '';
    this._hostIsWindow = false;
    this._treeFieldId = 0;
    this._isBindToExternal = false;
    this._deselectAllId = '';
    this._selectAllId = '';
    this._zIndex = 0;

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

      if (options.articlesCountId) {
        this.articlesCountId = options.articlesCountId;
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

    $q.bindProxies.call(this, [
      '_onDataBinding',
      '_onDataBound',
      '_onRowDataBound',
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
  }

  // eslint-disable-next-line camelcase
  get_gridGroupCodes() {
    return this._gridGroupCodes;
  }

  // eslint-disable-next-line camelcase
  get_gridElementId() {
    return this._gridElementId;
  }

  // eslint-disable-next-line camelcase
  set_gridElementId(value) {
    this._gridElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_gridElement() {
    return this._gridElement;
  }

  // eslint-disable-next-line camelcase
  set_gridElement(value) {
    this._gridElement = value;
  }

  // eslint-disable-next-line camelcase
  get_gridComponent() {
    return this._gridComponent;
  }

  // eslint-disable-next-line camelcase
  get_entityTypeCode() {
    return this._entityTypeCode;
  }

  // eslint-disable-next-line camelcase
  set_entityTypeFromRow(rowElem) {
    const $row = this.getRow(rowElem);
    const dataItem = this.getDataItem($row);

    if (dataItem.EntityTypeCode) {
      this._entityTypeCode = dataItem.EntityTypeCode;
    }
  }

  // eslint-disable-next-line camelcase
  set_actionCodeFromRow(rowElem) {
    const $row = this.getRow(rowElem);
    const dataItem = this.getDataItem($row);

    if (dataItem.ActionCode) {
      this._actionCodeForLink = dataItem.ActionCode;
    }
  }

  // eslint-disable-next-line camelcase
  set_entityTypeCode(value) {
    this._entityTypeCode = value;
  }

  // eslint-disable-next-line camelcase
  get_parentEntityId() {
    return this._parentEntityId;
  }

  // eslint-disable-next-line camelcase
  set_parentEntityId(value) {
    this._parentEntityId = value;
  }

  // eslint-disable-next-line camelcase
  get_actionCode() {
    return this._actionCode;
  }

  // eslint-disable-next-line camelcase
  set_actionCode(value) {
    this._actionCode = value;
  }

  // eslint-disable-next-line camelcase
  get_actionCodeForLink() {
    return this._actionCodeForLink;
  }

  // eslint-disable-next-line camelcase
  set_actionCodeForLink(value) {
    this._actionCodeForLink = value;
  }

  // eslint-disable-next-line camelcase
  get_allowMultipleRowSelection() {
    return this._allowMultipleRowSelection;
  }

  // eslint-disable-next-line camelcase
  set_allowMultipleRowSelection(value) {
    this._allowMultipleRowSelection = value;
  }

  // eslint-disable-next-line camelcase
  get_allowGlobalSelection() {
    return this._allowGlobalSelection;
  }

  // eslint-disable-next-line camelcase
  set_allowGlobalSelection(value) {
    this._allowGlobalSelection = value;
  }

  // eslint-disable-next-line camelcase
  get_selectedEntitiesIDs() {
    return this._selectedEntitiesIDs;
  }

  // eslint-disable-next-line camelcase
  set_selectedEntitiesIDs(value) {
    this._selectedEntitiesIDs = value;
  }

  // eslint-disable-next-line camelcase
  get_contextMenuCode() {
    return this._contextMenuCode;
  }

  // eslint-disable-next-line camelcase
  set_contextMenuCode(value) {
    this._contextMenuCode = value;
  }

  // eslint-disable-next-line camelcase
  get_keyColumnName() {
    return this._keyColumnName;
  }

  // eslint-disable-next-line camelcase
  set_keyColumnName(value) {
    this._keyColumnName = value;
  }

  // eslint-disable-next-line camelcase
  get_titleColumnName() {
    return this._titleColumnName;
  }

  // eslint-disable-next-line camelcase
  set_titleColumnName(value) {
    this._titleColumnName = value;
  }

  // eslint-disable-next-line camelcase
  get_autoGenerateLink() {
    return this._autoGenerateLink;
  }

  // eslint-disable-next-line camelcase
  set_autoGenerateLink(value) {
    this._autoGenerateLink = value;
  }

  // eslint-disable-next-line camelcase
  get_gridManager() {
    return this._gridManagerComponent;
  }

  // eslint-disable-next-line camelcase
  set_gridManager(value) {
    this._gridManagerComponent = value;
  }

  // eslint-disable-next-line camelcase
  get_treeFieldId() {
    return this._treeFieldId;
  }

  // eslint-disable-next-line camelcase
  set_treeFieldId(value) {
    this._treeFieldId = value;
  }

  // eslint-disable-next-line camelcase
  get_currentPage() {
    if (this._gridComponent) {
      return this._gridComponent.currentPage;
    }

    return undefined;
  }

  // eslint-disable-next-line camelcase
  get_orderBy() {
    if (this._gridComponent) {
      return this._gridComponent.orderBy;
    }

    return undefined;
  }


  // eslint-disable-next-line max-statements
  initialize() {
    $('.fullTextBlock label').addClass('hidden');

    const $grid = $(`#${this._gridElementId}`);
    const gridComponent = $grid.data('tGrid');

    if (this._currentPage) {
      gridComponent.currentPage = this._currentPage;
    }

    if (this._orderBy) {
      gridComponent.orderBy = this._orderBy;
    }

    this._gridElement = $grid.get(0);
    this._gridComponent = gridComponent;
    this._startingEntitiesIDs = this._selectedEntitiesIDs.slice();

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
      const $headerCheckbox = this._getHeaderCheckbox();

      if ($headerCheckbox) {
        if (this._allowMultipleRowSelection) {
          $headerCheckbox.bind('click', this._onHeaderCheckboxClickHandler);
        } else {
          $headerCheckbox.css('display', 'none');
        }
      }

      gridComponent.$tbody
        .undelegate(this.ROW_CLICKABLE_SELECTORS, 'click')
        .delegate(`${this.ROW_CLICKABLE_SELECTORS} td`, 'click', this._onRowCellClickHandler)
        .delegate(
          `${this.ROW_CLICKABLE_SELECTORS} ${this.CHECKBOX_CELL_SELECTORS}`,
          'click',
          this._onRowCheckboxCellClickHandler
        );
    }

    if (this._contextMenuCode) {
      const contextMenuComponent = new Quantumart.QP8.BackendContextMenu(
        this._contextMenuCode,
        `${this._gridElementId}_ContextMenu`,
        {
          targetElements: this._gridElement,
          allowManualShowing: 'true',
          isBindToExternal: this._isBindToExternal,
          zIndex: this._zIndex
        }
      );

      contextMenuComponent.initialize();
      contextMenuComponent.addMenuItemsToMenu(true);

      $grid.delegate(
        this.ROW_CLICKABLE_SELECTORS,
        $.fn.jeegoocontext.getContextMenuEventType(),
        this._onContextMenuHandler
      );

      contextMenuComponent.attachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_SHOWING,
        this._onRowContextMenuShowingHandler
      );

      contextMenuComponent.attachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING,
        this._onRowContextMenuItemClickingHandler
      );

      contextMenuComponent.attachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_HIDDEN,
        this._onRowContextMenuHiddenHandler
      );

      this._contextMenuComponent = contextMenuComponent;
    }

    if (this._deselectAllId) {
      $(`#${this._deselectAllId}`).bind('click', this._onDeselectAllClickHandler);
    }

    if (this._selectAllId) {
      $(`#${this._selectAllId}`).bind('click', this._onSelectAllClickHandler);
    }
  }

  _getCurrentAction() {
    return $a.getBackendActionByCode(this._actionCode);
  }

  _getTitleLinks() {
    return this.getRows().find(`${this._getLinkCellSelector()} > A`);
  }

  _addLinkToCell(rowElem) {
    let $row = $q.toJQuery(rowElem);
    let $titleCell = $row.find(this._getLinkCellSelector());

    if ($titleCell) {
      $titleCell.wrapInner('<a href="javascript:void(0)" />');
      $titleCell
        .find('A')
        .addClass(this._linkOpenNewTab ? 'js' : 'html')
        .bind('click', this._onTitleLinkClickHandler)
        .bind('mouseup', this._onTitleLinkClickHandler);
    }

    $titleCell = null;
    $row = null;
  }

  _getLinkCellSelector() {
    let result = `TD.${this.ID_CELL_CLASS_NAME}`;
    if (this._generateLinkOnTitle) {
      result += `, TD.${this.TITLE_CELL_CLASS_NAME}`;
    }

    return result;
  }

  _removeLinksFromCells() {
    this._getTitleLinks()
      .unbind('click', this._onTitleLinkClickHandler)
      .unbind('mouseup', this._onTitleLinkClickHandler)
      .empty()
      .remove();
  }

  getRows() {
    let $rows = [];
    if (this._gridComponent) {
      $rows = this._gridComponent.$rows();
    }

    return $rows;
  }

  getSelectedRows() {
    return this.getRows().filter(`.${this.ROW_SELECTED_CLASS_NAME}`);
  }

  getRow(row) {
    let $row = null;

    if ($q.isObject(row)) {
      $row = $q.toJQuery(row);
    } else if ($.isNumeric(row)) {
      const rowIndex = parseInt(row, 10);
      $row = this.getRows().eq(rowIndex);
    }

    return $row;
  }

  getRowByEntityId(entityId) {
    return [].find.call(this.getRows(), function (rowEl) {
      return this.getEntityId(rowEl) === +entityId;
    }, this);
  }

  getRowsByEntityIds(entityIds) {
    return [].filter.call(this.getRows(), function (rowEl) {
      return entityIds.includes(this.getEntityId(rowEl), this);
    }, this);
  }

  selectRow(rowElem, saveOtherRowsSelection) {
    if (!saveOtherRowsSelection) {
      this._setRowsSelectedState(this.getRows(), false);
      if (!this._allowMultipleRowSelection && this._allowGlobalSelection) {
        this._resetRowSelectionState();
      }
    }

    const $row = this.getRow(rowElem);
    this._setRowsSelectedState($row, !this.isRowSelected($row));
    this._saveRowSelectionState();
    this._executePostSelectActions();
  }

  selectRows(rowElems) {
    this._setRowsSelectedState(this.getRows(), false);
    if (rowElems) {
      this._setRowsSelectedState($q.toJQuery(rowElems), true);
    }

    this._saveRowSelectionState();
    this._executePostSelectActions();
  }

  selectAllRows() {
    this._setRowsSelectedState(this.getRows(), true);
    this._saveRowAllSelectionState();
    this._executePostSelectActions();
  }

  deselectAllRows() {
    this._setRowsSelectedState(this.getRows(), false);
    this._selectedEntitiesIDs = [];
    this._executePostSelectActions();
  }

  toggleDirectChildRows(parentArticleId, rowState) {
    this.getChildEntityIds(parentArticleId).done(response => {
      const $rowsToModify = $(this.getRowsByEntityIds(response.data));
      this._setRowsSelectedState($rowsToModify, rowState);
      if (rowState) {
        this._selectedEntitiesIDs = [...new Set(this._selectedEntitiesIDs.concat(response.data))];
      } else {
        this._selectedEntitiesIDs = $q.difference(this._selectedEntitiesIDs, response.data);
      }

      this._saveRowSelectionState();
      this._executePostSelectActions();
    });
  }

  selectPageRows(value) {
    this._setRowsSelectedState(this.getRows(), value);
    this._saveRowSelectionState();
    this._executePostSelectActions();
  }

  isRowSelected(rowElem) {
    return this.getRow(rowElem).hasClass(this.ROW_SELECTED_CLASS_NAME);
  }

  _setRowsSelectedState($rows, state) {
    $rows[state ? 'addClass' : 'removeClass'](this.ROW_SELECTED_CLASS_NAME);
    $rows.find(this.CHECKBOX_CELL_SELECTORS).prop('checked', state);
  }

  _getHeaderCheckbox() {
    return $(this.CHECKBOX_HEADER_SELECTORS, this._gridComponent.$header);
  }

  _refreshHeaderCheckbox() {
    this._getHeaderCheckbox().prop('checked', !!this._isAllRowsSelectedInCurrentPage());
  }

  _refreshCancelSelection() {
    if (this._deselectAllId) {
      const $linkButton = $(`#${this._deselectAllId}`);
      const actionLink = $linkButton.data('action_link_component');

      if (actionLink) {
        if (this._selectedEntitiesIDs.length > 0) {
          actionLink.enableActionLink();
        } else {
          actionLink.disableActionLink();
        }
      }
    }
  }

  getDataItem(rowElem) {
    const $row = this.getRow(rowElem);
    return $row ? this._gridComponent.dataItem($row.get(0)) : null;
  }

  getEntityId(rowElem) {
    const dataItem = this.getDataItem(this.getRow(rowElem));
    return dataItem && dataItem[this._keyColumnName] ? dataItem[this._keyColumnName] : 0;
  }

  getEntityName(rowElem) {
    const dataItem = this.getDataItem(this.getRow(rowElem));
    return dataItem && dataItem[this._titleColumnName] ? dataItem[this._titleColumnName] : '';
  }

  getParentEntityId(rowElem) {
    const dataItem = this.getDataItem(this.getRow(rowElem));
    return dataItem && dataItem[this._parentKeyColumnName] ? dataItem[this._parentKeyColumnName] : this._parentEntityId;
  }

  getChildEntityIds(parentArticleId) {
    return $q.getAjax(`${window.CONTROLLER_URL_ARTICLE}GetChildArticleIds`, {
      ids: [parentArticleId],
      filter: this._filter,
      fieldId: this._treeFieldId
    });
  }

  checkExistEntityInCurrentPage(entityId) {
    return !!this.getRowByEntityId(entityId);
  }

  getEntitiesFromRows(rows) {
    const entities = [];
    for (let rowIndex = 0; rowIndex < rows.length; rowIndex++) {
      let entityId = 0;
      let entityName = '';
      const dataItem = this.getDataItem(rows[rowIndex]);

      if (dataItem) {
        if (dataItem[this._keyColumnName]) {
          entityId = dataItem[this._keyColumnName];
        }

        if (dataItem[this._titleColumnName]) {
          entityName = `${dataItem[this._titleColumnName]}`;
        } else {
          entityName = entityId;
        }

        if (entityId) {
          Array.add(entities, { Id: entityId, Name: entityName });
        }
      }
    }

    return entities;
  }

  getSelectedEntities() {
    let selectedEntities = [];
    if (this._allowGlobalSelection) {
      const rows = this.getRowsByEntityIds(this._selectedEntitiesIDs);
      const that = this;

      selectedEntities = rows.map(row => ({ Id: that.getEntityId(row), Name: that.getEntityName(row) }));
      const notFoundIds = $.grep(this._selectedEntitiesIDs, elem => {
        for (let i = 0; i < selectedEntities.length; i++) {
          if (selectedEntities[i].Id === elem) {
            return false;
          }
        }

        return true;
      });

      $.each(notFoundIds, (i, elem) => {
        selectedEntities.push({ Id: elem, Name: '' });
      });
    } else {
      selectedEntities = this.getEntitiesFromRows(this.getSelectedRows());
    }

    return selectedEntities;
  }

  resetGrid(options) {
    if ($q.isObject(options)) {
      if (!$q.isNull(options.searchQuery)) {
        this._searchQuery = options.searchQuery;
      }

      if (!$q.isNull(options.contextQuery)) {
        this._contextQuery = options.contextQuery;
      }
    }

    this._gridComponent.currentPage = 1;
    if (this._gridComponent) {
      this._gridComponent.ajaxRequest();
    }
  }

  refreshGrid(options) {
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

    const gridComponent = this._gridComponent;
    if (gridComponent) {
      gridComponent.ajaxRequest();
    }
  }

  markGridAsBusy() {
    $(this._gridElement).addClass(this.GRID_BUSY_CLASS_NAME);
  }

  unmarkGridAsBusy() {
    $(this._gridElement).removeClass(this.GRID_BUSY_CLASS_NAME);
  }

  isGridBusy() {
    return $(this._gridElement).hasClass(this.GRID_BUSY_CLASS_NAME);
  }

  executeAction(row, actionCode, followLink, ctrlKey) {
    const $row = this.getRow(row);
    if ($row) {
      const action = $a.getBackendActionByCode(actionCode);
      if (!action) {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
        throw new Error($l.Common.ajaxDataReceivingErrorMessage);
      }

      const entityId = this.getEntityId($row);
      const context = { ctrlKey };
      if (actionCode === window.ACTION_CODE_ADD_NEW_CHILD_ARTICLE) {
        context.additionalUrlParameters = {
          fieldId: this._treeFieldId,
          articleId: entityId,
          isChild: true
        };
      }

      if (actionCode === window.ACTION_TYPE_SELECT_CHILD_ARTICLES) {
        this.toggleDirectChildRows(entityId, true);
      }

      if (actionCode === window.ACTION_TYPE_UNSELECT_CHILD_ARTICLES) {
        this.toggleDirectChildRows(entityId, false);
      }

      const entityName = this.getEntityName($row);
      const params = new Quantumart.QP8.BackendActionParameters({
        entityTypeCode: this._entityTypeCode,
        entityId,
        entityName,
        entities: action.ActionType.IsMultiple ? [{ Id: entityId, Name: entityName }] : null,
        parentEntityId: this.getParentEntityId($row),
        context
      });

      params.correct(action);
      const eventArgs = $a.getEventArgsFromActionWithParams(action, params);
      eventArgs.set_startedByExternal(this._isBindToExternal);

      const message = Quantumart.QP8.Backend.getInstance().checkOpenDocumentByEventArgs(eventArgs);
      if (this._hostIsWindow) {
        if (message) {
          $q.alertError(message);
        } else {
          eventArgs.set_isWindow(true);
          this.notify(window.EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING, eventArgs);
        }
      } else if (followLink && !this._linkOpenNewTab && !message) {
        this.notify(window.EVENT_TYPE_ENTITY_GRID_TITLE_LINK_CLICK, eventArgs);
      } else {
        this.notify(window.EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING, eventArgs);
      }
    }
  }

  _executePostSelectActions() {
    if (this.articlesCountId) {
      $(`#${this.articlesCountId}`).text(this._selectedEntitiesIDs.length);
    }

    this._raiseSelectEvent();
  }

  _raiseSelectEvent() {
    const action = this._getCurrentAction();
    const eventArgs = $a.getEventArgsFromAction(action);

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

    this.notify(window.EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED, eventArgs);

    this._refreshHeaderCheckbox();
    this._refreshCancelSelection();
  }

  _isAllRowsSelectedInCurrentPage() {
    const $rows = this.getRows();
    const $selectedRows = this.getSelectedRows();
    return $rows.length === $selectedRows.length;
  }

  _saveRowSelectionState() {
    const $rows = this.getRows();
    const selectedRowEntityIdsSet = new Set(this._selectedEntitiesIDs);
    const unselectedRowEntityIdsSet = new Set();
    for (let rowIndex = 0; rowIndex < $rows.length; rowIndex++) {
      const $row = $rows.eq(rowIndex);
      const rowEntityId = this.getEntityId($row);
      if (this.isRowSelected($row)) {
        selectedRowEntityIdsSet.add(rowEntityId);
      } else {
        unselectedRowEntityIdsSet.add(rowEntityId);
      }
    }

    this._selectedEntitiesIDs = $q.difference([...selectedRowEntityIdsSet], [...unselectedRowEntityIdsSet]);
  }

  _saveRowAllSelectionState() {
    let eventArgs = null;
    const onlyOnePage = !(this.getRows().length < this._gridComponent.total);

    if (onlyOnePage) {
      this._saveRowSelectionState();
    } else {
      const url = this._gridComponent.url('selectUrl');
      const queryData = Object.assign({ page: 1, size: 0, onlyIds: true }, this._createDataQueryParams());
      const action = this._getCurrentAction();

      if (action) {
        eventArgs = $a.getEventArgsFromAction(action);
        eventArgs.set_isMultipleEntities(true);
        eventArgs.set_entityTypeCode(this._entityTypeCode);
        eventArgs.set_entities(this.getSelectedEntities());
        eventArgs.set_parentEntityId(this._parentEntityId);
      }

      if (action) {
        this.notify(window.EVENT_TYPE_ENTITY_GRID_DATA_BINDING, eventArgs);
      }

      let rowsData = null;
      $q.postDataToUrl(url, queryData, false, data => {
        rowsData = data;
      }, $q.processGenericAjaxError);

      if (action) {
        this.notify(window.EVENT_TYPE_ENTITY_GRID_DATA_BOUND, eventArgs);
      }

      const that = this;
      if (rowsData && rowsData.data) {
        this._selectedEntitiesIDs = rowsData.data.map(item => item[that._keyColumnName]);
      }
    }
  }

  _restoreRowSelectionState() {
    const that = this;
    const selectedRowElems = [];

    this.getRows().each((rowIndex, rowElem) => {
      const $row = $(rowElem);
      const entityId = that.getEntityId($row);

      if (Array.contains(that._selectedEntitiesIDs, entityId)) {
        Array.add(selectedRowElems, rowElem);
      }
    });

    this.selectRows(selectedRowElems);
  }

  _resetRowSelectionState() {
    $q.clearArray(this._selectedEntitiesIDs);
  }

  _fixGridWidth() {
    const gridElement = this.get_gridElement();
    const tableElement = $('table', gridElement).get(0);

    if (tableElement.offsetWidth > gridElement.offsetWidth) {
      $(gridElement).css('width', `${tableElement.offsetWidth}px`);
    }
  }

  _createDataQueryParams() {
    const params = { gridParentId: this._parentEntityId };

    if (this._startingEntitiesIDs) {
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
  }

  _onDataBinding(e) {
    const params = this._createDataQueryParams();

    // eslint-disable-next-line no-param-reassign
    Object.assign(e, { data: params });
    if (this._isDataLoaded) {
      const action = this._getCurrentAction();
      if (action) {
        const eventArgs = $a.getEventArgsFromAction(action);
        eventArgs.set_isMultipleEntities(true);
        eventArgs.set_entityTypeCode(this._entityTypeCode);
        eventArgs.set_entities(this.getSelectedEntities());
        eventArgs.set_parentEntityId(this._parentEntityId);
        this.notify(window.EVENT_TYPE_ENTITY_GRID_DATA_BINDING, eventArgs);
      }

      if (this._allowSaveRowsSelection) {
        this._saveRowSelectionState();
      } else {
        this._resetRowSelectionState();
      }

      const contextMenuComponent = this._contextMenuComponent;
      if (contextMenuComponent) {
        contextMenuComponent.hideMenu();
      }
    }
  }

  _onDataBound() {
    const grid = this._gridComponent;
    if (grid.currentPage > 1 && !grid.total) {
      grid.pageTo(grid.currentPage - 1);
    }

    this._isDataLoaded = true;
    this._fixGridWidth();

    const that = this;
    this._selectedEntitiesIDs = $.grep(this._selectedEntitiesIDs, item => !Array.contains(that._removedIds, item));

    this._removedIds = [];
    const action = this._getCurrentAction();

    if (action) {
      const eventArgs = $a.getEventArgsFromAction(action);

      eventArgs.set_isMultipleEntities(true);
      eventArgs.set_entityTypeCode(this._entityTypeCode);
      eventArgs.set_entities(this.getSelectedEntities());
      eventArgs.set_parentEntityId(this._parentEntityId);

      this.notify(window.EVENT_TYPE_ENTITY_GRID_DATA_BOUND, eventArgs);
    }

    if (this._allowSaveRowsSelection) {
      this._restoreRowSelectionState();
    }
  }

  _onRowDataBound(e) {
    BackendEntityGrid.applyStatusColor(e.row, e.dataItem);
    if (this._autoGenerateLink) {
      this._addLinkToCell(e.row);
    }
  }

  _onHeaderCheckboxClick(e) {
    this.selectPageRows($(e.currentTarget).is(':checked'));
  }

  _onTitleLinkClick(e) {
    e.preventDefault();
    const isMiddleClick = e.type === 'mouseup' && e.which === 2;
    const isLeftClick = e.type === 'click' && (e.which === 1 || e.which === 0);
    const isRightClick = !isLeftClick && !isMiddleClick;
    if (this.isGridBusy() || isRightClick) {
      return false;
    }

    const $row = $(e.currentTarget).parent().parent();
    this.set_actionCodeFromRow($row);
    this.set_entityTypeFromRow($row);

    const actionCodeForLink = this._actionCodeForLink;
    if (!actionCodeForLink) {
      $q.alertError('Вы не задали код действия, которое открывает форму редактирования сущности!');
      throw new Error('Вы не задали код действия, которое открывает форму редактирования сущности!');
    }

    this.executeAction(
      $row,
      actionCodeForLink,
      !e.ctrlKey && !e.shiftKey && !isMiddleClick,
      e.ctrlKey || isMiddleClick
    );

    return undefined;
  }

  _onRowCellClick(e) {
    const $target = $(e.target);
    if (!$target.is(':button, A, :input, A > .t-icon')) {
      e.stopPropagation();
      this.selectRow($target.closest('TR'), this._allowMultipleRowSelection);
    }

    if (this._contextMenuComponent) {
      this._contextMenuComponent.hideMenu();
    }
  }

  _onRowCheckboxCellClick(e) {
    const $checkbox = $(e.target).closest('INPUT:checkbox');
    if ($checkbox) {
      const $row = $checkbox.parent().parent();
      this.selectRow($row, this._allowMultipleRowSelection);
    }
  }

  _onContextMenu(e) {
    const $element = $(e.currentTarget);
    this._currentRowId = this.getEntityId($($element.closest('TR')[0]));
    if (this._contextMenuComponent) {
      this._contextMenuComponent.showMenu(e, $element.get(0));
    }

    e.preventDefault();
  }

  _onRowContextMenuShowing(eventType, sender, args) {
    const $element = $(args.get_targetElement());
    const menuComponent = args.get_menu();
    if (menuComponent && $element.length) {
      menuComponent.tuneMenuItems(this._currentRowId, this._parentEntityId);
    }
  }

  _onRowContextMenuItemClicking(eventType, sender, args) {
    const $menuItem = $(args.get_menuItem());
    if ($menuItem.length) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }
  }

  _onRowContextMenuHidden(eventType, sender, args) {
    if (this._contextMenuActionCode) {
      this.executeAction($(args.get_targetElement()), this._contextMenuActionCode, false, false);
      this._contextMenuActionCode = '';
    }
  }

  _onSelectAllClick() {
    this.selectAllRows();
  }

  _onDeselectAllClick() {
    this.deselectAllRows();
  }

  onLoad() {
    if (this._autoLoad && this._delayAutoLoad) {
      this.refreshGrid();
    }
  }

  dispose() {
    super.dispose();

    this._stopDeferredOperations = true;
    if (this._autoGenerateLink) {
      this._removeLinksFromCells();
    }

    this._resetRowSelectionState();
    if (this._gridManagerComponent) {
      if (this._gridElementId) {
        this._gridManagerComponent.removeGrid(this._gridElementId);
      }
    }

    const $grid = $(this._gridElement);
    if (this._gridComponent) {
      if (this._contextMenuComponent) {
        if (this._gridComponent.selectable) {
          const $headerCheckbox = this._getHeaderCheckbox();
          if ($headerCheckbox && this._allowMultipleRowSelection) {
            $headerCheckbox.unbind('click', this._onHeaderCheckboxClickHandler);
          }

          this._gridComponent.$tbody
            .undelegate(
              `${this.ROW_CLICKABLE_SELECTORS} td`,
              'click',
              this._onRowCellClickHandler
            )
            .undelegate(
              `${this.ROW_CLICKABLE_SELECTORS} ${this.CHECKBOX_CELL_SELECTORS}`,
              'click',
              this._onRowCheckboxCellClickHandler
            );
        }

        $grid
          .unbind('dataBinding')
          .unbind('dataBound')
          .unbind('rowDataBound')
          .undelegate(
            this.ROW_CLICKABLE_SELECTORS,
            $.fn.jeegoocontext.getContextMenuEventType(),
            this._onContextMenuHandler
          );

        this._contextMenuComponent.detachObserver(
          window.EVENT_TYPE_CONTEXT_MENU_SHOWING,
          this._onRowContextMenuShowingHandler
        );

        this._contextMenuComponent.detachObserver(
          window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING,
          this._onRowContextMenuItemClickingHandler
        );

        this._contextMenuComponent.detachObserver(
          window.EVENT_TYPE_CONTEXT_MENU_HIDDEN,
          this._onRowContextMenuHiddenHandler
        );

        this._contextMenuComponent.dispose();
      }
    }

    $grid.removeData('tGrid').empty();
    if (this._deselectAllId) {
      $(`#${this._deselectAllId}`).unbind('click', this._onDeselectAllClickHandler);
    }

    if (this._selectAllId) {
      $(`#${this._selectAllId}`).unbind('click', this._onSelectAllClickHandler);
    }

    $q.dispose.call(this, [
      '_searchQuery',
      '_contextQuery',
      '_startingEntitiesIDs',
      '_selectedEntitiesIDs',
      '_gridComponent',
      '_gridManagerComponent',
      '_contextMenuComponent',
      '_gridElement',
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
      '_onDataBindingHandler'
    ]);

    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendEntityGrid = BackendEntityGrid;
