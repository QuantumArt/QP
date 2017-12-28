import { $q } from '../../Utils';

export class MultistepActionCopySiteSettings {
  constructor(options) {
    this.options = options;
  }


  initActions() {
    // empty fn
  }

  validate() {
    return '';
  }

  serializeForm() {
    return $q.serializeForm(this.options.wrapperElementId);
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
