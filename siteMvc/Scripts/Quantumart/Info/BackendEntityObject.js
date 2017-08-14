Quantumart.QP8.BackendEntityObject = function () {};
Quantumart.QP8.BackendEntityObject.getEntityByTypeAndIdForTree = function (entityTypeCode, entityId, loadChilds, filter, successHandler, errorHandler) {
  var actionUrl = `${window.CONTROLLER_URL_ENTITY_OBJECT}GetByTypeAndIdForTree`;
  var params = { entityTypeCode: entityTypeCode, entityId: entityId, loadChilds: loadChilds, filter: filter };
  if ($q.isFunction(successHandler)) {
    $q.getJsonFromUrl('GET', actionUrl, params, false, false, successHandler, errorHandler);
  } else {
    var entity = null;
    $q.getJsonFromUrl('GET', actionUrl, params, false, false, (data) => {
      entity = data;
    }, (jqXHR) => {
      entity = null;
      $q.processGenericAjaxError(jqXHR);
    });

    return entity;
  }
};

Quantumart.QP8.BackendEntityObject.getEntityChildList = function (ajaxParams, successHandler, errorHandler) {
  var actionUrl = `${window.CONTROLLER_URL_ENTITY_OBJECT}GetChildList`;
  if (!$q.isNullOrEmpty(ajaxParams.selectItemIDs)) {
    ajaxParams.selectItemIDs = ajaxParams.selectItemIDs.join(',');
  }

  if ($q.isFunction(successHandler)) {
    $q.getJsonFromUrl('POST', actionUrl, ajaxParams, false, false, successHandler, errorHandler);
  } else {
    var entities = null;

    $q.getJsonFromUrl('POST', actionUrl, ajaxParams, false, false, (data) => {
      entities = data;
    }, (jqXHR) => {
      entities = null;
      $q.processGenericAjaxError(jqXHR);
    });

    return entities;
  }
};

Quantumart.QP8.BackendEntityObject.getSimpleEntityList = function (entityTypeCode, parentEntityId, entityId, listId, selectionMode, selectedEntitiesIDs, filter, testEntityId) {
  var actionUrl = `${window.CONTROLLER_URL_ENTITY_OBJECT}GetSimpleList`;
  var params = {
    entityTypeCode: entityTypeCode,
    parentEntityId: parentEntityId,
    entityId: entityId,
    listId: listId,
    selectionMode: selectionMode,
    selectedEntitiesIDs: selectedEntitiesIDs,
    filter: filter,
    testEntityId: testEntityId
  };

  var entities = [];
  $q.getJsonFromUrl('POST', actionUrl, params, false, false, (data) => {
    if (data && data.ErrorMessage) {
      $q.alertError(data.ErrorMessage);
    } else {
      entities = data;
    }
  }, (jqXHR) => {
    entities = [];
    $q.processGenericAjaxError(jqXHR);
  });

  return entities;
};

Quantumart.QP8.BackendEntityObject.makeSimpleCall = function (verb, method, params) {
  var result = null;
  $q.getJsonFromUrl(verb, method, params, false, false, (data) => {
    result = data;
  }, (jqXHR) => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

Quantumart.QP8.BackendEntityObject.checkEntityExistence = function (entityTypeCode, entityId) {
  var result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}CheckExistence`, {
    entityTypeCode: entityTypeCode,
    entityId: entityId
  }, false, false, (data) => {
    result = data;
  }, (jqXHR) => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

Quantumart.QP8.BackendEntityObject.checkEntityForPresenceSelfRelations = function (entityTypeCode, entityId) {
  var result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}CheckPresenceSelfRelations`, {
    entityTypeCode: entityTypeCode,
    entityId: entityId
  }, false, false, (data) => {
    result = data;
  }, (jqXHR) => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

Quantumart.QP8.BackendEntityObject.checkEntityForVariations = function (entityTypeCode, entityId) {
  var result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}CheckForVariations`, {
    entityTypeCode: entityTypeCode,
    entityId: entityId
  }, false, false, (data) => {
    result = data;
  }, (jqXHR) => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

Quantumart.QP8.BackendEntityObject.getEntityName = function (entityTypeCode, entityId, parentEntityId) {
  var result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetName`, {
    entityTypeCode: entityTypeCode,
    entityId: entityId,
    parentEntityId: parentEntityId
  }, false, false, (data) => {
    result = data;
  }, (jqXHR) => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

Quantumart.QP8.BackendEntityObject.getParentEntityId = function (entityTypeCode, entityId) {
  var result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetParentId`, {
    entityTypeCode: entityTypeCode,
    entityId: +entityId || 0
  }, false, false, (data) => {
    result = data;
  }, (jqXHR) => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

Quantumart.QP8.BackendEntityObject.getParentIdsForTree = function (entityTypeCode, ids) {
  var result = null;
  $q.getJsonFromUrl('POST', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetParentIdsForTree`, {
    entityTypeCode: entityTypeCode,
    ids: ids
  }, false, false, (data) => {
    result = data;
  }, (jqXHR) => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

Quantumart.QP8.BackendEntityObject.getParentInfo = function (entityTypeCode, entityId, parentEntityId) {
  var result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetParentInfo`, {
    entityTypeCode: entityTypeCode,
    entityId: entityId,
    parentEntityId: parentEntityId
  }, false, false, (data) => {
    result = data;
  }, (jqXHR) => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

Quantumart.QP8.BackendEntityObject.getArticleFieldValue = function (contentId, fieldName, articleId) {
  return $o.makeSimpleCall('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetArticleFieldValue`, {
    contentId: contentId,
    fieldName: fieldName,
    articleId: articleId
  });
};

Quantumart.QP8.BackendEntityObject.getArticleLinkedItems = function (linkId, articleId) {
  return $o.makeSimpleCall('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetArticleLinkedItems`, {
    linkId: linkId, articleId: articleId
  });
};

Quantumart.QP8.BackendEntityObject.getArticleIdByFieldValue = function (contentId, fieldName, fieldValue) {
  return $o.makeSimpleCall('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetArticleIdByFieldValue`, {
    contentId: contentId,
    fieldName: fieldName,
    fieldValue: fieldValue
  });
};

Quantumart.QP8.BackendEntityObject.getParentsChain = function (entityTypeCode, entityId, parentEntityId) {
  var result = null;
  $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ENTITY_OBJECT}GetParentsChain`, {
    entityTypeCode: entityTypeCode,
    entityId: entityId,
    parentEntityId: parentEntityId
  }, false, false, (data) => {
    result = data;
  }, (jqXHR) => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

Quantumart.QP8.BackendEntityObject.checkEntitiesForPresenceEmptyNames = function (entities) {
  var isEmpty = false;
  $.each(entities, (index, entity) => {
    if ($q.isNullOrWhiteSpace(entity.Name)) {
      isEmpty = true;
      return false;
    }
  });

  return isEmpty;
};

Quantumart.QP8.BackendEntityObject.getEntityIDsFromEntities = function (entities) {
  return $.map(entities, (elem) => {
    return elem.Id;
  });
};

Quantumart.QP8.BackendEntityObject.getEntityNamesFromEntities = function (entities) {
  return $.map(entities, (elem) => {
    return elem.Name;
  });
};

Quantumart.QP8.BackendEntityObject.getEntityNamesStringFromEntities = function (entities) {
  var count = entities.length;
  return $.map(entities, (elem, index) => {
    var prefix = '';
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
  var result = false;
  if (action && !$q.isNullOrEmpty(action.Views)) {
    var views = action.Views;
    var treeViewTypeExist = false;
    for (var viewIndex = 0; viewIndex < views.length; viewIndex++) {
      var view = views[viewIndex];
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
  var result = null;
  $q.getJsonFromUrl('GET', 'Article/GetContextQuery', {
    id: contentId,
    currentContext: currentContext
  }, false, false, (data) => {
    result = data;
  }, (jqXHR) => {
    result = null;
    $q.processGenericAjaxError(jqXHR);
  });

  return result;
};

Quantumart.QP8.BackendEntityObject.registerClass('Quantumart.QP8.BackendEntityObject');
window.$o = Quantumart.QP8.BackendEntityObject;
