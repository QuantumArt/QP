import { $q } from '../../Utils';
import { $c } from '../../ControlHelpers';

export class MultistepActionCopySiteSettings {
  constructor(options) {
    this.options = options;
  }


  initActions() {
    const $form = $(`#${this.options.popupWindowId}_editingForm`);
    $c.initAllCheckboxToggles($form);
  }

  validate() {
    return '';
  }

  serializeForm() {
    return $q.serializeForm(this.options.wrapperElementId, true);
  }

  dispose() {
    // empty fn
  }
}


MultistepActionCopySiteSettings.addButtons = function (dataItems) {
  const exportButton = {
    Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
    Value: 'Create like site',
    Text: $l.MultistepAction.createLikeSite,
    Tooltip: $l.MultistepAction.createLikeSite,
    AlwaysEnabled: false,
    Icon: 'action.gif'
  };

  return dataItems.concat(exportButton);
};

Quantumart.QP8.MultistepActionCopySiteSettings = MultistepActionCopySiteSettings;
