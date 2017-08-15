Quantumart.QP8.BackendNotificationPropertiesMediator = function (rootElementId) {
  const $root = $(`#${rootElementId}`);

  const $statusPanel = $('.nfp-status-panel', $root);
  const $senderNamePanel = $('.nfp-senderNamePanel', $root);
  const $senderEmailPanel = $('.nfp-senderEmailPanel', $root);
  const $externalPanel = $('.nfp-externalPanel', $root);

  const $statusCheckBoxes = $('.nfp-status', $root);
  const $senderNameCheckbox = $('.nfp-useDefaultSenderNameCheckbox', $root);
  const $backendEmailCheckbox = $('.nfp-useBackendEmailCheckbox', $root);
  const $externalCheckbox = $('.nfp-external', $root);

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
    if ($statusCheckBoxes.filter(':checked').length != 0) {
      $statusPanel.show();
    } else {
      $statusPanel.hide();
    }
  };

  const dispose = function () {
    $statusCheckBoxes.unbind();
    $senderNameCheckbox.unbind();
    $backendEmailCheckbox.unbind();
  };

  onStatusChanged();
  onUseDefaultSenderNameChanged();
  onUseBackendEmailChanged();
  onExternalChanged();

  $statusCheckBoxes.bind('click', onStatusChanged);
  $senderNameCheckbox.bind('click', onUseDefaultSenderNameChanged);
  $backendEmailCheckbox.bind('click', onUseBackendEmailChanged);
  $externalCheckbox.bind('click', onExternalChanged);

  return {
    dispose
  };
};
