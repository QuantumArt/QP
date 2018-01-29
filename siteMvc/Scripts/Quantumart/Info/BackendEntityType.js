import { GlobalCache } from '../Cache';
import { $q } from '../Utils';

export class BackendEntityType {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor() {
    // empty constructor
  }
}

BackendEntityType.getEntityTypeByCode = function (entityTypeCode) {
  const cacheKey = `EntityTypeByEntityTypeCode_${entityTypeCode}`;
  let entityType = GlobalCache.getItem(cacheKey);

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

    GlobalCache.addItem(cacheKey, entityType);
  }

  return entityType;
};

BackendEntityType.getEntityTypeById = function (entityTypeId) {
  const cacheKey = `EntityTypeByEntityTypeId_${entityTypeId}`;
  let entityTypeCode = GlobalCache.getItem(cacheKey);

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

    GlobalCache.addItem(cacheKey, entityTypeCode);
  }

  return BackendEntityType.getEntityTypeByCode(entityTypeCode);
};

BackendEntityType.getParentEntityTypeCodeByCode = function (entityTypeCode) {
  const cacheKey = `ParentEntityTypeCodeByEntityTypeCode_${entityTypeCode}`;
  let parentEntityTypeCode = GlobalCache.getItem(cacheKey);

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

    GlobalCache.addItem(cacheKey, parentEntityTypeCode);
  }

  return parentEntityTypeCode;
};

BackendEntityType.getEntityTypeIdToActionListItemDictionary = function () {
  const cacheKey = 'EntityTypeIdToActionListItemDictionary';
  let dictionary = GlobalCache.getItem(cacheKey);

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
            GlobalCache.addItem(cacheKey, data.dictionary);
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


Quantumart.QP8.BackendEntityType = BackendEntityType;
