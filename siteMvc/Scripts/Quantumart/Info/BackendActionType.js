// #region class BackendActionType
Quantumart.QP8.BackendActionType = function () {};

// Возвращает код типа действия по коду действия
Quantumart.QP8.BackendActionType.getActionTypeCodeByActionCode = function Quantumart$QP8$BackendActionType$getActionTypeCodeByActionCode(actionCode) {
	var cacheKey = "ActionTypeCodeByActionCode_" + actionCode;
	var actionTypeCode = $cache.getItem(cacheKey);

	if (!actionTypeCode) {
		$q.getJsonFromUrl(
			"GET",
			CONTROLLER_URL_BACKEND_ACTION_TYPE + "/GetCodeByActionCode",
			{ actionCode: actionCode },
			false,
			false,
			function (data, textStatus, jqXHR) {
				actionTypeCode = data;
			},
			function (jqXHR, textStatus, errorThrown) {
				actionTypeCode = null;
				$q.processGenericAjaxError(jqXHR);
			}
		);

		$cache.addItem(cacheKey, actionTypeCode);
	}

	return actionTypeCode;
};

Quantumart.QP8.BackendActionType.registerClass("Quantumart.QP8.BackendActionType");
// #endregion
