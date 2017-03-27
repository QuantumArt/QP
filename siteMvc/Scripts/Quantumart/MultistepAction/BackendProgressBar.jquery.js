// based on: http://benogle.com/2009/06/16/simple-css-shiny-progress-bar-technique.html
// eslint-disable-next-line no-extra-semi
; (function module() {
  var methods;
  var BackendProgressBarComponent = function BackendProgressBarComponent($we, options) {
    var $wrapElement = $we;
    var settings = Object.assign({
      total: 0,
      value: 0,
      digits: 1
    }, options);

    var progressbarInnerHtml = '<div class="progressbar-value"><div class="progressbar-text">0%</div></div>';
    var currentValue = settings.value;
    var currentWidth = 0;

    var setValue = function setValue(val) {
      var result = val;
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

    var getValue = function getValue() {
      return currentValue;
    };

    var setTotal = function setTotal(val, dfr) {
      var result = val;
      if ($q.isNull(result)) {
        result = settings.total();
      }

      if ($.isNumeric(result)) {
        settings.total = result;
        setValue(result, dfr);
      }
    };

    var getTotal = function getTotal() {
      return settings.total;
    };

    var increment = function increment(val) {
      if ($.isNumeric(val)) {
        setValue(getValue() + val);
        return getValue();
      }

      return undefined;
    };

    var decriment = function decriment(val) {
      if ($.isNumeric(val)) {
        setValue(getValue() - val);
        return getValue();
      }

      return undefined;
    };

    var setText = function setText(val) {
      $wrapElement.find('.progressbar-text').text(val);
    };

    var refresh = function refresh() {
      $wrapElement.find('.progressbar-value').css({ width: $q.toFixed(currentWidth, settings.digits) + '%' });
      setText($q.toFixed(currentWidth, settings.digits) + '%');
    };

    var setColor = function setColor(color) {
      $wrapElement.css('background-color', color);
      $wrapElement.find('.progressbar-value').css('background-color', color);
    };

    var dispose = function dispose() {
      $wrapElement = null;
    };

    // Инициализация
    $wrapElement.addClass('progressbar-wrap');
    $wrapElement.html(progressbarInnerHtml);
    setValue(settings.value);

    return {
      value: function (val) {
        if ($.isNumeric(val)) {
          return setValue(val);
        }

        return getValue();
      },

      total: function (val) {
        if ($.isNumeric(val)) {
          return setTotal(val);
        }

        return getTotal();
      },

      increment: increment,
      decriment: decriment,

      refresh: refresh,
      setText: setText,
      setColor: setColor,

      dispose: dispose
    };
  };

  methods = {
    init: function (options) {
      return this.filter('div').each(function each() {
        var $this = $(this);
        var component = new BackendProgressBarComponent($this, options);
        $this.data('backendProgressBar', component);
        $this = null;
        component = null;
      });
    },

    dispose: function () {
      return this.each(function each() {
        var $this = $(this);
        var component = $this.data('backendProgressBar');
        component.dispose();
        component = null;
        $this.removeData('backendProgressBar');
      });
    }
  };

  $.fn.backendProgressBar = function backendProgressBar(method) {
    if (methods[method]) {
      return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
    } else if (typeof method === 'object' || !method) {
      return methods.init.apply(this, arguments);
    }

    $.error('Method ' + method + ' does not exist on backendProgressBar');
    return this;
  };
}());
