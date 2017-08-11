Quantumart.QP8.BackendNotificationPropertiesMediator = function (rootElementId) {

	var $root = jQuery('#' + rootElementId);

	var $statusPanel = jQuery('.nfp-status-panel', $root);
	var $senderNamePanel = jQuery('.nfp-senderNamePanel', $root);
	var $senderEmailPanel = jQuery('.nfp-senderEmailPanel', $root);
	var $externalPanel = jQuery('.nfp-externalPanel', $root);

	var $statusCheckBoxes = jQuery('.nfp-status', $root);
	var $senderNameCheckbox = jQuery('.nfp-useDefaultSenderNameCheckbox', $root);
	var $backendEmailCheckbox = jQuery('.nfp-useBackendEmailCheckbox', $root);
	var $externalCheckbox = jQuery('.nfp-external', $root);

	$statusCheckBoxes.bind("click", onStatusChanged);
	$senderNameCheckbox.bind("click", onUseDefaultSenderNameChanged);
	$backendEmailCheckbox.bind("click", onUseBackendEmailChanged);
	$externalCheckbox.bind("click", onExternalChanged);

	function onExternalChanged() {
		if ($externalCheckbox.is(':checked'))
			{ $externalPanel.hide(); }
		else
			{ $externalPanel.show(); }
	}

	function onUseBackendEmailChanged() {
		if ($backendEmailCheckbox.is(':checked'))
			{ $senderEmailPanel.hide(); }
		else
			{ $senderEmailPanel.show(); }
	}

	function onUseDefaultSenderNameChanged() {
		if ($senderNameCheckbox.is(':checked'))
			{ $senderNamePanel.hide(); }
		else
			{ $senderNamePanel.show(); }
	}

	function onStatusChanged() {
		if ($statusCheckBoxes.filter(':checked').length != 0) {
			$statusPanel.show();
		}
		else {
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
