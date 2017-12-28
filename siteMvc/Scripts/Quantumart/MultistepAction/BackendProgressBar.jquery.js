import { $q } from '../Utils';

// based on: http://benogle.com/2009/06/16/simple-css-shiny-progress-bar-technique.html
// eslint-disable-next-line no-extra-semi
; (function module() {
  const BackendProgressBarComponent = function BackendProgressBarComponent($we, options) {
    let $wrapElement = $we;

    /** @type {{total: number, value: number, digits: number }} */
    const settings = Object.assign({
      total: 0,
      value: 0,
      digits: 1
    }, options);

    const progressbarInnerHtml = '<div class="progressbar-value"><div class="progressbar-text">0%</div></div>';
    let currentValue = settings.value;
    let currentWidth = 0;

    const setValue = function setValue(val) {
      let result = val;
      if ($q.isNull(result)) {
        result = currentValue;
      }

      if ($.isNumeric(result)) {
        currentValue = result;
        currentWidth = 0;
        if (settings.total > 0) {
          currentWidth = result * 100 / settings.total;
        } else if (settings.total === 0) {
          currentWidth = 100;
        }
        if (currentWidth < 0) {
          currentValue = 0;
          currentWidth = 0;
        } else if (currentWidth > 100) {
          currentValue = settings.total;
          currentWidth = 100;
        }
      }
    };

    const getValue = function getValue() {
      return currentValue;
    };

    const setTotal = function setTotal(val) {
      let result = val;
      if ($q.isNull(result)) {
        result = settings.total;
      }

      if ($.isNumeric(result)) {
        settings.total = result;
        setValue(result);
      }
    };

    const getTotal = function getTotal() {
      return settings.total;
    };

    const increment = function increment(val) {
      if ($.isNumeric(val)) {
        setValue(getValue() + val);
        return getValue();
      }

      return undefined;
    };

    const decriment = function decriment(val) {
      if ($.isNumeric(val)) {
        setValue(getValue() - val);
        return getValue();
      }

      return undefined;
    };

    const setText = function setText(val) {
      $wrapElement.find('.progressbar-text').text(val);
    };

    const refresh = function refresh() {
      $wrapElement.find('.progressbar-value').css({ width: `${$q.toFixed(currentWidth, settings.digits)}%` });
      setText(`${$q.toFixed(currentWidth, settings.digits)}%`);
    };

    const setColor = function setColor(color) {
      $wrapElement.css('background-color', color);
      $wrapElement.find('.progressbar-value').css('background-color', color);
    };

    const dispose = function dispose() {
      $wrapElement = null;
    };

    $wrapElement.addClass('progressbar-wrap');
    $wrapElement.html(progressbarInnerHtml);
    setValue(settings.value);

    return {
      value(val) {
        if ($.isNumeric(val)) {
          return setValue(val);
        }

        return getValue();
      },

      total(val) {
        if ($.isNumeric(val)) {
          return setTotal(val);
        }

        return getTotal();
      },

      increment,
      decriment,

      refresh,
      setText,
      setColor,

      dispose
    };
  };

  const methods = {
    init(options) {
      return this.filter('div').each(function each() {
        let $this = $(this);
        // eslint-disable-next-line new-cap
        let component = BackendProgressBarComponent($this, options);
        $this.data('backendProgressBar', component);
        $this = null;
        component = null;
      });
    },

    dispose() {
      return this.each(function each() {
        const $this = $(this);
        let component = $this.data('backendProgressBar');
        component.dispose();
        component = null;
        $this.removeData('backendProgressBar');
      });
    }
  };

  $.fn.backendProgressBar = function backendProgressBar(method, ...params) {
    if (methods[method]) {
      return methods[method].apply(this, params);
    } else if (typeof method === 'object' || !method) {
      return methods.init.apply(this, [method, ...params]);
    }

    $.error(`Method ${method} does not exist on backendProgressBar`);
    return this;
  };
}());
