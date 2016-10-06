//#region class BackendBreadCrumbsManager
// === Класс "Менеджер хлебных крошек" ===
Quantumart.QP8.BackendBreadCrumbsManager = function (breadCrumbsContainerElementId, options) {
	Quantumart.QP8.BackendBreadCrumbsManager.initializeBase(this);

	if (!breadCrumbsContainerElementId) {
		throw new Error($l.BreadCrumbs.breadCrumbsContainerElementIdNotSpecified);
	}

	this._breadCrumbsContainerElementId = breadCrumbsContainerElementId;
};

Quantumart.QP8.BackendBreadCrumbsManager.prototype = {
	_breadCrumbsContainerElementId: "", // клиентский идентификатор контейнера, в котором располагаются хлебные крошки
	_breadCrumbs: {}, // список хлебных крошек

	ITEM_CLASS_NAME: "item",

	getBreadCrumbs: function (breadCrumbsElementId) {
		var breadCrumbs = null;

		if (this._breadCrumbs[breadCrumbsElementId]) {
			breadCrumbs = this._breadCrumbs[breadCrumbsElementId];
		}

		return breadCrumbs;
	},

	createBreadCrumbs: function (breadCrumbsElementId, options) {
		var breadCrumbs = new Quantumart.QP8.BackendBreadCrumbs(breadCrumbsElementId, options);
		breadCrumbs.set_manager(this);
		breadCrumbs.initialize();

		this._breadCrumbs[breadCrumbsElementId] = breadCrumbs;

		return breadCrumbs;
	},

	refreshBreadCrumbs: function (breadCrumbsElementId, callback) {
		var breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
		if (breadCrumbs) {
			breadCrumbs.addItemsToBreadCrumbs(callback);
		}

		breadCrumbs = null;
	},

	removeBreadCrumbs: function (breadCrumbsElementId) {
		var breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
		if (breadCrumbs) {
			$q.removeProperty(this._breadCrumbs, breadCrumbsElementId);
		}
	},

	destroyBreadCrumbs: function (breadCrumbsElementId) {
		var breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
		if (breadCrumbs) {
			this.removeBreadCrumbs(breadCrumbsElementId);

			if (breadCrumbs.dispose) {
				breadCrumbs.dispose();
			}
			breadCrumbs = null;
		}
	},

	generateItemCode: function (entityTypeCode, parentEntityId, entityId) {
		var itemCode = String.format("{0}_{1}_{2}", entityTypeCode, $q.toInt(parentEntityId, 0), entityId);

		return itemCode;
	},

	_getBreadCrumbsElementIdByItem: function (itemElem) {
		var breadCrumbsElementId = "";

		var $breadCrumbs = $q.toJQuery(itemElem).parent().parent();
		if (!$q.isNullOrEmpty($breadCrumbs)) {
			breadCrumbsElementId = $breadCrumbs.attr("id");
		}

		$breadCrumbs = null;

		return breadCrumbsElementId;
	},

	getItems: function (itemCode) {
		var $items = jQuery("#" + this._breadCrumbsContainerElementId)
		.find("LI[code = '" + itemCode + "']." + this.ITEM_CLASS_NAME)
		;

		return $items;
	},

	getBreadCrumbsListByItemCode: function (itemCode) {
		var breadCrumbsList = [];
		var $items = this.getItems(itemCode);

		for (var itemIndex = 0, itemCount = $items.length; itemIndex < itemCount; itemIndex++) {
			var breadCrumbsElementId = this._getBreadCrumbsElementIdByItem($items.eq(itemIndex));
			var breadCrumbs = this.getBreadCrumbs(breadCrumbsElementId);
			if (breadCrumbs) {
				Array.add(breadCrumbsList, breadCrumbs);
			}

			breadCrumbs = null;
		}

		$items = null;

		return breadCrumbsList;
	},

	refreshBreadCrumbsList: function (entityTypeCode, parentEntityId, entityId) {
		var breadCrumbsList = this.getBreadCrumbsListByItemCode(this.generateItemCode(entityTypeCode, parentEntityId, entityId));

		for (var breadCrumbsIndex = 0, breadCrumbsCount = breadCrumbsList.length; breadCrumbsIndex < breadCrumbsCount; breadCrumbsIndex++) {
			var breadCrumbs = breadCrumbsList[breadCrumbsIndex];
			if (breadCrumbs) {
				breadCrumbs.addItemsToBreadCrumbs();
			}

			breadCrumbs = null;
		}

		breadCrumbsList = null;
	},

	onActionExecuted: function (eventArgs) {
		var entityTypeCode = eventArgs.get_entityTypeCode();
		var parentEntityId = eventArgs.get_parentEntityId();
		var actionTypeCode = eventArgs.get_actionTypeCode();
		var entityId = eventArgs.get_entityId();
		var entities = eventArgs.get_entities();

		if (actionTypeCode == ACTION_TYPE_CODE_RESTORE || eventArgs.get_isUpdated()) {
			this.refreshBreadCrumbsList(entityTypeCode, parentEntityId, entityId);
		}
		else if (actionTypeCode == ACTION_TYPE_CODE_MULTIPLE_RESTORE) {
			for (var entityIndex = 0, entityCount = entities.length; entityIndex < entityCount; entityIndex++) {
				this.refreshBreadCrumbsList(entityTypeCode, parentEntityId, entities[entityIndex].Id);
			}
		}

		if (actionTypeCode == ACTION_TYPE_CODE_RESTORE && entityTypeCode == ENTITY_TYPE_CODE_ARTICLE_VERSION) {
			var newEntityTypeCode = ENTITY_TYPE_CODE_ARTICLE;
			var newParentEntityId = $q.toInt($o.getParentEntityId(newEntityTypeCode, parentEntityId), 0);
			this.refreshBreadCrumbsList(newEntityTypeCode, newParentEntityId, parentEntityId);
		}
	},

	dispose: function () {
		Quantumart.QP8.BackendBreadCrumbsManager.callBaseMethod(this, "dispose");

		if (this._breadCrumbs) {
			for (breadCrumbsElementId in this._breadCrumbs) {
				this.destroyBreadCrumbs(breadCrumbsElementId);
			}

			this._breadCrumbs = null;
		}

		Quantumart.QP8.BackendBreadCrumbsManager._instance = null;
	}
};

Quantumart.QP8.BackendBreadCrumbsManager._instance = null; // экземпляр класса

Quantumart.QP8.BackendBreadCrumbsManager.getInstance = function (breadCrumbsContainerElementId, options) {
	if (Quantumart.QP8.BackendBreadCrumbsManager._instance == null) {
		Quantumart.QP8.BackendBreadCrumbsManager._instance = new Quantumart.QP8.BackendBreadCrumbsManager(breadCrumbsContainerElementId, options);
	}

	return Quantumart.QP8.BackendBreadCrumbsManager._instance;
};

Quantumart.QP8.BackendBreadCrumbsManager.destroyInstance = function () {
	if (Quantumart.QP8.BackendBreadCrumbsManager._instance) {
		Quantumart.QP8.BackendBreadCrumbsManager._instance.dispose();
	}
};

Quantumart.QP8.BackendBreadCrumbsManager.registerClass("Quantumart.QP8.BackendBreadCrumbsManager", Quantumart.QP8.Observable);
//#endregion
