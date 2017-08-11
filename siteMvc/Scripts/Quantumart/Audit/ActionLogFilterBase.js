// #region class ActionLogFilterBase
Quantumart.QP8.ActionLogFilterBase = function (filterContainer) {
	this.$container = jQuery(filterContainer);
};

Quantumart.QP8.ActionLogFilterBase.prototype = {
	$container: null,

	initialize: function () { },
	onOpen: function () { },
	get_filterDetails: function () {
 return "?"; 
},
	get_value: function () { },
	dispose: function () {
		this.$container = null;
	}
};

Quantumart.QP8.ActionLogFilterBase.registerClass("Quantumart.QP8.ActionLogFilterBase");
// #endregion
