import { GlobalCache } from '../Cache';
import { $q } from '../Utils';

export class BackendActionType {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor() {
    // empty constructor
  }
}
BackendActionType.getActionTypeCodeByActionCode = function (actionCode) {
  const cacheKey = `ActionTypeCodeByActionCode_${actionCode}`;
  let actionTypeCode = GlobalCache.getItem(cacheKey);

  if (!actionTypeCode) {
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_BACKEND_ACTION_TYPE}GetCodeByActionCode`,
      { actionCode },
      false,
      false,
      data => {
        actionTypeCode = data;
      },
      jqXHR => {
        actionTypeCode = null;
        $q.processGenericAjaxError(jqXHR);
      }
    );

    GlobalCache.addItem(cacheKey, actionTypeCode);
  }

  return actionTypeCode;
};


Quantumart.QP8.BackendActionType = BackendActionType;
