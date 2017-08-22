Quantumart.QP8.BackendActionType = function () {};
Quantumart.QP8.BackendActionType.getActionTypeCodeByActionCode = function (actionCode) {
  const cacheKey = `ActionTypeCodeByActionCode_${actionCode}`;
  let actionTypeCode = Quantumart.QP8.Cache.getItem(cacheKey);

  if (!actionTypeCode) {
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_BACKEND_ACTION_TYPE}/GetCodeByActionCode`,
      { actionCode },
      false,
      false,
      (data, textStatus, jqXHR) => {
        actionTypeCode = data;
      },
      (jqXHR, textStatus, errorThrown) => {
        actionTypeCode = null;
        $q.processGenericAjaxError(jqXHR);
      }
    );

    Quantumart.QP8.Cache.addItem(cacheKey, actionTypeCode);
  }

  return actionTypeCode;
};

Quantumart.QP8.BackendActionType.registerClass('Quantumart.QP8.BackendActionType');
