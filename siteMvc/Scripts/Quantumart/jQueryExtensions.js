(function ($) {
	$.extend(
		{
			// Подготавливает значение к преобразованию в число
			_prepareNumber: function (value) {
				var number = null;

				if (value) {
					number = value.toString();
					if (number.length > 0) {
						number = number
							.replace(/\s/igm, "")
							.toLowerCase()
							.replace(",", ".")
							.replace(/^0*(\-?[0-9]+\.?[0-9]*)$/igm, "$1")
							;
					}
				}

				return number;
			},

			// Получает из значения целое число
			_getInt: function (value) {
				var number = 0;
				var processedValue = $._prepareNumber(value);

				if (processedValue) {
					number = parseInt(processedValue, 10);
					if (isNaN(number)) {
						number = 0;
					}
				}

				return number;
			},

			// Получает из значения число двойной точности
			_getFloat: function (value) {
				var number = 0;
				var processedValue = $._prepareNumber(value);

				if (processedValue) {
					number = parseFloat(processedValue);
					if (isNaN(number)) {
						number = 0;
					}
				}

				return number;
			},

			// Определяет ширину полосы прокрутки
			getScrollBarWidth: function () {
				var div = $('<div style="width: 50px; height: 50px; overflow: hidden; position: absolute; top: -200px; left: -200px;"><div style="height: 100px;"></div>');

				$("BODY").append(div);
				var w1 = $("DIV", div).innerWidth();
				div.css("overflow-y", "scroll");
				var w2 = $("DIV", div).innerWidth();
				$(div).remove();

				return (w1 - w2);
			},

			// Parse strings looking for color tuples [255,255,255]
			_getRGB: function (color) {
				var result;

				// Check if we're already dealing with an array of colors
				if (color && color.constructor == Array && color.length == 3) {
 return color;
}

				// Look for rgb(num,num,num)
        result = /rgb\(\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*\)/.exec(color);
				if (result) {
 return [parseInt(result[1], 10), parseInt(result[2], 10), parseInt(result[3], 10)];
}

				// Look for #a0b1c2
        result = /#([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})/.exec(color);
				if (result) {
 return [parseInt(result[1], 16), parseInt(result[2], 16), parseInt(result[3], 16)];
}

				// Otherwise, we're most likely dealing with a named color
				return colors[$.trim(color).toLowerCase()];
			},

			_getColor: function (elem, attr) {
				var color;

				do {
					color = $.curCSS(elem, attr);

					// Keep going until we find an element that has color, or we hit the body
					if ((color != '' && color != 'transparent') || $.nodeName(elem, "body")) {
 break;
}

					attr = "backgroundColor";
				} while (elem = elem.parentNode);

				return $._getRGB(color);
			}
		}
	);

	$.fn.extend(
		{
			// Получает значение верхнего внешнего отступа
			marginTop: function () {
				return $._getInt($(this).css("margin-top"));
			},

			// Получает значение правого внешнего отступа
			marginRight: function () {
				return $._getInt($(this).css("margin-right"));
			},

			// Получает значение нижнего внешнего отступа
			marginBottom: function () {
				return $._getInt($(this).css("margin-bottom"));
			},

			// Получает значение левого внешнего отступа
			marginLeft: function () {
				return $._getInt($(this).css("margin-left"));
			},

			// Получает ширину верхней рамки
			borderTopWidth: function () {
				return $._getInt($(this).css("border-top-width"));
			},

			// Получает ширину правой рамки
			borderRightWidth: function () {
				return $._getInt($(this).css("border-right-width"));
			},

			// Получает ширину нижней рамки
			borderBottomWidth: function () {
				return $._getInt($(this).css("border-bottom-width"));
			},

			// Получает ширину левой рамки
			borderLeftWidth: function () {
				return $._getInt($(this).css("border-left-width"));
			},

			// Получает значение верхнего внутреннего отступа
			paddingTop: function () {
				return $._getInt($(this).css("padding-top"));
			},

			// Получает значение правого внутреннего отступа
			paddingRight: function () {
				return $._getInt($(this).css("padding-right"));
			},

			// Получает значение нижнего внутреннего отступа
			paddingBottom: function () {
				return $._getInt($(this).css("padding-bottom"));
			},

			// Получает значение левого внутреннего отступа
			paddingLeft: function () {
				return $._getInt($(this).css("padding-left"));
			},

			maxZIndex: function (opt) {
				// / <summary>
				// / Returns the max zOrder in the document (no parameter)
				// / Sets max zOrder by passing a non-zero number
				// / which gets added to the highest zOrder.
				// / </summary>
				// / <param name="opt" type="object">
				// / inc: increment value,
				// / group: selector for zIndex elements to find max for
				// / </param>
				// / <returns type="jQuery" />
				var def = { inc: 10, group: "*" };
				$.extend(def, opt);
				var zmax = 0;
				$(def.group).each(function () {
					var cur = parseInt($(this).css('z-index'), 10);
					zmax = cur > zmax ? cur : zmax;
				});
				if (!this.jquery) {
 return zmax;
}

				return this.each(function () {
					zmax += def.inc;
					$(this).css("z-index", zmax);
				});
			}
		}
	);

	// Переопределяем анимацию для всех цветовых стилей
	$.each(["backgroundColor", "borderBottomColor", "borderLeftColor", "borderRightColor", "borderTopColor", "color", "outlineColor"], function (i, attr) {
		$.fx.step[attr] = function (fx) {
			if (fx.state == 0 || typeof fx.end == typeof "") {
				fx.start = $._getColor(fx.elem, attr);
				fx.end = $._getRGB(fx.end);
			}

			fx.elem.style[attr] = ["rgb(", [
				Math.max(Math.min(parseInt((fx.pos * (fx.end[0] - fx.start[0])) + fx.start[0], 10), 255), 0),
				Math.max(Math.min(parseInt((fx.pos * (fx.end[1] - fx.start[1])) + fx.start[1], 10), 255), 0),
				Math.max(Math.min(parseInt((fx.pos * (fx.end[2] - fx.start[2])) + fx.start[2], 10), 255), 0)
			].join(","), ")"].join("");
		};
	});

	// Добавляем в jQuery возможность проверки поддержки CSS-свойства border-radius (взято из библиотеки Modernizr 2.0)
	var mod = "modernizr";
	var modElem = document.createElement(mod);
	var mStyle = modElem.style;
	var domPrefixes = "Webkit Moz O ms Khtml".split(" ");

	function testProps(props, prefixed) {
		for (var i in props) {
			if (mStyle[props[i]] !== undefined) {
				return prefixed == "pfx" ? props[i] : true;
			}
		}
		return false;
	};

	function testPropsAll(prop, prefixed) {
		var ucProp = prop.charAt(0).toUpperCase() + prop.substr(1),
            props = (prop + " " + domPrefixes.join(ucProp + " ") + ucProp).split(" ");

		return testProps(props, prefixed);
	};

	$.support.borderRadius = testPropsAll("borderRadius");
}(jQuery));
