Quantumart.QP8.BackendEntityObject = function () {};
Quantumart.QP8.BackendEntityObject.getEntityByTypeAndIdForTree = function (entityTypeCode, entityId, loadChilds, filter, successHandler, errorHandler) {
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
};

Quantumart.QP8.BackendEntityObject.getEntityChildList = function (ajaxParams, successHandler, errorHandler) {
  const actionUrl = `${window.CONTROLLER_URL_ENTITY_OBJECT}GetChildList`;
  if (!$q.isNullOrEmpty(ajaxParams.selectItemIDs)) {
    ajaxParams.selectItemIDs = ajaxParams.selectItemIDs.join(',');
  }

  if ($q.isFunction(successHandler)) {
    $q.getJsonFromUrl('POST', actionUrl, ajaxParams, false, false, successHandler, errorHandler);
  } else {
    let entities = null;

    $q.getJsonFromUrl('POST', actionUrl, ajaxParams, false, false, data => {
      entities = data;
    }, jqXHR => {
      entities = null;
      $q.processGenericAjaxError(jqXHR);
    });

    return entities;
  }
};

Quantumart.QP8.BackendEntityObject.getSimpleEntityList = function (entityTypeCode, parentEntityId, entityId, listId, selectionMode, selectedEntitiesIDs, filter, testEntityId) {
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

Quantumart.QP8.BackendEntityObject.makeSimpleCall = function (verb, method, params) {
  let result = null;
  $q.getJsonFromUrl(verb, method, params, false, false, data => {
    result = data;
  }, jqXHR => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

Quantumart.QP8.BackendEntityObject.checkEntityExistence = function (entityTypeCode, entityId) {
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

Quantumart.QP8.BackendEntityObject.checkEntityForPresenceSelfRelations = function (entityTypeCode, entityId) {
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

Quantumart.QP8.BackendEntityObject.checkEntityForVariations = function (entityTypeCode, entityId) {
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

Quantumart.QP8.BackendEntityObject.getEntityName = function (entityTypeCode, entityId, parentEntityId) {
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

Quantumart.QP8.BackendEntityObject.getParentEntityId = function (entityTypeCode, entityId) {
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

Quantumart.QP8.BackendEntityObject.getParentIdsForTree = function (entityTypeCode, ids) {
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

Quantumart.QP8.BackendEntityObject.getParentInfo = function (entityTypeCode, entityId, parentEntityId) {
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

Quantumart.QP8.BackendEntityObject.getArticleFieldValue = function (contentId, fieldName, articleId) {
  return $o.makeSimpleCall('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetArticleFieldValue`, {
    contentId,
    fieldName,
    articleId
  });
};

Quantumart.QP8.BackendEntityObject.getArticleLinkedItems = function (linkId, articleId) {
  return $o.makeSimpleCall('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetArticleLinkedItems`, {
    linkId, articleId
  });
};

Quantumart.QP8.BackendEntityObject.getArticleIdByFieldValue = function (contentId, fieldName, fieldValue) {
  return $o.makeSimpleCall('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetArticleIdByFieldValue`, {
    contentId,
    fieldName,
    fieldValue
  });
};

Quantumart.QP8.BackendEntityObject.getParentsChain = function (entityTypeCode, entityId, parentEntityId) {
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

Quantumart.QP8.BackendEntityObject.checkEntitiesForPresenceEmptyNames = function (entities) {
  let isEmpty = false;
  $.each(entities, (index, entity) => {
    if ($q.isNullOrWhiteSpace(entity.Name)) {
      isEmpty = true;
      return false;
    }
  });

  return isEmpty;
};

Quantumart.QP8.BackendEntityObject.getEntityIDsFromEntities = function (entities) {
  return $.map(entities, elem => elem.Id);
};

Quantumart.QP8.BackendEntityObject.getEntityNamesFromEntities = function (entities) {
  return $.map(entities, elem => elem.Name);
};

Quantumart.QP8.BackendEntityObject.getEntityNamesStringFromEntities = function (entities) {
  const count = entities.length;
  return $.map(entities, (elem, index) => {
    let prefix = '';
    if (index > 0) {
      if (index == (count - 1)) {
        prefix = ` ${$l.Common.andUnion.toLowerCase()} `;
      } else {
        prefix = ', ';
      }
    }

    return `${prefix}"${elem.Name}"`;
  }).join('');
};

Quantumart.QP8.BackendEntityObject.isTreeViewTypeAllowed = function (entityTypeCode, parentEntityId, action) {
  let result = false;
  if (action && !$q.isNullOrEmpty(action.Views)) {
    const views = action.Views;
    let treeViewTypeExist = false;
    for (let viewIndex = 0; viewIndex < views.length; viewIndex++) {
      const view = views[viewIndex];
      if (view.ViewType.Code == window.VIEW_TYPE_CODE_TREE) {
        treeViewTypeExist = true;
        break;
      }
    }

    if (treeViewTypeExist) {
      result = Quantumart.QP8.BackendEntityObject.checkEntityForPresenceSelfRelations(
        entityTypeCode, parentEntityId);
    }
  }

  return result;
};

Quantumart.QP8.BackendEntityObject.getContextQuery = function (contentId, currentContext) {
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

Quantumart.QP8.BackendEntityObject.registerClass('Quantumart.QP8.BackendEntityObject');
window.$o = Quantumart.QP8.BackendEntityObject;
