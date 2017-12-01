window.EVENT_TYPE_ACTION_PERMISSIONS_TREE_EXECUTING = 'OnActionPermissionsTreeExecuting';
Quantumart.QP8.BackendActionPermissionTree = function (treeElementId) {
  Quantumart.QP8.BackendActionPermissionTree.initializeBase(this, [treeElementId]);

  this._onDataBindingHandler = $.proxy(this._onDataBinding, this);
  this._onContextMenuHandler = $.proxy(this._onContextMenu, this);
  this._onNodeContextMenuShowingHandler = $.proxy(this._onNodeContextMenuShowing, this);
  this._onNodeContextMenuItemClickingHandler = $.proxy(this._onNodeContextMenuItemClicking, this);
  this._onNodeContextMenuHiddenHandler = $.proxy(this._onNodeContextMenuHidden, this);
};

Quantumart.QP8.BackendActionPermissionTree.prototype = {
  _userId: null,
  _groupId: null,
  _currentNodeId: -1,
  _contextMenuActionCode: '',

  _entityTypeContextMenuComponent: null,
  _actionContextMenuComponent: null,

  _onContextMenuHandler: null,
  _onNodeContextMenuItemClickingHandler: null,
  _onNodeContextMenuHiddenHandler: null,
  _onNodeContextMenuShowingHandler: null,

  // eslint-disable-next-line camelcase
  set_userId(val) {
    this._userId = val;
  },

  // eslint-disable-next-line camelcase
  get_userId() {
    return this._userId;
  },

  // eslint-disable-next-line camelcase
  set_groupId(val) {
    this._groupId = val;
  },

  // eslint-disable-next-line camelcase
  get_groupId() {
    return this._groupId;
  },

  initialize() {
    Quantumart.QP8.BackendActionPermissionTree.callBaseMethod(this, 'initialize');
    const createContextMenu = $.proxy(function (contextMenuCode) {
      const contextMenuComponent = new Quantumart.QP8.BackendContextMenu(
        contextMenuCode,
        `${this._treeElementId}_${contextMenuCode}_ContextMenu`,
        {
          targetElements: this._treeElement,
          allowManualShowing: true
        }
      );

      contextMenuComponent.initialize();
      contextMenuComponent.addMenuItemsToMenu(false);
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

      return contextMenuComponent;
    }, this);

    this._entityTypeContextMenuComponent = createContextMenu(window.CONTEXT_MENU_CODE_ENTITY_TYPE_PERMISSION_NODE);
    this._actionContextMenuComponent = createContextMenu(window.CONTEXT_MENU_CODE_ACTION_PERMISSION_NODE);

    const contextMenuEventType = $.fn.jeegoocontext.getContextMenuEventType();
    $(this._treeElement).on(contextMenuEventType, this.NODE_NEW_CLICKABLE_SELECTORS, this._onContextMenuHandler);
  },

  executeAction($node, actionCode) {
    if ($node) {
      const action = $a.getBackendActionByCode(actionCode);
      if (!action) {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
        return;
      }
      if (action.ActionType.Code === window.ACTION_TYPE_CODE_REFRESH) {
        this.refreshNode($node);
      } else {
        const eventArgs = $a.getEventArgsFromAction(action);
        eventArgs.set_entityId(0);
        eventArgs.set_parentEntityId(this._getNodeId($node));
        this.notify(window.EVENT_TYPE_ACTION_PERMISSIONS_TREE_EXECUTING, eventArgs);
      }
    }
  },

  refreshPermissionNode(entityTypeCode, nodeValueId) {
    let nodeType = '';
    if (entityTypeCode === window.ENTITY_TYPE_CODE_ENTITY_TYPE_PERMISSION) {
      nodeType = Quantumart.QP8.Enums.ActionPermissionTreeNodeType.EntityTypeNode;
    } else if (entityTypeCode === window.ENTITY_TYPE_CODE_ACTION_PERMISSION) {
      nodeType = Quantumart.QP8.Enums.ActionPermissionTreeNodeType.ActionNode;
    }

    const nodeValue = this._converToItemValue({ NodeType: nodeType, Id: nodeValueId });
    const $node = this.getNode(nodeValue);
    if ($node) {
      this.refreshNode($node);
    }
  },

  _onDataBinding(sender) {
    this.addNodesToParentNode(sender, 0);
  },

  _onContextMenu(e) {
    const $element = $(e.currentTarget);
    const $node = $($element.closest('.t-item')[0]);
    const nodeType = this._getNodeType($node);

    this._currentNodeId = this.getNodeValue($node);
    if (nodeType === Quantumart.QP8.Enums.ActionPermissionTreeNodeType.EntityTypeNode) {
      this._entityTypeContextMenuComponent.showMenu(e, $element.get(0));
    } else if (nodeType === Quantumart.QP8.Enums.ActionPermissionTreeNodeType.ActionNode) {
      this._actionContextMenuComponent.showMenu(e, $element.get(0));
    }

    e.preventDefault();
  },

  _onNodeContextMenuShowing(eventType, sender, args) {
    const menuComponent = args.get_menu();
    const $node = $(args.get_targetElement());

    if ($node && !$q.isNullOrEmpty(menuComponent)) {
      menuComponent.tuneMenuItems(this._getNodeId($node.closest('.t-item')));
    }
  },

  _onNodeContextMenuItemClicking(eventType, sender, args) {
    const $menuItem = $(args.get_menuItem());
    if (!$q.isNullOrEmpty($menuItem)) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }
  },

  _onNodeContextMenuHidden() {
    const $node = this.getNode(this._currentNodeId);
    if (!$q.isNullOrEmpty(this._contextMenuActionCode)) {
      this.executeAction($node, this._contextMenuActionCode);
    }
  },

  addNodesToParentNode(parentNode, maxExpandLevel, callback) {
    const $parentNode = this.getNode(parentNode);
    const isRootNode = this.isRootNode($parentNode);
    let entityTypeId;
    if (isRootNode) {
      $parentNode.empty();
    } else {
      entityTypeId = this._getNodeId($parentNode);
    }

    this._showAjaxLoadingIndicatorForNode($parentNode);

    const that = this;
    this._loadChildNodes(entityTypeId).done(data => {
      if (that._stopDeferredOperations) {
        return;
      }

      if (data) {
        if (data.Type === window.ACTION_MESSAGE_TYPE_ERROR) {
          Quantumart.QP8.BackendActionExecutor.showResult(data);
        } else {
          const dataItems = that._convertToTreeViewItems(data);
          that._renderChildNodes($parentNode, dataItems, isRootNode, false, true);
          $q.clearArray(dataItems);

          that._hideAjaxLoadingIndicatorForNode($parentNode);
          that._extendNodeElements($parentNode, data);

          $q.callFunction(callback);
        }
      }
    }).fail(jqXHR => {
      if (that._stopDeferredOperations) {
        return;
      }

      $q.processGenericAjaxError(jqXHR);
      $q.callFunction(callback);
    });
  },

  _refreshNodeInner($node, loadChildNodes, callback) {
    const isRootNode = this.isRootNode($node);
    if (isRootNode) {
      this.addNodesToParentNode($node, 0, callback);
    } else {
      const nodeType = this._getNodeType($node);
      let entityTypeId, actionId;
      if (nodeType === Quantumart.QP8.Enums.ActionPermissionTreeNodeType.EntityTypeNode) {
        entityTypeId = this._getNodeId($node);
      } else if (nodeType === Quantumart.QP8.Enums.ActionPermissionTreeNodeType.ActionNode) {
        actionId = this._getNodeId($node);
        entityTypeId = this._getNodeId(this.getParentNode($node));
      }

      this._showAjaxLoadingIndicatorForNode($node);

      const that = this;
      this._loadNode(entityTypeId, actionId).done(data => {
        if (that._stopDeferredOperations) {
          return;
        }

        if (data) {
          let dataItem = that._convertToTreeViewItem(data);
          that._renderNode($node, dataItem, false);
          dataItem = null;

          if (data.Children) {
            const dataItems = that._convertToTreeViewItems(data.Children);
            that._renderChildNodes($node, dataItems, false, false);
            $q.clearArray(dataItems);
          }

          that._hideAjaxLoadingIndicatorForNode($node);
          that._extendNodeElement($node, data);
          if (data.Children) {
            that._extendNodeElements($node, data.Children);
          }
          $q.callFunction(callback);
        }
      }).fail(jqXHR => {
        if (that._stopDeferredOperations) {
          return;
        }

        $q.processGenericAjaxError(jqXHR);
        $q.callFunction(callback);
      });
    }
  },

  _getNodeId($node) {
    let entityTypeId = null;
    if ($node) {
      const nodeCode = this.getNodeValue($node);
      if (nodeCode !== this.ROOT_NODE_CODE) {
        entityTypeId = $node.data('node_id');
      }
    }

    return entityTypeId;
  },

  _getNodeType($node) {
    let nodeType = null;
    if ($node) {
      const nodeCode = this.getNodeValue($node);
      if (nodeCode !== this.ROOT_NODE_CODE) {
        nodeType = $node.data('node_type');
      }
    }

    return nodeType;
  },

  _convertToTreeViewItems(items) {
    return $.isArray(items)
      ? items.map($.proxy(this._convertToTreeViewItem, this))
      : undefined;
  },

  _convertToTreeViewItem(item) {
    let iconUrl = '';
    iconUrl = item.IconUrl.left(7).toLowerCase() === 'http://'
      ? item.IconUrl
      : window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + item.IconUrl;

    return {
      Value: this._converToItemValue(item),
      Text: item.Text,
      ImageUrl: iconUrl,
      LoadOnDemand: item.HasChildren,
      Expanded: false,
      Enabled: true
    };
  },

  _converToItemValue(item) {
    return item ? `${item.NodeType}-${item.Id}` : undefined;
  },

  _extendNodeElement(nodeElem, item) {
    const $node = this.getNode(nodeElem);
    if ($node) {
      $node.data('node_type', item.NodeType);
      $node.data('node_id', item.Id);
    }
  },

  _extendNodeElements(parentNodeElem, items) {
    const that = this;
    const $parentNode = $q.toJQuery(parentNodeElem);
    $.each(items, (index, item) => {
      const $node = that.getNode(that._converToItemValue(item), $parentNode);
      that._extendNodeElement($node, item);
    });
  },

  _loadChildNodes(entityTypeId) {
    return $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_ACTION_PERMISSION_TREE}GetTreeNodes`, {
      entityTypeId,
      userId: this._userId,
      groupId: this._groupId
    }, true, false);
  },

  _loadNode(entityTypeId, actionId) {
    return $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_ACTION_PERMISSION_TREE}GetTreeNode`, {
      entityTypeId,
      actionId,
      userId: this._userId,
      groupId: this._groupId
    }, true, false);
  },

  dispose() {
    $(this.NODE_NEW_CLICKABLE_SELECTORS, this._treeElement).off();
    const disposeContextMenu = contextMenuComponent => {
      if (contextMenuComponent) {
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
      }
    };

    disposeContextMenu(this._entityTypeContextMenuComponent);
    this._entityTypeContextMenuComponent = null;

    disposeContextMenu(this._actionContextMenuComponent);
    this._actionContextMenuComponent = null;

    this._onNodeContextMenuShowingHandler = null;
    this._onNodeContextMenuItemClickingHandler = null;
    this._onNodeContextMenuHiddenHandler = null;

    Quantumart.QP8.BackendActionPermissionTree.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendActionPermissionTree.registerClass(
  'Quantumart.QP8.BackendActionPermissionTree',
  Quantumart.QP8.BackendTreeBase
);
