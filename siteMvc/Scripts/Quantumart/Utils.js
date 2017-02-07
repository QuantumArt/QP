window.$q = Quantumart.QP8.Utils = function() {};

$q.trace = function() {
  if (window.Sys.Debug.isDebug) {
    var args = [].slice.call(arguments);
    var firstArg = args.slice(0, 1)[0];
    var otherArgs = args.slice(1);

    if ($.isFunction(window.console.groupCollapsed)
      && $.isFunction(window.console.groupEnd)
      && $.isFunction(window.console.trace)) {
      window.console.groupCollapsed(firstArg);
      window.console.log.apply(window.console, otherArgs);
      window.console.trace('%cView tracing', 'color: darkblue;font-weight:bold;');
      window.console.groupEnd(firstArg);
    } else {
      window.console.log.apply(window.console, args);
    }
  }
};

$q.alertSuccess = function(msg) {
  window.alert(msg);
  if (window.Sys.Debug.isDebug) {
    window.console.log.apply(window.console, Array.prototype.slice.call(arguments));
  }
};

$q.alertFail = function(msg) {
  window.alert(msg);
  if (window.Sys.Debug.isDebug) {
    window.console.warn.apply(window.console, Array.prototype.slice.call(arguments));
  }
};

$q.alertError = function(msg) {
  window.alert(msg);
  $q.trace(msg);
};

/**
 * Basic implementation of jQuery ajax request with JSend response support
 * @param  {Object} opts jQuery options for ajax request
 * @return {Object}      jQuery XHR deffered
 */
$q.sendAjax = function(opts) {
  var defaultOptions = {
    type: 'GET',
    dataType: 'json',
    contentType: 'application/json; charset=utf-8',
    async: true,
    cache: false,
    traditional: true
  };

  var options = Object.assign({}, defaultOptions, opts);
  if (options.dataType.toLowerCase() === 'jsonp' && !options.async) {
    window.console.error('Sync requests cannot be combined with jsonp');
  }

  var maxLogDataLengthToLog = 300;
  var logData = JSON.stringify(options.data || {});
  var logDataLength = logData.length;
  var cuttedLogData = logDataLength > maxLogDataLengthToLog
    ? logData.slice(0, maxLogDataLengthToLog) + '..'
    : logData;

  var debugMessage = 'ajax: ' + options.type + ' ' + options.url + '. Data: ' + cuttedLogData;
  $q.trace('Sending ' + debugMessage, 'Request object: ', options);
  return $.ajax(options).done(function(response) {
    $q.trace('Parsing ' + debugMessage, 'Response object: ', response);
    if (response.status.toUpperCase() === 'SUCCESS') {
      if(options.jsendSuccess) {
        options.jsendSuccess(response.data, response);
      }
    } else if (response.status.toUpperCase() === 'FAIL') {
      if(options.jsendFail) {
        options.jsendFail(response.data, response);
      } else {
        $q.alertFail(response.message || 'There was an errors at request');
      }
    } else {
      if(options.jsendError) {
        options.jsendError(response.data, response);
      } else {
        $q.alertError(response.message || 'Unknown server error');
      }
    }
  }).done(function() {
    $q.hideLoader();
  });
};

$q.getAjax = function(url, data, jsendSuccess, jsendFail, jsendError) {
  return $q.sendAjax({
    url: url,
    data: data,
    jsendSuccess: jsendSuccess,
    jsendFail: jsendFail,
    jsendError: jsendError
  });
};

$q.postAjax = function(url, data, jsendSuccess, jsendFail, jsendError) {
  return $q.sendAjax({
    url: url,
    type: 'POST',
    data: JSON.stringify(data),
    jsendSuccess: jsendSuccess,
    jsendFail: jsendFail,
    jsendError: jsendError
  });
};

$q.showLoader = function() {
  if ($ctx) {
    $ctx.getArea().showAjaxLoadingLayer();
  }
}

$q.hideLoader = function() {
  if ($ctx) {
    $ctx.getArea().hideAjaxLoadingLayer();
  }
}

$q.isBoolean = function Quantumart$QP8$Utils$isBoolean(value) {
  return typeof value === 'boolean' || value && ['true', 'false'].includes(value.toString().toLowerCase().trim());
};

$q.toBoolean = function Quantumart$QP8$Utils$toBoolean(value, defaultValue) {
  if ($q.isBoolean(value)) {
    return value.toString().toLowerCase().trim() === 'true';
  }

  if ($q.isBoolean(defaultValue)) {
    return defaultValue.toString().toLowerCase().trim() === 'true';
  }

  return false;
};

Quantumart.QP8.Utils.isNull = function Quantumart$QP8$Utils$isNull(value) {
  /// <summary>
  /// Проверяет переменную на Null-значения
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <returns type="Boolean">результат проверки (true - Null; false - не Null)</returns>
  var result = false;
  if (value == undefined || value == null || typeof (value) == 'undefined') {
    result = true;
  }

  return result;
};

Quantumart.QP8.Utils.isNullOrEmpty = function Quantumart$QP8$Utils$isNullOrEmpty(value) {
  /// <summary>
  /// Проверяет переменную на пустые значения
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <returns type="Boolean">результат проверки (true - пусто; false - не пусто)</returns>
  var result = false;

  if (Quantumart.QP8.Utils.isNull(value)) {
    result = true;
  } else {
    if (Quantumart.QP8.Utils.isArray(value) || (Quantumart.QP8.Utils.isObject(value) && value.jquery)) {
      if (value.length == 0) {
        result = true;
      }
    } else {
      if (value.toString().length == 0) {
        result = true;
      }
    }
  }

  return result;
};

Quantumart.QP8.Utils.isNullOrWhiteSpace = function Quantumart$QP8$Utils$isNullOrWhiteSpace(value) {
  /// <summary>
  /// Проверяет переменную на пустые значения
  // с предварительным удалением оконечных пробелов
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <returns type="Boolean">результат проверки (true - пусто; false - не пусто)</returns>
  var result = false;

  if (Quantumart.QP8.Utils.isNull(value)) {
    result = true;
  } else {
    if (Quantumart.QP8.Utils.isArray(value) || (Quantumart.QP8.Utils.isObject(value) && value.jquery)) {
      if (value.length == 0) {
        result = true;
      }
    } else {
      if (value.toString().trim().length == 0) {
        result = true;
      }
    }
  }

  return result;
};

Quantumart.QP8.Utils.toString = function Quantumart$QP8$Utils$toString(value, defaultValue) {
  /// <summary>
  /// Преобразует значение в строку
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <param name="defaultValue" type="String">значение по умолчанию</param>
  var string = null;

  if (Quantumart.QP8.Utils.isNull(defaultValue)) {
    defaultValue = null;
  }

  if (!Quantumart.QP8.Utils.isNull(value)) {
    string = value.toString();
  } else {
    string = defaultValue;
  }

  return string;
};

Quantumart.QP8.Utils.isString = function Quantumart$QP8$Utils$isString(value) {
  /// <summary>
  /// Проверяет является ли значение строкой
  /// </summary>
  /// <param name="value" type="String">значение</param>
  var result = false;

  if (!Quantumart.QP8.Utils.isNull(value)) {
    result = (typeof (value.valueOf()) == 'string');
  }

  return result;
};

Quantumart.QP8.Utils._prepareNumber = function Quantumart$QP8$Utils$_prepareNumber(value, forCheck) {
  /// <summary>
  /// Подготавливает значение к преобразованию в число
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <param name="forCheck" type="Boolean">признак, указывающий что преобразование используется при проверки типа</param>
  var number = null;

  if (!Quantumart.QP8.Utils.isNullOrEmpty(value)) {
    var processedValue = value.toString()
    .replace(/\s/igm, '')
    .replace(',', '.')
    .toLowerCase();

    if (!forCheck) {
      if (processedValue == 'true') {
        number = 1;
        return number;
      } else if (processedValue == 'false') {
        number = 0;
        return number;
      }
    }

    number = Number.parseLocale(processedValue);
  }

  return number;
};

Quantumart.QP8.Utils.toInt = function Quantumart$QP8$Utils$toInt(value, defaultValue) {
  /// <summary>
  /// Преобразует значение в целое число
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <param name="defaultValue" type="Number" integer="true">значение по умолчанию</param>
  if (Quantumart.QP8.Utils.isNullOrEmpty(defaultValue)) {
    defaultValue = null;
  }

  var number = null;

  if (!Quantumart.QP8.Utils.isNullOrEmpty(value)) {
    number = Quantumart.QP8.Utils._prepareNumber(value, false);
    if (!Quantumart.QP8.Utils.isNullOrEmpty(number)) {
      number = parseInt(number, 10);
      if (isNaN(number)) {
        number = null;
      }
    }
  }

  if (number == null) {
    number = defaultValue;
  }

  return number;
};

Quantumart.QP8.Utils.isInt = function Quantumart$QP8$Utils$isInt(value) {
  /// <summary>
  /// Проверяет является ли значение целым числом
  /// </summary>
  /// <param name="value" type="String">значение</param>
  var result = false;
  var number = Quantumart.QP8.Utils._prepareNumber(value, true);

  if (!Quantumart.QP8.Utils.isNullOrEmpty(number)) {
    if (!isNaN(number)) {
      if (number.toString().indexOf('.') == -1) {
        result = true;
      }
    }
  }

  return result;
};

Quantumart.QP8.Utils.toFloat = function Quantumart$QP8$Utils$toFloat(value, defaultValue) {
  /// <summary>
  /// Преобразует значение в число двойной точности
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <param name="defaultValue" type="Number">значение по умолчанию</param>
  if (Quantumart.QP8.Utils.isNullOrEmpty(defaultValue)) {
    defaultValue = null;
  }

  var number = null;

  if (!Quantumart.QP8.Utils.isNullOrEmpty(value)) {
    number = Quantumart.QP8.Utils._prepareNumber(value, false);
    if (!Quantumart.QP8.Utils.isNullOrEmpty(number)) {
      number = parseFloat(number);
      if (isNaN(number)) {
        number = null;
      }
    }
  }

  if (number == null) {
    number = defaultValue;
  }

  return number;
};

Quantumart.QP8.Utils.isFloat = function Quantumart$QP8$Utils$isFloat(value) {
  /// <summary>
  /// Проверяет является ли значение числом двойной точности
  /// </summary>
  /// <param name="value" type="String">значение</param>
  var result = false;
  var number = Quantumart.QP8.Utils._prepareNumber(value, true);

  if (!Quantumart.QP8.Utils.isNullOrEmpty(number)) {
    if (!isNaN(number)) {
      result = true;
    }
  }

  return result;
};

Quantumart.QP8.Utils.toDate = function Quantumart$QP8$Utils$toDate(value, defaultValue) {
  /// <summary>
  /// Проверяет значение в дату
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <param name="defaultValue" type="Date">значение по умолчанию</param>

  if (Quantumart.QP8.Utils.isNullOrEmpty(defaultValue)) {
    defaultValue = null;
  }

  var date = null;

  if (!Quantumart.QP8.Utils.isNullOrEmpty(value)) {
    date = Date.parseLocale(value);
  }

  if (date == null) {
    date = defaultValue;
  }

  return date;
};

Quantumart.QP8.Utils.isDate = function Quantumart$QP8$Utils$isDate(value) {
  /// <summary>
  /// Проверяет является ли значение датой
  /// </summary>
  /// <param name="value" type="String">значение</param>
  var result = false;

  if (!Quantumart.QP8.Utils.isNullOrEmpty(value)) {
    date = Date.parseLocale(value);
    if (date != null) {
      result = true;
    }
  }

  return result;
};

Quantumart.QP8.Utils.isArray = function Quantumart$QP8$Utils$isArray(value) {
    /// <summary>
    /// Проверяет является ли значение массивом
    /// </summary>
    /// <param name="value" type="Object">значение</param>
  var result = false;

  if (!Quantumart.QP8.Utils.isNull(value)) {
    result = (typeof (value) == 'array' || (typeof (value) == 'object' && value.join));
  }

  return result;
};

Quantumart.QP8.Utils.isObject = function Quantumart$QP8$Utils$isObject(value) {
    /// <summary>
    /// Проверяет является ли значение объектом
    /// </summary>
    /// <param name="value" type="Object">значение</param>
  var result = false;

  if (!Quantumart.QP8.Utils.isNull(value)) {
    result = (typeof (value) == 'object');
  }

  return result;
};

Quantumart.QP8.Utils.isFunction = function Quantumart$QP8$Utils$isFunction(value) {
  /// <summary>
  /// Проверяет является ли значение функцией
  /// </summary>
  /// <param name="value" type="Object">значение</param>
  var result = false;

  if (!Quantumart.QP8.Utils.isNull(value)) {
    result = (typeof (value.valueOf()) == 'function');
  }

  return result;
};

Quantumart.QP8.Utils.toJQuery = function Quantumart$QP8$Utils$toJQuery(value) {
  /// <summary>
  /// Преобразует DOM-элемент или массив DOM-элементов в объект jQuery
  /// </summary>
  /// <param name="value">значение</param>
  /// <returns type="Object">объект jQuery</returns>
  var result = null;

  if (Quantumart.QP8.Utils.isObject(value)) {
    if (!value.jquery) {
      result = jQuery(value);
    } else {
      result = value;
    }
  } else if (Quantumart.QP8.Utils.isArray(value)) {
    result = jQuery(value);
  }

  return result;
};

Quantumart.QP8.Utils.isJQuery = function Quantumart$QP8$Utils$isJQuery(value) {
  /// <summary>
  /// Проверяет является ли значение jQuery-объектом
  /// </summary>
  /// <param name="value">значение</param>
  /// <returns type="Boolean">результат проверки (true - jQuery; false - не jQuery)</returns>
  var result = (Quantumart.QP8.Utils.isObject(value) && value.jquery);

  return result;
};

Quantumart.QP8.Utils.changeType = function Quantumart$QP8$Utils$changeType(value, typeCode) {
  /// <summary>
  /// Преобразует значение к указанному типу данных
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <param name="typeCode" type="String">код типа данных в JavaScript</param>
  /// <returns>преобразованное значение</returns>
  var convertedValue = value;

  if (typeCode == JS_TYPE_CODE_STRING) {
    convertedValue = Quantumart.QP8.Utils.toString(value);
  } else if (typeCode == JS_TYPE_CODE_INT) {
    convertedValue = Quantumart.QP8.Utils.toInt(value);
  } else if (typeCode == JS_TYPE_CODE_FLOAT) {
    convertedValue = Quantumart.QP8.Utils.toFloat(value);
  } else if (typeCode == JS_TYPE_CODE_BOOLEAN) {
    convertedValue = Quantumart.QP8.Utils.toBoolean(value);
  } else if (typeCode == JS_TYPE_CODE_DATE) {
    convertedValue = Quantumart.QP8.Utils.toDate(value);
  }

  return convertedValue;
};

Quantumart.QP8.Utils.isInRange = function Quantumart$QP8$Utils$isInRange(value, lowerBound, lowerBoundType, upperBound, upperBoundType) {
  /// <summary>
  /// Проверяет попадание значения в диапазон
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <param name="lowerBound" type="Number" integer="true">нижняя границы</param>
  /// <param name="lowerBoundType" type="Quantumart.QP8.Enums.RangeBoundaryType">тип вхожения в нижнюю границу</param>
  /// <param name="upperBound" type="Number" integer="true">верхняя граница</param>
  /// <param name="upperBoundType" type="Quantumart.QP8.Enums.RangeBoundaryType">тип вхождения в верхнюю границу</param>
  /// <returns type="Boolean">результат проверки (true - входит; false - не входит)</returns>
  if (lowerBoundType == Quantumart.QP8.Enums.RangeBoundaryType.Inclusive) {
    if (value < lowerBound) {
      return false;
    }
  } else if (lowerBoundType == Quantumart.QP8.Enums.RangeBoundaryType.Exclusive) {
    if (value <= lowerBound) {
      return false;
    }
  }

  if (upperBoundType == Quantumart.QP8.Enums.RangeBoundaryType.Inclusive) {
    if (value > upperBound) {
      return false;
    }
  } else if (upperBoundType == Quantumart.QP8.Enums.RangeBoundaryType.Exclusive) {
    if (value >= upperBound) {
      return false;
    }
  }

  return true;
};

Quantumart.QP8.Utils.compareValues = function Quantumart$QP8$Utils$compareValues(firstValue, secondValue, comparisonOperator) {
  /// <summary>
  /// Сравнивает два значения
  /// </summary>
  /// <param name="firstValue" type="String">первое значение</param>
  /// <param name="secondValue" type="String">второе значение</param>
  /// <param name="comparisonOperator" type="Quantumart.QP8.Enums.ComparisonOperator">оператор сравнения</param>
  var result = false;

  if (comparisonOperator == Quantumart.QP8.Enums.ComparisonOperator.Equal) {
    result = (firstValue == secondValue);
  } else if (comparisonOperator == Quantumart.QP8.Enums.ComparisonOperator.NotEqual) {
    result = (firstValue != secondValue);
  } else if (comparisonOperator == Quantumart.QP8.Enums.ComparisonOperator.GreaterThan) {
    result = (firstValue > secondValue);
  } else if (comparisonOperator == Quantumart.QP8.Enums.ComparisonOperator.GreaterThanEqual) {
    result = (firstValue >= secondValue);
  } else if (comparisonOperator == Quantumart.QP8.Enums.ComparisonOperator.LessThan) {
    result = (firstValue < secondValue);
  } else if (comparisonOperator == Quantumart.QP8.Enums.ComparisonOperator.LessThanEqual) {
    result = (firstValue <= secondValue);
  }

  return result;
};

Quantumart.QP8.Utils.getJsonFromUrl = function Quantumart$QP8$Utils$getJsonFromUrl(method, url, params, async, allowCaching, callbackSuccess, callbackError) {
  /// <summary>
  /// Получает JSON-контент, который расположен по указанному URL
  /// </summary>
  /// <param name="method" type="String">HTTP-метод (GET или POST)</param>
  /// <param name="url" type="String">URL веб-сервиса</param>
  /// <param name="params" type="Object">параметры, передаваемые методу веб-сервиса</param>
  /// <param name="async" type="Boolean">признак, асинхронного запроса</param>
  /// <param name="allowCaching" type="Boolean">признак, разрешающий кэширование запроса</param>
  /// <param name="callbackSuccess" type="Function">обработчик успешного вызова веб-сервиса</param>
  /// <param name="callbackError" type="Function">обработчик ошибок</param>
  var methodName = method.toUpperCase();
  var data = params;

  if (methodName.toUpperCase() === 'POST') {
    data = JSON.stringify(params);
  }

  var settings = {
    type: methodName,
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    url: url,
    data: data,
    async: async,
    cache: allowCaching,
    error: callbackError || $q.processGenericAjaxError
  };

  settings.success = $q.ajaxCallbackDecorator(callbackSuccess, settings);
  var result = jQuery.ajax(settings);

  return $q.decorateDeferred(result, $q.ajaxCallbackDecorator, settings);
};

Quantumart.QP8.Utils.getJsonPFromUrl = function Quantumart$QP8$Utils$getJsonPFromUrl(method, url, params, allowCaching, timeout, callbackSuccess, callbackError) {
  /// <summary>
  /// Получает JSON-контент, который расположен по указанному URL
  /// </summary>
  /// <param name="method" type="String">HTTP-метод (GET или POST)</param>
  /// <param name="url" type="String">URL веб-сервиса</param>
  /// <param name="params" type="Object">параметры, передаваемые методу веб-сервиса</param>
  /// <param name="allowCaching" type="Boolean">признак, разрешающий кэширование запроса</param>
  /// <param name="callbackSuccess" type="Function">обработчик успешного вызова веб-сервиса</param>
  /// <param name="callbackError" type="Function">обработчик ошибок</param>
  var methodName = method.toUpperCase();
  var data = params;

  if (methodName.toUpperCase() === 'POST') {
    data = JSON.stringify(params);
  }

  return jQuery.ajax({
    type: methodName,
    contentType: 'application/json; charset=utf-8',
    dataType: 'jsonp',
    url: url,
    data: data,
    cache: allowCaching,
    timeout: timeout,
    success: callbackSuccess,
    error: callbackError
  });
};

Quantumart.QP8.Utils.getCustomActionJson = function Quantumart$QP8$UtilsQuantumart$QP8$Utils$getCustomActionJson(url, params, callbackSuccess, callbackError) {
  Quantumart.QP8.Utils.getJsonFromUrl('POST', CONTROLLER_URL_CUSTOM_ACTION + 'Proxy', _.extend(params, { url: url }), false, false, callbackSuccess, callbackError);
};

Quantumart.QP8.Utils.getTextContentFromUrl = function Quantumart$QP8$Utils$getTextContentFromUrl(url, allowCaching) {
  /// <summary>
  /// Получает текстовый контент, который расположен по указанному URL
  /// </summary>
  /// <param name="url" type="String">URL веб-сервиса</param>
  /// <param name="allowCaching" type="Boolean">признак, разрешающий кэширование запроса</param>
  var textContent = '';

  jQuery.ajax({
    type: 'GET',
    dataType: 'text',
    url: url,
    async: false,
    cache: allowCaching,
    success: function(data) {
      textContent = data;
    },
    error: function() {
      textContent = '';
    }
  });

  return textContent;
};

Quantumart.QP8.Utils.getHtmlContentFromUrl = function Quantumart$QP8$Utils$getHtmlContentFromUrl(url, allowCaching) {
  /// <summary>
  /// Получает HTML-контент, который расположен по указанному URL
  /// </summary>
  /// <param name="url" type="String">URL веб-сервиса</param>
  /// <param name="allowCaching" type="Boolean">признак, разрешающий кэширование запроса</param>
  var htmlContent = '';

  jQuery.ajax({
    type: 'GET',
    dataType: 'html',
    url: url,
    async: false,
    cache: allowCaching,
    success: function(data) {
      htmlContent = data;
    },
    error: function() {
      htmlContent = '';
    }
  });

  return htmlContent;
};

Quantumart.QP8.Utils.postDataToUrl = function Quantumart$QP8$Utils$postDataToUrl(url, data, async, callbackSuccess, callbackError) {
  /// <summary>
  /// Отправляет данные на указанный URL
  /// </summary>
  /// <param name="url" type="String">URL веб-сервиса</param>
  /// <param name="data" type="String">данные</param>
  /// <param name="async" type="Boolean">признак, асинхронного запроса</param>
  /// <param name="callbackSuccess" type="Function">обработчик успешного вызова веб-сервиса</param>
  /// <param name="callbackError" type="Function">обработчик ошибок</param>

  return jQuery.ajax({
    type: 'POST',
    contentType: 'application/x-www-form-urlencoded',
    dataType: 'json',
    url: url,
    data: data,
    async: async,
    cache: false,
    success: callbackSuccess,
    error: callbackError
  });
};

Quantumart.QP8.Utils.getJsonSync = function Quantumart$QP8$Utils$getJsonSync(url, params) {
  return jQuery.parseJSON(jQuery.ajax({
    type: 'GET',
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    url: url,
    data: params,
    async: false,
    cache: false,
    success: null,
    error: Quantumart.QP8.Utils.processGenericAjaxError
  }).responseText);
};

Quantumart.QP8.Utils.getTypeNameForJson = function Quantumart$QP8$Utils$getTypeNameForJson(typeName) {
  /// <summary>
  /// Преобразует название типа данных в название JSON-типа данных (для передачи объекта WCF-сервису)
  /// </summary>
  /// <param name="typeName" type="String">название типа данных</param>
  /// <returns type="String">название JSON-типа данных</returns>
  var jsonTypeName = typeName;

  if (typeName.length > 0) {
    if (typeName.indexOf('.') != -1) {
      var type = '';
      var namespace = '';
      var parts = typeName.split('.');
      var partCount = parts.length;

      for (var partIndex = 0; partIndex < partCount; partIndex++) {
        var part = parts[partIndex];

        if (partIndex == (partCount - 1)) {
          type = part;
        } else {
          if (partIndex > 0) {
            namespace += '.';
          }

          namespace += part;
        }
      }

      jsonTypeName = String.format('{0}:#{1}', type, namespace);
    }
  }

  return jsonTypeName;
};

/// <summary>
/// Обрабатывает ошибку, возникшую при AJAX-запросе
/// </summary>
/// <param name="status" type="Number" integer="true">HTTP-статус</param>
Quantumart.QP8.Utils.processGenericAjaxError = function Quantumart$QP8$Utils$processGenericAjaxError(jqXHR) {
  var errorMessage = String.format($l.Common.ajaxGenericErrorMessage, status);

  if (status === 401 || jqXHR.getResponseHeader('QP-Not-Authenticated')) {
    errorMessage = $l.Common.ajaxUserSessionExpiredErrorMessage;
  } else if (status === 500) {
    errorMessage = $l.Common.ajaxDataReceivingErrorMessage;
  }

  window.alert(errorMessage);
};

Quantumart.QP8.Utils.generateErrorMessageText = function Quantumart$QP8$Utils$generateErrorMessageText(status) {
  /// <summary>
  /// Генерирует сообщение об ошибке
  /// </summary>
  /// <param name="status" type="Number" integer="true">HTTP-статус</param>
  var html = new $.telerik.stringBuilder();

  if (status === 500 || status === 404) {
    html.cat('<div class="alignCenter" style="margin-top: 1.0em;">');
    html.cat('  <div class="alignCenterToLeft">');
    html.cat('    <div class="alignCenterToRight">');
    html.cat('      <table cellpadding="0" cellspacing="0" border="0" style="margin: 0 120px;">');
    html.cat('        <tr class="top">');
    if (status === 500) {
      html.cat('          <td style="width: 110px;"><img src="' + COMMON_IMAGE_FOLDER_URL_ROOT + '/errors/bug.gif" width="100" height="118" border="0" style="margin: 0 5px;" /></td>');
      html.cat('          <td style="padding: 0 10px;">');
      html.cat('            <h1>' + $l.EditingArea.error500Title + '</h1>');
      html.cat($l.EditingArea.error500Text);
      html.cat('          </td>');
    } else if (status === 404) {
      html.cat('          <td style="width: 110px;"><img src="' + COMMON_IMAGE_FOLDER_URL_ROOT + '/errors/compass.png" width="100" height="115" border="0" alt="Компас" style="margin: 0 5px;" /></td>');
      html.cat('          <td style="padding: 0 10px;">');
      html.cat('            <h1>' + $l.EditingArea.error404Title + '</h1>');
      html.cat($l.EditingArea.error404Text);
      html.cat('          </td>');
    }

    html.cat('        </tr>');
    html.cat('      </table>');
    html.cat('    </div>');
    html.cat('  </div>');
    html.cat('</div>');
  }

  return html.string();
};

Quantumart.QP8.Utils.ajaxCallbackDecorator = function Quantumart$QP8$Utils$ajaxCallbackDecorator(callback, settings) {
  if (callback) {
    return function(data, textStatus, jqXHR) {
      if (!Quantumart.QP8.BackendLogOnWindow.deferredExecution(data, jqXHR, callback, settings)) {
        return callback(data, textStatus, jqXHR);
      }
    };
  } else {
    return callback;
  }
};

Quantumart.QP8.Utils.decorateDeferred = function Quantumart$QP8$Utils$decorateDeferred(deferred, decorator, settings) {
  deferred._doneBase = deferred.done;

  deferred.done = function(callback) {
    var currentCallback = decorator(callback, settings);
    var currentDeferred = this._doneBase(currentCallback);

    return Quantumart.QP8.Utils.decorateDeferred(currentDeferred);
  };

  return deferred;
};

/*Возвращает строковое представление числа без использования экспоненциальной записи, и ровно с digits цифр после запятой.
Число округляется при необходимости, и дробная часть добивается нулями до нужной длины.
Если число целое, то дробная часть отбрасывается*/
Quantumart.QP8.Utils.toFixed = function(n, digits) {
  digits = digits || 0;
  return n.toFixed(digits).replace(new RegExp('\\.0{' + digits + '}'), '');
};

if (typeof String.prototype.left !== 'function') {
  String.prototype.left = function(length) {
  /// <summary>
  /// Возвращает указанное число символов с левого конца строки
  /// </summary>
    if (!/\d+/.test(length)) {
      return this;
    }

    var result = this.substr(0, length);

    return result;
  };
}

if (typeof String.prototype.right !== 'function') {
  String.prototype.right = function(length) {
  /// <summary>
  /// Возвращает указанное число символов с правого конца строки
  /// </summary>
    if (!/\d+/.test(length)) {
      return this;
    }

    var result = this.substr(this.length - length);

    return result;
  };
}

Quantumart.QP8.Utils.generateRandomString = function Quantumart$QP8$Utils$generateRandomString(stringLength) {
  /// <summary>
  /// Генерирует из случайных символов строку заданной длины
  /// </summary>
  /// <param name="stringLength" type="Number">длина строки</param>
  /// <returns>случайная строка</returns>
  var symbolString = 'QuantumArt98BCDEFGHIJKLMNOPRSTUVWXYZbcdefghijklopqsvwxyz01234567'; // строка символов
  var symbolStringLength = symbolString.length; // длина строки символов
  var randomNumber = 0; // случайное число
  var randomSymbol = ''; // случайный символ
  var result = ''; // результирующая переменная

  for (var i = 0; i < stringLength; i++) {
    randomNumber = parseInt(symbolStringLength * Math.random());
    randomSymbol = symbolString.substr(randomNumber, 1);

    result += randomSymbol;
  }

  return result;
};

Quantumart.QP8.Utils.htmlEncode = function Quantumart$QP8$Utils$htmlEncode(htmlContent, allowFormatText) {
  /// <summary>
  /// Преобразует строку в HTML-строку
  /// </summary>
  /// <param name="htmlContent" type="String">HTML-код</param>
  /// <param name="allowFormatText" type="Boolean">разрешает форматирование кода</param>
  /// <returns>HTML-строка</returns>
  if (Quantumart.QP8.Utils.isNull(allowFormatText)) {
    allowFormatText = false;
  }

  var processedContent = htmlContent;

  if (!Quantumart.QP8.Utils.isNullOrWhiteSpace(htmlContent)) {
    processedContent = processedContent.toString()
      .replace(/&/igm, '&amp;')
      .replace(/\"/igm, '&quot;')
      .replace(/</igm, '&lt;')
      .replace(/>/igm, '&gt;');

    if (allowFormatText) {
      processedContent = processedContent
        .replace(/\n/igm, '<br />\n')
      .replace(/\t/igm, '&nbsp;&nbsp;&nbsp;&nbsp;');;
    }
  }

  return processedContent;
};

Quantumart.QP8.Utils.cutShort = function Quantumart$QP8$Utils$cutShort(value, maxLength, endSymbol) {
  /// <summary>
  /// Обрезает строку до указанного количества символов
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <param name="maxLength" type="Number">максимальная длина строки</param>
  /// <param name="endSymbol" type="String" integer="true">символ, который ставится в конце обрезанной строки</param>
  /// <returns>обрезанная строка</returns>

  if (!endSymbol) {
    endSymbol = '…';
  }

  var result = Quantumart.QP8.Utils.toString(value, '').trim();

  if (result.length > maxLength) {
    result = result.left(maxLength).trim() + endSymbol;
  }

  return result;
};

Quantumart.QP8.Utils.middleCutShort = function Quantumart$QP8$Utils$middleCutShort(value, maxLength, dividingSymbol) {
  /// <summary>
  /// Обрезает строку до указанного количества символов из середины
  /// </summary>
  /// <param name="value" type="String">значение</param>
  /// <param name="maxLength" type="Number">максимальная длина строки</param>
  /// <param name="dividingSymbol" type="String" integer="true">символ, который ставится по середине обрезанной строки</param>
  /// <returns>обрезанная строка</returns>

  if (!dividingSymbol) {
    dividingSymbol = '…';
  }

  var result = Quantumart.QP8.Utils.toString(value, '').trim();

  if (result.length > maxLength) {
    var halfMaxLength = Math.floor((maxLength - dividingSymbol.length) / 2);
    var leftPart = value.left(halfMaxLength).trim();
    var rightPart = value.right(halfMaxLength).trim();

    result = leftPart + dividingSymbol + rightPart;
  }

  return result;
};

Quantumart.QP8.Utils.setPropertyValue = function Quantumart$QP8$Utils$setPropertyValue(object, propertyName, value) {
  /// <summary>
  /// Задает значение свойству объекта
  /// </summary>
  /// <param name="object" type="Object">объект</param>
  /// <param name="propertyName" type="String">название свойства</param>
  /// <param name="value" type="String">значение</param>
  if (propertyName.indexOf('.') != -1) {
    var parts = propertyName.split('.');
    var partCount = parts.length;
    var newObject = object;

    for (var partIndex = 0; partIndex < partCount; partIndex++) {
      var part = parts[partIndex];

      if (partIndex == (partCount - 1)) {
        newObject[part] = value;
      } else {
        if (Quantumart.QP8.Utils.isNull(newObject[part])) {
          newObject[part] = {};
        }

        newObject = newObject[part];
      }
    }
  } else {
    object[propertyName] = value;
  }
};

Quantumart.QP8.Utils.removeProperty = function Quantumart$QP8$Utils$removeProperty(object, propertyName) {
  /// <summary>
  /// Удаляет свойство объекта
  /// </summary>
  /// <param name="object" type="Object">объект</param>
  /// <param name="propertyName" type="String">название свойства</param>
  if (object && object[propertyName]) {
    if (jQuery.browser.msie && jQuery.browser.version < 8) {
      jQuery(object).removeAttr(propertyName);
    } else {
      delete object[propertyName];
    }
  }
};

Quantumart.QP8.Utils.hashToString = function Quantumart$QP8$Utils$hashToString(obj) {
  var result = '';

  for (var key in obj) {
    result = result + key + '=' + obj[key] + ',';
  }

  return result.substr(0, result.length - 1);
};

Quantumart.QP8.Utils.getHashKeysCount = function Quantumart$QP8$Utils$getHashKeysCount(hash) {
  /// <summary>
  /// Возвращает количество ключей в хэше
  /// </summary>
  /// <param name="hash" type="Object">хэш</param>
  /// <returns type="Number" integer="true">количество ключей в хэше</returns>
  var keysCount = 0;

  if (hash) {
    for (var key in hash) {
      keysCount++;
    }
  }

  return keysCount;
};

Array.distinct = function Array$distinct(array) {
  /// <summary>
  /// Возвращает массив, состоящий из уникальных элементов
  /// </summary>
  /// <param name="array" type="Array" elementMayBeNull="true">исходный массив</param>
  /// <returns type="Array">массив уникальных элементов</returns>
  var uniqueArray = [];

  if (Quantumart.QP8.Utils.isArray(array)) {
    for (var itemIndex = 0; itemIndex < array.length; itemIndex++) {
      var item = array[itemIndex];

      if (!Array.contains(uniqueArray, item)) {
        Array.add(uniqueArray, item);
      }
    }
  }

  return uniqueArray;
};

Quantumart.QP8.Utils.clearArray = function(array) {
  if (Quantumart.QP8.Utils.isArray(array)) {
    Array.clear(array);
  }

  array = null;
};

Quantumart.QP8.Utils.callFunction = function Quantumart$QP8$Utils$callFunction(callback, context) {
  /// <summary>
  /// Вызывает функцию
  /// </summary>
  /// <param name="callback" type="Function">функция обратного вызова</param>
  if (Quantumart.QP8.Utils.isFunction(callback)) {
    if (context) {
      callback.call(context);
    } else {
      callback();
    }
  }
};

Quantumart.QP8.Utils.preventDefaultFunction = function Quantumart$QP8$Utils$preventDefaultFunction(e) {
  /// <summary>
  /// Отменяет поведение по умолчанию
  /// </summary>
  /// <param name="e" type="Object">событие</param>
  e.preventDefault();
};

Quantumart.QP8.Utils.hashToQueryString = function Quantumart$QP8$Utils$hashToQueryString(hash) {
  /// <summary>
  /// Возвращает строку запроса
  /// </summary>
  /// <param name="hash" type="Object" elementMayBeNull="true">хэш</param>
  /// <returns type="String">строка запроса</returns>
  var result = '';

  if (hash) {
    var index = 0;

    for (var parameterName in hash) {
      var value = Quantumart.QP8.Utils.toString(hash[parameterName], '');

      if (index > 0) {
        result += '&';
      }

      result += parameterName + '=' + escape(value);
      index++;
    }
  }

  return result;
};

Quantumart.QP8.Utils.arrayToQueryString = function Quantumart$QP8$Utils$arrayToQueryString(parameterName, array) {
  /// <summary>
  /// Возвращает строку запроса
  /// </summary>
  /// <param name="parameterName" type="String">название параметра</param>
  /// <param name="array" type="Array" elementMayBeNull="true">массив значений</param>
  /// <returns type="String">строка запроса</returns>
  var result = '';

  if (!Quantumart.QP8.Utils.isNullOrWhiteSpace(parameterName) && !Quantumart.QP8.Utils.isNullOrEmpty(array)) {
    for (var index = 0; index < array.length; index++) {
      var value = Quantumart.QP8.Utils.toString(array[index], '');

      if (index > 0) {
        result += '&';
      }

      result += parameterName + '=' + escape(value);
    }
  }

  return result;
};

Quantumart.QP8.Utils.updateQueryStringParameter = function Quantumart$QP8$Utils$updateQueryStringParameter(uri, key, value) {
  var re = new RegExp('([?|&])' + key + '=.*?(&|$)', 'i');

  separator = uri.indexOf('?') !== -1 ? '&' : '?';
  if (uri.match(re)) {
    return uri.replace(re, '$1' + key + '=' + value + '$2');
  } else {
    return uri + separator + key + '=' + value;
  }
};

Quantumart.QP8.Utils.collectGarbageInIE = function Quantumart$QP8$Utils$collectGarbageInIE() {
  if (jQuery.browser.msie) {
    CollectGarbage();
  }
};

$q.addRemoveToArrUniq = function Quantumart$QP8$Utils$addRemoveToArrUniq(arrToModify, valToAddRemove, shouldBeExcluded) {
  var underscoreMethod = shouldBeExcluded ? 'difference' : 'union';
  return _[underscoreMethod](arrToModify, valToAddRemove);
};

$q.bindProxies = function Quantumart$QP8$Utils$bindProxies(listOfFnNames, fnPostfix) {
  var postfix = fnPostfix || 'Handler';
  [].forEach.call(listOfFnNames, function(fnName) {
    try {
      this[fnName + postfix] = this[fnName].bind(this);
    } catch(e) {
      console.error('Failed to register: ' + fnName, e);
    }
  }, this);
};

$q.dispose = function Quantumart$QP8$Utils$dispose(listOfObjs) {
  [].forEach.call(listOfObjs, function(obj) {
    try {
      if(this[obj]) {
        this[obj] = null;
      }
    } catch(e) {
      console.error('Failed to dispose: ' + obj, e);
    }
  }, this);
};

Quantumart.QP8.Utils.registerClass('Quantumart.QP8.Utils');
