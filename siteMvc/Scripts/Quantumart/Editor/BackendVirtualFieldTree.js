//#region class BackendEntityTree
// === Класс "Дерево виртуальных полей виртуального контента" ===
Quantumart.QP8.BackendVirtualFieldTree = function (treeGroupCode, treeElementId, entityTypeCode, parentEntityId, actionCode, options) {
	Quantumart.QP8.BackendVirtualFieldTree.initializeBase(this, [treeGroupCode, treeElementId, entityTypeCode, parentEntityId, actionCode, options]);

    if ($q.isObject(options)) {
		  if (!$q.isNullOrEmpty(options.virtualContentId)) {
        this._virtualContentId = options.virtualContentId;
		  }
    }
};

Quantumart.QP8.BackendVirtualFieldTree.prototype = {
    convertNodeCodeToEntityId: function (nodeCode) {
		if (nodeCode != this.ROOT_NODE_CODE) {
			return nodeCode;
		}
		else {
			return null;
		}
	},

    _getEntityChildList: function (entityId, returnSelf, successHandler, errorHandler){
        if (this._parentEntityId)
        {
            var selectItemIDsParam,
                entityIdParam,
                alias;

            if (!$q.isNullOrEmpty(this._selectedEntitiesIDs))
                selectItemIDsParam = this._selectedEntitiesIDs.join(';');
            if (!$q.isNullOrWhiteSpace(entityId)) {
                entityIdParam = entityId;
                alias = this.getNodeText(this.getNodeByEntityId(entityId));
            }

            var actionUrl = CONTROLLER_URL_VIRTUAL_CONTENT + "GetChildFieldList",
                params = {
                    "virtualContentId": this._virtualContentId,
                    "joinedContentId": this._parentEntityId,
                    "entityId": entityIdParam,
                    "selectItemIDs": selectItemIDsParam,
                    "parentAlias": alias
                };

            $q.getJsonFromUrl(
                "POST",
                actionUrl,
                params,
                true,
                false,
                successHandler,
                errorHandler
            );
        }
	},

    _getIcon: function (entity){
	    var icon = entity.IconUrl;
	    return icon;
	},

    _virtualContentId: null
};

Quantumart.QP8.BackendVirtualFieldTree.registerClass("Quantumart.QP8.BackendVirtualFieldTree", Quantumart.QP8.BackendEntityTree);
//#endregion
