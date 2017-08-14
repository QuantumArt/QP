window.EVENT_TYPE_ENTITY_TREE_DATA_BINDING = 'OnEntityTreeDataBinding';
window.EVENT_TYPE_ENTITY_TREE_DATA_BOUND = 'OnEntityTreeDataBound';
window.EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING = 'OnEntityTreeActionExecuting';
window.EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED = 'OnEntityTreeEntitySelected';

Quantumart.QP8.BackendEntityTree = function (treeGroupCode, treeElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions) {
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

  get_treeGroupCode: function () {
    return this._treeGroupCode;
  },
  set_treeGroupCode: function () {
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

  get_actionCode: function () {
    return this._actionCode;
  },

  set_actionCode: function (value) {
    this._actionCode = value;
  },

  get_allowMultipleNodeSelection: function () {
    return this._allowMultipleNodeSelection;
  },

  set_allowMultipleNodeSelection: function (value) {
    this._allowMultipleNodeSelection = value;
  },

  get_allowGlobalSelection: function () {
    return this._allowGlobalSelection;
  },
  set_allowGlobalSelection: function (value) {
    this._allowGlobalSelection = value;
  },

  get_selectedEntitiesIDs: function () {
    return this._selectedEntitiesIDs;
  },

  set_selectedEntitiesIDs: function (value) {
    this._selectedEntitiesIDs = value;
  },

  get_contextMenuCode: function () {
    return this._contextMenuCode;
  },

  set_contextMenuCode: function (value) {
    this._contextMenuCode = value;
  },

  get_treeManager: function () {
    return this._treeManagerComponent;
  },

  set_treeManager: function (value) {
    this._treeManagerComponent = value;
  },

  get_treeFieldId: function () {
    return this._treeFieldId;
  },

  set_treeFieldId: function (value) {
    this._treeFieldId = value;
  },

  _onDataBindingHandler: null,
  _onDataBoundHandler: null,
  _onNodeClickingHandler: null,
  _onContextMenuHandler: null,
  _onNodeContextMenuShowingHandler: null,
  _onNodeContextMenuItemClickingHandler: null,
  _onNodeContextMenuHiddenHandler: null,

  initialize: function () {
    Quantumart.QP8.BackendEntityTree.callBaseMethod(this, 'initialize');

    $('.fullTextBlock label').removeClass('hidden');

    let treeComponent = this._treeComponent;
    treeComponent.showCheckBox = this._allowMultipleNodeSelection;
    let $tree = $(this._treeElement);
    $tree.bind('dataBound', this._onDataBoundHandler);
    if (!$q.isNullOrWhiteSpace(this._contextMenuCode)) {
      let contextMenuComponent = new Quantumart.QP8.BackendContextMenu(this._contextMenuCode, String.format('{0}_ContextMenu', this._treeElementId), {
        targetElements: this._treeElement,
        allowManualShowing: true,
        zIndex: this._zIndex
      });

      contextMenuComponent.initialize();
      contextMenuComponent.addMenuItemsToMenu(false);

      let contextMenuEventType = contextMenuComponent.getContextMenuEventType();

      $tree.delegate(this.NODE_NEW_CLICKABLE_SELECTORS, contextMenuEventType, this._onContextMenuHandler);

      contextMenuComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onNodeContextMenuShowingHandler);
      contextMenuComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onNodeContextMenuItemClickingHandler);
      contextMenuComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onNodeContextMenuHiddenHandler);

      this._contextMenuComponent = contextMenuComponent;
    }

    treeComponent = null;
    $tree = null;

    this.refreshTree();
  },

  _getCurrentAction: function () {
    return $a.getBackendActionByCode(this._actionCode);
  },

  getNodeByEntityId: function (entityId, parentNodeElem) {
    return this.getNode(this.convertEntityIdToNodeCode(entityId), parentNodeElem);
  },

  getNodesByEntitiesIDs: function (entitiesIDs) {
    let selectedNodeElems = [];

    if (!$q.isNullOrEmpty(entitiesIDs)) {
      let self = this;
      let $nodes = this.getAllNodes();

      $nodes.each(
        (index, nodeElem) => {
          if (Array.contains(entitiesIDs, $q.toString(self.getEntityId(nodeElem)))) {
            Array.add(selectedNodeElems, nodeElem);
          }
        }
      );
    }

    return selectedNodeElems;
  },

  getParentNode: function (node) {
    let $parentNode = Quantumart.QP8.BackendEntityTree.callBaseMethod(this, 'getParentNode', [node]);
    if ($q.isNullOrEmpty($parentNode) && $q.isString(node)) {
      let entityTypeCode = this._entityTypeCode;
      let entityId = this.convertNodeCodeToEntityId(node);
      let parentId = $o.getParentEntityId(entityTypeCode, entityId);
      if (parentId) {
        $parentNode = this.getNode(this.convertEntityIdToNodeCode(parentId));
      }
    }

    return $parentNode;
  },

  getParentNodesByEntitiesIDs: function (entitiesIDs) {
    let entityCount = entitiesIDs.length;

    if (entityCount <= 0) {
      return $([]);
    }

    // Получаем коды родительских узлов
    let parentNodeCodes = [];

    for (let entityIndex = 0; entityIndex < entityCount; entityIndex++) {
      let entityId = entitiesIDs[entityIndex];
      let $node = this.getNodeByEntityId(entityId);

      if (!$q.isNullOrEmpty($node)) {
        let $parentNode = this.getParentNode($node);
        if (!$q.isNullOrEmpty($parentNode)) {
          let parentNodeCode = this.getNodeValue($parentNode);
          Array.add(parentNodeCodes, parentNodeCode);
        }
      }
    }

    if (parentNodeCodes.length > 0) {
      parentNodeCodes = Array.distinct(parentNodeCodes);
    } else {
      parentNodeCodes = [this.ROOT_NODE_CODE];
    }

    // Формируем список родительских узлов с информацией об уровне вложенности
    let parentNodeInfos = [];

    for (let parentNodeCodeIndex = 0; parentNodeCodeIndex < parentNodeCodes.length; parentNodeCodeIndex++) {
      let parentNodeCode = parentNodeCodes[parentNodeCodeIndex];
      let level = this.getNodeLevel(parentNodeCode);

      Array.add(parentNodeInfos, { nodeCode: parentNodeCode, level: level });
    }

    $q.clearArray(parentNodeCodes);

    // Формируем список уровней вложенности
    let levels = [];
    for (let parentNodeInfoIndex = 0; parentNodeInfoIndex < parentNodeInfos.length; parentNodeInfoIndex++) {
      let parentNodeInfo = parentNodeInfos[parentNodeInfoIndex];
      let level = parentNodeInfo.level;
      if (!Array.contains(levels, level)) {
        Array.add(levels, level);
      }
    }

    levels = levels.sort();

    // Очищаем список родительских узлов от узлов, которые являются дочерними
    // по отношению к узлам, содержащимся в списке
    let levelCount = levels.length;

    if (levelCount > 1) {
      for (let levelIndex = levelCount - 2; levelIndex >= 0; levelIndex--) {
        let level = levels[levelIndex];
        let nodeInfos = $.grep(parentNodeInfos, (parentNodeInfo) => parentNodeInfo.level == level);

        let nodeInfoCount = nodeInfos.length;
        let childNodeInfos = $.grep(parentNodeInfos, (parentNodeInfo) => parentNodeInfo.level > level);

        let childNodeInfoCount = childNodeInfos.length;

        if (childNodeInfoCount > 0) {
          for (let nodeInfoIndex = 0; nodeInfoIndex < nodeInfoCount; nodeInfoIndex++) {
            let nodeInfo = nodeInfos[nodeInfoIndex];
            let $node = this.getNode(nodeInfo.nodeCode);

            for (let childNodeInfoIndex = childNodeInfoCount - 1; childNodeInfoIndex >= 0; childNodeInfoIndex--) {
              let childNodeInfo = childNodeInfos[childNodeInfoIndex];
              let $childNode = this.getNode(childNodeInfo.nodeCode, $node);

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

    let self = this;
    let parentNodeElems = $.map(parentNodeInfos, (parentNodeInfo) => self.getNode(parentNodeInfo.nodeCode).get(0));

    $q.clearArray(parentNodeInfos);
    return $q.toJQuery(parentNodeElems);
  },

  _isSearchQueryEmpty: function (searchQuery) {
    let query = searchQuery || this._searchQuery;

    if ($q.isNullOrWhiteSpace(query)) {
      return true;
    }

    let i, j;
    let jsonData = JSON.parse(query);

    for (i = 0; i < jsonData.length; i++) {
      for (j = 0; j < jsonData[i].QueryParams.length; j++) {
        if (!$q.isNullOrWhiteSpace(jsonData[i].QueryParams[j])) {
          return false;
        }
      }
    }

    return true;
  },

  _drawData: function (entities, $parentNode, irn) {
    if ($q.isNullOrEmpty(entities)) {
      return;
    }

    let dataItems = this.getTreeViewItemCollectionFromEntityObjects(entities);

    if (dataItems.length == 0) {
      $('.t-icon', $parentNode).hide();
      dataItems = null;
      return;
    }

    this._renderChildNodes($parentNode, dataItems, irn);
    this._extendNodeElements($parentNode, entities);
    $q.clearArray(dataItems);

    $(entities).each((index, ent) => {
      let $currentNode = this.getNodeByEntityId(ent.Id);

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

  _getEntityChildListSuccess: function (options, data) {
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

  _getEntityChildListError: function (options, jqXHR) {
    if (!this._stopDeferredOperations) {
      this._raiseDataBoundEvent();
      $q.processGenericAjaxError(jqXHR);
      $q.callFunction(options.callback);
    }
  },

  addNodesToParentNode: function (parentNode, maxExpandLevel, callback) {
    let $parentNode = this.getNode(parentNode);
    let isRootNode = this.isRootNode($parentNode);
    let level = 0;
    let entityId;

    level = this.getNodeLevel($parentNode);
    if (isRootNode) {
      $parentNode.empty();
      entityId = this._rootEntityId;
    } else {
      entityId = this.getEntityId($parentNode);
    }

    let options = {
      $parentNode: $parentNode,
      isRootNode: isRootNode,
      maxExpandLevel: maxExpandLevel,
      level: level,
      callback: callback
    };

    if (level < maxExpandLevel || maxExpandLevel == 0) {
      this._raiseDataBindingEvent();
      this._showAjaxLoadingIndicatorForNode($parentNode);
      let returnSelf = (this._rootEntityId != null) && isRootNode;

      this._getEntityChildList(entityId,
        returnSelf,
        this._getEntityChildListSuccess.bind(this, options),
        this._getEntityChildListError.bind(this, options));
    } else {
      $q.callFunction(callback);
    }
  },

  _refreshNodeInner: function ($node, loadChildNodes, callback) {
    let isRootNode = this.isRootNode($node);

    this._raiseDataBindingEvent();
    this._showAjaxLoadingIndicatorForNode($node);

    if (isRootNode) {
      this.addNodesToParentNode($node, 0, callback);
    } else {
      let self = this;

      $o.getEntityByTypeAndIdForTree(this._entityTypeCode, this.getEntityId($node), loadChildNodes, this._filter, (data) => {
        if (self._stopDeferredOperations) {
          return;
        }

        let entity = data;
        if (entity) {
          let dataItem = self.getTreeViewItemFromEntityObject(entity);
          self._renderNode($node, dataItem, isRootNode);
          self._extendNodeElement($node, entity);
          dataItem = null;

          let childEntities = entity.Children;
          let dataItems = self.getTreeViewItemCollectionFromEntityObjects(childEntities);
          if (dataItems.length == 0) {
            self._hideAjaxLoadingIndicatorForNode($node);
            dataItems = null;
            return;
          }

          self._renderChildNodes($node, dataItems, isRootNode);
          $q.clearArray(dataItems);

          self._extendNodeElements($node, childEntities);
          $q.clearArray(childEntities);
        }

        self._hideAjaxLoadingIndicatorForNode($node);
        self._raiseDataBoundEvent();
        $q.callFunction(callback);
      }, (jqXHR) => {
        if (self._stopDeferredOperations) {
          return;
        }

        self._hideAjaxLoadingIndicatorForNode($node);
        self._raiseDataBoundEvent();

        $q.processGenericAjaxError(jqXHR);
        $q.callFunction(callback);
      });
    }
  },

  removeNode: function (node) {
    let parentNode = this.getParentNode(node);
    let options = {};

    let $node = this.getNode(node);
    let entityId = this.convertNodeCodeToEntityId($node);
    let selectedEntitiesIDs = this._selectedEntitiesIDs;

    if ($q.isArray(selectedEntitiesIDs) && Array.contains(selectedEntitiesIDs, entityId)) {
      Array.remove(selectedEntitiesIDs, entityId);
    }

    if ($node) {
      if ($node.siblings().length > 0) {
        Quantumart.QP8.BackendEntityTree.callBaseMethod(this, 'removeNode', [node]);
      } else {
        this.refreshNode(parentNode, options);
      }
    }
  },

  selectNode: function (nodeElem, saveOtherNodesSelection) {
    if ($q.isNull(saveOtherNodesSelection)) {
      saveOtherNodesSelection = false;
    }

    let $node = this.getNode(nodeElem);
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

  deselectNode: function (node) {
    let $node = this.getNode(node);

    $node.find(this.NODE_WRAPPER_SELECTOR).removeClass(this.NODE_SELECTED_CLASS_NAME);
    $node.find(this.NODE_CHECKBOX_SELECTORS).prop('checked', false);

    this._saveNodeSelectionState();
    this._executePostSelectActions();
  },

  selectNodes: function (nodeElems) {
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

  selectAllNodes: function (value) {
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

  isNodeSelected: function (node) {
    let $node = this.getNode(node);
    let isSelected = false;
    if (!$q.isNullOrEmpty($node)) {
      isSelected = $node.find(this.NODE_WRAPPER_SELECTOR).hasClass(this.NODE_SELECTED_CLASS_NAME);
    }

    return isSelected;
  },

  selectRoot: function () {
    this.selectNode(this.getNodeByEntityId(this._rootEntityId));
  },

  convertNodeCodeToEntityId: function (nodeCode) {
    let entityId = -1;
    if (nodeCode == this.ROOT_NODE_CODE) {
      entityId = null;
    } else {
      entityId = $q.toInt(nodeCode, -1);
    }

    return entityId;
  },

  convertEntityIdToNodeCode: function (entityId) {
    let nodeCode = '';
    if (entityId == 0 || entityId == null || entityId == '0') {
      nodeCode = this.ROOT_NODE_CODE;
    } else {
      nodeCode = $q.toString(entityId, '');
    }

    return nodeCode;
  },

  getEntityId: function (nodeElem) {
    let entityId = -1;
    let $node = this.getNode(nodeElem);

    if (!$q.isNullOrEmpty($node)) {
      let nodeCode = this.getNodeValue($node);
      entityId = this.convertNodeCodeToEntityId(nodeCode);
    }

    return entityId;
  },

  getEntityName: function (nodeElem) {
    let entityName = '';
    let $node = this.getNode(nodeElem);

    if (!$q.isNullOrEmpty($node)) {
      entityName = $q.toString($node.data('entity_name'));
    }

    return entityName;
  },

  getEntitiesFromNodes: function (nodeElems) {
    let entities = [];
    let $nodes = $q.toJQuery(nodeElems);
    let self = this;

    $nodes.each(
      (index) => {
        let $node = $nodes.eq(index);
        let entityId = self.getEntityId($node);
        let entityName = self.getEntityName($node);

        if (entityId != -1) {
          Array.add(entities, { Id: entityId, Name: entityName });
        }
      }
    );

    return entities;
  },

  getSelectedEntities: function () {
    return this.getEntitiesFromNodes(this.getSelectedNodes());
  },

  checkExistEntityInCurrentPage: function (entityId) {
    let result = false;
    let nodeCode = this.convertEntityIdToNodeCode(entityId);
    let $node = this.getNode(nodeCode);

    if (!$q.isNullOrEmpty($node)) {
      result = true;
    }

    return result;
  },

  searchByTerm: function (options) {
    if (options && $q.isObject(options)) {
      this._searchQuery = options.searchQuery;
    }

    let treeComponent = this._treeComponent;

    if (treeComponent) {
      this.refreshTree();
    }

    treeComponent = null;
  },

  _saveNodeSelectionState: function () {
    let self = this;
    let $nodes = this.getAllNodes();

    $nodes.each((index) => {
      let $node = $nodes.eq(index);
      let isSelected = self.isNodeSelected($node);
      let entityId = self.getEntityId($node);

      if (Array.contains(self._selectedEntitiesIDs, entityId)) {
        if (!isSelected) {
          Array.remove(self._selectedEntitiesIDs, entityId);
        }
      } else if (isSelected) {
        Array.add(self._selectedEntitiesIDs, entityId);
      }
    });

    $nodes = null;
  },

  _restoreNodeSelectionState: function () {
    let self = this;

    let selectedEntitiesIDs = this._selectedEntitiesIDs;
    let selectedNodeElems = [];
    this.getAllNodes().each((index, nodeElem) => {
      let entityId = self.getEntityId($(nodeElem));
      if (Array.contains(selectedEntitiesIDs, entityId)) {
        Array.add(selectedNodeElems, nodeElem);
      }
    });

    this.selectNodes(selectedNodeElems);
  },

  _resetNodeSelectionState: function () {
    $q.clearArray(this._selectedEntitiesIDs);
  },

  fillTreeViewItemFromEntityObject: function (dataItem, entity) {
    let icon = this._getIcon(entity);

    let iconUrl = icon.left(7).toLowerCase() !== 'http://' ? window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + icon : icon;

    dataItem.Value = entity.Id;
    dataItem.Text = entity.Alias;
    dataItem.ImageUrl = iconUrl;
    dataItem.LoadOnDemand = entity.HasChildren;
    dataItem.Expanded = entity.Expanded;
    dataItem.Enabled = entity.Enabled;

    if (entity.Checked === true || (!$q.isNullOrEmpty(this._selectedEntitiesIDs) && Array.contains(this._selectedEntitiesIDs, +entity.Id || 0))) {
      dataItem.Checked = true;
      dataItem.Selected = true;
    }

    return dataItem;
  },

  getTreeViewItemFromEntityObject: function (entity) {
    return this.fillTreeViewItemFromEntityObject({}, entity);
  },

  fillTreeViewItemCollectionFromEntityObjects: function (dataItems, entities) {
    let self = this;
    let entityCount = 0;

    if (entities) {
      entityCount = entities.length;
    }

    if (entityCount > 0) {
      $.each(entities, (index, entity) => {
        let dataItem = self.getTreeViewItemFromEntityObject(entity);
        Array.add(dataItems, dataItem);
      });
    }
  },

  getTreeViewItemCollectionFromEntityObjects: function (entities) {
    let dataItems = [];
    this.fillTreeViewItemCollectionFromEntityObjects(dataItems, entities);
    return dataItems;
  },

  _extendNodeElement: function (nodeElem, entity) {
    let $node = this.getNode(nodeElem);
    if (!$q.isNullOrEmpty($node)) {
      $node.data('entity_id', entity.Id);
      $node.data('entity_name', entity.Alias);

      let $icon = $node.find('> DIV > SPAN.t-in > IMG.t-image');
      $icon.attr('title', entity.LockedByToolTip);
    }
  },

  _extendNodeElements: function (parentNodeElem, entities) {
    let self = this;
    let $parentNode = $q.toJQuery(parentNodeElem);
    $.each(entities || [],
      (index, entity) => {
        let $node = self.getNode(entity.Id, $parentNode);
        self._extendNodeElement($node, entity);
      }
    );
  },

  _raiseDataBindingEvent: function () {
    let action = this._getCurrentAction();

    if (action) {
      let eventArgs = $a.getEventArgsFromAction(action);

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

  _raiseDataBoundEvent: function () {
    this._isDataLoaded = true;

    let action = this._getCurrentAction();

    if (action) {
      let eventArgs = $a.getEventArgsFromAction(action);
      eventArgs.set_isMultipleEntities(true);
      eventArgs.set_entityTypeCode(this._entityTypeCode);
      eventArgs.set_entities(this.getSelectedEntities());
      eventArgs.set_parentEntityId(this._parentEntityId);
      this.notify(window.EVENT_TYPE_ENTITY_TREE_DATA_BOUND, eventArgs);
    }

    this._restoreNodeSelectionState();
  },

  _executePostSelectActions: function () {
    if (this.articlesCountId) {
      $(`#${this.articlesCountId}`).text(this._selectedEntitiesIDs.length);
    }

    this._raiseSelectEvent();
  },

  _raiseSelectEvent: function () {
    let nodes = this.getSelectedNodes();
    let action = this._getCurrentAction();

    if (action) {
      let eventArgs = $a.getEventArgsFromAction(action);
      eventArgs.set_isMultipleEntities(true);
      eventArgs.set_entityTypeCode(this._entityTypeCode);
      eventArgs.set_entities(this.getSelectedEntities());
      eventArgs.set_parentEntityId(this._parentEntityId);
      this.notify(window.EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED, eventArgs);
    }
  },

  _getEntityChildList: function (entityId, returnSelf, succ, fail) {
    return $o.getEntityChildList({
      entityTypeCode: this._entityTypeCode,
      parentEntityId: this._parentEntityId,
      entityId: entityId,
      returnSelf: returnSelf,
      filter: this._filter,
      hostFilter: this._hostFilter,
      selectItemIDs: this._selectedEntitiesIDs,
      searchQuery: this._searchQuery
    }, succ, fail);
  },

  _getIcon: function (entity) {
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

  executeAction: function (node, actionCode, options) {
    let $node = this.getNode(node);
    options = Object.assign({
      ctrlKey: false,
      shiftKey: false
    }, options);

    if (!$q.isNullOrEmpty($node)) {
      let action = $a.getBackendActionByCode(actionCode);

      if (!action) {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
        return;
      }

      if (!action.IsCustom && action.ActionType.Code === window.ACTION_TYPE_CODE_REFRESH) {
        this.refreshNode($node, { saveNodesSelection: true });
        return;
      }

      if (actionCode === window.ACTION_TYPE_SELECT_CHILD_ARTICLES) {
        let nodesToSelect = [].filter.call(this.getAllNodes(), function (entry) {
          return this.getEntityId(entry) === this.getEntityId(node);
        }, this);

        this._treeComponent.nodeCheckExcludeSelf(node, true, false, true);
      }

      if (actionCode === window.ACTION_TYPE_UNSELECT_CHILD_ARTICLES) {
        let nodesToSelect = [].filter.call(this.getAllNodes(), function (entry) {
          return this.getEntityId(entry) === this.getEntityId(node);
        }, this);

        this._treeComponent.nodeCheckExcludeSelf(node, false, false, true);
      }

      let context = actionCode === window.ACTION_CODE_ADD_NEW_CHILD_ARTICLE ? {
        additionalUrlParameters: {
          isChild: true,
          fieldId: this._treeFieldId,
          articleId: this.getEntityId($node)
        }
      } : null;

      let params = new Quantumart.QP8.BackendActionParameters({
        entityTypeCode: this._entityTypeCode,
        entityId: this.getEntityId($node),
        entityName: this.getEntityName($node),
        parentEntityId: this._parentEntityId,
        context: context
      });

      params.correct(action);
      let eventArgs = $a.getEventArgsFromActionWithParams(action, params);

      if (this._hostIsWindow) {
        let message = Quantumart.QP8.Backend.getInstance().checkOpenDocumentByEventArgs(eventArgs);

        if (message) {
          $q.alertError(message);
        } else {
          eventArgs.set_isWindow(true);
          this.notify(window.EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING, eventArgs);
        }
      } else {
        this.notify(window.EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING, eventArgs);
      }

      params = null;
      eventArgs = null;
      action = null;
    }
  },

  _createDataQueryParams: function () {
    let params = {};

    if (this._searchQuery) {
      params.searchQuery = this._searchQuery;
    }

    return params;
  },

  _onDataBinding: function (sender) {
    sender.data = Object.assign({}, sender.data, this._createDataQueryParams());
    this.addNodesToParentNode(sender, 0);
  },

  _onNodeClicking: function (e) {
    let $element = $(e.currentTarget);
    let $node = $($element.closest('.t-item')[0]);

    if (!this._treeComponent.shouldNavigate($element)) {
      $node = null;
      e.preventDefault();
      return false;
    }

    let saveOtherNodesSelection = this._allowMultipleNodeSelection || this._treeComponent.showCheckBox === true;
    $node.find(this.NODE_WRAPPER_SELECTOR).removeClass(this.NODE_HOVER_CLASS_NAME);
    this.selectNode($node, saveOtherNodesSelection);
  },

  beforeCustomNodeCheck: function (checkbox, isChecked, suppressAutoCheck, autoCheckChildren) {
    let self = this;
    let $checkbox = $(checkbox);

    if ($q.isNullOrEmpty($checkbox)) {
      return;
    }

    let $node = $checkbox.closest('.t-item');
    if (this.isNodeSelected($node) && !isChecked) {
      $node.each((index, item) => {
        self.deselectNode(item);
      });
    } else if ($checkbox.is(':checked')) {
      $node.each((index, item) => {
        self.selectNode(item, self._allowMultipleNodeSelection);
      });
    }
  },

  _onContextMenu: function (e) {
    let $element = $(e.currentTarget);
    let $node = $($element.closest('.t-item')[0]);

    this._currentNodeId = this.getEntityId($node);

    if (this._contextMenuComponent) {
      this._contextMenuComponent.showMenu(e, $element.get(0));
    }

    $element = null;
    e.preventDefault();
  },

  _onNodeContextMenuShowing: function (eventType, sender, args) {
    let menuComponent = args.get_menu();
    let $node = $(args.get_targetElement());

    if (!$q.isNullOrEmpty($node) && !$q.isNullOrEmpty(menuComponent)) {
      menuComponent.tuneMenuItems(this._currentNodeId, this._parentEntityId);
    }

    $node = null;
  },

  _onNodeContextMenuItemClicking: function (eventType, sender, args) {
    let $menuItem = $(args.get_menuItem());

    if (!$q.isNullOrEmpty($menuItem)) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }

    $menuItem = null;
  },

  _onNodeContextMenuHidden: function () {
    if (!$q.isNullOrEmpty(this._contextMenuActionCode)) {
      this.executeAction(this.getNode(this._currentNodeId), this._contextMenuActionCode);
      this._contextMenuActionCode = null;
    }
  },

  dispose: function () {
    this._stopDeferredOperations = true;
    this._resetNodeSelectionState();
    this._selectedEntitiesIDs = null;

    if (this._treeManagerComponent) {
      this._treeManagerComponent.removeTree(this._treeElementId);
      this._treeManagerComponent = null;
    }

    let $tree = $(this._treeElement);

    $tree.unbind('dataBinding').unbind('dataBound');

    if (this._contextMenuComponent) {
      let contextMenuComponent = this._contextMenuComponent;
      let contextMenuEventType = contextMenuComponent.getContextMenuEventType();

      $tree.undelegate(this.NODE_NEW_CLICKABLE_SELECTORS, contextMenuEventType, this._onContextMenuHandler);

      contextMenuComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onNodeContextMenuShowingHandler);
      contextMenuComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onNodeContextMenuItemClickingHandler);
      contextMenuComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onNodeContextMenuHiddenHandler);

      contextMenuComponent.dispose();
      contextMenuComponent = null;

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
