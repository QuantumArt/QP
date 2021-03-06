(function ($) {
  // Константы
  var VERTICAL_RESIZER_COMPONENT_NAME = "vertical_resizer"; // название компонента

  // Компонент VerticalResizer
  VerticalResizer = function (element, options) {

    this._onBottomHandleMouseDownHandler = jQuery.proxy(this._onBottomHandleMouseDown, this);
    this._onDocumentMouseMoveHandler = jQuery.proxy(this._onDocumentMouseMove, this);
    this._onDocumentMouseUpHandler = jQuery.proxy(this._onDocumentMouseUp, this);

    this.initialize(element, options);
  };

  VerticalResizer.prototype = {
    _panelElement: null, // DOM-элемент, образующий панель
    _bottomHandleElement: null, // DOM-элемент, образующий нижнюю рукоятку для перетаскивания
    _heightOffset: 0, // величина отклонения высоты панели
    _lastMousePositionTop: 0, // последняя вертикальная позиция курсора мыши
    _options: null, // настройки компонента

    _onBottomHandleMouseDownHandler: null,
    _onDocumentMouseMoveHandler: null,
    _onDocumentMouseUpHandler: null,

    // Возвращает DOM-элемент, образующий нижнюю рукоятку для перетаскивания
    get_bottomHandleElement: function () {
      return this._bottomHandleElement;
    },

    // Инициализирует компонент
    initialize: function (element, options) {
      this._options = $.extend({}, $.fn.verticalResizer.defaults, options);

      var bottomHandleCssClassName = this._options.bottomHandleCssClassName;

      var $panel = $(element);
      $panel
        .wrap("<div></div>")
        .parent()
        .append($('<div class="' + bottomHandleCssClassName + '" style="display:none"></div>'))
        ;

      var $bottomHandle = $("." + bottomHandleCssClassName, $panel.parent());
      if (this._options.allowFixBottomHandleWidth) {
        $bottomHandle.css("margin-right", ($bottomHandle.outerWidth() - $panel.outerWidth()) + "px");
      }
      $bottomHandle.bind("mousedown", this._onBottomHandleMouseDownHandler);

      this._panelElement = $panel.get(0);
      this._bottomHandleElement = $bottomHandle.get(0);

      $panel = null;
      $bottomHandle = null;
    },

    // Запускает перетаскивание
    _startDrag: function (mousePositionTop) {
      this._lastMousePositionTop = mousePositionTop;
      this._heightOffset = $(this._panelElement).height() - this._lastMousePositionTop;

      var $doc = $(document);
      this._disableTextSelect($doc);
      $doc
        .mousemove(this._onDocumentMouseMoveHandler)
        .mouseup(this._onDocumentMouseUpHandler)
        ;

      $doc = null;

      jQuery(this._bottomHandleElement).addClass("active");

      return false;
    },

    // Возвращает новую высоту панели
    _getNewPanelHeight: function (mousePositionTop) {
      var newPanelHeight = this._heightOffset + mousePositionTop;
      if (this._lastMousePositionTop >= mousePositionTop) {
        newPanelHeight -= 5;
      }
      this._lastMousePositionTop = mousePositionTop;

      return newPanelHeight;
    },

    // Возвращает координаты курсора мыши
    _getMousePosition: function (e) {
      var docElem = document.documentElement;

      return { x: e.clientX + docElem.scrollLeft, y: e.clientY + docElem.scrollTop };
    },

    // Останавливает перетаскивание
    _endDrag: function () {
      var $doc = $(document);
      this._enableTextSelect($doc);
      $doc
        .unbind("mousemove", this._onDocumentMouseMoveHandler)
        .unbind("mouseup", this._onDocumentMouseUpHandler)
        ;

      $doc = null;

      jQuery(this._bottomHandleElement).removeClass("active");

      this._heightOffset = null;
      this._lastMousePositionTop = 0;
    },

    // Отменяет поведение события по умолчанию
    _cancelDefaultFunction: function (e) {
      return false;
    },

    // Запрещает выделение текста в заданном элементе
    _disableTextSelect: function ($elem) {
      if ($.browser.msie) {
        $elem.bind("selectstart", this._cancelDefaultFunction);
      } else {
        $elem.bind("mousedown", this._cancelDefaultFunction);
      }
    },

    // Разрешает выделение текста в заданном элементе
    _enableTextSelect: function ($elem) {
      if ($.browser.msie) {
        $elem.unbind("selectstart", this._cancelDefaultFunction);
      } else {
        $elem.unbind("mousedown", this._cancelDefaultFunction);
      }
    },

    // Показывает контейнер, в котором находится панель
    showPanelWrapper: function (callback) {
      $(this._panelElement).parent().show();
      if(callback) {
        $q.callFunction(callback);
      }
    },

    hidePanelWrapper: function (callback) {
      $(this._panelElement).parent();
      if(callback) {
        $q.callFunction(callback);
      }
    },

    showBottomHandle: function (callback) {
      $(this._bottomHandleElement).show();
      if(callback) {
        $q.callFunction(callback);
      }
    },

    hideBottomHandle: function (callback) {
      $(this._bottomHandleElement).hide();
      if(callback) {
        $q.callFunction(callback);
      }
    },

    _onBottomHandleMouseDown: function (e) {
      var mousePositionTop = this._getMousePosition(e).y;
      this._startDrag(mousePositionTop);
    },

    _onDocumentMouseMove: function (e) {
      var $panel = jQuery(this._panelElement);
      var mousePositionTop = this._getMousePosition(e).y;
      var newPanelHeight = this._getNewPanelHeight(mousePositionTop);
      var minPanelHeight = this._options.minPanelHeight;
      var maxPanelHeight = this._options.maxPanelHeight;

      if (newPanelHeight < minPanelHeight) {
        newPanelHeight = minPanelHeight;
      } else if (newPanelHeight > maxPanelHeight) {
        newPanelHeight = maxPanelHeight;
      }

      $panel.height(newPanelHeight).trigger("resize");
      return false;
    },

    _onDocumentMouseUp: function (e) {
      this._endDrag();
    },

    // Уничтожает компонент
    dispose: function () {
      var $doc = $(document);
      this._enableTextSelect($doc);
      $doc.unbind("mousemove", this._onDocumentMouseMoveHandler).unbind("mouseup", this._onDocumentMouseUpHandler);

      $(this._bottomHandleElement).unbind("mousedown", this._onBottomHandleMouseDownHandler).hide().empty().remove();
      $(this._panelElement).unwrap();

      this._panelElement = null;
      this._bottomHandleElement = null;
      this._options = null;

      this._onBottomHandleMouseDownHandler = null;
      this._onDocumentMouseMoveHandler = null;
      this._onDocumentMouseUpHandler = null;
    }
  };

  // Инициализирует плагин Vertical Resizer
  $.fn.verticalResizer = function (options) {
    return this.each(function () {
      var $panel = $(this);
      if (!$panel.data(VERTICAL_RESIZER_COMPONENT_NAME)) {
        $panel.data(VERTICAL_RESIZER_COMPONENT_NAME, new VerticalResizer(this, options));
      }
    });
  };

  // Уничтожает плагин Vertical Resizer
  $.fn.noVerticalResizer = function (options) {
    return this.each(function () {
      var $panel = $(this);
      var component = $panel.data(VERTICAL_RESIZER_COMPONENT_NAME);
      if (component) {
        component.dispose();
        $panel.removeData(VERTICAL_RESIZER_COMPONENT_NAME);
      }
    });
  };

  // Настройки по умолчанию
  $.fn.verticalResizer.defaults = {
    bottomHandleCssClassName: "bottomHandle", // имя CSS-класса для нижней рукояти
    minPanelHeight: 100, // минимальная высота панели
    maxPanelHeight: 400, // максимальная высота панели
    allowFixBottomHandleWidth: false // признак, разрешающий корректировку ширины нижней рукояти
  };
})(jQuery);
