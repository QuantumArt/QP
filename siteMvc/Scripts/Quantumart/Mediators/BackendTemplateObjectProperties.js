Quantumart.QP8.BackendTemplateObjectPropertiesMediator = function (rootElementId) {

    var $componentElem = jQuery('#' + rootElementId);
    var CONTENT_CHANGE_TRACK_SELECTORS = ".containerContentSelector .singleItemPicker";
    var $parentObjectSelector = $componentElem.find('.parentTemplateObjectsSelector');
    var $nameField = $componentElem.find('.name');
    var $netNameField = $componentElem.find('.netName');
    var $overrideChkbx = $componentElem.find('.overrideChkbx');
    var $globalChkbx = $componentElem.find('.globalChkbx');
    var $typeSelector = $componentElem.find('.typeDlist');
    var $statusSelector = $componentElem.find('.multipleItemPicker');
    var $selectionIsStarting = $componentElem.find('.selection-is-starting .radioButtonsList');
    var $selectionIncludes = $componentElem.find('.selection-includes .radioButtonsList');

    $componentElem.on(JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, CONTENT_CHANGE_TRACK_SELECTORS, onContentValueChanged);
    $parentObjectSelector.change(jQuery.proxy(onParentTemplateObjectChanged, $parentObjectSelector));
    $overrideChkbx.click(jQuery.proxy(onParentTemplateObjectChanged, $parentObjectSelector));
    $typeSelector.change(manageGlobalVisibility);

    checkPublished();
    if ($statusSelector.data('has-workflow') == "False") {
        $statusSelector.data('entity_data_list_component').disableList();
    }

    manageGlobalVisibility();

    function onContentValueChanged(e, data) {
        if (data.value) {
            $q.getJsonFromUrl('POST', CONTROLLER_URL_PAGE_TEMPLATE + "GetFieldsByContentId",
            {
                contentId: data.value
            },
            true, false).done(
            function (data) {
                var newFields = data.fields.split(",");
                var newStatuses = data.statuses;
                var publishedStatusId = $statusSelector.data('published-id');
                var vm = $componentElem.find('.sortingItems .aggregationList').data('component')._viewModel;

                if (vm.fields) {
                    vm.fields.removeAll();
                    for (var i in newFields) {
                        vm.fields.push(newFields[i]);
                    }
                }

                $statusSelector.data('entity_data_list_component').removeAllListItems();

                if (data.hasWorkflow == true) {
                    $statusSelector.data('entity_data_list_component').selectEntities(newStatuses);
                    $statusSelector.data('entity_data_list_component').deselectAllListItems();
                    $statusSelector.data('entity_data_list_component').enableList();
                }

                else {
                    $statusSelector.data('entity_data_list_component').selectEntities([$statusSelector.data('published-id')]);
                    $statusSelector.data('entity_data_list_component').disableList();
                }
            });
        }
    }

    function checkPublished() {
        $statusSelector.find('.multi-picker-item[value="' + $statusSelector.data('published-id') + '"]').attr('checked', true);
    }

    function onParentTemplateObjectChanged() {
        if ($overrideChkbx.is(':checked') && $parentObjectSelector.children('option').size()) {
            var objId = $parentObjectSelector.val();
            var targetObj = $(this.data('objects')).filter(function () { return this.Id == objId; })[0];
            $nameField.val(targetObj.Name);
            $netNameField.val(targetObj.NetName);
        }
        else {
            $nameField.val('');
            $netNameField.val('');
        }
    }



    function manageGlobalVisibility() {
        if ($globalChkbx.get(0) && $globalChkbx.data('visibletypes').split(",").indexOf($typeSelector.val()) != -1) {
            $globalChkbx.parent('.field').show();
        }
        else
            { $globalChkbx.parent('.field').hide(); }
    }

    function dispose() {
        $componentElem.unbind();
        $parentObjectSelector.unbind();

        $componentElem = null;
        CONTENT_CHANGE_TRACK_SELECTORS = null;
        $parentObjectSelector = null;
        $statusSelector = null;
        $selectionIsStarting = null;
        $selectionIncludes = null;
    }

    return {
        dispose: dispose
    };
};
