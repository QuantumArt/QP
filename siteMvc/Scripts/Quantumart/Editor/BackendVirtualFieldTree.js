import { BackendEntityTree } from '../Tree/BackendEntityTree';
import { $q } from '../Utils';

export class BackendVirtualFieldTree extends BackendEntityTree {
  static _getIcon(entity) {
    return entity.IconUrl;
  }

  // eslint-disable-next-line max-params
  constructor(
    treeGroupCode,
    treeElementId,
    entityTypeCode,
    parentEntityId,
    actionCode,
    options,
    hostOptions
  ) {
    super(
      treeGroupCode,
      treeElementId,
      entityTypeCode,
      parentEntityId,
      actionCode,
      options,
      hostOptions
    );

    this._virtualContentId = options ? options.virtualContentId : null;
  }

  convertNodeCodeToEntityId(nodeCode) {
    return nodeCode === this.ROOT_NODE_CODE ? null : nodeCode;
  }

  _getEntityChildList(entityId, returnSelf, successHandler, errorHandler) {
    if (this._parentEntityId) {
      let selectItemIDsParam, entityIdParam, alias;

      if (!$q.isNullOrEmpty(this._selectedEntitiesIDs)) {
        selectItemIDsParam = this._selectedEntitiesIDs.join(';');
      }
      if (!$q.isNullOrWhiteSpace(entityId)) {
        entityIdParam = entityId;
        alias = this.getNodeText(this.getNodeByEntityId(entityId));
      }

      const params = {
        virtualContentId: this._virtualContentId,
        joinedContentId: this._parentEntityId,
        entityId: entityIdParam,
        selectItemIDs: selectItemIDsParam,
        parentAlias: alias
      };

      $q.getJsonFromUrl(
        'POST',
        `${window.CONTROLLER_URL_VIRTUAL_CONTENT}GetChildFieldList`,
        params,
        true,
        false,
        successHandler,
        errorHandler
      );
    }
  }
}

Quantumart.QP8.BackendVirtualFieldTree = BackendVirtualFieldTree;
