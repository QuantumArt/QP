Quantumart.QP8.BackendNotificationPropertiesMediator = function (rootElementId) {
	var $root = $(`#${  rootElementId}`);

	var $statusPanel = $('.nfp-status-panel', $root);
	var $senderNamePanel = $('.nfp-senderNamePanel', $root);
	var $senderEmailPanel = $('.nfp-senderEmailPanel', $root);
	var $externalPanel = $('.nfp-externalPanel', $root);

	var $statusCheckBoxes = $('.nfp-status', $root);
	var $senderNameCheckbox = $('.nfp-useDefaultSenderNameCheckbox', $root);
	var $backendEmailCheckbox = $('.nfp-useBackendEmailCheckbox', $root);
	var $externalCheckbox = $('.nfp-external', $root);

	$statusCheckBoxes.bind('click', onStatusChanged);
	$senderNameCheckbox.bind('click', onUseDefaultSenderNameChanged);
	$backendEmailCheckbox.bind('click', onUseBackendEmailChanged);
	$externalCheckbox.bind('click', onExternalChanged);

	function onExternalChanged() {
		if ($externalCheckbox.is(':checked')) {
 $externalPanel.hide();
} else {
 $externalPanel.show();
}
	}

	function onUseBackendEmailChanged() {
		if ($backendEmailCheckbox.is(':checked')) {
 $senderEmailPanel.hide();
} else {
 $senderEmailPanel.show();
}
	}

	function onUseDefaultSenderNameChanged() {
		if ($senderNameCheckbox.is(':checked')) {
 $senderNamePanel.hide();
} else {
 $senderNamePanel.show();
}
	}

	function onStatusChanged() {
		if ($statusCheckBoxes.filter(':checked').length != 0) {
			$statusPanel.show();
		} else {
			$statusPanel.hide();
		}
	}

	function dispose() {
		$statusCheckBoxes.unbind();
		$senderNameCheckbox.unbind();
		$backendEmailCheckbox.unbind();

		$root = null;

		$statusPanel = null;
		$senderNamePanel = null;
		$senderEmailPanel = null;
		$externalPanel = null;

		$statusCheckBoxes = null;
		$senderNameCheckbox = null;
		$backendEmailCheckbox = null;
		$externalCheckbox = null;
	}

	onStatusChanged();
	onUseDefaultSenderNameChanged();
	onUseBackendEmailChanged();
	onExternalChanged();

	return {
		dispose: dispose
	};
};
