// #region class BackendEntityTreeManager
// === Класс "Менеджер деревьев сущностей" ===
Quantumart.QP8.BackendEntityTreeManager = function () {
	Quantumart.QP8.BackendEntityTreeManager.initializeBase(this);
};

Quantumart.QP8.BackendEntityTreeManager.prototype = {
	_treeGroups: {}, // список групп деревьев

	initialize: function () {
	},

	generateTreeGroupCode: function (entityTypeCode, parentEntityId) {
		var treeGroupCode = String.format("{0}_{1}", entityTypeCode, parentEntityId);

		return treeGroupCode;
	},

	getTreeGroup: function (treeGroupCode) {
		var treeGroup = null;
		if (this._treeGroups[treeGroupCode]) {
			treeGroup = this._treeGroups[treeGroupCode];
		}

		return treeGroup;
	},

	createTreeGroup: function (treeGroupCode) {
		var treeGroup = this.getTreeGroup(treeGroupCode);
		if (!treeGroup) {
			treeGroup = {};
			this._treeGroups[treeGroupCode] = treeGroup;
		}

		return treeGroup;
	},

	removeTreeGroup: function (treeGroupCode) {
		$q.removeProperty(this._treeGroups, treeGroupCode);
	},

	getTree: function (treeElementId) {
		var tree = null;

		for (var treeGroupCode in this._treeGroups) {
			var treeGroup = this._treeGroups[treeGroupCode];
			if (treeGroup[treeElementId]) {
				tree = treeGroup[treeElementId];
				break;
			}
		}

		return tree;
	},

	createTree: function (treeElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions) {
		var treeGroupCode = this.generateTreeGroupCode(entityTypeCode, parentEntityId);

		var tree = null;
		if ($q.isNullOrEmpty(options.virtualContentId)) {
			tree = new Quantumart.QP8.BackendEntityTree(treeGroupCode, treeElementId, entityTypeCode, parentEntityId, actionCode, options, hostOptions);
    } else {
			tree = new Quantumart.QP8.BackendVirtualFieldTree(treeGroupCode, treeElementId, entityTypeCode, parentEntityId, actionCode, options);
    }

		tree.set_treeManager(this);

		var treeGroup = this.createTreeGroup(treeGroupCode);
		treeGroup[treeElementId] = tree;
		return tree;
	},

	removeTree: function (treeElementId) {
		var tree = this.getTree(treeElementId);
		if (tree) {
			var treeGroupCode = tree.get_treeGroupCode();
			var treeGroup = this.getTreeGroup(treeGroupCode);

			$q.removeProperty(treeGroup, treeElementId);

			if ($q.getHashKeysCount(treeGroup) == 0) {
				this.removeTreeGroup(treeGroupCode);
			}
		}
	},

	destroyTree: function (treeElementId) {
		var tree = this.getTree(treeElementId);
		if (tree != null) {
			this.removeTree(treeElementId);
			if (tree.dispose) {
				tree.dispose();
			}
			tree = null;
		}
	},

	refreshNode: function (entityTypeCode, parentEntityId, entityId, options) {
		var treeGroup = this.getTreeGroup(this.generateTreeGroupCode(entityTypeCode, parentEntityId));
		if (treeGroup) {
			for (var treeElementId in treeGroup) {
				var tree = this.getTree(treeElementId);
				var nodeCode = tree.convertEntityIdToNodeCode(entityId);

				tree.refreshNode(nodeCode, options);
			}
		}
	},

	refreshNodes: function (entityTypeCode, parentEntityId, ids, options) {
		var self = this;
		if ($q.isNullOrEmpty(ids)) {
			var treeGroup = this.getTreeGroup(this.generateTreeGroupCode(entityTypeCode, parentEntityId));
			if (treeGroup) {
				for (var treeElementId in treeGroup) {
					var tree = this.getTree(treeElementId);
					tree.refreshTree();
				}
			}
		} else {
			jQuery.each(ids, function (index, id) {
				self.refreshNode(entityTypeCode, parentEntityId, id, options);
			});
		}
	},

	removeNode: function (entityTypeCode, parentEntityId, entityId) {
		var treeGroup = this.getTreeGroup(this.generateTreeGroupCode(entityTypeCode, parentEntityId));
		if (treeGroup) {
			for (var treeElementId in treeGroup) {
				var tree = this.getTree(treeElementId);
				var nodeCode = tree.convertEntityIdToNodeCode(entityId);

				tree.removeNode(nodeCode);
			}
		}
	},

	removeNodes: function (entityTypeCode, parentEntityId, ids) {
		var self = this;
		jQuery.each(ids, function (index, id) {
			self.removeNode(entityTypeCode, parentEntityId, id);
		});
	},

	onActionExecuted: function (eventArgs) {
		var entityTypeCode = eventArgs.get_entityTypeCode();
		var parentEntityId = eventArgs.get_parentEntityId();
		var actionTypeCode = eventArgs.get_actionTypeCode();
		var entityId = eventArgs.get_entityId();
		var entityIds = eventArgs.get_isMultipleEntities() ? $o.getEntityIDsFromEntities(eventArgs.get_entities()) : [entityId];

		if (eventArgs.get_isRemoving() || eventArgs.get_isArchiving()) {
			this.removeNodes(entityTypeCode, parentEntityId, entityIds);
		} else if ((eventArgs.get_isUpdated()
      || eventArgs.get_isLoaded()
      || actionTypeCode == ACTION_TYPE_CODE_CANCEL
      || actionTypeCode == ACTION_TYPE_CODE_CHANGE_LOCK)
      && entityTypeCode != ENTITY_TYPE_CODE_VIRTUAL_ARTICLE) {
			this.refreshNode(entityTypeCode, parentEntityId, entityId, { loadChildNodes: true, saveNodesSelection: true });
		} else if (eventArgs.get_isSaved() || actionTypeCode == ACTION_TYPE_CODE_COPY) {
			var parentIdsInTree = $o.getParentIdsForTree(entityTypeCode, entityIds);
			this.refreshNodes(entityTypeCode, parentEntityId, parentIdsInTree, { loadChildNodes: true, saveNodesSelection: false });
		} else if (eventArgs.get_isRestoring() && entityTypeCode == ENTITY_TYPE_CODE_ARCHIVE_ARTICLE) {
			var parentArticlesInTree = $o.getParentIdsForTree(ENTITY_TYPE_CODE_ARTICLE, entityIds);
			this.refreshNodes(ENTITY_TYPE_CODE_ARTICLE, parentEntityId, parentArticlesInTree, { loadChildNodes: true, saveNodesSelection: false });
		} else if (eventArgs.get_isRestored() && entityTypeCode == ENTITY_TYPE_CODE_ARTICLE_VERSION) {
			var newParentEntityId = +$o.getParentEntityId(ENTITY_TYPE_CODE_ARTICLE, entityId) || 0;
			this.refreshNode(ENTITY_TYPE_CODE_ARTICLE, newParentEntityId, entityId, { loadChildNodes: true, saveNodesSelection: false });
		}
	},

	dispose: function () {
		Quantumart.QP8.BackendEntityTreeManager.callBaseMethod(this, "dispose");

		if (this._trees) {
			for (treeCode in this._trees) {
				this.destroyTree(treeCode);
			}

			this._trees = null;
		}

		Quantumart.QP8.BackendEntityTreeManager._instance = null;

		$q.collectGarbageInIE();
	}
};

Quantumart.QP8.BackendEntityTreeManager._instance = null; // экземпляр класса

// Возвращает экземпляр класса "Менеджер деревьев сущностей"
Quantumart.QP8.BackendEntityTreeManager.getInstance = function () {
	if (Quantumart.QP8.BackendEntityTreeManager._instance == null) {
		Quantumart.QP8.BackendEntityTreeManager._instance = new Quantumart.QP8.BackendEntityTreeManager();
	}

	return Quantumart.QP8.BackendEntityTreeManager._instance;
};

// Уничтожает экземпляр класса "Менеджер деревьев сущностей"
Quantumart.QP8.BackendEntityTreeManager.destroyInstance = function () {
	if (Quantumart.QP8.BackendEntityTreeManager._instance) {
		Quantumart.QP8.BackendEntityTreeManager._instance.dispose();
	}
};

Quantumart.QP8.BackendEntityTreeManager.registerClass("Quantumart.QP8.BackendEntityTreeManager", Quantumart.QP8.Observable);

// #endregion
