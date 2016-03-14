Quantumart.QP8.MultistepActionExportSettings = function (options) {
	this.options = options;
};
Quantumart.QP8.MultistepActionExportSettings.prototype = {
	EXPORT_BUTTON: "Export",
	options: null,
    //Public Methods
    AddButtons: function (dataItems) {
        var exportButton = {
            Type: TOOLBAR_ITEM_TYPE_BUTTON,
            Value: this.EXPORT_BUTTON,
            Text: $l.MultistepAction.exportTitle,
            Tooltip: $l.MultistepAction.exportTitle,
            AlwaysEnabled: false,
            Icon: "action.gif"
        };
        Array.add(dataItems, exportButton);

        return dataItems;
    },
    InitActions: function (object, options) {
    	var fieldValues = this.options._popupWindowComponent.loadHostState();
    	var id = this.options._popupWindowId;
    	var $root = jQuery("#" + id + "_editingForm");
    	$c.setAllBooleanValues($root, fieldValues);
    	$c.setAllRadioListValues($root, fieldValues);

    	$c.initAllCheckboxToggles($root);

		$c.initAllEntityDataLists($root);
    	$c.setAllEntityDataListValues($root, fieldValues);

    },
    Validate: function () {
        var id = this.options._popupWindowId;
        var $root = jQuery("#" + id + "_editingForm");
        var fieldValues = $c.getAllFieldValues($root);
        this.options._popupWindowComponent.saveHostState(fieldValues);

        var errorMessage = '';
        return errorMessage;
    },

    dispose: function () {
    	var id = this.options._popupWindowId;
    	var $root = jQuery("#" + id + "_editingForm");
    	$c.destroyAllEntityDataLists($root);
    	$c.destroyAllCheckboxToggles($root)
    }
}