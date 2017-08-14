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
    var treeComponent = this._treeComponent;
    var $tree = $(this._treeComponent.element);
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
        $('#tree').css('height', `${leftSplitterPaneHeight  }px`);
        $('#menuContainer').show();
      }
    }
  },

  markTreeAsBusy: function () {
    var $tree = $(this._treeComponent.element);
    $tree.find('UL:first').addClass(this.NODE_BUSY_CLASS_NAME);
  },

  unmarkTreeAsBusy: function () {
    var $tree = $(this._treeComponent.element);
    $tree.find('UL:first').removeClass(this.NODE_BUSY_CLASS_NAME);
  },

  isTreeBusy: function () {
    var $tree = $(this._treeComponent.element);
    return $tree.find('UL:first').hasClass(this.NODE_BUSY_CLASS_NAME);
  },

  generateNodeCode: function (entityTypeCode, entityId, parentEntityId, isFolder) {
    return String.format('{0}{1}_{2}_{3}', entityTypeCode, isFolder ? 's' : '', entityId, parentEntityId ? parentEntityId : 0);
  },

  generateNodeCodeToHighlight: function (eventArgs) {
    var isFolder = eventArgs.get_actionTypeCode() == window.ACTION_TYPE_CODE_LIST;
    var entityId = !isFolder ? eventArgs.get_entityId() : Quantumart.QP8.BackendEntityType.getEntityTypeByCode(eventArgs.get_entityTypeCode()).Id;
    return this.generateNodeCode(eventArgs.get_entityTypeCode(), entityId, eventArgs.get_parentEntityId(), isFolder);
  },

  addNodesToParentNode: function (parentNode, maxExpandLevel) {
    this._addNodesToParentNode(parentNode, maxExpandLevel, Quantumart.QP8.BackendTreeMenu.getTreeMenuChildNodesList, function (jqXHR) {
      $q.processGenericAjaxError(jqXHR);
    });
  },

  _addNodesToParentNode: function (parentNode, maxExpandLevel, getChildNodes, errorHandler) {
    var self = this;
    let $parentNode = this.getNode(parentNode);
    var isRootNode = this.isRootNode($parentNode);
    var level = 0;
    var entityTypeCode = null;
    var entityId = null;
    var parentEntityId = null;
    var isFolder = false;
    var isGroup = false;
    var groupItemCode = null;

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
      $parentNode.data('loading_icon_timeout', setTimeout(function () {
        $parentNode.find('> DIV > .t-icon').addClass('t-loading');
      }, 100));

      getChildNodes({
        entityTypeCode: entityTypeCode,
        parentEntityId: parentEntityId,
        isFolder: isFolder,
        isGroup: isGroup,
        groupItemCode: groupItemCode
      }, function (nodes) {
        var dataItems = self.getTreeViewItemCollectionFromTreeNodes(nodes);
        if (dataItems.length === 0) {
          dataItems = null;
          return;
        }

        self._renderChildNodes($parentNode, dataItems, isRootNode);
        $q.clearArray(dataItems);
        self._hideAjaxLoadingIndicatorForNode($parentNode);
        self._extendNodeElements($parentNode, nodes);

        if (maxExpandLevel != 0) {
          $parentNode.find('> UL > LI').each(function (index, $childNode) {
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

    var self = this;
    let $parentNode = this.getParentNode($node);

    var entityTypeCode = $parentNode ? $parentNode.data('entity_type_code') : null;
    var entityId = $parentNode ? $node.data('entity_id') : 0;
    var isFolder = $parentNode ? $parentNode.data('is_folder') : false;
    var parentEntityId = $parentNode ? isFolder ? $parentNode.data('parent_entity_id') : $parentNode.data('entity_id') : null;
    var isGroup = $parentNode ? $parentNode.data('is_group') : false;
    var groupItemCode = $parentNode ? $parentNode.data('group_item_code') : '';
    var isRootNode = this.isRootNode($node);

    this._showAjaxLoadingIndicatorForNode($node);
    Quantumart.QP8.BackendTreeMenu.getTreeMenuNode(entityTypeCode, entityId, parentEntityId, isFolder, isGroup, groupItemCode, loadChildNodes, function (data) {
      var node = data;
      if (node) {
        node.NodeCode = self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder);
        var dataItem = self.getTreeViewItemFromTreeNode(node);

        self._renderNode($node, dataItem, isRootNode);
        self._extendNodeElement($node, node);

        dataItem = null;

        var childNodes = node.ChildNodes;
        var dataItems = self.getTreeViewItemCollectionFromTreeNodes(childNodes);

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
    }, function (jqXHR) {
      $q.processGenericAjaxError(jqXHR);
      $q.callFunction(callback);
    });
  },

  highlightNode: function (node) {
    var $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      this.unhighlightAllNodes();
      $node.find(this.NODE_WRAPPER_SELECTOR).addClass(this.NODE_SELECTED_CLASS_NAME);
      this.scrollToNode($node);
    }
  },

  highlightNodeWithEventArgs: function (eventArgs) {
    var nodeCode = this.generateNodeCodeToHighlight(eventArgs);
    this._deferredNodeCodeToHighlight = nodeCode;
    var $highlightedNode = this.getNode(nodeCode);

    if (!$q.isNullOrEmpty($highlightedNode)) {
      var $node = $highlightedNode;
      if (eventArgs.isExpandRequested) {
        var self = this;
        Quantumart.QP8.BackendTreeMenu.getSubTreeToEntity(eventArgs.get_entityTypeCode(), eventArgs.get_parentEntityId(), eventArgs.get_entityId(), function (data) {
          if (!data) {
            return;
          }

          var _exp = function (node) {
            if (!node || $q.isNullOrEmpty(self.getNode(self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder)))) {
              return null;
            }

              $node = self.getNode(self.getNode(self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder)));
              if (self._isNodeCollapsed($node)) {
                self._treeComponent.nodeToggle(null, $node, true);
              }

              return _exp($.grep(node.ChildNodes, function (n) {
                return n.ChildNodes != null;
              })[0]) || node;

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
    var self = this;
    Quantumart.QP8.BackendTreeMenu.getSubTreeToEntity(entityTypeCode, parentEntityId, entityId, function (data) {
      if (data) {
        return;
      }

      var findDeepest = function (node, toExpand) {
        if (!node || $q.isNullOrEmpty(self.getNode(self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder)))) {
          return null;
        }

          if (toExpand) {
            let $node = self.getNode(self.getNode(self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder)));
            if (self._isNodeCollapsed($node)) {
              self._treeComponent.nodeToggle(null, $node, true);
            }
          }

          return findDeepest($.grep(node.ChildNodes, function (n) {
            return n.ChildNodes != null;
          })[0], toExpand) || node;

      };

      var findChildren = function (options, parent) {
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
          return findChildren(options, $.grep(parent.ChildNodes || [], function (n) {
            return n.ChildNodes != null;
          })[0]);
      };

      var deepestExistedNode = findDeepest(data);
      if (deepestExistedNode) {
        let nodeCode = self.generateNodeCode(deepestExistedNode.Code, deepestExistedNode.Id, deepestExistedNode.ParentId, deepestExistedNode.IsFolder);

        self._addNodesToParentNode(self.getNode(nodeCode), 100, function (options, successHandler) {
          successHandler(findChildren(options, deepestExistedNode).slice(0));
        }, function () {});

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
    var $nodes = this.getAllNodes();
    $nodes.find(this.NODE_WRAPPER_SELECTOR).removeClass(this.NODE_SELECTED_CLASS_NAME);
    $nodes = null;
  },

  scrollToNode: function (node) {
    var $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      $(`#${  this._treeContainerElementId}`).scrollTo($node, { offset: -100, duration: 300, axis: 'y' });
    }
  },

  fillTreeViewItemFromTreeNode: function (dataItem, node) {
    var iconUrl = node.Icon.left(7).toLowerCase() !== 'http://'
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
    var self = this;
    var nodeCount = 0;
    if (nodes != null) {
      nodeCount = nodes.length;
    }

    if (nodeCount > 0) {
      $.each(nodes, function (index, node) {
        node.NodeCode = self.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder);
        var dataItem = self.getTreeViewItemFromTreeNode(node);
        Array.add(dataItems, dataItem);
      });
    }
  },

  getTreeViewItemCollectionFromTreeNodes: function (nodes) {
    var dataItems = [];
    this.fillTreeViewItemCollectionFromTreeNodes(dataItems, nodes);
    return dataItems;
  },

  _extendNodeElement: function (nodeElem, node) {
    var $node = this.getNode(nodeElem);
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

    var contextMenuCode = node.ContextMenuCode;
    if (!$q.isNullOrWhiteSpace(contextMenuCode) && this._contextMenuManagerComponent) {
      var contextMenu = this._contextMenuManagerComponent.getContextMenu(contextMenuCode);
      if ($q.isNullOrEmpty(contextMenu)) {
        var targetElems = this._treeComponent.element;
        contextMenu = this._contextMenuManagerComponent.createContextMenu(contextMenuCode, String.format('treeContextMenu_{0}', contextMenuCode), {
          targetElements: targetElems,
          allowManualShowing: true
        });

        contextMenu.addMenuItemsToMenu(false);
      }
    }
  },

  _extendNodeElements: function (parentNodeElem, nodes) {
    var self = this;
    let $parentNode = $q.toJQuery(parentNodeElem);
    $.each(nodes, function (index, node) {
      var $node = self.getNode(node.NodeCode, $parentNode);
      self._extendNodeElement($node, node);
    });
  },

  executeAction: function (node, actionCode) {
    var $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      var action = $a.getBackendActionByCode(actionCode);
      if (!action) {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
      } else {
        var actionTypeCode = action.ActionType.Code;
        var isCustomAction = action.IsCustom;
        if (!isCustomAction && actionTypeCode === window.ACTION_TYPE_CODE_REFRESH) {
          this.refreshNode($node);
          return;
        }

        var isFolder = $q.toBoolean($node.data('is_folder'), false);
        var isGroup = $q.toBoolean($node.data('is_group'), false);

        var params = new Quantumart.QP8.BackendActionParameters({
          entityTypeCode: $node.data('entity_type_code'),
          entityId: isFolder ? 0 : $node.data('entity_id'),
          entityName: $node.data('entity_name'),
          parentEntityId: +$node.data('parent_entity_id') || 0,
          isGroup: isGroup
        });

        params.correct(action);
        var eventArgs = $a.getEventArgsFromActionWithParams(action, params);
        this.notify(window.EVENT_TYPE_TREE_MENU_ACTION_EXECUTING, eventArgs);
      }
    }
  },

  onActionExecuted: function (eventArgs) {
    var entityTypeCode = eventArgs.get_entityTypeCode();
    var actionTypeCode = eventArgs.get_actionTypeCode();
    var parentEntityId = eventArgs.get_parentEntityId();
    var entityId = eventArgs.get_entityId();

    if (eventArgs.get_isRemoving() || actionTypeCode == window.ACTION_TYPE_CODE_COPY || eventArgs.get_isSaved() || eventArgs.get_isUpdated()) {
      var entityType = Quantumart.QP8.BackendEntityType.getEntityTypeByCode(entityTypeCode);
      if (!entityType) {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
      } else {
        var parentNodeCode = this.generateNodeCode(entityTypeCode, entityType.Id, parentEntityId, true);
        var nodeCode = this.generateNodeCode(entityTypeCode, entityId, parentEntityId, false);
        var orderChanged = eventArgs.get_context() && eventArgs.get_context().orderChanged;
        var groupChanged = eventArgs.get_context() && eventArgs.get_context().groupChanged;
        var options = { loadChildNodes: true };

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
    var $element = $(e.currentTarget);
    var nodeEl = $($element.closest('.t-item')[0]);
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
    var $element = $(e.currentTarget);
    var $node = $($element.closest('.t-item')[0]);
    var contextMenuCode = $node.data('context_menu_code');
    if (!this.isTreeBusy()) {
      if (contextMenuCode && this._contextMenuManagerComponent) {
        var contextMenuComponent = this._contextMenuManagerComponent.getContextMenu(contextMenuCode);
        if (contextMenuComponent) {
          contextMenuComponent.showMenu(e, $element.parent().parent().get(0));
        }
      }
    }

    e.preventDefault();
  },

  _onNodeContextMenuShowing: function (eventType, sender, args) {
    var menuComponent = args.get_menu();
    var $node = $(args.get_targetElement());
    if (menuComponent && $node.length) {
      menuComponent.tuneMenuItems($node.data('entity_id'));
    }
  },

  _onNodeContextMenuItemClicking: function (eventType, sender, args) {
    var $menuItem = $(args.get_menuItem());
    if ($menuItem.length) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }
  },

  _onNodeContextMenuHidden: function (eventType, sender, args) {
    var $node = $(args.get_targetElement());
    if (this._contextMenuActionCode) {
      this.executeAction($node, this._contextMenuActionCode);
      this._contextMenuActionCode = '';
    }
  },

  dispose: function () {
    var $tree = $(`#${  this._treeElementId}`);
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
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_TREE_MENU  }GetNode`, {
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
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_TREE_MENU  }GetChildNodesList`, {
    entityTypeCode: options.entityTypeCode,
    parentEntityId: options.parentEntityId,
    isFolder: options.isFolder,
    isGroup: options.isGroup,
    groupItemCode: options.groupItemCode
  }, false, false, successHandler, errorHandler);
};

Quantumart.QP8.BackendTreeMenu.getSubTreeToEntity = function (entityTypeCode, parentEntityId, entityId, successHandler) {
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_TREE_MENU  }GetSubTreeToEntity`, {
    entityTypeCode: entityTypeCode,
    parentEntityId: parentEntityId,
    entityId: entityId
  }, false, false, successHandler, function (jqXHR) {
    $q.processGenericAjaxError(jqXHR);
  });
};

Quantumart.QP8.BackendTreeMenu.registerClass('Quantumart.QP8.BackendTreeMenu', Quantumart.QP8.BackendTreeBase);
