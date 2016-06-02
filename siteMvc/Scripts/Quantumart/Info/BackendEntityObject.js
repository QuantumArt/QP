//#region class BackendEntityObject
Quantumart.QP8.BackendEntityObject = function() {};

// Возвращает сущность
Quantumart.QP8.BackendEntityObject.getEntityByTypeAndIdForTree = function(entityTypeCode, entityId, loadChilds, filter, successHandler, errorHandler) {
  var actionUrl = window.CONTROLLER_URL_ENTITY_OBJECT + 'GetByTypeAndIdForTree';
  var params = { entityTypeCode: entityTypeCode, entityId: entityId, loadChilds: loadChilds, filter: filter };

  if ($q.isFunction(successHandler)) {
    $q.getJsonFromUrl(
    'GET',
    actionUrl,
    params,
    false,
    false,
        successHandler,
        errorHandler
    );
  } else {
    var entity = null;

    $q.getJsonFromUrl(
    'GET',
    actionUrl,
    params,
    false,
    false,
      function(data) {
  entity = data;
      },
      function(jqXHR) {
  entity = null;
  $q.processGenericAjaxError(jqXHR);
      }
    );

    return entity;
  }
};

// Возвращает список дочерних сущностей
Quantumart.QP8.BackendEntityObject.getEntityChildList = function(ajaxParams, successHandler, errorHandler) {
  var actionUrl = window.CONTROLLER_URL_ENTITY_OBJECT + 'GetChildList';
  if (!$q.isNullOrEmpty(ajaxParams.selectItemIDs)) {
    ajaxParams.selectItemIDs = ajaxParams.selectItemIDs.join(',');
  }

  if ($q.isFunction(successHandler)) {
    $q.getJsonFromUrl('POST', actionUrl, ajaxParams, false, false, successHandler, errorHandler);
  } else {
    var entities = null;

    $q.getJsonFromUrl('POST', actionUrl, ajaxParams, false, false, function(data) {
      entities = data;
    }, function(jqXHR) {
      entities = null;
      $q.processGenericAjaxError(jqXHR);
    });

    return entities;
  }
};

// Возвращает упрощенный список сущностей
Quantumart.QP8.BackendEntityObject.getSimpleEntityList = function(entityTypeCode, parentEntityId, entityId,
  listId, selectionMode, selectedEntitiesIDs, filter, testEntityId) {
  var actionUrl = window.CONTROLLER_URL_ENTITY_OBJECT + 'GetSimpleList';
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

  $q.getJsonFromUrl(
  'POST',
  actionUrl,
  params,
  false,
  false,
    function(data) {
  if (data && data.ErrorMessage)
  alert(data.ErrorMessage);
  else
  entities = data;
    },
    function(jqXHR) {
  entities = [];
  $q.processGenericAjaxError(jqXHR);
    }
  );

  return entities;
};

Quantumart.QP8.BackendEntityObject.makeSimpleCall = function(verb, method, params) {
  var result = null;

  $q.getJsonFromUrl(
  verb,
  method,
  params,
          false,
  false,
    function(data) {
  result = data;
    },
    function(jqXHR) {
  result = null;
  $q.processGenericAjaxError(jqXHR);
    }
  );

  return result;
};

// Проверяет существование сущности
Quantumart.QP8.BackendEntityObject.checkEntityExistence = function Quantumart$QP8$BackendEntityObject$checkEntityExistence(entityTypeCode, entityId) {
  var result = null;

  $q.getJsonFromUrl(
  'GET',
  window.CONTROLLER_URL_ENTITY_OBJECT + 'CheckExistence',
  { entityTypeCode: entityTypeCode, entityId: entityId },
          false,
  false,
    function(data) {
  result = data;
    },
    function(jqXHR) {
  result = null;
  $q.processGenericAjaxError(jqXHR);
    }
  );

  return result;
};

// Проверяет сущность на наличие рекурсивных связей
Quantumart.QP8.BackendEntityObject.checkEntityForPresenceSelfRelations = function Quantumart$QP8$BackendEntityObject$checkEntityForPresenceSelfRelations(entityTypeCode, entityId) {
  var result = null;

  $q.getJsonFromUrl(
  'GET',
  window.CONTROLLER_URL_ENTITY_OBJECT + 'CheckPresenceSelfRelations',
  { entityTypeCode: entityTypeCode, entityId: entityId },
          false,
  false,
    function(data) {
  result = data;
    },
    function(jqXHR) {
  result = null;
  $q.processGenericAjaxError(jqXHR);
    }
  );

  return result;
};

// Проверяет сущность на наличие вариаций
Quantumart.QP8.BackendEntityObject.checkEntityForVariations = function Quantumart$QP8$BackendEntityObject$checkEntityForVariations(entityTypeCode, entityId) {
  var result = null;

  $q.getJsonFromUrl(
    'GET',
    window.CONTROLLER_URL_ENTITY_OBJECT + 'CheckForVariations',
    { entityTypeCode: entityTypeCode, entityId: entityId },
      false,
    false,
    function(data) {
  result = data;
    },
    function(jqXHR) {
  result = null;
  $q.processGenericAjaxError(jqXHR);
    }
  );

  return result;
};

// Возвращает название сущности
Quantumart.QP8.BackendEntityObject.getEntityName = function Quantumart$QP8$BackendEntityObject$getEntityName(entityTypeCode, entityId, parentEntityId) {
  var result = null;

  $q.getJsonFromUrl(
  'GET',
  window.CONTROLLER_URL_ENTITY_OBJECT + 'GetName',
  { entityTypeCode: entityTypeCode, entityId: entityId, parentEntityId: parentEntityId },
          false,
  false,
    function(data) {
  result = data;
    },
    function(jqXHR) {
  result = null;
  $q.processGenericAjaxError(jqXHR);
    }
  );

  return result;
};

// Возвращает идентификатор родительской сущности
Quantumart.QP8.BackendEntityObject.getParentEntityId = function Quantumart$QP8$BackendEntityObject$getParentEntityId(entityTypeCode, entityId) {
  var result = null;

  $q.getJsonFromUrl(
  'GET',
  window.CONTROLLER_URL_ENTITY_OBJECT + 'GetParentId',
  { entityTypeCode: entityTypeCode, entityId: $q.toInt(entityId, 0) },
          false,
  false,
    function(data) {
  result = data;
    },
    function(jqXHR) {
  result = null;
  $q.processGenericAjaxError(jqXHR);
    }
  );

  return result;
};

// Возвращает идентификаторы родительских сущностей того же типа (для обновления дерева)
Quantumart.QP8.BackendEntityObject.getParentIdsForTree = function Quantumart$QP8$BackendEntityObject$getParentIdsForTree(entityTypeCode, ids) {
  var result = null;

  $q.getJsonFromUrl(
  'POST',
  window.CONTROLLER_URL_ENTITY_OBJECT + 'GetParentIdsForTree',
  { entityTypeCode: entityTypeCode, ids: ids },
          false,
  false,
    function(data) {
  result = data;
    },
    function(jqXHR) {
  result = null;
  $q.processGenericAjaxError(jqXHR);
    }
  );

  return result;
};

// Возвращает информацию о текущей и родительской сущности
// по коду типа и идентификатору
Quantumart.QP8.BackendEntityObject.getParentInfo = function Quantumart$QP8$BackendEntityObject$getParentInfo(entityTypeCode, entityId, parentEntityId) {
  var result = null;

  $q.getJsonFromUrl(
  'GET',
  window.CONTROLLER_URL_ENTITY_OBJECT + 'GetParentInfo',
  { entityTypeCode: entityTypeCode, entityId: entityId, parentEntityId: parentEntityId },
          false,
  false,
    function(data) {
  result = data;
    },
    function(jqXHR) {
  result = null;
  $q.processGenericAjaxError(jqXHR);
    }
  );

  return result;
};

Quantumart.QP8.BackendEntityObject.getArticleFieldValue = function(contentId, fieldName, articleId) {
  return $o.makeSimpleCall(
  'GET',
  window.CONTROLLER_URL_ENTITY_OBJECT + 'GetArticleFieldValue',
  { contentId: contentId, fieldName: fieldName, articleId: articleId }
  );
};

Quantumart.QP8.BackendEntityObject.getArticleLinkedItems = function Quantumart$QP8$BackendEntityObject$getArticleLinkedItems(linkId, articleId) {
  return $o.makeSimpleCall(
  'GET',
  window.CONTROLLER_URL_ENTITY_OBJECT + 'GetArticleLinkedItems',
  { linkId: linkId, articleId: articleId }
  );
};

Quantumart.QP8.BackendEntityObject.getArticleIdByFieldValue = function Quantumart$QP8$BackendEntityObject$getArticleIdByFieldValue(contentId, fieldName, fieldValue) {
  return $o.makeSimpleCall(
  'GET',
  window.CONTROLLER_URL_ENTITY_OBJECT + 'GetArticleIdByFieldValue',
  { contentId: contentId, fieldName: fieldName, fieldValue: fieldValue }
  );
};

// Возвращает информацию о текущей сущности и всех предках
// по коду типа и идентификатору
Quantumart.QP8.BackendEntityObject.getParentsChain = function(entityTypeCode, entityId, parentEntityId) {
  var result = null;

  $q.getJsonFromUrl(
    'GET',
    window.CONTROLLER_URL_ENTITY_OBJECT + 'GetParentsChain',
    { entityTypeCode: entityTypeCode, entityId: entityId, parentEntityId: parentEntityId },
      false,
    false,
    function(data) {
  result = data;
    },
    function(jqXHR) {
  result = null;
  $q.processGenericAjaxError(jqXHR);
    }
  );

  return result;
};

// Проверяет список сущностей на наличие пустых имен
Quantumart.QP8.BackendEntityObject.checkEntitiesForPresenceEmptyNames = function Quantumart$QP8$BackendEntityObject$checkEntitiesForPresenceEmptyNames(entities) {
  var isEmpty = false;

  jQuery.each(entities,
    function(index, entity) {
  if ($q.isNullOrWhiteSpace(entity.Name)) {
    isEmpty = true;

    return false;
  }
    }
  );

  return isEmpty;
};

// Возвращает массив идентификаторов сущностей из списка сущностей
Quantumart.QP8.BackendEntityObject.getEntityIDsFromEntities = function Quantumart$QP8$BackendEntityObject$getEntityIDsFromEntities(entities) {
  return jQuery.map(entities, function(elem) {
    return elem.Id;
  });
};

// Возвращает массив названий сущностей из списка сущностей
Quantumart.QP8.BackendEntityObject.getEntityNamesFromEntities = function Quantumart$QP8$BackendEntityObject$getEntityNamesFromEntities(entities) {
  return jQuery.map(entities, function(elem) {
    return elem.Name;
  });
};

// Возвращает строку названий сущностей из списка сущностей
Quantumart.QP8.BackendEntityObject.getEntityNamesStringFromEntities = function Quantumart$QP8$BackendEntityObject$getEntityNamesFromEntities(entities) {
  var count = entities.length;

  return jQuery.map(entities, function(elem, index) {
    var prefix = '';

    if (index > 0) {
      if (index == (count - 1)) {
        prefix = ' ' + $l.Common.andUnion.toLowerCase() + ' ';
      } else {
        prefix = ', ';
      }
    }

    return prefix + '"' + elem.Name + '"';
  }).join('');
};

// Проверяет наличие разрешений на использование представления типа "Дерево"
Quantumart.QP8.BackendEntityObject.isTreeViewTypeAllowed = function Quantumart$QP8$BackendEntityObject$isTreeViewTypeAllowed(entityTypeCode,
  parentEntityId, action) {
  var result = false;

  if (action && !$q.isNullOrEmpty(action.Views)) {
    var views = action.Views;
    var treeViewTypeExist = false;

    for (var viewIndex = 0; viewIndex < views.length; viewIndex++) {
      var view = views[viewIndex];

      if (view.ViewType.Code == VIEW_TYPE_CODE_TREE) {
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

Quantumart.QP8.BackendEntityObject.getContextQuery = function(contentId, currentContext) {
  var result = null;

  $q.getJsonFromUrl('GET', 'Article/GetContextQuery', { id: contentId, currentContext: currentContext }, false, false,
    function(data) {
  result = data;
    },
    function(jqXHR) {
  result = null;
  $q.processGenericAjaxError(jqXHR);
    }
  );
  return result;
};

Quantumart.QP8.BackendEntityObject.registerClass('Quantumart.QP8.BackendEntityObject');

window.$o = Quantumart.QP8.BackendEntityObject;

//#endregion
