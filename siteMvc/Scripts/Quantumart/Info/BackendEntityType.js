Quantumart.QP8.BackendEntityType = function () {
  // empty constructor
};

Quantumart.QP8.BackendEntityType.getEntityTypeByCode = function (entityTypeCode) {
  const cacheKey = `EntityTypeByEntityTypeCode_${entityTypeCode}`;
  let entityType = Quantumart.QP8.Cache.getItem(cacheKey);

  if (!entityType) {
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_ENTITY_TYPE}GetByCode`,
      { entityTypeCode },
      false,
      false,
      data => {
        entityType = data;
      },
      () => {
        entityType = null;
      }
    );

    Quantumart.QP8.Cache.addItem(cacheKey, entityType);
  }

  return entityType;
};

Quantumart.QP8.BackendEntityType.getEntityTypeById = function (entityTypeId) {
  const cacheKey = `EntityTypeByEntityTypeId_${entityTypeId}`;
  let entityTypeCode = Quantumart.QP8.Cache.getItem(cacheKey);

  if (!entityTypeCode) {
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_ENTITY_TYPE}GetCodeById`,
      { entityTypeId },
      false,
      false,
      data => {
        entityTypeCode = data;
      },
      () => {
        entityTypeCode = null;
      }
    );

    Quantumart.QP8.Cache.addItem(cacheKey, entityTypeCode);
  }

  return Quantumart.QP8.BackendEntityType.getEntityTypeByCode(entityTypeCode);
};

Quantumart.QP8.BackendEntityType.getParentEntityTypeCodeByCode = function (entityTypeCode) {
  const cacheKey = `ParentEntityTypeCodeByEntityTypeCode_${entityTypeCode}`;
  let parentEntityTypeCode = Quantumart.QP8.Cache.getItem(cacheKey);

  if (!parentEntityTypeCode) {
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_ENTITY_TYPE}GetParentCodeByCode`,
      { entityTypeCode },
      false,
      false,
      data => {
        parentEntityTypeCode = data;
      },
      jqXHR => {
        parentEntityTypeCode = null;
        $q.processGenericAjaxError(jqXHR);
      }
    );

    Quantumart.QP8.Cache.addItem(cacheKey, parentEntityTypeCode);
  }

  return parentEntityTypeCode;
};

Quantumart.QP8.BackendEntityType.getEntityTypeIdToActionListItemDictionary = function () {
  const cacheKey = 'EntityTypeIdToActionListItemDictionary';
  let dictionary = Quantumart.QP8.Cache.getItem(cacheKey);

  if (!dictionary) {
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_BACKEND_ACTION}GetEntityTypeIdToActionListItemsDictionary`,
      null,
      false,
      false
    )
      .done(
        data => {
          if (data.success) {
            ({ dictionary } = data);
            Quantumart.QP8.Cache.addItem(cacheKey, data.dictionary);
          } else {
            dictionary = null;
            $q.alertError(data.Text);
          }
        })
      .fail(jqXHR => {
        dictionary = null;
        $q.processGenericAjaxError(jqXHR);
      });
  }

  return dictionary;
};

Quantumart.QP8.BackendEntityType.registerClass('Quantumart.QP8.BackendEntityType');
