window.EVENT_TYPE_ACTION_PERMISSIONS_TREE_EXECUTING = 'OnActionPermissionsTreeExecuting';
Quantumart.QP8.BackendActionPermissionTree = function (treeElementId, options, hostOptions) {
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

  set_userId(val) {
    this._userId = val;
  },
  get_userId() {
    return this._userId;
  },

  set_groupId(val) {
    this._groupId = val;
  },
  get_groupId() {
    return this._groupId;
  },

  initialize() {
    Quantumart.QP8.BackendActionPermissionTree.callBaseMethod(this, 'initialize');
    const createContextMenu = $.proxy(function (contextMenuCode) {
      const contextMenuComponent = new Quantumart.QP8.BackendContextMenu(contextMenuCode, String.format('{0}_{1}_ContextMenu', this._treeElementId, contextMenuCode), {
        targetElements: this._treeElement,
        allowManualShowing: true
      });

      contextMenuComponent.initialize();
      contextMenuComponent.addMenuItemsToMenu(false);
      contextMenuComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onNodeContextMenuShowingHandler);
      contextMenuComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onNodeContextMenuItemClickingHandler);
      contextMenuComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onNodeContextMenuHiddenHandler);
      return contextMenuComponent;
    }, this);

    this._entityTypeContextMenuComponent = createContextMenu(window.CONTEXT_MENU_CODE_ENTITY_TYPE_PERMISSION_NODE);
    this._actionContextMenuComponent = createContextMenu(window.CONTEXT_MENU_CODE_ACTION_PERMISSION_NODE);

    const contextMenuEventType = this._entityTypeContextMenuComponent.getContextMenuEventType();
    $(this._treeElement).on(contextMenuEventType, this.NODE_NEW_CLICKABLE_SELECTORS, this._onContextMenuHandler);
  },

  executeAction($node, actionCode) {
    if (!$q.isNullOrEmpty($node)) {
      const action = $a.getBackendActionByCode(actionCode);
      if (!action) {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
        return;
      }
      if (action.ActionType.Code == window.ACTION_TYPE_CODE_REFRESH) {
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
    if (entityTypeCode == window.ENTITY_TYPE_CODE_ENTITY_TYPE_PERMISSION) {
      nodeType = Quantumart.QP8.Enums.ActionPermissionTreeNodeType.EntityTypeNode;
    } else if (entityTypeCode == window.ENTITY_TYPE_CODE_ACTION_PERMISSION) {
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
    let $element = $(e.currentTarget);
    let $node = $($element.closest('.t-item')[0]);

    this._currentNodeId = this.getNodeValue($node);

    const nodeType = this._getNodeType($node);
    if (nodeType == Quantumart.QP8.Enums.ActionPermissionTreeNodeType.EntityTypeNode) {
      this._entityTypeContextMenuComponent.showMenu(e, $element.get(0));
    } else if (nodeType == Quantumart.QP8.Enums.ActionPermissionTreeNodeType.ActionNode) {
      this._actionContextMenuComponent.showMenu(e, $element.get(0));
    }


    $element = null;
    $node = null;

    e.preventDefault();
  },

  _onNodeContextMenuShowing(eventType, sender, args) {
    const menuComponent = args.get_menu();
    const $node = $(args.get_targetElement());

    if (!$q.isNullOrEmpty($node) && !$q.isNullOrEmpty(menuComponent)) {
      menuComponent.tuneMenuItems(this._getNodeId($node.closest('.t-item')));
    }
  },

  _onNodeContextMenuItemClicking(eventType, sender, args) {
    const $menuItem = $(args.get_menuItem());
    if (!$q.isNullOrEmpty($menuItem)) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }
  },

  _onNodeContextMenuHidden(eventType, sender, args) {
    const $node = this.getNode(this._currentNodeId);
    if (!$q.isNullOrEmpty(this._contextMenuActionCode)) {
      this.executeAction($node, this._contextMenuActionCode);
    }
  },

  addNodesToParentNode(parentNode, maxExpandLevel, callback) {
    const $parentNode = this.getNode(parentNode);
    const isRootNode = this.isRootNode($parentNode);
    const nodeType = this._getNodeType($parentNode);
    let entityTypeId;
    if (isRootNode) {
      $parentNode.empty();
    } else {
      entityTypeId = this._getNodeId($parentNode);
    }

    this._showAjaxLoadingIndicatorForNode($parentNode);
    const self = this;

    this._loadChildNodes(entityTypeId).done((data, textStatus, jqXHR) => {
      if (self._stopDeferredOperations) {
        return;
      }

      if (data) {
        if (data.Type == window.ACTION_MESSAGE_TYPE_ERROR) {
          Quantumart.QP8.BackendActionExecutor.showResult(data);
        } else {
          const dataItems = self._convertToTreeViewItems(data);
          self._renderChildNodes($parentNode, dataItems, isRootNode, false, true);
          $q.clearArray(dataItems);

          self._hideAjaxLoadingIndicatorForNode($parentNode);
          self._extendNodeElements($parentNode, data);

          $q.callFunction(callback);
        }
      }
    }).fail((jqXHR, textStatus, errorThrown) => {
      if (self._stopDeferredOperations) {
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
      if (nodeType == Quantumart.QP8.Enums.ActionPermissionTreeNodeType.EntityTypeNode) {
        entityTypeId = this._getNodeId($node);
      } else if (nodeType == Quantumart.QP8.Enums.ActionPermissionTreeNodeType.ActionNode) {
        actionId = this._getNodeId($node);
        entityTypeId = this._getNodeId(this.getParentNode($node));
      }

      this._showAjaxLoadingIndicatorForNode($node);
      const self = this;

      this._loadNode(entityTypeId, actionId)
        .done((data, textStatus, jqXHR) => {
          if (self._stopDeferredOperations) {
            return;
          }
          if (data) {
            let dataItem = self._convertToTreeViewItem(data);
            self._renderNode($node, dataItem, false);
            dataItem = null;

            if (data.Children) {
              const dataItems = self._convertToTreeViewItems(data.Children);
              self._renderChildNodes($node, dataItems, false, false);
              $q.clearArray(dataItems);
            }

            self._hideAjaxLoadingIndicatorForNode($node);
            self._extendNodeElement($node, data);
            if (data.Children) {
              self._extendNodeElements($node, data.Children);
            }
            $q.callFunction(callback);
          }
        })
        .fail((jqXHR, textStatus, errorThrown) => {
          if (self._stopDeferredOperations) {
            return;
          }

          $q.processGenericAjaxError(jqXHR);
          $q.callFunction(callback);
        });
    }
  },

  _getNodeId($node) {
    let entityTypeId = null;
    if (!$q.isNullOrEmpty($node)) {
      const nodeCode = this.getNodeValue($node);
      if (nodeCode != this.ROOT_NODE_CODE) {
        entityTypeId = $node.data('node_id');
      }
    }

    return entityTypeId;
  },

  _getNodeType($node) {
    let nodeType = null;
    if (!$q.isNullOrEmpty($node)) {
      const nodeCode = this.getNodeValue($node);
      if (nodeCode != this.ROOT_NODE_CODE) {
        nodeType = $node.data('node_type');
      }
    }

    return nodeType;
  },

  _convertToTreeViewItems(items) {
    if (jQuery.isArray(items)) {
      return jQuery.map(items, $.proxy(this._convertToTreeViewItem, this));
    }
  },

  _convertToTreeViewItem(item) {
    let iconUrl = '';
    iconUrl = item.IconUrl.left(7).toLowerCase() !== 'http://'
      ? window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + item.IconUrl
      : item.IconUrl;

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
    if (item) {
      return `${item.NodeType}-${item.Id}`;
    }
  },

  _extendNodeElement(nodeElem, item) {
    const $node = this.getNode(nodeElem);
    if (!$q.isNullOrEmpty($node)) {
      $node.data('node_type', item.NodeType);
      $node.data('node_id', item.Id);
    }
  },

  _extendNodeElements(parentNodeElem, items) {
    const self = this;
    const $parentNode = $q.toJQuery(parentNodeElem);
    $.each(items, (index, item) => {
      const $node = self.getNode(self._converToItemValue(item), $parentNode);
      self._extendNodeElement($node, item);
    });
  },

  _loadChildNodes(entityTypeId) {
    return $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_ACTION_PERMISSION_TREE}GetTreeNodes`,
      {
        entityTypeId,
        userId: this._userId,
        groupId: this._groupId
      },
      true, false);
  },

  _loadNode(entityTypeId, actionId) {
    return $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_ACTION_PERMISSION_TREE}GetTreeNode`,
      {
        entityTypeId,
        actionId,
        userId: this._userId,
        groupId: this._groupId
      },
      true, false);
  },

  dispose() {
    const disposeContextMenu = $.proxy(
      function (contextMenuComponent) {
        if (contextMenuComponent) {
          contextMenuComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onNodeContextMenuShowingHandler);
          contextMenuComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onNodeContextMenuItemClickingHandler);
          contextMenuComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onNodeContextMenuHiddenHandler);

          contextMenuComponent.dispose();
        }
      }, this);

    $(this.NODE_NEW_CLICKABLE_SELECTORS, this._treeElement).off();

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

Quantumart.QP8.BackendActionPermissionTree.registerClass('Quantumart.QP8.BackendActionPermissionTree', Quantumart.QP8.BackendTreeBase);
