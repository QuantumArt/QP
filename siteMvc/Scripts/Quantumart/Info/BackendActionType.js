// eslint-disable-next-line no-useless-constructor, FIXME
Quantumart.QP8.BackendActionType = function () {
  // empty constructor
};
Quantumart.QP8.BackendActionType.getActionTypeCodeByActionCode = function (actionCode) {
  const cacheKey = `ActionTypeCodeByActionCode_${actionCode}`;
  let actionTypeCode = Quantumart.QP8.GlobalCache.getItem(cacheKey);

  if (!actionTypeCode) {
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_BACKEND_ACTION_TYPE}/GetCodeByActionCode`,
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

    Quantumart.QP8.GlobalCache.addItem(cacheKey, actionTypeCode);
  }

  return actionTypeCode;
};

Quantumart.QP8.BackendActionType.registerClass('Quantumart.QP8.BackendActionType');
