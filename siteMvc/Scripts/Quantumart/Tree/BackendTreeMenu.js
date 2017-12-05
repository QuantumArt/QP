/* eslint max-lines: 'off' */

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

  // eslint-disable-next-line camelcase
  get_contextMenuManager() {
    return this._contextMenuManager;
  },

  // eslint-disable-next-line camelcase
  set_contextMenuManager(value) {
    this._contextMenuManager = value;
  },

  initialize() {
    Quantumart.QP8.BackendTreeMenu.callBaseMethod(this, 'initialize');
    const treeComponent = this._treeComponent;
    const $tree = $(this._treeComponent.element);
    if (this._contextMenuManagerComponent) {
      $tree.on(
        $.fn.jeegoocontext.getContextMenuEventType(),
        this.NODE_NEW_CLICKABLE_SELECTORS,
        this._onContextMenuHandler
      );

      this._contextMenuManagerComponent.attachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_SHOWING,
        this._onNodeContextMenuShowingHandler
      );

      this._contextMenuManagerComponent.attachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING,
        this._onNodeContextMenuItemClickingHandler
      );

      this._contextMenuManagerComponent.attachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_HIDDEN,
        this._onNodeContextMenuHiddenHandler
      );
    }

    this.addNodesToParentNode(treeComponent.element, 2);
  },

  fixTreeHeight(leftSplitterPaneHeight) {
    if (leftSplitterPaneHeight || leftSplitterPaneHeight === 0) {
      const newHeight = leftSplitterPaneHeight - 46;
      if (newHeight > 0) {
        $('#tree').css('height', `${newHeight}px`);
        $('#menuContainer').show();
      }
    }
  },

  markTreeAsBusy() {
    const $tree = $(this._treeComponent.element);
    $tree.find('UL:first').addClass(this.NODE_BUSY_CLASS_NAME);
  },

  unmarkTreeAsBusy() {
    const $tree = $(this._treeComponent.element);
    $tree.find('UL:first').removeClass(this.NODE_BUSY_CLASS_NAME);
  },

  isTreeBusy() {
    const $tree = $(this._treeComponent.element);
    return $tree.find('UL:first').hasClass(this.NODE_BUSY_CLASS_NAME);
  },

  generateNodeCode(entityTypeCode, entityId, parentEntityId, isFolder) {
    return `${entityTypeCode}${isFolder ? 's' : ''}_${entityId}_${parentEntityId || 0}`;
  },

  generateNodeCodeToHighlight(eventArgs) {
    const isFolder = eventArgs.get_actionTypeCode() === window.ACTION_TYPE_CODE_LIST;
    const entityId = isFolder
      ? Quantumart.QP8.BackendEntityType.getEntityTypeByCode(eventArgs.get_entityTypeCode()).Id
      : eventArgs.get_entityId();

    return this.generateNodeCode(eventArgs.get_entityTypeCode(), entityId, eventArgs.get_parentEntityId(), isFolder);
  },

  addNodesToParentNode(parentNode, maxExpandLevel) {
    this._addNodesToParentNode(
      parentNode,
      maxExpandLevel,
      Quantumart.QP8.BackendTreeMenu.getTreeMenuChildNodesList,
      jqXHR => {
        $q.processGenericAjaxError(jqXHR);
      }
    );
  },

  _addNodesToParentNode(parentNode, maxExpandLevel, getChildNodes, errorHandler) {
    const that = this;
    const $parentNode = this.getNode(parentNode);
    const isRootNode = this.isRootNode($parentNode);
    let level = 0;
    let entityTypeCode = null;
    let parentEntityId = null;
    let isFolder = false;
    let isGroup = false;
    let groupItemCode = null;

    if (!isRootNode) {
      level = this.getNodeLevel($parentNode);
      entityTypeCode = $parentNode.data('entity_type_code');
      parentEntityId = $parentNode.data('is_folder')
        ? $parentNode.data('parent_entity_id')
        : $parentNode.data('entity_id');

      isFolder = $parentNode.data('is_folder');
      isGroup = $parentNode.data('is_group');
      groupItemCode = $parentNode.data('group_item_code');
    }

    if (level < maxExpandLevel || maxExpandLevel === 0) {
      $parentNode.data('loading_icon_timeout', setTimeout(() => {
        $parentNode.find('> DIV > .t-icon').addClass('t-loading');
      }, 100));

      getChildNodes({
        entityTypeCode,
        parentEntityId,
        isFolder,
        isGroup,
        groupItemCode
      }, nodes => {
        let dataItems = that.getTreeViewItemCollectionFromTreeNodes(nodes);
        if (dataItems.length === 0) {
          dataItems = null;
          return;
        }

        that._renderChildNodes($parentNode, dataItems, isRootNode);
        $q.clearArray(dataItems);
        that._hideAjaxLoadingIndicatorForNode($parentNode);
        that._extendNodeElements($parentNode, nodes);

        if (maxExpandLevel !== 0) {
          $parentNode.find('> UL > LI').each((index, $childNode) => {
            that._addNodesToParentNode($childNode, maxExpandLevel, getChildNodes);
          });
        }

        $q.clearArray(nodes);
      }, errorHandler);
    }
  },

  _refreshNodeInner($node, loadChildNodes, callback) {
    if ($q.isNullOrEmpty($node)) {
      return;
    }

    const that = this;
    const $parentNode = this.getParentNode($node);

    const entityTypeCode = $parentNode ? $parentNode.data('entity_type_code') : null;
    const entityId = $parentNode ? $node.data('entity_id') : 0;
    const isFolder = $parentNode ? $parentNode.data('is_folder') : false;

    let parentEntityId = null;
    if ($parentNode) {
      parentEntityId = isFolder ? $parentNode.data('parent_entity_id') : $parentNode.data('entity_id');
    }

    const isGroup = $parentNode ? $parentNode.data('is_group') : false;
    const groupItemCode = $parentNode ? $parentNode.data('group_item_code') : '';
    const isRootNode = this.isRootNode($node);

    this._showAjaxLoadingIndicatorForNode($node);
    Quantumart.QP8.BackendTreeMenu.getTreeMenuNode(
      entityTypeCode,
      entityId,
      parentEntityId,
      isFolder,
      isGroup,
      groupItemCode,
      loadChildNodes,
      data => {
        const node = data;
        if (node) {
          node.NodeCode = that.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder);
          const dataItem = that.getTreeViewItemFromTreeNode(node);

          that._renderNode($node, dataItem, isRootNode);
          that._extendNodeElement($node, node);

          const childNodes = node.ChildNodes;
          let dataItems = that.getTreeViewItemCollectionFromTreeNodes(childNodes);

          if (!dataItems.length) {
            that._hideAjaxLoadingIndicatorForNode($node);
            dataItems = null;
            return;
          }

          that._renderChildNodes($node, dataItems, isRootNode);
          $q.clearArray(dataItems);

          that._hideAjaxLoadingIndicatorForNode($node);
          that._extendNodeElements($node, childNodes);
          $q.clearArray(childNodes);
        }

        if (that._deferredNodeCodeToHighlight) {
          that.highlightNode(that._deferredNodeCodeToHighlight);
        }

        $q.callFunction(callback);
      }, jqXHR => {
        $q.processGenericAjaxError(jqXHR);
        $q.callFunction(callback);
      }
    );
  },

  highlightNode(node) {
    const $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      this.unhighlightAllNodes();
      $node.find(this.NODE_WRAPPER_SELECTOR).addClass(this.NODE_SELECTED_CLASS_NAME);
      this.scrollToNode($node);
    }
  },

  highlightNodeWithEventArgs(eventArgs) {
    const nodeCode = this.generateNodeCodeToHighlight(eventArgs);
    this._deferredNodeCodeToHighlight = nodeCode;
    const $highlightedNode = this.getNode(nodeCode);

    if (!$q.isNullOrEmpty($highlightedNode)) {
      let $node = $highlightedNode;
      if (eventArgs.isExpandRequested) {
        const that = this;
        Quantumart.QP8.BackendTreeMenu.getSubTreeToEntity(
          eventArgs.get_entityTypeCode(),
          eventArgs.get_parentEntityId(),
          eventArgs.get_entityId(),
          data => {
            if (!data) {
              return;
            }

            const _exp = function (node) {
              if (node) {
                const tempNode = that.getNode(that.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder));
                $node = that.getNode(tempNode);
                if (that._isNodeCollapsed($node)) {
                  that._treeComponent.nodeToggle(null, $node, true);
                }

                return _exp($.grep(node.ChildNodes, leaf => !!leaf.ChildNodes)[0]) || node;
              }

              return undefined;
            };

            _exp(data);
          }
        );
      }

      this.highlightNode($highlightedNode);
    } else if (eventArgs.isExpandRequested) {
      this._expandToEntityNode(
        eventArgs.get_entityTypeCode(),
        eventArgs.get_parentEntityId(),
        eventArgs.get_entityId()
      );
    }
  },

  _expandToEntityNode(entityTypeCode, parentEntityId, entityId) {
    const that = this;
    Quantumart.QP8.BackendTreeMenu.getSubTreeToEntity(entityTypeCode, parentEntityId, entityId, data => {
      if (data) {
        return;
      }

      const findDeepest = function (node, toExpand) {
        let tempNode = that.getNode(that.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder));
        if (!node || $q.isNullOrEmpty(tempNode)) {
          return null;
        }

        if (toExpand) {
          tempNode = that.getNode(that.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder));
          const $node = that.getNode(tempNode);
          if (that._isNodeCollapsed($node)) {
            that._treeComponent.nodeToggle(null, $node, true);
          }
        }

        return findDeepest($.grep(node.ChildNodes, leaf => !!leaf.ChildNodes)[0], toExpand) || node;
      };

      let deepestExistedNode = findDeepest(data);
      const findChildren = function (options) {
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

        return findChildren(options, $.grep(parent.ChildNodes || [], leaf => !!leaf.ChildNodes)[0]);
      };

      if (deepestExistedNode) {
        let nodeCode = that.generateNodeCode(
          deepestExistedNode.Code,
          deepestExistedNode.Id,
          deepestExistedNode.ParentId,
          deepestExistedNode.IsFolder
        );

        that._addNodesToParentNode(that.getNode(nodeCode), 100, (options, successHandler) => {
          successHandler(findChildren(options, deepestExistedNode).slice(0));
        }, () => {
          $q.alertError('Error handler was called while rendering tree');
        });

        deepestExistedNode = findDeepest(data, true);
        nodeCode = that.generateNodeCode(
          deepestExistedNode.Code,
          deepestExistedNode.Id,
          deepestExistedNode.ParentId,
          deepestExistedNode.IsFolder
        );

        const $node = that.getNode(nodeCode);
        that._deferredNodeCodeToHighlight = nodeCode;
        that.highlightNode($node);
      }
    });
  },

  unhighlightAllNodes() {
    this._deferredNodeCodeToHighlight = '';
    this.getAllNodes().find(this.NODE_WRAPPER_SELECTOR).removeClass(this.NODE_SELECTED_CLASS_NAME);
  },

  scrollToNode(node) {
    const $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      $(`#${this._treeContainerElementId}`).scrollTo($node, { offset: -100, duration: 300, axis: 'y' });
    }
  },

  fillTreeViewItemFromTreeNode(dataItem, node) {
    const iconUrl = node.Icon.left(7).toLowerCase() === 'http://'
      ? node.Icon
      : window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + node.Icon;

    return Object.assign({}, dataItem, {
      Value: node.NodeCode,
      Text: node.Title,
      ImageUrl: iconUrl,
      LoadOnDemand: node.HasChildren,
      Expanded: false
    });
  },

  getTreeViewItemFromTreeNode(node) {
    return this.fillTreeViewItemFromTreeNode({}, node);
  },

  fillTreeViewItemCollectionFromTreeNodes(dataItems, nodes) {
    const that = this;
    if (nodes && nodes.length) {
      $.each(nodes, (index, node) => {
        const dataItem = that.getTreeViewItemFromTreeNode(Object.assign(node, {
          NodeCode: that.generateNodeCode(node.Code, node.Id, node.ParentId, node.IsFolder)
        }));

        Array.add(dataItems, dataItem);
      });
    }
  },

  getTreeViewItemCollectionFromTreeNodes(nodes) {
    const dataItems = [];
    this.fillTreeViewItemCollectionFromTreeNodes(dataItems, nodes);
    return dataItems;
  },

  _extendNodeElement(nodeElem, node) {
    const $node = this.getNode(nodeElem);

    $node.data({
      // eslint-disable-next-line camelcase
      entity_type_code: node.Code,

      // eslint-disable-next-line camelcase
      entity_id: node.Id,

      // eslint-disable-next-line camelcase
      entity_name: node.Title,

      // eslint-disable-next-line camelcase
      parent_entity_id: node.ParentId,

      // eslint-disable-next-line camelcase
      is_folder: node.IsFolder,

      // eslint-disable-next-line camelcase
      is_group: node.IsGroup,

      // eslint-disable-next-line camelcase
      group_item_code: node.GroupItemCode,

      // eslint-disable-next-line camelcase
      default_action_code: node.DefaultActionCode,

      // eslint-disable-next-line camelcase
      default_action_type_code: node.DefaultActionTypeCode,

      // eslint-disable-next-line camelcase
      context_menu_code: node.ContextMenuCode
    });

    const contextMenuCode = node.ContextMenuCode;
    if (!$q.isNullOrWhiteSpace(contextMenuCode) && this._contextMenuManagerComponent) {
      let contextMenu = this._contextMenuManagerComponent.getContextMenu(contextMenuCode);
      if ($q.isNullOrEmpty(contextMenu)) {
        const targetElems = this._treeComponent.element;
        contextMenu = this._contextMenuManagerComponent.createContextMenu(
          contextMenuCode,
          `treeContextMenu_${contextMenuCode}`,
          {
            targetElements: targetElems,
            allowManualShowing: true
          }
        );

        contextMenu.addMenuItemsToMenu(false);
      }
    }
  },

  _extendNodeElements(parentNodeElem, nodes) {
    const that = this;
    const $parentNode = $q.toJQuery(parentNodeElem);
    $.each(nodes, (index, node) => {
      const $node = that.getNode(node.NodeCode, $parentNode);
      that._extendNodeElement($node, node);
    });
  },

  executeAction(node, actionCode) {
    const $node = this.getNode(node);
    if (!$q.isNullOrEmpty($node)) {
      const action = $a.getBackendActionByCode(actionCode);
      if (action) {
        const actionTypeCode = action.ActionType.Code;
        const isCustomAction = action.IsCustom;
        if (!isCustomAction && actionTypeCode === window.ACTION_TYPE_CODE_REFRESH) {
          this.refreshNode($node);
          return;
        }

        const isFolder = $q.toBoolean($node.data('is_folder'), false);
        const isGroup = $q.toBoolean($node.data('is_group'), false);

        const params = new Quantumart.QP8.BackendActionParameters({
          entityTypeCode: $node.data('entity_type_code'),
          entityId: isFolder ? 0 : $node.data('entity_id'),
          entityName: $node.data('entity_name'),
          parentEntityId: +$node.data('parent_entity_id') || 0,
          isGroup
        });

        params.correct(action);
        const eventArgs = $a.getEventArgsFromActionWithParams(action, params);
        this.notify(window.EVENT_TYPE_TREE_MENU_ACTION_EXECUTING, eventArgs);
      } else {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
      }
    }
  },

  onActionExecuted(eventArgs) {
    const entityTypeCode = eventArgs.get_entityTypeCode();
    const actionTypeCode = eventArgs.get_actionTypeCode();
    const parentEntityId = eventArgs.get_parentEntityId();
    const entityId = eventArgs.get_entityId();

    if (eventArgs.get_isRemoving()
      || actionTypeCode === window.ACTION_TYPE_CODE_COPY || eventArgs.get_isSaved() || eventArgs.get_isUpdated()) {
      const entityType = Quantumart.QP8.BackendEntityType.getEntityTypeByCode(entityTypeCode);
      if (entityType) {
        const parentNodeCode = this.generateNodeCode(entityTypeCode, entityType.Id, parentEntityId, true);
        const nodeCode = this.generateNodeCode(entityTypeCode, entityId, parentEntityId, false);
        const orderChanged = eventArgs.get_context() && eventArgs.get_context().orderChanged;
        const groupChanged = eventArgs.get_context() && eventArgs.get_context().groupChanged;
        const options = { loadChildNodes: true };

        if (actionTypeCode === window.ACTION_TYPE_CODE_REMOVE) {
          this.removeNodeOrRefreshParent(nodeCode, parentNodeCode, options);
        } else if (actionTypeCode === window.ACTION_TYPE_CODE_MULTIPLE_REMOVE
          || actionTypeCode === window.ACTION_TYPE_CODE_COPY
          || eventArgs.get_isSaved()
          || (eventArgs.get_isUpdated() && (orderChanged || groupChanged))
        ) {
          this.refreshNode(parentNodeCode);
        } else if (eventArgs.get_isUpdated() && !orderChanged) {
          this.refreshNode(nodeCode, options);
        }
      } else {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
      }
    }
  },

  _onDataBinding(sender) {
    this.addNodesToParentNode(sender, 0);
  },

  _onNodeClicking(e) {
    const $element = $(e.currentTarget);
    const nodeEl = $($element.closest('.t-item')[0]);
    if (!this._treeComponent.shouldNavigate($element)) {
      e.preventDefault();
      return false;
    }

    if (this.isTreeBusy()) {
      e.preventDefault();
    } else {
      nodeEl.find(this.NODE_WRAPPER_SELECTOR).removeClass(this.NODE_HOVER_CLASS_NAME);
      this.highlightNode(nodeEl);
      if (!nodeEl.data('is_folder') && this._isNodeCollapsed(nodeEl)) {
        this._treeComponent.nodeToggle(null, nodeEl, true);
      }

      if (!$q.isNullOrWhiteSpace(nodeEl.data('default_action_code'))) {
        this.executeAction(nodeEl, nodeEl.data('default_action_code'));
      }
    }

    return undefined;
  },

  _onContextMenu(e) {
    const $element = $(e.currentTarget);
    const $node = $($element.closest('.t-item')[0]);
    const contextMenuCode = $node.data('context_menu_code');
    if (!this.isTreeBusy()) {
      if (contextMenuCode && this._contextMenuManagerComponent) {
        const contextMenuComponent = this._contextMenuManagerComponent.getContextMenu(contextMenuCode);
        if (contextMenuComponent) {
          contextMenuComponent.showMenu(e, $element.parent().parent().get(0));
        }
      }
    }

    e.preventDefault();
  },

  _onNodeContextMenuShowing(eventType, sender, args) {
    const menuComponent = args.get_menu();
    const $node = $(args.get_targetElement());
    if (menuComponent && $node.length) {
      menuComponent.tuneMenuItems($node.data('entity_id'));
    }
  },

  _onNodeContextMenuItemClicking(eventType, sender, args) {
    const $menuItem = $(args.get_menuItem());
    if ($menuItem.length) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }
  },

  _onNodeContextMenuHidden(eventType, sender, args) {
    const $node = $(args.get_targetElement());
    if (this._contextMenuActionCode) {
      this.executeAction($node, this._contextMenuActionCode);
      this._contextMenuActionCode = '';
    }
  },

  dispose() {
    const $tree = $(`#${this._treeElementId}`);
    $tree.off('click', this.NODE_NEW_CLICKABLE_SELECTORS, this._onNodeClickingHandler);

    if (this._contextMenuManagerComponent) {
      $tree.off(
        $.fn.jeegoocontext.getContextMenuEventType(),
        this.NODE_NEW_CLICKABLE_SELECTORS,
        this._onContextMenuHandler
      );

      this._contextMenuManagerComponent.detachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_SHOWING,
        this._onNodeContextMenuShowingHandler
      );

      this._contextMenuManagerComponent.detachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING,
        this._onNodeContextMenuItemClickingHandler
      );

      this._contextMenuManagerComponent.detachObserver(
        window.EVENT_TYPE_CONTEXT_MENU_HIDDEN,
        this._onNodeContextMenuHiddenHandler
      );
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

// eslint-disable-next-line max-params
Quantumart.QP8.BackendTreeMenu.getTreeMenuNode = function (
  entityTypeCode,
  entityId,
  parentEntityId,
  isFolder,
  isGroup,
  groupItemCode,
  loadChildNodes,
  successHandler,
  errorHandler
) {
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_TREE_MENU}GetNode`, {
    entityTypeCode,
    entityId,
    parentEntityId,
    isFolder,
    isGroup,
    groupItemCode,
    loadChildNodes
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

Quantumart.QP8.BackendTreeMenu.getSubTreeToEntity = function (
  entityTypeCode,
  parentEntityId,
  entityId,
  successHandler
) {
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_TREE_MENU}GetSubTreeToEntity`, {
    entityTypeCode,
    parentEntityId,
    entityId
  }, false, false, successHandler, jqXHR => {
    $q.processGenericAjaxError(jqXHR);
  });
};

Quantumart.QP8.BackendTreeMenu.registerClass('Quantumart.QP8.BackendTreeMenu', Quantumart.QP8.BackendTreeBase);
