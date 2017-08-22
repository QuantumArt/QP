window.EVENT_TYPE_ACTION_PERMISSIONS_VIEW_EXECUTING = 'OnActionPermissionsViewExecuting';

Quantumart.QP8.BackendActionPermissionView = function (viewElementId, options, hostOptions) {
  Quantumart.QP8.BackendActionPermissionView.initializeBase(this);

  this._viewElementId = viewElementId;
};

Quantumart.QP8.BackendActionPermissionView.prototype = {
  _viewElementId: '',
  _treeComponent: null,
  _searchBlockComponent: null,

  initialize() {
    const $view = $(`#${this._viewElementId}`);
    const treeElementId = $('.treeContainer .t-treeview').attr('id');

    this._treeComponent = new Quantumart.QP8.BackendActionPermissionTree(treeElementId);
    this._treeComponent.attachObserver(window.EVENT_TYPE_ACTION_PERMISSIONS_TREE_EXECUTING, jQuery.proxy(this._onActionExecuting, this));
    this._treeComponent.initialize();
    this._searchBlockComponent = new Quantumart.QP8.BackendUserAndGroupSearchBlock(this._viewElementId, jQuery.proxy(this._onApplyFilter, this));
  },

  _onApplyFilter() {
    const searchData = this._searchBlockComponent.getSearchData();
    this._treeComponent.set_userId(searchData.userId);
    this._treeComponent.set_groupId(searchData.groupId);
    this._treeComponent.refreshTree();
  },

  _onActionExecuting(eventType, sender, eventArgs) {
    const actionCode = eventArgs.get_actionCode();
    if (actionCode == window.ACTION_CODE_CHANGE_ENTITY_TYPE_PERMISSION_NODE || actionCode == window.ACTION_CODE_REMOVE_ENTITY_TYPE_PERMISSION_NODE
      || actionCode == window.ACTION_CODE_CHANGE_ACTION_PERMISSION_NODE || actionCode == window.ACTION_CODE_REMOVE_ACTION_PERMISSION_NODE) {
      const eventArgsContext = eventArgs.get_context();
      eventArgs.set_context(Object.assign({}, eventArgsContext,
        {
          additionalUrlParameters: this._searchBlockComponent.getSearchData()
        }));
    }

    const action = $a.getBackendActionByCode(eventArgs.get_actionCode());
    if (action) {
      this.notify(window.EVENT_TYPE_ACTION_PERMISSIONS_VIEW_EXECUTING, eventArgs);
    }
  },

  get_Tree() {
    return this._treeComponent;
  },

  dispose() {
    Quantumart.QP8.BackendActionPermissionViewManager.getInstance().dispose();

    if (this._searchBlockComponent) {
      this._searchBlockComponent.dispose();
      this._searchBlockComponent = null;
    }

    if (this._treeComponent) {
      this._treeComponent.detachObserver(window.EVENT_TYPE_ACTION_PERMISSIONS_TREE_EXECUTING);
      this._treeComponent.dispose();
      this._treeComponent = null;
    }
  }
};

Quantumart.QP8.BackendActionPermissionView.registerClass('Quantumart.QP8.BackendActionPermissionView', Quantumart.QP8.Observable);
