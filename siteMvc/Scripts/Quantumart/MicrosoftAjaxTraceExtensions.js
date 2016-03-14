Sys._Debug.prototype._isStopped = true;

Sys._Debug.prototype.isStopped = function () {
	return Sys.Debug._isStopped;
};

Sys._Debug.prototype.startTrace = function () {
	Sys.Debug._isStopped = false;
};

Sys._Debug.prototype.stopTrace = function () {
	Sys.Debug._isStopped = true;
};

Sys._Debug.prototype._appendTrace = function (text) {
	if (!Sys.Debug._isStopped) {
		var $area = jQuery("#TraceConsole");
		var $message = jQuery("<div />");
		$message
			.text(text)
			.appendTo($area)
			.animate({ "marginTop": 0 }, "fast")
			.animate({ "backgroundColor": "#ffffff" }, 800)
			;

		$message = null;
		$area = null;
	}
}

Sys._Debug.prototype.clearTrace = function() {
	var $area = jQuery("#TraceConsole");
	$area.empty();
}