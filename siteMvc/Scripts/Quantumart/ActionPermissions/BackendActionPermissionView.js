// #region event types of Action Permission view
var EVENT_TYPE_ACTION_PERMISSIONS_VIEW_EXECUTING = "OnActionPermissionsViewExecuting";

// #endregion


Quantumart.QP8.BackendActionPermissionView = function (viewElementId, options, hostOptions) {
	Quantumart.QP8.BackendActionPermissionView.initializeBase(this);

	this._viewElementId = viewElementId;
};

Quantumart.QP8.BackendActionPermissionView.prototype = {
	_viewElementId: "",
	_treeComponent: null,
	_searchBlockComponent: null,

	initialize: function () {
		var $view = jQuery('#' + this._viewElementId);
		var treeElementId = jQuery(".treeContainer .t-treeview").attr("id");

		this._treeComponent = new Quantumart.QP8.BackendActionPermissionTree(treeElementId);
		this._treeComponent.attachObserver(EVENT_TYPE_ACTION_PERMISSIONS_TREE_EXECUTING, jQuery.proxy(this._onActionExecuting, this));
		this._treeComponent.initialize();

		this._searchBlockComponent = new Quantumart.QP8.BackendUserAndGroupSearchBlock(this._viewElementId, jQuery.proxy(this._onApplyFilter, this));

		$view = null;
	},

	_onApplyFilter: function () {
		var searchData = this._searchBlockComponent.getSearchData();
		this._treeComponent.set_userId(searchData.userId);
		this._treeComponent.set_groupId(searchData.groupId);
		this._treeComponent.refreshTree();
	},

	_onActionExecuting: function (eventType, sender, eventArgs) {
		var actionCode = eventArgs.get_actionCode();
		if (actionCode == ACTION_CODE_CHANGE_ENTITY_TYPE_PERMISSION_NODE || actionCode == ACTION_CODE_REMOVE_ENTITY_TYPE_PERMISSION_NODE
			|| actionCode == ACTION_CODE_CHANGE_ACTION_PERMISSION_NODE || actionCode == ACTION_CODE_REMOVE_ACTION_PERMISSION_NODE) {
			var eventArgsContext = eventArgs.get_context();
			eventArgs.set_context(jQuery.extend(eventArgsContext,
			{
				additionalUrlParameters: this._searchBlockComponent.getSearchData()
			}));
		}

		var action = $a.getBackendActionByCode(eventArgs.get_actionCode());
		if (action) {
			this.notify(EVENT_TYPE_ACTION_PERMISSIONS_VIEW_EXECUTING, eventArgs);
		}
	},

	get_Tree: function () {
		return this._treeComponent;
	},

	dispose: function () {
		Quantumart.QP8.BackendActionPermissionViewManager.getInstance().destroyView();

		if (this._searchBlockComponent) {
			this._searchBlockComponent.dispose();
			this._searchBlockComponent = null;
		}

		if (this._treeComponent) {
			this._treeComponent.detachObserver(EVENT_TYPE_ACTION_PERMISSIONS_TREE_EXECUTING);
			this._treeComponent.dispose();
			this._treeComponent = null;
		}
	}
};

Quantumart.QP8.BackendActionPermissionView.registerClass("Quantumart.QP8.BackendActionPermissionView", Quantumart.QP8.Observable);
