// based on: http://benogle.com/2009/06/16/simple-css-shiny-progress-bar-technique.html
(function ($) {

	function backendProgressBarComponent($we, options) {

		var $wrapElement = $we;
		var settings = $.extend({
			'total': 0,
			'value': 0,
			'digits': 1
		}, options);
		var progressbarInnerHtml = '<div class="progressbar-value"><div class="progressbar-text">0%</div></div>';
		var currentValue = settings.value;
		var currentWidth = 0;


		function setValue(val) {
			if ($q.isNull(val)) {
				val = currentValue;
			}
			if ($.isNumeric(val)) {
				currentValue = val;
				currentWidth = 0;
				if (settings.total > 0) {
					currentWidth = (val * 100 / settings.total);
				}
				else if (settings.total == 0) {
				    currentWidth = 100;
				}
				if (currentWidth < 0) {
					currentValue = 0;
					currentWidth = 0;
				}
				else if (currentWidth > 100) {
					currentValue = settings.total;
					currentWidth = 100;
				}


			}
		};

		function getValue() {
			return currentValue;
		};

		function setTotal(val, dfr) {
			if ($q.isNull(val)) {
				val = settings.total();
			}
			if ($.isNumeric(val)) {
				settings.total = val;
				setValue(val, dfr);
			}
		};

		function getTotal() {
			return settings.total;
		};

		function increment(val) {
			if ($.isNumeric(val)) {
				setValue(getValue() + val);
				return getValue();
			}
		};

		function decriment(val) {
			if ($.isNumeric(val)) {
				setValue(getValue() - val);
				return getValue();
			}
		};

		function refresh() {
			$wrapElement.find(".progressbar-value").css({ width: $q.toFixed(currentWidth, settings.digits) + "%" });
			setText($q.toFixed(currentWidth, settings.digits) + "%");
		}

		function setText(val) {
			$wrapElement.find(".progressbar-text").text(val);
		}

		function setColor(color) {
			$wrapElement.css('background-color', color);
			$wrapElement.find(".progressbar-value").css('background-color', color);
		}

		function dispose() {
			$wrapElement = null;
		}

		// Инициализация
		$wrapElement.addClass("progressbar-wrap");
		$wrapElement.html(progressbarInnerHtml);
		setValue(settings.value);
		/* --------  */

		return {
			value: function (val) {
				if ($.isNumeric(val)) {
					return setValue(val);
				}
				else {
					return getValue();
				}
			},

			total: function (val) {
				if ($.isNumeric(val)) {
					return setTotal(val);
				}
				else {
					return getTotal();
				}
			},

			increment: increment,
			decriment: decriment,

			refresh: refresh,
			setText: setText,
			setColor: setColor,

			dispose: dispose
		};
	}

	var methods = {
		init: function (options) {
			return this.filter("div").each(function () {
				var $this = $(this);
				var component = new backendProgressBarComponent($this, options);
				$this.data("backendProgressBar", component);
				$this = null;
				component = null;
			});
		},

		dispose: function () {
			return this.each(function () {
				var $this = $(this);
				var component = $this.data("backendProgressBar");
				component.dispose();
				component = null;
				$this.removeData("backendProgressBar");
			});
		}
	};

	$.fn.backendProgressBar = function (method) {
		if (methods[method]) {
			return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
		}
		else if (typeof method === 'object' || !method) {
			return methods.init.apply(this, arguments);
		}
		else {
			$.error('Method ' + method + ' does not exist on backendProgressBar');
		}
	};
})(jQuery);
