Quantumart.QP8.ContentDefaultFiltersMediator = function (parentElementId) {
  const $parentElement = $(`#${parentElementId}`);
  const $siteCombo = $parentElement.find('.qp-deffilter-site');
  const contentPicker = $parentElement.find('.qp-deffilter-content').data('entity_data_list_component');
  const articlePicker = $parentElement.find('.qp-deffilter-articles').data('entity_data_list_component');

  $siteCombo.change($.proxy(() => {
    contentPicker.deselectAllListItems();
    contentPicker.set_parentEntityId(+$siteCombo.val() || 0);
  }, this));

  const onContentSelectedHandler = $.proxy(() => {
    if (contentPicker.getSelectedListItemCount() === 0) {
      articlePicker.disableList();
      articlePicker.removeAllListItems();
      articlePicker.set_parentEntityId(0);
    } else {
      const selectedContent = contentPicker.getSelectedEntityIDs()[0];
      articlePicker.enableList();
      if (articlePicker.get_parentEntityId() !== selectedContent) {
        articlePicker.removeAllListItems();
        articlePicker.set_parentEntityId(selectedContent);
      }
    }
  }, this);

  return {
    initialize() {
      if (contentPicker.getSelectedListItemCount() === 0) {
        articlePicker.disableList();
        articlePicker.set_parentEntityId(0);
      } else {
        articlePicker.set_parentEntityId(contentPicker.getSelectedEntityIDs()[0]);
        articlePicker.enableList();
      }

      contentPicker.attachObserver(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, onContentSelectedHandler);
    },

    dispose() {
      if (contentPicker) {
        contentPicker.detachObserver(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, onContentSelectedHandler);
      }

      if ($siteCombo) {
        $siteCombo.off();
      }
    }
  };
};
