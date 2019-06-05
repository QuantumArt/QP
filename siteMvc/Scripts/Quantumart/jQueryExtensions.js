/* eslint-disable no-restricted-properties, no-param-reassign, no-console */

// eslint-disable-next-line id-length, no-shadow
(function ($) {
  const colors = {
    aliceblue: '#f0f8ff',
    antiquewhite: '#faebd7',
    aqua: '#00ffff',
    aquamarine: '#7fffd4',
    azure: '#f0ffff',
    beige: '#f5f5dc',
    bisque: '#ffe4c4',
    black: '#000000',
    blanchedalmond: '#ffebcd',
    blue: '#0000ff',
    blueviolet: '#8a2be2',
    brown: '#a52a2a',
    burlywood: '#deb887',
    cadetblue: '#5f9ea0',
    chartreuse: '#7fff00',
    chocolate: '#d2691e',
    coral: '#ff7f50',
    cornflowerblue: '#6495ed',
    cornsilk: '#fff8dc',
    crimson: '#dc143c',
    cyan: '#00ffff',
    darkblue: '#00008b',
    darkcyan: '#008b8b',
    darkgoldenrod: '#b8860b',
    darkgray: '#a9a9a9',
    darkgreen: '#006400',
    darkkhaki: '#bdb76b',
    darkmagenta: '#8b008b',
    darkolivegreen: '#556b2f',
    darkorange: '#ff8c00',
    darkorchid: '#9932cc',
    darkred: '#8b0000',
    darksalmon: '#e9967a',
    darkseagreen: '#8fbc8f',
    darkslateblue: '#483d8b',
    darkslategray: '#2f4f4f',
    darkturquoise: '#00ced1',
    darkviolet: '#9400d3',
    deeppink: '#ff1493',
    deepskyblue: '#00bfff',
    dimgray: '#696969',
    dodgerblue: '#1e90ff',
    firebrick: '#b22222',
    floralwhite: '#fffaf0',
    forestgreen: '#228b22',
    fuchsia: '#ff00ff',
    gainsboro: '#dcdcdc',
    ghostwhite: '#f8f8ff',
    gold: '#ffd700',
    goldenrod: '#daa520',
    gray: '#808080',
    green: '#008000',
    greenyellow: '#adff2f',
    honeydew: '#f0fff0',
    hotpink: '#ff69b4',
    indianred: '#cd5c5c',
    indigo: '#4b0082',
    ivory: '#fffff0',
    khaki: '#f0e68c',
    lavender: '#e6e6fa',
    lavenderblush: '#fff0f5',
    lawngreen: '#7cfc00',
    lemonchiffon: '#fffacd',
    lightblue: '#add8e6',
    lightcoral: '#f08080',
    lightcyan: '#e0ffff',
    lightgoldenrodyellow: '#fafad2',
    lightgrey: '#d3d3d3',
    lightgreen: '#90ee90',
    lightpink: '#ffb6c1',
    lightsalmon: '#ffa07a',
    lightseagreen: '#20b2aa',
    lightskyblue: '#87cefa',
    lightslategray: '#778899',
    lightsteelblue: '#b0c4de',
    lightyellow: '#ffffe0',
    lime: '#00ff00',
    limegreen: '#32cd32',
    linen: '#faf0e6',
    magenta: '#ff00ff',
    maroon: '#800000',
    mediumaquamarine: '#66cdaa',
    mediumblue: '#0000cd',
    mediumorchid: '#ba55d3',
    mediumpurple: '#9370d8',
    mediumseagreen: '#3cb371',
    mediumslateblue: '#7b68ee',
    mediumspringgreen: '#00fa9a',
    mediumturquoise: '#48d1cc',
    mediumvioletred: '#c71585',
    midnightblue: '#191970',
    mintcream: '#f5fffa',
    mistyrose: '#ffe4e1',
    moccasin: '#ffe4b5',
    navajowhite: '#ffdead',
    navy: '#000080',
    oldlace: '#fdf5e6',
    olive: '#808000',
    olivedrab: '#6b8e23',
    orange: '#ffa500',
    orangered: '#ff4500',
    orchid: '#da70d6',
    palegoldenrod: '#eee8aa',
    palegreen: '#98fb98',
    paleturquoise: '#afeeee',
    palevioletred: '#d87093',
    papayawhip: '#ffefd5',
    peachpuff: '#ffdab9',
    peru: '#cd853f',
    pink: '#ffc0cb',
    plum: '#dda0dd',
    powderblue: '#b0e0e6',
    purple: '#800080',
    rebeccapurple: '#663399',
    red: '#ff0000',
    rosybrown: '#bc8f8f',
    royalblue: '#4169e1',
    saddlebrown: '#8b4513',
    salmon: '#fa8072',
    sandybrown: '#f4a460',
    seagreen: '#2e8b57',
    seashell: '#fff5ee',
    sienna: '#a0522d',
    silver: '#c0c0c0',
    skyblue: '#87ceeb',
    slateblue: '#6a5acd',
    slategray: '#708090',
    snow: '#fffafa',
    springgreen: '#00ff7f',
    steelblue: '#4682b4',
    tan: '#d2b48c',
    teal: '#008080',
    thistle: '#d8bfd8',
    tomato: '#ff6347',
    turquoise: '#40e0d0',
    violet: '#ee82ee',
    wheat: '#f5deb3',
    white: '#ffffff',
    whitesmoke: '#f5f5f5',
    yellow: '#ffff00',
    yellowgreen: '#9acd32'
  };

  $.extend({
    // Подготавливает значение к преобразованию в число
    _prepareNumber(value) {
      let number = null;

      if (value) {
        number = value.toString();
        if (number.length > 0) {
          number = number
            .replace(/\s/igm, '')
            .toLowerCase()
            .replace(',', '.')
            .replace(/^0*(-?[0-9]+\.?[0-9]*)$/igm, '$1');
        }
      }

      return number;
    },

    // Получает из значения целое число
    _getInt(value) {
      let number = 0;
      const processedValue = $._prepareNumber(value);

      if (processedValue) {
        number = parseInt(processedValue, 10);
        if (isNaN(number)) {
          number = 0;
        }
      }

      return number;
    },

    // Получает из значения число двойной точности
    _getFloat(value) {
      let number = 0;
      const processedValue = $._prepareNumber(value);

      if (processedValue) {
        number = parseFloat(processedValue);
        if (isNaN(number)) {
          number = 0;
        }
      }

      return number;
    },

    // Определяет ширину полосы прокрутки
    getScrollBarWidth() {
      // eslint-disable-next-line max-len
      const div = $('<div style="width: 50px; height: 50px; overflow: hidden; position: absolute; top: -200px; left: -200px;"><div style="height: 100px;"></div>');

      $('BODY').append(div);
      const w1 = $('DIV', div).innerWidth();
      div.css('overflow-y', 'scroll');
      const w2 = $('DIV', div).innerWidth();
      $(div).remove();

      return w1 - w2;
    },

    // Parse strings looking for color tuples [255,255,255]
    _getRGB(color) {
      if (!color) {
        return color;
      }

      // Check if we're already dealing with an array of colors
      if (Array.isArray(color) && color.length === 3) {
        return color;
      }

      // Look for rgb(num,num,num)
      let result = /rgb\(\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*,\s*([0-9]{1,3})\s*\)/.exec(color);
      if (result) {
        return [parseInt(result[1], 10), parseInt(result[2], 10), parseInt(result[3], 10)];
      }

      // Look for #a0b1c2
      result = /#([a-fA-F0-9]{2})([a-fA-F0-9]{2})([a-fA-F0-9]{2})/.exec(color);
      if (result) {
        return [parseInt(result[1], 16), parseInt(result[2], 16), parseInt(result[3], 16)];
      }

      // Otherwise, we're most likely dealing with a named color
      return $._getRGB(colors[$.trim(color).toLowerCase()]);
    },

    _getColor(elem, attr) {
      let color;

      // eslint-disable-next-line no-cond-assign
      do {
        color = $.css(elem, attr);

        // Keep going until we find an element that has color, or we hit the body
        if ((color !== '' && color !== 'transparent') || $.nodeName(elem, 'body')) {
          break;
        }

        attr = 'backgroundColor';
      } while (elem = elem.parentNode);

      return $._getRGB(color);
    }
  });

  $.fn.extend({
    // Получает значение верхнего внешнего отступа
    marginTop() {
      return $._getInt($(this).css('margin-top'));
    },

    // Получает значение правого внешнего отступа
    marginRight() {
      return $._getInt($(this).css('margin-right'));
    },

    // Получает значение нижнего внешнего отступа
    marginBottom() {
      return $._getInt($(this).css('margin-bottom'));
    },

    // Получает значение левого внешнего отступа
    marginLeft() {
      return $._getInt($(this).css('margin-left'));
    },

    // Получает ширину верхней рамки
    borderTopWidth() {
      return $._getInt($(this).css('border-top-width'));
    },

    // Получает ширину правой рамки
    borderRightWidth() {
      return $._getInt($(this).css('border-right-width'));
    },

    // Получает ширину нижней рамки
    borderBottomWidth() {
      return $._getInt($(this).css('border-bottom-width'));
    },

    // Получает ширину левой рамки
    borderLeftWidth() {
      return $._getInt($(this).css('border-left-width'));
    },

    // Получает значение верхнего внутреннего отступа
    paddingTop() {
      return $._getInt($(this).css('padding-top'));
    },

    // Получает значение правого внутреннего отступа
    paddingRight() {
      return $._getInt($(this).css('padding-right'));
    },

    // Получает значение нижнего внутреннего отступа
    paddingBottom() {
      return $._getInt($(this).css('padding-bottom'));
    },

    // Получает значение левого внутреннего отступа
    paddingLeft() {
      return $._getInt($(this).css('padding-left'));
    },

    maxZIndex(opt) {
      const def = { inc: 10, group: '*' };
      $.extend(def, opt);
      let zmax = 0;
      $(def.group).each(function () {
        const cur = parseInt($(this).css('z-index'), 10);
        zmax = cur > zmax ? cur : zmax;
      });
      if (!this.jquery) {
        return zmax;
      }

      return this.each(function () {
        zmax += def.inc;
        $(this).css('z-index', zmax);
      });
    },

    // Инициализатор грида
    qpGrid(opt) {
      (function checkGridParams() {
        try {
          if (!opt.columns) {
            console.error('Columns is required');
          }
          if (!opt.dataSource.transport.read.url) {
            console.error('Url is required');
          }
        } catch (e) {
          console.error(e, opt, 'Grid options lack of required keys');
        }
      }());
      return $(this).kendoGrid($.extend(true, {
        noRecords: {
          template: 'No records to display.'
        },
        reorderable: true,
        selectable: true,
        scrollable: false,
        pageable: {
          alwaysVisible: false
        },
        sortable: {
          allowUnsort: true
        },
        autoBind: false,
        dataSource: {
          transport: {
            read: {
              type: 'post',
              dataType: 'json',
              ...opt.dataSource.read
            }
          },
          schema: {
            data: 'data',
            total: 'total'
          },
          pageSize: 20,
          serverPaging: true,
          serverSorting: true
        }
      }, opt)).addClass(`qpGrid ${opt.class}`);
    }
  });

  // Переопределяем анимацию для всех цветовых стилей
  $.each([
    'backgroundColor', 'borderBottomColor', 'borderLeftColor',
    'borderRightColor', 'borderTopColor', 'color', 'outlineColor'
  ], (i, attr) => {
    $.fx.step[attr] = function (fx) {
      if (fx.state === 0 || typeof fx.end === typeof '') {
        fx.start = $._getColor(fx.elem, attr);
        fx.end = $._getRGB(fx.end);
      }

      fx.elem.style[attr] = ['rgb(', [
        Math.max(Math.min(parseInt((fx.pos * (fx.end[0] - fx.start[0])) + fx.start[0], 10), 255), 0),
        Math.max(Math.min(parseInt((fx.pos * (fx.end[1] - fx.start[1])) + fx.start[1], 10), 255), 0),
        Math.max(Math.min(parseInt((fx.pos * (fx.end[2] - fx.start[2])) + fx.start[2], 10), 255), 0)
      ].join(','), ')'].join('');
    };
  });

  // Добавляем в jQuery возможность проверки поддержки CSS-свойства border-radius (взято из библиотеки Modernizr 2.0)
  const mod = 'modernizr';
  const modElem = document.createElement(mod);
  const mStyle = modElem.style;
  const domPrefixes = 'Webkit Moz O ms Khtml'.split(' ');

  const testProps = (props, prefixed) => {
    // eslint-disable-next-line no-restricted-syntax
    for (const i in props) {
      if (mStyle[props[i]] !== undefined) {
        return prefixed === 'pfx' ? props[i] : true;
      }
    }
    return false;
  };

  const testPropsAll = (prop, prefixed) => {
    const ucProp = prop.charAt(0).toUpperCase() + prop.substr(1);
    const props = `${prop} ${domPrefixes.join(`${ucProp} `)}${ucProp}`.split(' ');

    return testProps(props, prefixed);
  };

  $.support.borderRadius = testPropsAll('borderRadius');
}(jQuery));
