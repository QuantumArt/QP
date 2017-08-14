Quantumart.QP8.BackendNotificationPropertiesMediator = function (rootElementId) {
  let $root = $(`#${rootElementId}`);

  let $statusPanel = $('.nfp-status-panel', $root);
  let $senderNamePanel = $('.nfp-senderNamePanel', $root);
  let $senderEmailPanel = $('.nfp-senderEmailPanel', $root);
  let $externalPanel = $('.nfp-externalPanel', $root);

  let $statusCheckBoxes = $('.nfp-status', $root);
  let $senderNameCheckbox = $('.nfp-useDefaultSenderNameCheckbox', $root);
  let $backendEmailCheckbox = $('.nfp-useBackendEmailCheckbox', $root);
  let $externalCheckbox = $('.nfp-external', $root);

  let onExternalChanged = function () {
    if ($externalCheckbox.is(':checked')) {
      $externalPanel.hide();
    } else {
      $externalPanel.show();
    }
  };

  let onUseBackendEmailChanged = function () {
    if ($backendEmailCheckbox.is(':checked')) {
      $senderEmailPanel.hide();
    } else {
      $senderEmailPanel.show();
    }
  };

  let onUseDefaultSenderNameChanged = function () {
    if ($senderNameCheckbox.is(':checked')) {
      $senderNamePanel.hide();
    } else {
      $senderNamePanel.show();
    }
  };

  let onStatusChanged = function () {
    if ($statusCheckBoxes.filter(':checked').length != 0) {
      $statusPanel.show();
    } else {
      $statusPanel.hide();
    }
  };

  let dispose = function () {
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
    dispose: dispose
  };
};
