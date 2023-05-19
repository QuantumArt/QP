export class BackendNotificationPropertiesMediator {
  constructor(rootElementId) {
    const $root = $(`#${rootElementId}`);

    const $statusPanel = $('.nfp-status-panel', $root);
    const $senderNamePanel = $('.nfp-senderNamePanel', $root);
    const $senderEmailPanel = $('.nfp-senderEmailPanel', $root);
    const $externalPanel = $('.nfp-externalPanel', $root);
    const $multipleRecipientsPanel = $('.nfp-multipleRecipients', $root);

    const $statusCheckBoxes = $('.nfp-status', $root);
    const $senderNameCheckbox = $('.nfp-useDefaultSenderNameCheckbox', $root);
    const $backendEmailCheckbox = $('.nfp-useBackendEmailCheckbox', $root);
    const $externalCheckbox = $('.nfp-external', $root);
    const $multipleRecipientsRadioButton = $('.nfp-receiver-radio', $root);

    const nonMultipleRecipientTypes = $('#NonMultipleRecipientTypes')[0].value.split(',');

    const onMultipleRecipientsChanged = function () {
      if (nonMultipleRecipientTypes.includes($(':checked', $multipleRecipientsRadioButton)[0].value)) {
        $multipleRecipientsPanel.hide();
      } else {
        $multipleRecipientsPanel.show();
      }
    };

    const onExternalChanged = function () {
      if ($externalCheckbox.is(':checked')) {
        $externalPanel.hide();
      } else {
        $externalPanel.show();
      }
    };

    const onUseBackendEmailChanged = function () {
      if ($backendEmailCheckbox.is(':checked')) {
        $senderEmailPanel.hide();
      } else {
        $senderEmailPanel.show();
      }
    };

    const onUseDefaultSenderNameChanged = function () {
      if ($senderNameCheckbox.is(':checked')) {
        $senderNamePanel.hide();
      } else {
        $senderNamePanel.show();
      }
    };

    const onStatusChanged = function () {
      if ($statusCheckBoxes.filter(':checked').length === 0) {
        $statusPanel.hide();
      } else {
        $statusPanel.show();
      }
    };

    const dispose = function () {
      $statusCheckBoxes.unbind();
      $senderNameCheckbox.unbind();
      $backendEmailCheckbox.unbind();
      $externalCheckbox.unbind();
      $multipleRecipientsRadioButton.unbind();
    };

    onStatusChanged();
    onUseDefaultSenderNameChanged();
    onUseBackendEmailChanged();
    onExternalChanged();
    onMultipleRecipientsChanged();

    $statusCheckBoxes.bind('click', onStatusChanged);
    $senderNameCheckbox.bind('click', onUseDefaultSenderNameChanged);
    $backendEmailCheckbox.bind('click', onUseBackendEmailChanged);
    $externalCheckbox.bind('click', onExternalChanged);
    $multipleRecipientsRadioButton.bind('change', onMultipleRecipientsChanged);

    return {
      dispose
    };
  }
}

Quantumart.QP8.BackendNotificationPropertiesMediator = BackendNotificationPropertiesMediator;
