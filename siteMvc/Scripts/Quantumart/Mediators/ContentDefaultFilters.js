// #region class ContentDefaultFilters
Quantumart.QP8.ContentDefaultFiltersMediator = function (parentElementId) {
	var $parentElement = jQuery("#" + parentElementId);
	var $siteCombo = $parentElement.find(".qp-deffilter-site");
	var contentPicker = $parentElement.find(".qp-deffilter-content").data("entity_data_list_component");
	var articlePicker = $parentElement.find(".qp-deffilter-articles").data("entity_data_list_component");

	$siteCombo.change(jQuery.proxy(function () {
		contentPicker.deselectAllListItems();
		contentPicker.set_parentEntityId(+$siteCombo.val() || 0);
	}, this));

	var onContentSelectedHandler = jQuery.proxy(function () {
		if (contentPicker.getSelectedListItemCount() === 0) {
			articlePicker.disableList();
			articlePicker.removeAllListItems();
			articlePicker.set_parentEntityId(0);
		} else {
			var selectedContent = contentPicker.getSelectedEntityIDs()[0];
			articlePicker.enableList();
			if (articlePicker.get_parentEntityId() !== selectedContent) {
				articlePicker.removeAllListItems();
				articlePicker.set_parentEntityId(selectedContent);
			}
		}
	}, this);

	return {
		initialize: function () {
			if (contentPicker.getSelectedListItemCount() === 0) {
				articlePicker.disableList();
				articlePicker.set_parentEntityId(0);
			} else {
				articlePicker.set_parentEntityId(contentPicker.getSelectedEntityIDs()[0]);
				articlePicker.enableList();
			}
			contentPicker.attachObserver(EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, onContentSelectedHandler);
		},

		dispose: function () {
			if (contentPicker) {
				contentPicker.detachObserver(EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, onContentSelectedHandler);
				contentPicker = null;
			}
			if ($siteCombo) {
				$siteCombo.off();
			}

			$siteCombo = null;
			articlePicker = null;
			$parentElement = null;
		}
	};
};

// #endregion
