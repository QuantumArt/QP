import { $q } from '../Utils';

export class BackendEntityObject {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor() {
    // empty constructor
  }
}

// eslint-disable-next-line max-params
BackendEntityObject.getEntityByTypeAndIdForTree = function (
  entityTypeCode,
  entityId,
  loadChilds,
  filter,
  successHandler,
  errorHandler
) {
  const actionUrl = `${window.CONTROLLER_URL_ENTITY_OBJECT}GetByTypeAndIdForTree`;
  const params = { entityTypeCode, entityId, loadChilds, filter };
  if ($q.isFunction(successHandler)) {
    $q.getJsonFromUrl('GET', actionUrl, params, false, false, successHandler, errorHandler);
  } else {
    let entity = null;
    $q.getJsonFromUrl('GET', actionUrl, params, false, false, data => {
      entity = data;
    }, jqXHR => {
      entity = null;
      $q.processGenericAjaxError(jqXHR);
    });

    return entity;
  }

  return undefined;
};

BackendEntityObject.getEntityChildList = function (ajaxParams, successHandler, errorHandler) {
  const actionUrl = `${window.CONTROLLER_URL_ENTITY_OBJECT}GetChildList`;
  const params = ajaxParams;
  if (!$q.isNull(params.selectItemIDs)) {
    params.selectItemIDs = params.selectItemIDs.join(',');
  }

  if ($q.isFunction(successHandler)) {
    $q.getJsonFromUrl('POST', actionUrl, params, false, false, successHandler, errorHandler);
  } else {
    let entities = null;

    $q.getJsonFromUrl('POST', actionUrl, params, false, false, data => {
      entities = data;
    }, jqXHR => {
      entities = null;
      $q.processGenericAjaxError(jqXHR);
    });

    return entities;
  }

  return undefined;
};

// eslint-disable-next-line max-params
BackendEntityObject.getSimpleEntityList = function (
  entityTypeCode,
  parentEntityId,
  entityId,
  listId,
  selectionMode,
  selectedEntitiesIDs,
  filter,
  testEntityId
) {
  const actionUrl = `${window.CONTROLLER_URL_ENTITY_OBJECT}GetSimpleList`;
  const params = {
    entityTypeCode,
    parentEntityId,
    entityId,
    listId,
    selectionMode,
    selectedEntitiesIDs,
    filter,
    testEntityId
  };

  let entities = [];
  $q.getJsonFromUrl('POST', actionUrl, params, false, false, data => {
    if (data && data.ErrorMessage) {
      $q.alertError(data.ErrorMessage);
    } else {
      entities = data;
    }
  }, jqXHR => {
    entities = [];
    $q.processGenericAjaxError(jqXHR);
  });

  return entities;
};

BackendEntityObject.makeSimpleCall = function (verb, method, params) {
  let result = null;
  $q.getJsonFromUrl(verb, method, params, false, false, data => {
    result = data;
  }, jqXHR => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

BackendEntityObject.checkEntityExistence = function (entityTypeCode, entityId) {
  let result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}CheckExistence`, {
    entityTypeCode,
    entityId
  }, false, false, data => {
    result = data;
  }, jqXHR => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

BackendEntityObject.checkEntityForPresenceSelfRelations = function (entityTypeCode, entityId) {
  let result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}CheckPresenceSelfRelations`, {
    entityTypeCode,
    entityId
  }, false, false, data => {
    result = data;
  }, jqXHR => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

BackendEntityObject.checkEntityForVariations = function (entityTypeCode, entityId) {
  let result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}CheckForVariations`, {
    entityTypeCode,
    entityId
  }, false, false, data => {
    result = data;
  }, jqXHR => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

BackendEntityObject.getEntityName = function (entityTypeCode, entityId, parentEntityId) {
  let result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetName`, {
    entityTypeCode,
    entityId,
    parentEntityId
  }, false, false, data => {
    result = data;
  }, jqXHR => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

BackendEntityObject.getParentEntityId = function (entityTypeCode, entityId) {
  let result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetParentId`, {
    entityTypeCode,
    entityId: +entityId || 0
  }, false, false, data => {
    result = data;
  }, jqXHR => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

BackendEntityObject.getParentIdsForTree = function (entityTypeCode, ids) {
  let result = null;
  $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetParentIdsForTree`, {
    entityTypeCode,
    ids
  }, false, false, data => {
    result = data;
  }, jqXHR => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

BackendEntityObject.getParentInfo = function (entityTypeCode, entityId, parentEntityId) {
  let result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetParentInfo`, {
    entityTypeCode,
    entityId,
    parentEntityId
  }, false, false, data => {
    result = data;
  }, jqXHR => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

BackendEntityObject.getArticleFieldValue = function (contentId, fieldName, articleId) {
  return BackendEntityObject.makeSimpleCall(
    'GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetArticleFieldValue`, {
      contentId,
      fieldName,
      articleId
    });
};

BackendEntityObject.getContentFieldValues = function (contentId, fieldName) {
  return BackendEntityObject.makeSimpleCall(
    'GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetContentFieldValues`, {
      contentId,
      fieldName
    });
};

BackendEntityObject.getArticleLinkedItems = function (linkId, articleId) {
  return BackendEntityObject.makeSimpleCall(
    'GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetArticleLinkedItems`, {
      linkId,
      articleId
    });
};

BackendEntityObject.getArticleIdByFieldValue = function (contentId, fieldName, fieldValue) {
  return BackendEntityObject.makeSimpleCall(
    'GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetArticleIdByFieldValue`, {
      contentId,
      fieldName,
      fieldValue
    });
};

BackendEntityObject.getParentsChain = function (entityTypeCode, entityId, parentEntityId) {
  let result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetParentsChain`, {
    entityTypeCode,
    entityId,
    parentEntityId
  }, false, false, data => {
    result = data;
  }, jqXHR => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

BackendEntityObject.checkEntitiesForPresenceEmptyNames = function (entities) {
  let isEmpty = false;
  $.each(entities, (index, entity) => {
    if ($q.isNullOrWhiteSpace(entity.Name)) {
      isEmpty = true;
      return false;
    }
    return true;
  });

  return isEmpty;
};

BackendEntityObject.getEntityIDsFromEntities = function (entities) {
  return entities.map(elem => elem.Id);
};

BackendEntityObject.getEntityNamesFromEntities = function (entities) {
  return entities.map(elem => elem.Name);
};

BackendEntityObject.getEntityNamesStringFromEntities = function (entities) {
  const count = entities.length;
  return entities.map((elem, index) => {
    let prefix = '';
    if (index > 0) {
      if (index === (count - 1)) {
        prefix = ` ${$l.Common.andUnion.toLowerCase()} `;
      } else {
        prefix = ', ';
      }
    }

    return `${prefix}"${elem.Name}"`;
  }).join('');
};

BackendEntityObject.isTreeViewTypeAllowed = function (entityTypeCode, parentEntityId, action) {
  let result = false;
  if (action && !$q.isNullOrEmpty(action.Views)) {
    const views = action.Views;
    let treeViewTypeExist = false;
    for (let viewIndex = 0; viewIndex < views.length; viewIndex++) {
      const view = views[viewIndex];
      if (view.ViewType.Code === window.VIEW_TYPE_CODE_TREE) {
        treeViewTypeExist = true;
        break;
      }
    }

    if (treeViewTypeExist) {
      result = BackendEntityObject.checkEntityForPresenceSelfRelations(
        entityTypeCode, parentEntityId);
    }
  }

  return result;
};

BackendEntityObject.getContextQuery = function (contentId, currentContext) {
  let result = null;
  $q.getJsonFromUrl('GET', 'Article/GetContextQuery', {
    id: contentId,
    currentContext
  }, false, false, data => {
    result = data;
  }, jqXHR => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};


// eslint-disable-next-line no-shadow
export const $o = BackendEntityObject;

window.$o = $o;

Quantumart.QP8.BackendEntityObject = BackendEntityObject;
