Quantumart.QP8.FieldPropertiesMediator = function (tabId)
{
	var $root = jQuery("#" + tabId + "_editingForm");
	$root.find("input[name='Data.IsInteger']").on("click", onIntegerClick);
	onIntegerClick();


	function onIntegerClick()
	{
		var isInteger = $root.find("input[name='Data.IsInteger']").prop("checked");
		$root.find("input[name='Data.IsLong']").closest("dl").toggle(isInteger)
		$root.find("input[name='Data.IsDecimal']").closest("dl").toggle(!isInteger)
	}

	function dispose()
	{
		$root.find("input[name='Data.IsInteger']").off("click", onIntegerClick);
	}

	return 	{
		dispose: dispose
	};

}

Quantumart.QP8.RelateToAndDisplayFieldMediator = function (relateToSelectElementId, displayFieldSelectElementId, currentFieldIdHiddenElementId, listOrderSelectElementId) {
    var contentPicker = jQuery("#" + relateToSelectElementId).data("entity_data_list_component"),
        $displayFieldSelectElement = jQuery("#" + displayFieldSelectElementId),
        $listOrderSelectElement = jQuery("#" + listOrderSelectElementId),
        currentFieldId = jQuery("#" + currentFieldIdHiddenElementId).val(),
        relateableFieldsUrl = CONTROLLER_URL_CONTENT + '_RelateableFields';

	function onRelatedToChanged() {
	    var selectedContentId = jQuery(contentPicker.getStateFieldElement()).val();

        if(!$q.isNullOrEmpty(selectedContentId)){
        	$q.getJsonFromUrl(
                "GET",
                relateableFieldsUrl,
                {
                	"contentId": selectedContentId,
                	"fieldId": currentFieldId
                },
                true,
				false
			)
			.done(function (data) {
				if (data.success) {
				    $displayFieldSelectElement.empty();
				    $listOrderSelectElement.empty();
				    var html, html2;
					if (!$q.isNullOrEmpty(data.data)) {
						var htmlBuilder = new $.telerik.stringBuilder();
						jQuery(data.data).each(function () {
						    htmlBuilder.cat('<option value="').cat(this.id).cat('">').cat(this.text).cat('</option>');
						});
						html = htmlBuilder.string();
						html2 = '<option value="">' + $l.EntityEditor.selectField + '</option>' + html;
					}
					else {
					    html = '<option value=""></option>';
					    html2 = html;
					}
					$displayFieldSelectElement.append(html);
					$listOrderSelectElement.append(html2);
				}
				else {
					alert(data.message);
				}
			})
			.fail(function (jqXHR, textStatus, errorThrown) {
				$q.processGenericAjaxError(jqXHR);
			});
        }
	}


	function dispose() {
	    jQuery(contentPicker.getStateFieldElement()).off("change", onRelatedToChanged);
	    contentPicker = null;
        $displayFieldSelectElement = null;
	}

	jQuery(contentPicker.getStateFieldElement()).on("change", onRelatedToChanged);


	return {
		dispose: dispose
	};
};

Quantumart.QP8.RelateToAndClassifierFieldMediator = function (relateToSelectElementId, classifierSelectElementId, aggregatedElementId, multiplePickerId) {
    var contentPicker = jQuery("#" + relateToSelectElementId).data("entity_data_list_component"),
        $classifierSelectElement = jQuery("#" + classifierSelectElementId),
        $aggregatedElement = jQuery("#" + aggregatedElementId),
		classifierFieldsUrl = CONTROLLER_URL_CONTENT + '_ClassifierFields';
    
    function onRelatedToChanged() {        
	    var selectedContentId = jQuery(contentPicker.getStateFieldElement()).val();	    
		if (!$q.isNullOrEmpty(selectedContentId)) {
			$q.getJsonFromUrl(
                "GET",
				classifierFieldsUrl,
				{
					"contentId": selectedContentId
				},
				true,
				false
			)
			.done(function (data) {
				if (data.success) {
					$classifierSelectElement.empty();
					if (!$q.isNullOrEmpty(data.data)) {
						var html = new $.telerik.stringBuilder();
						jQuery(data.data).each(function () {
							html.cat('<option value="').cat(this.id).cat('">').cat(this.text).cat('</option>');
						});
						$classifierSelectElement.append(html.string());
					}
					else {
						$classifierSelectElement.append('<option value=""></option>');
					}
				}
				else {
					alert(data.message);
				}								
			})
			.fail(function (jqXHR, textStatus, errorThrown) {
				$q.processGenericAjaxError(jqXHR);
			});
		}
	};

	function dispose() {
	    jQuery(contentPicker.getStateFieldElement()).off("change", onRelatedToChanged);
	    contentPicker = null;
		$classifierSelectElement = null;
		$aggregatedElement = null;
	}

	jQuery(contentPicker.getStateFieldElement()).on("change", onRelatedToChanged);

	return {
		dispose: dispose
	};
};


Quantumart.QP8.RelateToAndO2MDefaultMediator = function (relateToSelectElementId, O2MPickerListElementId, M2MPickerListElementId) {

    var contentPicker = jQuery("#" + relateToSelectElementId).data("entity_data_list_component"),
        singleItemPickerComponent = Quantumart.QP8.BackendEntityDataListManager.getInstance().getList(O2MPickerListElementId),
        multipleItemPickerComponent = Quantumart.QP8.BackendEntityDataListManager.getInstance().getList(M2MPickerListElementId+'_list');


	function onRelatedToChanged() {
        // Получить список полей контена
	    var selectedContentId = jQuery(contentPicker.getStateFieldElement()).val();
	    multipleItemPickerComponent.removeAllListItems();
	    multipleItemPickerComponent.set_parentEntityId(selectedContentId);
        singleItemPickerComponent.set_parentEntityId(selectedContentId);
        singleItemPickerComponent.deselectAllListItems();        
	}


	function dispose() {
	    jQuery(contentPicker.getStateFieldElement()).off("change", onRelatedToChanged);
	    contentPicker = null;
        singleItemPickerComponent = null;
	}

	jQuery(contentPicker.getStateFieldElement()).on("change", onRelatedToChanged);

	return {
		dispose: dispose
	};
};




Quantumart.QP8.FieldTypeFileDefaultMediator = function(fieldTypeSelectElementId, fileFieldElementId){

    var $fieldTypeSelectElement = jQuery("#" + fieldTypeSelectElementId),
        $fileFieldElement = jQuery("#" + fileFieldElementId),
        fileFieldComponent = $fileFieldElement.data("file_field");

    function onFieldTypeChanged(){
        var fieldType = jQuery("option:selected", $fieldTypeSelectElement).val();
        if(fieldType == FILE_FIELD_TYPE){
            fileFieldComponent.set_isImage(false);
        }
        else if(fieldType == IMAGE_FIELD_TYPE){
            fileFieldComponent.set_isImage(true);
        }
    }

    function dispose(){
        $fieldTypeSelectElement.unbind();
        $fieldTypeSelectElement = null;
        $fileFieldElement = null;
        fileFieldComponent = null;
    }

    $fieldTypeSelectElement.bind("change keyup", onFieldTypeChanged);

    return {
        dispose: dispose
    };
}

// Показывает/скрывает панели при выборе контента на который ссылается поле
Quantumart.QP8.RelateToAndPanelsMediator = function (relateToSelectElementId, panelsSelector, fieldContentID) {
    var contentPicker = jQuery("#" + relateToSelectElementId).data("entity_data_list_component"),
		$panels = jQuery(panelsSelector);

	function onRelatedToChanged() {
		// Получить список полей контена
	    var selectedContentId = jQuery(contentPicker.getStateFieldElement()).val();

		if (!selectedContentId) {
			// ничего не выбрано - закрываем все панели
			$panels.hide();
		}
		else {
			if ($q.isNullOrEmpty(selectedContentId)) {
				$panels.hide();
			}
			else {
				$panels.show();
				if (selectedContentId == fieldContentID) {
					// тот же контент
					$panels.filter('[showforcurrent]').show();
					$panels.filter('[hideforcurrent]').hide();
				}
				else {
					$panels.filter('[showforcurrent]').hide();
					$panels.filter('[hideforcurrent]').show();
				}
			}
		}
	}


	function dispose() {
	    jQuery(contentPicker.getStateFieldElement()).off("change", onRelatedToChanged);
	    contentPicker = null;
		$panels = null;
	}

	jQuery(contentPicker.getStateFieldElement()).on("change", onRelatedToChanged);

	return {
		refresh: onRelatedToChanged,
		dispose: dispose
	};
}
