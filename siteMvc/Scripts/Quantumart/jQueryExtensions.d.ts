/// <reference path="../kendo/kendo.all.d.ts" />

interface JQueryStatic {
  /** Подготавливает значение к преобразованию в число */
  _prepareNumber(value: any): string;

  /** Получает из значения целое число */
  _getInt(value: any): number;

  /** Получает из значения число двойной точности */
  _getFloat(value: any): number;

  /** Определяет ширину полосы прокрутки */
  getScrollBarWidth(): number;

  /** Parse strings looking for color tuples [255,255,255] */
  _getRGB(color: any): [number, number, number];

  _getColor(elem: any, attr: string): [number, number, number];
}

interface JQuery {
  /** Получает значение верхнего внешнего отступа */
  marginTop(): number;

  /** Получает значение правого внешнего отступа */
  marginRight(): number;

  /** Получает значение нижнего внешнего отступа */
  marginBottom(): number;

  /** Получает значение левого внешнего отступа */
  marginLeft(): number;

  /** Получает ширину верхней рамки */
  borderTopWidth(): number;

  /** Получает ширину правой рамки */
  borderRightWidth(): number;

  /** Получает ширину нижней рамки */
  borderBottomWidth(): number;

  /** Получает ширину левой рамки */
  borderLeftWidth(): number;

  /** Получает значение верхнего внутреннего отступа */
  paddingTop(): number;

  /** Получает значение правого внутреннего отступа */
  paddingRight(): number;

  /** Получает значение нижнего внутреннего отступа */
  paddingBottom(): number;

  /** Получает значение левого внутреннего отступа */
  paddingLeft(): number;

  /**
   * Returns the max zOrder in the document (no parameter)
   * Sets max zOrder by passing a non-zero number
   */
  maxZIndex(opt?: { inc?: number, group?: string }): JQuery;

  /**
   * Preconfigured @see .kendoGrid()
   * https://demos.telerik.com/kendo-ui/grid/index
   * https://docs.telerik.com/kendo-ui/framework/templates/overview
   */
  qpGrid(options: kendo.ui.GridOptions): JQuery;
}

interface JQuerySupport {
  /**
   * Добавляем в jQuery возможность проверки поддержки
   * CSS-свойства border-radius (взято из библиотеки Modernizr 2.0)
   */
  borderRadius: boolean;
}

declare namespace kendo {
  namespace data {
    interface DataSource {
      [x: string]: any;
    }
    interface ObservableObject {
      [x: string]: any;
    }
  }

  namespace ui {
    interface Grid {
      [x: string]: any;
    }
  }
}
