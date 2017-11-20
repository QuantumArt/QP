/* eslint max-lines: 'off' */

window.EVENT_TYPE_ENTITY_TREE_DATA_BINDING = 'OnEntityTreeDataBinding';
window.EVENT_TYPE_ENTITY_TREE_DATA_BOUND = 'OnEntityTreeDataBound';
window.EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING = 'OnEntityTreeActionExecuting';
window.EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED = 'OnEntityTreeEntitySelected';

// eslint-disable-next-line max-params, max-statements
Quantumart.QP8.BackendEntityTree = function (
  treeGroupCode,
  treeElementId,
  entityTypeCode,
  parentEntityId,
  actionCode,
  options,
  hostOptions
) {
  Quantumart.QP8.BackendEntityTree.initializeBase(this, [treeElementId, options]);
  this._treeGroupCode = treeGroupCode;
  this._entityTypeCode = entityTypeCode;
  this._parentEntityId = parentEntityId;
  this._actionCode = actionCode;
  if ($q.isObject(options)) {
    if (options.selectedEntitiesIDs) {
      this._selectedEntitiesIDs = options.selectedEntitiesIDs;
    }

    if (options.contextMenuCode) {
      this._contextMenuCode = options.contextMenuCode;
    }

    if (options.rootEntityId) {
      this._rootEntityId = options.rootEntityId;
    }

    if (!$q.isNull(options.allowMultipleNodeSelection)) {
      this._allowMultipleNodeSelection = options.allowMultipleNodeSelection;
    }

    if (!$q.isNull(options.allowGlobalSelection)) {
      this._allowGlobalSelection = options.allowGlobalSelection;
    }

    if (!$q.isNull(options.treeName)) {
      this._treeElementName = options.treeName;
    }

    if (options.filter) {
      this._filter = options.filter;
    }

    if (options.treeFieldId) {
      this._treeFieldId = options.treeFieldId;
    }

    if (options.zIndex) {
      this._zIndex = options.zIndex;
    }

    if (options.articlesCountId) {
      this.articlesCountId = options.articlesCountId;
    }
  }

  if ($q.isObject(hostOptions)) {
    if (hostOptions.searchQuery) {
      this._searchQuery = hostOptions.searchQuery;
    }

    if (hostOptions.filter) {
      this._hostFilter = hostOptions.filter;
    }
  }

  $q.bindProxies.call(this, [
    '_onDataBinding',
    '_onNodeClicking',
    '_onContextMenu',
    '_onNodeContextMenuShowing',
    '_onNodeContextMenuItemClicking',
    '_onNodeContextMenuHidden'
  ]);
};

Quantumart.QP8.BackendEntityTree.prototype = {
  _treeGroupCode: '',
  _currentNodeId: -1,
  _allowMultipleNodeSelection: false,
  _allowGlobalSelection: false,
  _entityTypeCode: '',
  _parentEntityId: 0,
  _actionCode: '',
  _contextMenuCode: '',
  _selectedEntitiesIDs: [],
  _allowSaveNodesSelection: true,
  _isDataLoaded: false,
  _contextMenuComponent: null,
  _treeManagerComponent: null,
  _rootEntityId: null,
  _contextMenuActionCode: '',
  _filter: '',
  _hostFilter: '',
  _zIndex: 0,

  // eslint-disable-next-line camelcase
  get_treeGroupCode() {
    return this._treeGroupCode;
  },

  // eslint-disable-next-line camelcase
  set_treeGroupCode() {
    window.console.error('TODO: delete this call');
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
  get_actionCode() {
    return this._actionCode;
  },

  // eslint-disable-next-line camelcase
  set_actionCode(value) {
    this._actionCode = value;
  },

  // eslint-disable-next-line camelcase
  get_allowMultipleNodeSelection() {
    return this._allowMultipleNodeSelection;
  },

  // eslint-disable-next-line camelcase
  set_allowMultipleNodeSelection(value) {
    this._allowMultipleNodeSelection = value;
  },

  // eslint-disable-next-line camelcase
  get_allowGlobalSelection() {
    return this._allowGlobalSelection;
  },

  // eslint-disable-next-line camelcase
  set_allowGlobalSelection(value) {
    this._allowGlobalSelection = value;
  },

  // eslint-disable-next-line camelcase
  get_selectedEntitiesIDs() {
    return this._selectedEntitiesIDs;
  },

  // eslint-disable-next-line camelcase
  set_selectedEntitiesIDs(value) {
    this._selectedEntitiesIDs = value;
  },

  // eslint-disable-next-line camelcase
  get_contextMenuCode() {
    return this._contextMenuCode;
  },

  // eslint-disable-next-line camelcase
  set_contextMenuCode(value) {
    this._contextMenuCode = value;
  },

  // eslint-disable-next-line camelcase
  get_treeManager() {
    return this._treeManagerComponent;
  },

  // eslint-disable-next-line camelcase
  set_treeManager(value) {
    this._treeManagerComponent = value;
  },

  // eslint-disable-next-line camelcase
  get_treeFieldId() {
    return this._treeFieldId;
  },

  // eslint-disable-next-line camelcase
  set_treeFieldId(value) {
    this._treeFieldId = value;
  },

  _onDataBindingHandler: null,
  _onDataBoundHandler: null,
  _onNodeClickingHandler: null,
  _onContextMenuHandler: null,
  _onNodeContextMenuShowingHandler: null,
  _onNodeContextMenuItemClickingHandler: null,
  _onNodeContextMenuHiddenHandler: null,

  initialize() {
    Quantumart.QP8.BackendEntityTree.callBaseMethod(this, 'initialize');
    $('.fullTextBlock label').removeClass('hidden');

    const treeComponent = this._treeComponent;
    treeComponent.showCheckBox = this._allowMultipleNodeSelection;

    const $tree = $(this._treeElement);
    $tree.bind('dataBound', this._onDataBoundHandler);
    if (!$q.isNullOrWhiteSpace(this._contextMenuCode)) {
      const contextMenuComponent = new Quantumart.QP8.BackendContextMenu(
        this._contextMenuCode,
        `${this._treeElementId}_ContextMenu`,
        {
          targetElements: this._treeElement,
          allowManualShowing: true,
          zIndex: this._zIndex
        }
      );

      contextMenuComponent.initialize();
      contextMenuComponent.addMenuItemsToMenu(false);

      const contextMenuEventType = $.fn.jeegoocontext.getContextMenuEventType();
      $tree.delegate(this.NODE_NEW_CLICKABLE_SELECTORS, contextMenuEventType, this._onContextMenuHandler);

      contextMenuComponent.attachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_SHOWING,
        this._onNodeContextMenuShowingHandler
      );

      contextMenuComponent.attachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING,
        this._onNodeContextMenuItemClickingHandler
      );

      contextMenuComponent.attachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_HIDDEN,
        this._onNodeContextMenuHiddenHandler
      );

      this._contextMenuComponent = contextMenuComponent;
    }

    this.refreshTree();
  },

  _getCurrentAction() {
    return $a.getBackendActionByCode(this._actionCode);
  },

  getNodeByEntityId(entityId, parentNodeElem) {
    return this.getNode(this.convertEntityIdToNodeCode(entityId), parentNodeElem);
  },

  getNodesByEntitiesIDs(entitiesIDs) {
    const selectedNodeElems = [];

    if (!$q.isNullOrEmpty(entitiesIDs)) {
      const that = this;
      const $nodes = this.getAllNodes();

      $nodes.each(
        (index, nodeElem) => {
          if (Array.contains(entitiesIDs, $q.toString(that.getEntityId(nodeElem)))) {
            Array.add(selectedNodeElems, nodeElem);
          }
        }
      );
    }

    return selectedNodeElems;
  },

  getParentNode(node) {
    let $parentNode = Quantumart.QP8.BackendEntityTree.callBaseMethod(this, 'getParentNode', [node]);
    if ($q.isNullOrEmpty($parentNode) && $q.isString(node)) {
      const entityTypeCode = this._entityTypeCode;
      const entityId = this.convertNodeCodeToEntityId(node);
      const parentId = $o.getParentEntityId(entityTypeCode, entityId);
      if (parentId) {
        $parentNode = this.getNode(this.convertEntityIdToNodeCode(parentId));
      }
    }

    return $parentNode;
  },

  getParentNodesByEntitiesIDs(entitiesIDs) {
    const entityCount = entitiesIDs.length;

    if (entityCount <= 0) {
      return $([]);
    }

    // Получаем коды родительских узлов
    let parentNodeCodes = [];

    for (let entityIndex = 0; entityIndex < entityCount; entityIndex++) {
      const entityId = entitiesIDs[entityIndex];
      const $node = this.getNodeByEntityId(entityId);

      if (!$q.isNullOrEmpty($node)) {
        const $parentNode = this.getParentNode($node);
        if (!$q.isNullOrEmpty($parentNode)) {
          const parentNodeCode = this.getNodeValue($parentNode);
          Array.add(parentNodeCodes, parentNodeCode);
        }
      }
    }

    if (parentNodeCodes.length) {
      parentNodeCodes = Array.distinct(parentNodeCodes);
    } else {
      parentNodeCodes = [this.ROOT_NODE_CODE];
    }

    const parentNodeInfos = [];
    for (let parentNodeCodeIndex = 0; parentNodeCodeIndex < parentNodeCodes.length; parentNodeCodeIndex++) {
      const parentNodeCode = parentNodeCodes[parentNodeCodeIndex];
      const level = this.getNodeLevel(parentNodeCode);
      Array.add(parentNodeInfos, { nodeCode: parentNodeCode, level });
    }

    $q.clearArray(parentNodeCodes);

    let levels = [];
    for (let parentNodeInfoIndex = 0; parentNodeInfoIndex < parentNodeInfos.length; parentNodeInfoIndex++) {
      const parentNodeInfo = parentNodeInfos[parentNodeInfoIndex];
      const level = parentNodeInfo.level;
      if (!Array.contains(levels, level)) {
        Array.add(levels, level);
      }
    }

    levels = levels.sort();
    const levelCount = levels.length;

    if (levelCount > 1) {
      for (let levelIndex = levelCount - 2; levelIndex >= 0; levelIndex--) {
        const level = levels[levelIndex];
        const nodeInfos = $.grep(parentNodeInfos, parentNodeInfo => parentNodeInfo.level === level);
        const nodeInfoCount = nodeInfos.length;
        const childNodeInfos = $.grep(parentNodeInfos, parentNodeInfo => parentNodeInfo.level > level);
        const childNodeInfoCount = childNodeInfos.length;
        if (childNodeInfoCount > 0) {
          for (let nodeInfoIndex = 0; nodeInfoIndex < nodeInfoCount; nodeInfoIndex++) {
            const nodeInfo = nodeInfos[nodeInfoIndex];
            const $node = this.getNode(nodeInfo.nodeCode);

            for (let childNodeInfoIndex = childNodeInfoCount - 1; childNodeInfoIndex >= 0; childNodeInfoIndex--) {
              const childNodeInfo = childNodeInfos[childNodeInfoIndex];
              const $childNode = this.getNode(childNodeInfo.nodeCode, $node);

              if (!$q.isNullOrEmpty($childNode)) {
                Array.remove(parentNodeInfos, childNodeInfo);
              }
            }
          }
        }

        $q.clearArray(nodeInfos);
        $q.clearArray(childNodeInfos);
      }
    }

    $q.clearArray(levels);

    const that = this;
    const parentNodeElems = parentNodeInfos.map(parentNodeInfo => that.getNode(parentNodeInfo.nodeCode).get(0));

    $q.clearArray(parentNodeInfos);
    return $q.toJQuery(parentNodeElems);
  },

  _isSearchQueryEmpty(searchQuery) {
    const query = searchQuery || this._searchQuery;

    if ($q.isNullOrWhiteSpace(query)) {
      return true;
    }

    let i, j;
    const jsonData = JSON.parse(query);

    for (i = 0; i < jsonData.length; i++) {
      for (j = 0; j < jsonData[i].QueryParams.length; j++) {
        if (!$q.isNullOrWhiteSpace(jsonData[i].QueryParams[j])) {
          return false;
        }
      }
    }

    return true;
  },

  _drawData(entities, $parentNode, irn) {
    if ($q.isNullOrEmpty(entities)) {
      return;
    }

    let dataItems = this.getTreeViewItemCollectionFromEntityObjects(entities);
    if (!dataItems.length) {
      $('.t-icon', $parentNode).hide();
      dataItems = null;
      return;
    }

    this._renderChildNodes($parentNode, dataItems, irn);
    this._extendNodeElements($parentNode, entities);
    $q.clearArray(dataItems);

    $(entities).each((index, ent) => {
      const $currentNode = this.getNodeByEntityId(ent.Id);
      this._drawData(ent.Children, $currentNode, false);
      if (!this._isSearchQueryEmpty()) {
        if (ent.IsHighlighted) {
          $currentNode.find('.t-in').first().addClass('tree__leaf--fts-founded');
        }

        if (ent.HasChildren && $q.isNullOrEmpty(ent.Children)) {
          $currentNode.find('.t-icon').first().hide();
        }
      }
    });
  },

  _getEntityChildListSuccess(options, data) {
    if (!this._stopDeferredOperations) {
      this._drawData(data, options.$parentNode, options.isRootNode);
      this._hideAjaxLoadingIndicatorForNode(options.$parentNode);
      this._raiseDataBoundEvent();

      if (options.maxExpandLevel !== 0) {
        options.$parentNode.find('> UL > LI').each((index, $childNode) => {
          this.addNodesToParentNode($childNode, options.maxExpandLevel);
        });
      }

      if (options.level === options.maxExpandLevel || options.maxExpandLevel === 0) {
        $q.callFunction(options.callback);
      }
    }
  },

  _getEntityChildListError(options, jqXHR) {
    if (!this._stopDeferredOperations) {
      this._raiseDataBoundEvent();
      $q.processGenericAjaxError(jqXHR);
      $q.callFunction(options.callback);
    }
  },

  addNodesToParentNode(parentNode, maxExpandLevel, callback) {
    const $parentNode = this.getNode(parentNode);
    const isRootNode = this.isRootNode($parentNode);
    let level = 0;
    let entityId;

    level = this.getNodeLevel($parentNode);
    if (isRootNode) {
      $parentNode.empty();
      entityId = this._rootEntityId;
    } else {
      entityId = this.getEntityId($parentNode);
    }

    const options = {
      $parentNode,
      isRootNode,
      maxExpandLevel,
      level,
      callback
    };

    if (level < maxExpandLevel || maxExpandLevel === 0) {
      this._raiseDataBindingEvent();
      this._showAjaxLoadingIndicatorForNode($parentNode);
      const returnSelf = this._rootEntityId && isRootNode;

      this._getEntityChildList(entityId,
        !!returnSelf,
        this._getEntityChildListSuccess.bind(this, options),
        this._getEntityChildListError.bind(this, options));
    } else {
      $q.callFunction(callback);
    }
  },

  _refreshNodeInner($node, loadChildNodes, callback) {
    const isRootNode = this.isRootNode($node);
    this._raiseDataBindingEvent();
    this._showAjaxLoadingIndicatorForNode($node);

    if (isRootNode) {
      this.addNodesToParentNode($node, 0, callback);
    } else {
      const that = this;
      $o.getEntityByTypeAndIdForTree(
        this._entityTypeCode,
        this.getEntityId($node),
        loadChildNodes,
        this._filter,
        data => {
          if (that._stopDeferredOperations) {
            return;
          }

          const entity = data;
          if (entity) {
            const dataItem = that.getTreeViewItemFromEntityObject(entity);
            that._renderNode($node, dataItem, isRootNode);
            that._extendNodeElement($node, entity);

            const childEntities = entity.Children;
            const dataItems = that.getTreeViewItemCollectionFromEntityObjects(childEntities);
            if (!dataItems.length) {
              that._hideAjaxLoadingIndicatorForNode($node);
              return;
            }

            that._renderChildNodes($node, dataItems, isRootNode);
            $q.clearArray(dataItems);

            that._extendNodeElements($node, childEntities);
            $q.clearArray(childEntities);
          }

          that._hideAjaxLoadingIndicatorForNode($node);
          that._raiseDataBoundEvent();
          $q.callFunction(callback);
        }, jqXHR => {
          if (that._stopDeferredOperations) {
            return;
          }

          that._hideAjaxLoadingIndicatorForNode($node);
          that._raiseDataBoundEvent();

          $q.processGenericAjaxError(jqXHR);
          $q.callFunction(callback);
        }
      );
    }
  },

  removeNode(node) {
    const parentNode = this.getParentNode(node);
    const options = {};

    const $node = this.getNode(node);
    const entityId = this.convertNodeCodeToEntityId($node);
    const selectedEntitiesIDs = this._selectedEntitiesIDs;

    if ($q.isArray(selectedEntitiesIDs) && Array.contains(selectedEntitiesIDs, entityId)) {
      Array.remove(selectedEntitiesIDs, entityId);
    }

    if ($node) {
      if ($node.siblings().length) {
        Quantumart.QP8.BackendEntityTree.callBaseMethod(this, 'removeNode', [node]);
      } else {
        this.refreshNode(parentNode, options);
      }
    }
  },

  selectNode(nodeElem, saveOtherNodesSelection) {
    const $node = this.getNode(nodeElem);
    if (saveOtherNodesSelection) {
      $node.find(this.NODE_WRAPPER_SELECTOR).addClass(this.NODE_SELECTED_CLASS_NAME);
      $node.find(this.NODE_CHECKBOX_SELECTORS).prop('checked', this.isNodeSelected($node));
    } else {
      this.getAllNodes()
        .find(this.NODE_WRAPPER_SELECTOR)
        .removeClass(this.NODE_SELECTED_CLASS_NAME)
        .end()
        .find(this.NODE_CHECKBOX_SELECTORS)
        .prop('checked', false);

      $node
        .find(this.NODE_WRAPPER_SELECTOR)
        .addClass(this.NODE_SELECTED_CLASS_NAME)
        .end()
        .find(this.NODE_CHECKBOX_SELECTORS)
        .prop('checked', true);

      if (!this._allowMultipleNodeSelection) {
        this._resetNodeSelectionState();
      }
    }

    this._saveNodeSelectionState();
    this._executePostSelectActions();
  },

  deselectNode(node) {
    const $node = this.getNode(node);

    $node.find(this.NODE_WRAPPER_SELECTOR).removeClass(this.NODE_SELECTED_CLASS_NAME);
    $node.find(this.NODE_CHECKBOX_SELECTORS).prop('checked', false);

    this._saveNodeSelectionState();
    this._executePostSelectActions();
  },

  selectNodes(nodeElems) {
    this
      .getAllNodes()
      .find(this.NODE_WRAPPER_SELECTOR)
      .removeClass(this.NODE_SELECTED_CLASS_NAME)
      .end()
      .find(this.NODE_CHECKBOX_SELECTORS)
      .prop('checked', false);

    if (!$q.isNullOrEmpty(nodeElems)) {
      $q.toJQuery(nodeElems)
        .find(this.NODE_WRAPPER_SELECTOR)
        .addClass(this.NODE_SELECTED_CLASS_NAME)
        .end()
        .find(this.NODE_CHECKBOX_SELECTORS)
        .prop('checked', true);
    }

    this._saveNodeSelectionState();
    this._executePostSelectActions();
  },

  selectAllNodes(value) {
    this
      .getAllNodes()
      .find(this.NODE_WRAPPER_SELECTOR)
      .addClass(this.NODE_SELECTED_CLASS_NAME)
      .end()
      .find(this.NODE_CHECKBOX_SELECTORS)
      .prop('checked', value);

    this._saveNodeSelectionState();
    this._executePostSelectActions();
  },

  isNodeSelected(node) {
    const $node = this.getNode(node);
    let isSelected = false;
    if (!$q.isNullOrEmpty($node)) {
      isSelected = $node.find(this.NODE_WRAPPER_SELECTOR).hasClass(this.NODE_SELECTED_CLASS_NAME);
    }

    return isSelected;
  },

  selectRoot() {
    this.selectNode(this.getNodeByEntityId(this._rootEntityId));
  },

  convertNodeCodeToEntityId(nodeCode) {
    let entityId = -1;
    if (nodeCode === this.ROOT_NODE_CODE) {
      entityId = null;
    } else {
      entityId = $q.toInt(nodeCode, -1);
    }

    return entityId;
  },

  convertEntityIdToNodeCode(entityId) {
    let nodeCode = '';
    if (!entityId || entityId === '0') {
      nodeCode = this.ROOT_NODE_CODE;
    } else {
      nodeCode = $q.toString(entityId, '');
    }

    return nodeCode;
  },

  getEntityId(nodeElem) {
    let entityId = -1;
    const $node = this.getNode(nodeElem);

    if (!$q.isNullOrEmpty($node)) {
      const nodeCode = this.getNodeValue($node);
      entityId = this.convertNodeCodeToEntityId(nodeCode);
    }

    return entityId;
  },

  getEntityName(nodeElem) {
    let entityName = '';
    const $node = this.getNode(nodeElem);

    if (!$q.isNullOrEmpty($node)) {
      entityName = $q.toString($node.data('entity_name'));
    }

    return entityName;
  },

  getEntitiesFromNodes(nodeElems) {
    const entities = [];
    const $nodes = $q.toJQuery(nodeElems);
    const that = this;

    $nodes.each(index => {
      const $node = $nodes.eq(index);
      const entityId = that.getEntityId($node);
      const entityName = that.getEntityName($node);
      if (entityId !== -1) {
        Array.add(entities, { Id: entityId, Name: entityName });
      }
    });

    return entities;
  },

  getSelectedEntities() {
    return this.getEntitiesFromNodes(this.getSelectedNodes());
  },

  checkExistEntityInCurrentPage(entityId) {
    const nodeCode = this.convertEntityIdToNodeCode(entityId);
    return !!this.getNode(nodeCode);
  },

  searchByTerm(options) {
    if (options && $q.isObject(options)) {
      this._searchQuery = options.searchQuery;
    }

    if (this._treeComponent) {
      this.refreshTree();
    }
  },

  _saveNodeSelectionState() {
    const that = this;
    const $nodes = this.getAllNodes();
    $nodes.each(index => {
      const $node = $nodes.eq(index);
      const isSelected = that.isNodeSelected($node);
      const entityId = that.getEntityId($node);
      if (Array.contains(that._selectedEntitiesIDs, entityId)) {
        if (!isSelected) {
          Array.remove(that._selectedEntitiesIDs, entityId);
        }
      } else if (isSelected) {
        Array.add(that._selectedEntitiesIDs, entityId);
      }
    });
  },

  _restoreNodeSelectionState() {
    const that = this;
    const selectedEntitiesIDs = this._selectedEntitiesIDs;
    const selectedNodeElems = [];
    this.getAllNodes().each((index, nodeElem) => {
      const entityId = that.getEntityId($(nodeElem));
      if (Array.contains(selectedEntitiesIDs, entityId)) {
        Array.add(selectedNodeElems, nodeElem);
      }
    });

    this.selectNodes(selectedNodeElems);
  },

  _resetNodeSelectionState() {
    $q.clearArray(this._selectedEntitiesIDs);
  },

  fillTreeViewItemFromEntityObject(dataItem, entity) {
    const icon = this._getIcon(entity);
    const iconUrl = icon.left(7).toLowerCase() === 'http://' ? icon : window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + icon;
    const result = {
      Value: entity.Id,
      Text: entity.Alias,
      ImageUrl: iconUrl,
      LoadOnDemand: entity.HasChildren,
      Expanded: entity.Expanded,
      Enabled: entity.Enabled
    };

    if (entity.Checked
      || (!$q.isNullOrEmpty(this._selectedEntitiesIDs) && Array.contains(this._selectedEntitiesIDs, +entity.Id || 0))
    ) {
      result.Checked = true;
      result.Selected = true;
    }

    return result;
  },

  getTreeViewItemFromEntityObject(entity) {
    return this.fillTreeViewItemFromEntityObject({}, entity);
  },

  fillTreeViewItemCollectionFromEntityObjects(dataItems, entities) {
    const that = this;
    let entityCount = 0;
    if (entities) {
      entityCount = entities.length;
    }

    if (entityCount > 0) {
      $.each(entities, (index, entity) => {
        const dataItem = that.getTreeViewItemFromEntityObject(entity);
        Array.add(dataItems, dataItem);
      });
    }
  },

  getTreeViewItemCollectionFromEntityObjects(entities) {
    const dataItems = [];
    this.fillTreeViewItemCollectionFromEntityObjects(dataItems, entities);
    return dataItems;
  },

  _extendNodeElement(nodeElem, entity) {
    const $node = this.getNode(nodeElem);
    if (!$q.isNullOrEmpty($node)) {
      $node.data('entity_id', entity.Id);
      $node.data('entity_name', entity.Alias);

      const $icon = $node.find('> DIV > SPAN.t-in > IMG.t-image');
      $icon.attr('title', entity.LockedByToolTip);
    }
  },

  _extendNodeElements(parentNodeElem, entities) {
    const that = this;
    const $parentNode = $q.toJQuery(parentNodeElem);
    $.each(entities || [],
      (index, entity) => {
        const $node = that.getNode(entity.Id, $parentNode);
        that._extendNodeElement($node, entity);
      }
    );
  },

  _raiseDataBindingEvent() {
    const action = this._getCurrentAction();

    if (action) {
      const eventArgs = $a.getEventArgsFromAction(action);

      eventArgs.set_isMultipleEntities(true);
      eventArgs.set_entityTypeCode(this._entityTypeCode);
      eventArgs.set_entities(this.getSelectedEntities());
      eventArgs.set_parentEntityId(this._parentEntityId);

      this.notify(window.EVENT_TYPE_ENTITY_TREE_DATA_BINDING, eventArgs);
    }

    if (this._isDataLoaded) {
      if (this._allowSaveNodesSelection) {
        this._saveNodeSelectionState();
      } else {
        this._resetNodeSelectionState();
      }

      this._allowSaveNodesSelection = true;
    }
  },

  _raiseDataBoundEvent() {
    this._isDataLoaded = true;
    const action = this._getCurrentAction();
    if (action) {
      const eventArgs = $a.getEventArgsFromAction(action);
      eventArgs.set_isMultipleEntities(true);
      eventArgs.set_entityTypeCode(this._entityTypeCode);
      eventArgs.set_entities(this.getSelectedEntities());
      eventArgs.set_parentEntityId(this._parentEntityId);
      this.notify(window.EVENT_TYPE_ENTITY_TREE_DATA_BOUND, eventArgs);
    }

    this._restoreNodeSelectionState();
  },

  _executePostSelectActions() {
    if (this.articlesCountId) {
      $(`#${this.articlesCountId}`).text(this._selectedEntitiesIDs.length);
    }

    this._raiseSelectEvent();
  },

  _raiseSelectEvent() {
    const action = this._getCurrentAction();
    if (action) {
      const eventArgs = $a.getEventArgsFromAction(action);
      eventArgs.set_isMultipleEntities(true);
      eventArgs.set_entityTypeCode(this._entityTypeCode);
      eventArgs.set_entities(this.getSelectedEntities());
      eventArgs.set_parentEntityId(this._parentEntityId);
      this.notify(window.EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED, eventArgs);
    }
  },

  _getEntityChildList(entityId, returnSelf, succ, fail) {
    return $o.getEntityChildList({
      entityTypeCode: this._entityTypeCode,
      parentEntityId: this._parentEntityId,
      entityId,
      returnSelf,
      filter: this._filter,
      hostFilter: this._hostFilter,
      selectItemIDs: this._selectedEntitiesIDs,
      searchQuery: this._searchQuery
    }, succ, fail);
  },

  _getIcon(entity) {
    if ($q.isNullOrEmpty(entity.IconUrl)) {
      let icon = 'article_node.gif';
      if (entity.LockedByAnyone) {
        if (entity.LockedByYou) {
          icon = 'article_node_locked.gif';
        } else {
          icon = 'article_node_locked_by_user.gif';
        }
      }

      return icon;
    }

    return entity.IconUrl;
  },

  executeAction(node, actionCode) {
    const $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      const action = $a.getBackendActionByCode(actionCode);
      if (!action) {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
        return;
      }

      if (!action.IsCustom && action.ActionType.Code === window.ACTION_TYPE_CODE_REFRESH) {
        this.refreshNode($node, { saveNodesSelection: true });
        return;
      }

      if (actionCode === window.ACTION_TYPE_SELECT_CHILD_ARTICLES) {
        this._treeComponent.nodeCheckExcludeSelf(node, true, false, true);
      }

      if (actionCode === window.ACTION_TYPE_UNSELECT_CHILD_ARTICLES) {
        this._treeComponent.nodeCheckExcludeSelf(node, false, false, true);
      }

      const context = actionCode === window.ACTION_CODE_ADD_NEW_CHILD_ARTICLE ? {
        additionalUrlParameters: {
          isChild: true,
          fieldId: this._treeFieldId,
          articleId: this.getEntityId($node)
        }
      } : null;

      const params = new Quantumart.QP8.BackendActionParameters({
        entityTypeCode: this._entityTypeCode,
        entityId: this.getEntityId($node),
        entityName: this.getEntityName($node),
        parentEntityId: this._parentEntityId,
        context
      });

      params.correct(action);
      const eventArgs = $a.getEventArgsFromActionWithParams(action, params);
      if (this._hostIsWindow) {
        const message = Quantumart.QP8.Backend.getInstance().checkOpenDocumentByEventArgs(eventArgs);

        if (message) {
          $q.alertError(message);
        } else {
          eventArgs.set_isWindow(true);
          this.notify(window.EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING, eventArgs);
        }
      } else {
        this.notify(window.EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING, eventArgs);
      }
    }
  },

  _onDataBinding(sender) {
    this.addNodesToParentNode(sender, 0);
  },

  _onNodeClicking(e) {
    const $element = $(e.currentTarget);
    const $node = $($element.closest('.t-item')[0]);
    if (!this._treeComponent.shouldNavigate($element)) {
      e.preventDefault();
      return false;
    }

    const saveOtherNodesSelection = this._allowMultipleNodeSelection || this._treeComponent.showCheckBox;
    $node.find(this.NODE_WRAPPER_SELECTOR).removeClass(this.NODE_HOVER_CLASS_NAME);
    this.selectNode($node, saveOtherNodesSelection);
    return undefined;
  },

  beforeCustomNodeCheck(checkbox, isChecked) {
    const that = this;
    const $checkbox = $(checkbox);
    if (!$q.isNullOrEmpty($checkbox)) {
      const $node = $checkbox.closest('.t-item');
      if (this.isNodeSelected($node) && !isChecked) {
        $node.each((index, item) => {
          that.deselectNode(item);
        });
      } else if ($checkbox.is(':checked')) {
        $node.each((index, item) => {
          that.selectNode(item, that._allowMultipleNodeSelection);
        });
      }
    }
  },

  _onContextMenu(e) {
    const $element = $(e.currentTarget);
    const $node = $($element.closest('.t-item')[0]);
    this._currentNodeId = this.getEntityId($node);
    if (this._contextMenuComponent) {
      this._contextMenuComponent.showMenu(e, $element.get(0));
    }

    e.preventDefault();
  },

  _onNodeContextMenuShowing(eventType, sender, args) {
    const menuComponent = args.get_menu();
    const $node = $(args.get_targetElement());
    if (!$q.isNullOrEmpty($node) && !$q.isNullOrEmpty(menuComponent)) {
      menuComponent.tuneMenuItems(this._currentNodeId, this._parentEntityId);
    }
  },

  _onNodeContextMenuItemClicking(eventType, sender, args) {
    const $menuItem = $(args.get_menuItem());
    if (!$q.isNullOrEmpty($menuItem)) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }
  },

  _onNodeContextMenuHidden() {
    if (!$q.isNullOrEmpty(this._contextMenuActionCode)) {
      this.executeAction(this.getNode(this._currentNodeId), this._contextMenuActionCode);
      this._contextMenuActionCode = null;
    }
  },

  dispose() {
    this._stopDeferredOperations = true;
    this._resetNodeSelectionState();
    this._selectedEntitiesIDs = null;

    if (this._treeManagerComponent) {
      this._treeManagerComponent.removeTree(this._treeElementId);
      this._treeManagerComponent = null;
    }

    const $tree = $(this._treeElement);
    $tree.unbind('dataBinding').unbind('dataBound');

    if (this._contextMenuComponent) {
      const contextMenuComponent = this._contextMenuComponent;
      const contextMenuEventType = $.fn.jeegoocontext.getContextMenuEventType();

      $tree.undelegate(this.NODE_NEW_CLICKABLE_SELECTORS, contextMenuEventType, this._onContextMenuHandler);
      contextMenuComponent.detachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_SHOWING,
        this._onNodeContextMenuShowingHandler
      );

      contextMenuComponent.detachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING,
        this._onNodeContextMenuItemClickingHandler
      );

      contextMenuComponent.detachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_HIDDEN,
        this._onNodeContextMenuHiddenHandler
      );


      contextMenuComponent.dispose();
      this._contextMenuComponent = null;
    }

    $q.dispose.call(this, [
      '_onDataBindingHandler',
      '_onDataBoundHandler',
      '_onNodeClickingHandler',
      '_onContextMenuHandler',
      '_onNodeContextMenuShowingHandler',
      '_onNodeContextMenuItemClickingHandler',
      '_onNodeContextMenuHiddenHandler'
    ]);

    Quantumart.QP8.BackendEntityTree.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendEntityTree.registerClass('Quantumart.QP8.BackendEntityTree', Quantumart.QP8.BackendTreeBase);
