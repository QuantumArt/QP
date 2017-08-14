window.EVENT_TYPE_TREE_MENU_ACTION_EXECUTING = 'OnTreeMenuActionExecuting';

Quantumart.QP8.BackendTreeMenu = function (treeElementId, options) {
  Quantumart.QP8.BackendTreeMenu.initializeBase(this, [treeElementId, options]);

  if ($q.isObject(options)) {
    if (options.leftSplitterPaneHeight) {
      this.leftSplitterPaneHeight = options.leftSplitterPaneHeight;
    }

    if (options.contextMenuManager) {
      this._contextMenuManagerComponent = options.contextMenuManager;
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

Quantumart.QP8.BackendTreeMenu.prototype = {
  _leftSplitterPaneHeight: 0,
  _contextMenuActionCode: '',
  NODE_BUSY_CLASS_NAME: 'busy',

  _onDataBindingHandler: null,
  _onNodeClickingHandler: null,

  _contextMenuManagerComponent: null,
  _onContextMenuHandler: null,
  _onNodeContextMenuShowingHandler: null,
  _onNodeContextMenuItemClickingHandler: null,
  _onNodeContextMenuHiddenHandler: null,

  get_contextMenuManager: function () {
    return this._contextMenuManager;
  },

  set_contextMenuManager: function (value) {
    this._contextMenuManager = value;
  },

  initialize: function () {
    Quantumart.QP8.BackendTreeMenu.callBaseMethod(this, 'initialize');
    let treeComponent = this._treeComponent;
    let $tree = $(this._treeComponent.element);
    if (this._contextMenuManagerComponent != null) {
      $tree.on(this._contextMenuManagerComponent.getContextMenuEventType(), this.NODE_NEW_CLICKABLE_SELECTORS, this._onContextMenuHandler);
      this._contextMenuManagerComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onNodeContextMenuShowingHandler);
      this._contextMenuManagerComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onNodeContextMenuItemClickingHandler);
      this._contextMenuManagerComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onNodeContextMenuHiddenHandler);
    }

    this.addNodesToParentNode(treeComponent.element, 2);
  },

  fixTreeHeight: function (leftSplitterPaneHeight) {
    if (!$q.isNull(leftSplitterPaneHeight)) {
      leftSplitterPaneHeight -= 46;
      if (leftSplitterPaneHeight > 0) {
        $('#tree').css('height', `${leftSplitterPaneHeight}px`);
        $('#menuContainer').show();
      }
    }
  },

  markTreeAsBusy: function () {
    let $tree = $(this._treeComponent.element);
    $tree.find('UL:first').addClass(this.NODE_BUSY_CLASS_NAME);
  },

  unmarkTreeAsBusy: function () {
    let $tree = $(this._treeComponent.element);
    $tree.find('UL:first').removeClass(this.NODE_BUSY_CLASS_NAME);
  },

  isTreeBusy: function () {
    let $tree = $(this._treeComponent.element);
    return $tree.find('UL:first').hasClass(this.NODE_BUSY_CLASS_NAME);
  },

  generateNodeCode: function (entityTypeCode, entityId, parentEntityId, isFolder) {
    return String.format('{0}{1}_{2}_{3}', entityTypeCode, isFolder ? 's' : '', entityId, parentEntityId ? parentEntityId : 0);
  },

  generateNodeCodeToHighlight: function (eventArgs) {
    let isFolder = eventArgs.get_actionTypeCode() == window.ACTION_TYPE_CODE_LIST;
    let entityId = !isFolder ? eventArgs.get_entityId() : Quantumart.QP8.BackendEntityType.getEntityTypeByCode(eventArgs.get_entityTypeCode()).Id;
    return this.generateNodeCode(eventArgs.get_entityTypeCode(), entityId, eventArgs.get_parentEntityId(), isFolder);
  },

  addNodesToParentNode: function (parentNode, maxExpandLevel) {
    this._addNodesToParentNode(parentNode, maxExpandLevel, Quantumart.QP8.BackendTreeMenu.getTreeMenuChildNodesList, jqXHR => {
      $q.processGenericAjaxError(jqXHR);
    });
  },

  _addNodesToParentNode: function (parentNode, maxExpandLevel, getChildNodes, errorHandler) {
    let self = this;
    let $parentNode = this.getNode(parentNode);
    let isRootNode = this.isRootNode($parentNode);
    let level = 0;
    let entityTypeCode = null;
    let entityId = null;
    let parentEntityId = null;
    let isFolder = false;
    let isGroup = false;
    let groupItemCode = null;

    if (!isRootNode) {
      level = this.getNodeLevel($parentNode);
      entityTypeCode = $parentNode.data('entity_type_code');
      entityId = $parentNode.data('entity_id');
      parentEntityId = $parentNode.data('is_folder') ? $parentNode.data('parent_entity_id') : $parentNode.data('entity_id');
      isFolder = $parentNode.data('is_folder');
      isGroup = $parentNode.data('is_group');
      groupItemCode = $parentNode.data('group_item_code');
    }

    if (level < maxExpandLevel || maxExpandLevel == 0) {
      $parentNode.data('loading_icon_timeout', setTimeout(() => {
        $parentNode.find('> DIV > .t-icon').addClass('t-loading');
      }, 100));

      getChildNodes({
        entityTypeCode: entityTypeCode,
        parentEntityId: parentEntityId,
        isFolder: isFolder,
        isGroup: isGroup,
        groupItemCode: groupItemCode
      }, nodes => {
        let dataItems = self.getTreeViewItemCollectionFromTreeNodes(nodes);
        if (dataItems.length === 0) {
          dataItems = null;
          return;
        }

        self._renderChildNodes($parentNode, dataItems, isRootNode);
        $q.clearArray(dataItems);
        self._hideAjaxLoadingIndicatorForNode($parentNode);
        self._extendNodeElements($parentNode, nodes);

        if (maxExpandLevel != 0) {
          $parentNode.find('> UL > LI').each((index, $childNode) => {
            self._addNodesToParentNode($childNode, maxExpandLevel, getChildNodes);
          });
        }

        $q.clearArray(nodes);
      }, errorHandler);
    }
  },

  _refreshNodeInner: function ($node, loadChildNodes, callback) {
    if ($q.isNullOrEmpty($node)) {
      return;
    }

    let self = this;
    let $parentNode = this.getParentNode($node);

    let entityTypeCode = $parentNode ? $parentNode.data('entity_type_code') : null;
    let entityId = $parentNode ? $node.data('entity_id') : 0;
    let isFolder = $parentNode ? $parentNode.data('is_folder') : false;
    let parentEntityId = $parentNode ? isFolder ? $parentNode.data('parent_entity_id') : $parentNode.data('entity_id') : null;
    let isGroup = $parentNode ? $parentNode.data('is_group') : false;
    let groupItemCode = $parentNode ? $parentNode.data('group_item_code') : '';
    let isRootNode = this.isRootNode($node);

    this._showAjaxLoadingIndicatorForNode($node);
    Quantumart.QP8.BackendTreeMenu.getTreeMenuNode(entityTypeCode, entityId, parentEntityId, isFolder, isGroup, groupItemCode, loadChildNodes, data => {
      let node = data;
      if (node) {
        node.NodeCode = self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder);
        let dataItem = self.getTreeViewItemFromTreeNode(node);

        self._renderNode($node, dataItem, isRootNode);
        self._extendNodeElement($node, node);

        dataItem = null;

        let childNodes = node.ChildNodes;
        let dataItems = self.getTreeViewItemCollectionFromTreeNodes(childNodes);

        if (dataItems.length == 0) {
          self._hideAjaxLoadingIndicatorForNode($node);
          dataItems = null;
          return;
        }

        self._renderChildNodes($node, dataItems, isRootNode);
        $q.clearArray(dataItems);

        self._hideAjaxLoadingIndicatorForNode($node);
        self._extendNodeElements($node, childNodes);
        $q.clearArray(childNodes);
      }

      if (self._deferredNodeCodeToHighlight) {
        self.highlightNode(self._deferredNodeCodeToHighlight);
      }

      $q.callFunction(callback);
    }, jqXHR => {
      $q.processGenericAjaxError(jqXHR);
      $q.callFunction(callback);
    });
  },

  highlightNode: function (node) {
    let $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      this.unhighlightAllNodes();
      $node.find(this.NODE_WRAPPER_SELECTOR).addClass(this.NODE_SELECTED_CLASS_NAME);
      this.scrollToNode($node);
    }
  },

  highlightNodeWithEventArgs: function (eventArgs) {
    let nodeCode = this.generateNodeCodeToHighlight(eventArgs);
    this._deferredNodeCodeToHighlight = nodeCode;
    let $highlightedNode = this.getNode(nodeCode);

    if (!$q.isNullOrEmpty($highlightedNode)) {
      let $node = $highlightedNode;
      if (eventArgs.isExpandRequested) {
        let self = this;
        Quantumart.QP8.BackendTreeMenu.getSubTreeToEntity(eventArgs.get_entityTypeCode(), eventArgs.get_parentEntityId(), eventArgs.get_entityId(), data => {
          if (!data) {
            return;
          }

          let _exp = function (node) {
            if (!node || $q.isNullOrEmpty(self.getNode(self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder)))) {
              return null;
            }

            $node = self.getNode(self.getNode(self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder)));
            if (self._isNodeCollapsed($node)) {
              self._treeComponent.nodeToggle(null, $node, true);
            }

            return _exp($.grep(node.ChildNodes, n => n.ChildNodes != null)[0]) || node;
          };

          _exp(data);
        });
      }

      this.highlightNode($highlightedNode);
    } else if (eventArgs.isExpandRequested) {
      this._expandToEntityNode(eventArgs.get_entityTypeCode(), eventArgs.get_parentEntityId(), eventArgs.get_entityId());
    }
  },

  _expandToEntityNode: function (entityTypeCode, parentEntityId, entityId) {
    let self = this;
    Quantumart.QP8.BackendTreeMenu.getSubTreeToEntity(entityTypeCode, parentEntityId, entityId, data => {
      if (data) {
        return;
      }

      let findDeepest = function (node, toExpand) {
        if (!node || $q.isNullOrEmpty(self.getNode(self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder)))) {
          return null;
        }

        if (toExpand) {
          let $node = self.getNode(self.getNode(self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder)));
          if (self._isNodeCollapsed($node)) {
            self._treeComponent.nodeToggle(null, $node, true);
          }
        }

        return findDeepest($.grep(node.ChildNodes, n => n.ChildNodes != null)[0], toExpand) || node;

      };

      let deepestExistedNode = findDeepest(data);
      let findChildren = function (options, parent) {
        if (!parent || $q.isNullOrEmpty(parent.ChildNodes)) {
          return [];
        } else if (parent.Code === options.entityTypeCode
          && ((parent.IsFolder && parent.ParentId === options.parentEntityId)
            || (!parent.IsFolder && parent.Id === options.parentEntityId))
          && parent.IsFolder === options.isFolder
          && parent.IsGroup === options.isGroup
          && parent.GroupItemCode === options.groupItemCode
        ) {
          deepestExistedNode = parent;
          return parent.ChildNodes;
        }

        return findChildren(options, $.grep(parent.ChildNodes || [], n => n.ChildNodes != null)[0]);
      };

      if (deepestExistedNode) {
        let nodeCode = self.generateNodeCode(deepestExistedNode.Code, deepestExistedNode.Id, deepestExistedNode.ParentId, deepestExistedNode.IsFolder);

        self._addNodesToParentNode(self.getNode(nodeCode), 100, (options, successHandler) => {
          successHandler(findChildren(options, deepestExistedNode).slice(0));
        }, () => {});

        deepestExistedNode = findDeepest(data, true);
        nodeCode = self.generateNodeCode(
          deepestExistedNode.Code,
          deepestExistedNode.Id,
          deepestExistedNode.ParentId,
          deepestExistedNode.IsFolder
        );

        let $node = self.getNode(nodeCode);
        self._deferredNodeCodeToHighlight = nodeCode;
        self.highlightNode($node);
      }
    });
  },

  unhighlightAllNodes: function () {
    this._deferredNodeCodeToHighlight = '';
    let $nodes = this.getAllNodes();
    $nodes.find(this.NODE_WRAPPER_SELECTOR).removeClass(this.NODE_SELECTED_CLASS_NAME);
    $nodes = null;
  },

  scrollToNode: function (node) {
    let $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      $(`#${this._treeContainerElementId}`).scrollTo($node, { offset: -100, duration: 300, axis: 'y' });
    }
  },

  fillTreeViewItemFromTreeNode: function (dataItem, node) {
    let iconUrl = node.Icon.left(7).toLowerCase() !== 'http://'
      ? window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + node.Icon
      : node.Icon;

    dataItem.Value = node.NodeCode;
    dataItem.Text = node.Title;
    dataItem.ImageUrl = iconUrl;
    dataItem.LoadOnDemand = node.HasChildren;
    dataItem.Expanded = false;

    return dataItem;
  },

  getTreeViewItemFromTreeNode: function (node) {
    return this.fillTreeViewItemFromTreeNode({}, node);
  },

  fillTreeViewItemCollectionFromTreeNodes: function (dataItems, nodes) {
    let self = this;
    let nodeCount = 0;
    if (nodes != null) {
      nodeCount = nodes.length;
    }

    if (nodeCount > 0) {
      $.each(nodes, (index, node) => {
        node.NodeCode = self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder);
        let dataItem = self.getTreeViewItemFromTreeNode(node);
        Array.add(dataItems, dataItem);
      });
    }
  },

  getTreeViewItemCollectionFromTreeNodes: function (nodes) {
    let dataItems = [];
    this.fillTreeViewItemCollectionFromTreeNodes(dataItems, nodes);
    return dataItems;
  },

  _extendNodeElement: function (nodeElem, node) {
    let $node = this.getNode(nodeElem);
    $node.data({
      entity_type_code: node.Code,
      entity_id: node.Id,
      entity_name: node.Title,
      parent_entity_id: node.ParentId,
      is_folder: node.IsFolder,
      is_group: node.IsGroup,
      group_item_code: node.GroupItemCode,
      default_action_code: node.DefaultActionCode,
      default_action_type_code: node.DefaultActionTypeCode,
      context_menu_code: node.ContextMenuCode
    });

    let contextMenuCode = node.ContextMenuCode;
    if (!$q.isNullOrWhiteSpace(contextMenuCode) && this._contextMenuManagerComponent) {
      let contextMenu = this._contextMenuManagerComponent.getContextMenu(contextMenuCode);
      if ($q.isNullOrEmpty(contextMenu)) {
        let targetElems = this._treeComponent.element;
        contextMenu = this._contextMenuManagerComponent.createContextMenu(contextMenuCode, String.format('treeContextMenu_{0}', contextMenuCode), {
          targetElements: targetElems,
          allowManualShowing: true
        });

        contextMenu.addMenuItemsToMenu(false);
      }
    }
  },

  _extendNodeElements: function (parentNodeElem, nodes) {
    let self = this;
    let $parentNode = $q.toJQuery(parentNodeElem);
    $.each(nodes, (index, node) => {
      let $node = self.getNode(node.NodeCode, $parentNode);
      self._extendNodeElement($node, node);
    });
  },

  executeAction: function (node, actionCode) {
    let $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      let action = $a.getBackendActionByCode(actionCode);
      if (!action) {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
      } else {
        let actionTypeCode = action.ActionType.Code;
        let isCustomAction = action.IsCustom;
        if (!isCustomAction && actionTypeCode === window.ACTION_TYPE_CODE_REFRESH) {
          this.refreshNode($node);
          return;
        }

        let isFolder = $q.toBoolean($node.data('is_folder'), false);
        let isGroup = $q.toBoolean($node.data('is_group'), false);

        let params = new Quantumart.QP8.BackendActionParameters({
          entityTypeCode: $node.data('entity_type_code'),
          entityId: isFolder ? 0 : $node.data('entity_id'),
          entityName: $node.data('entity_name'),
          parentEntityId: +$node.data('parent_entity_id') || 0,
          isGroup: isGroup
        });

        params.correct(action);
        let eventArgs = $a.getEventArgsFromActionWithParams(action, params);
        this.notify(window.EVENT_TYPE_TREE_MENU_ACTION_EXECUTING, eventArgs);
      }
    }
  },

  onActionExecuted: function (eventArgs) {
    let entityTypeCode = eventArgs.get_entityTypeCode();
    let actionTypeCode = eventArgs.get_actionTypeCode();
    let parentEntityId = eventArgs.get_parentEntityId();
    let entityId = eventArgs.get_entityId();

    if (eventArgs.get_isRemoving() || actionTypeCode == window.ACTION_TYPE_CODE_COPY || eventArgs.get_isSaved() || eventArgs.get_isUpdated()) {
      let entityType = Quantumart.QP8.BackendEntityType.getEntityTypeByCode(entityTypeCode);
      if (!entityType) {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
      } else {
        let parentNodeCode = this.generateNodeCode(entityTypeCode, entityType.Id, parentEntityId, true);
        let nodeCode = this.generateNodeCode(entityTypeCode, entityId, parentEntityId, false);
        let orderChanged = eventArgs.get_context() && eventArgs.get_context().orderChanged;
        let groupChanged = eventArgs.get_context() && eventArgs.get_context().groupChanged;
        let options = { loadChildNodes: true };

        if (actionTypeCode == window.ACTION_TYPE_CODE_REMOVE) {
          this.removeNodeOrRefreshParent(nodeCode, parentNodeCode, options);
        } else if (actionTypeCode == window.ACTION_TYPE_CODE_MULTIPLE_REMOVE
          || actionTypeCode == window.ACTION_TYPE_CODE_COPY
          || eventArgs.get_isSaved()
          || (eventArgs.get_isUpdated() && (orderChanged || groupChanged))
        ) {
          this.refreshNode(parentNodeCode);
        } else if (eventArgs.get_isUpdated() && !orderChanged) {
          this.refreshNode(nodeCode, options);
        }
      }
    }
  },

  _onDataBinding: function (sender) {
    this.addNodesToParentNode(sender, 0);
  },

  _onNodeClicking: function (e) {
    let $element = $(e.currentTarget);
    let nodeEl = $($element.closest('.t-item')[0]);
    if (!this._treeComponent.shouldNavigate($element)) {
      e.preventDefault();
      return false;
    }

    if (!this.isTreeBusy()) {
      nodeEl.find(this.NODE_WRAPPER_SELECTOR).removeClass(this.NODE_HOVER_CLASS_NAME);
      this.highlightNode(nodeEl);
      if (!nodeEl.data('is_folder') && this._isNodeCollapsed(nodeEl)) {
        this._treeComponent.nodeToggle(null, nodeEl, true);
      }

      if (!$q.isNullOrWhiteSpace(nodeEl.data('default_action_code'))) {
        this.executeAction(nodeEl, nodeEl.data('default_action_code'));
      }
    } else {
      e.preventDefault();
    }
  },

  _onContextMenu: function (e) {
    let $element = $(e.currentTarget);
    let $node = $($element.closest('.t-item')[0]);
    let contextMenuCode = $node.data('context_menu_code');
    if (!this.isTreeBusy()) {
      if (contextMenuCode && this._contextMenuManagerComponent) {
        let contextMenuComponent = this._contextMenuManagerComponent.getContextMenu(contextMenuCode);
        if (contextMenuComponent) {
          contextMenuComponent.showMenu(e, $element.parent().parent().get(0));
        }
      }
    }

    e.preventDefault();
  },

  _onNodeContextMenuShowing: function (eventType, sender, args) {
    let menuComponent = args.get_menu();
    let $node = $(args.get_targetElement());
    if (menuComponent && $node.length) {
      menuComponent.tuneMenuItems($node.data('entity_id'));
    }
  },

  _onNodeContextMenuItemClicking: function (eventType, sender, args) {
    let $menuItem = $(args.get_menuItem());
    if ($menuItem.length) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }
  },

  _onNodeContextMenuHidden: function (eventType, sender, args) {
    let $node = $(args.get_targetElement());
    if (this._contextMenuActionCode) {
      this.executeAction($node, this._contextMenuActionCode);
      this._contextMenuActionCode = '';
    }
  },

  dispose: function () {
    let $tree = $(`#${this._treeElementId}`);
    $tree.off('click', this.NODE_NEW_CLICKABLE_SELECTORS, this._onNodeClickingHandler);

    if (this._contextMenuManagerComponent != null) {
      $tree.off(this._contextMenuManagerComponent.getContextMenuEventType(), this.NODE_NEW_CLICKABLE_SELECTORS, this._onContextMenuHandler);
      this._contextMenuManagerComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onNodeContextMenuShowingHandler);
      this._contextMenuManagerComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onNodeContextMenuItemClickingHandler);
      this._contextMenuManagerComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onNodeContextMenuHiddenHandler);
    }

    $q.dispose.call(this, [
      '_onDataBindingHandler',
      '_onNodeClickingHandler',

      '_contextMenuManagerComponent',
      '_onContextMenuHandler',
      '_onNodeContextMenuShowingHandler',
      '_onNodeContextMenuItemClickingHandler',
      '_onNodeContextMenuHiddenHandler'
    ]);

    Quantumart.QP8.BackendTreeMenu.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendTreeMenu.getTreeMenuNode = function (entityTypeCode, entityId, parentEntityId, isFolder, isGroup, groupItemCode, loadChildNodes, successHandler, errorHandler) {
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_TREE_MENU}GetNode`, {
    entityTypeCode: entityTypeCode,
    entityId: entityId,
    parentEntityId: parentEntityId,
    isFolder: isFolder,
    isGroup: isGroup,
    groupItemCode: groupItemCode,
    loadChildNodes: loadChildNodes
  }, false, false, successHandler, errorHandler);
};

Quantumart.QP8.BackendTreeMenu.getTreeMenuChildNodesList = function (options, successHandler, errorHandler) {
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_TREE_MENU}GetChildNodesList`, {
    entityTypeCode: options.entityTypeCode,
    parentEntityId: options.parentEntityId,
    isFolder: options.isFolder,
    isGroup: options.isGroup,
    groupItemCode: options.groupItemCode
  }, false, false, successHandler, errorHandler);
};

Quantumart.QP8.BackendTreeMenu.getSubTreeToEntity = function (entityTypeCode, parentEntityId, entityId, successHandler) {
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_TREE_MENU}GetSubTreeToEntity`, {
    entityTypeCode: entityTypeCode,
    parentEntityId: parentEntityId,
    entityId: entityId
  }, false, false, successHandler, jqXHR => {
    $q.processGenericAjaxError(jqXHR);
  });
};

Quantumart.QP8.BackendTreeMenu.registerClass('Quantumart.QP8.BackendTreeMenu', Quantumart.QP8.BackendTreeBase);
