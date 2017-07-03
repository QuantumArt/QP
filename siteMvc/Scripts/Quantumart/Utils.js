/* global _ */

/* eslint no-extend-native: 'off' */
/* eslint max-params: 'off' */
/* eslint max-lines: 'off' */
/* eslint no-alert: 'off' */
/* eslint no-sync: 'off' */

/* TODO: wait4{webpack|browserify}
var config = require("./config");
if (config.env === 'development') {
  $q.isDebug = true;
}

$q.isDebug = process.env.NODE_ENV === 'development';
*/

window.$q = {
  isDebug: window.Sys.Debug.isDebug
};

$q.trace = function trace() {
  let args, firstArg, otherArgs;
  if ($q.isDebug || window.Sys.Debug.isDebug) {
    args = [].slice.call(arguments);
    firstArg = args.slice(0, 1)[0];
    otherArgs = args.slice(1);

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

$q.alertSuccess = function alertSuccess(msg) {
  window.alert(msg);
  if ($q.isDebug || window.Sys.Debug.isDebug) {
    window.console.log.apply(window.console, Array.prototype.slice.call(arguments));
  }
};

$q.alertWarn = function alertWarn(msg) {
  window.alert(msg);
  $q.trace(msg);
};

$q.alertError = function alertError(msg) {
  window.alert(msg);
  $q.trace(msg);
};

$q.alertFail = function alertFail(msg) {
  window.alert(msg);
  if ($q.isDebug || window.Sys.Debug.isDebug) {
    window.console.warn.apply(window.console, Array.prototype.slice.call(arguments));
  }
};

$q.confirmMessage = function confirmMessage(msg) {
  return window.confirm(msg);
};

/**
 * Basic implementation of jQuery ajax request with JSend response support
 * @param  {Object} opts jQuery options for ajax request
 * @return {Object}      jQuery XHR deffered
 */
$q.sendAjax = function sendAjax(opts) {
  const defaultOptions = {
    type: 'GET',
    dataType: 'json',
    contentType: 'application/json; charset=utf-8',
    async: true,
    cache: false,
    traditional: true
  };

  const options = Object.assign({}, defaultOptions, opts);
  const maxLogDataLengthToLog = 300;
  const logData = JSON.stringify(options.data || {});
  const logDataLength = logData.length;
  const cuttedLogData = logDataLength > maxLogDataLengthToLog
    ? `${logData.slice(0, maxLogDataLengthToLog)}..`
    : logData;

  const debugMessage = `ajax: ${options.type} ${options.url}. Data: ${cuttedLogData}`;
  if (options.dataType.toLowerCase() === 'jsonp' && !options.async) {
    window.console.error('Sync requests cannot be combined with jsonp');
  }

  $q.trace(`Sending ${debugMessage}`, 'Request object: ', options);
  return $.ajax(options).done(response => {
    $q.trace(`Parsing ${debugMessage}`, 'Response object: ', response);
    if (response.status.toUpperCase() === 'SUCCESS') {
      if (options.jsendSuccess) {
        options.jsendSuccess(response.data, response);
      }
    } else if (response.status.toUpperCase() === 'FAIL') {
      if (options.jsendFail) {
        options.jsendFail(response.data, response);
      } else {
        $q.alertFail(response.message || 'There was an errors at request');
      }
    } else if (options.jsendError) {
      options.jsendError(response.data, response);
    } else {
      $q.alertError(response.message || 'Unknown server error');
    }
  }).done(() => {
    $q.hideLoader();
  });
};

$q.getAjax = function getAjax(url, data, jsendSuccess, jsendFail, jsendError) {
  return $q.sendAjax({
    url,
    data,
    jsendSuccess,
    jsendFail,
    jsendError
  });
};

$q.postAjax = function postAjax(url, data, jsendSuccess, jsendFail, jsendError) {
  return $q.sendAjax({
    url,
    type: 'POST',
    data: JSON.stringify(data),
    jsendSuccess,
    jsendFail,
    jsendError
  });
};

$q.showLoader = function showLoader() {
  if ($ctx) {
    $ctx.getArea().showAjaxLoadingLayer();
  }
};

$q.hideLoader = function hideLoader() {
  if ($ctx) {
    $ctx.getArea().hideAjaxLoadingLayer();
  }
};

$q.isBoolean = function isBoolean(value) {
  return typeof value === 'boolean' || (value && ['true', 'false'].includes(value.toString().toLowerCase().trim()));
};

$q.toBoolean = function toBoolean(value, defaultValue) {
  if ($q.isBoolean(value)) {
    return value.toString().toLowerCase().trim() === 'true';
  }

  if ($q.isBoolean(defaultValue)) {
    return defaultValue.toString().toLowerCase().trim() === 'true';
  }

  return false;
};

$q.isNull = function isNull(value) {
  let result = false;
  if (value === undefined || value === null) {
    result = true;
  }

  return result;
};

$q.isNullOrEmpty = function isNullOrEmpty(value) {
  let result = false;
  if ($q.isNull(value)) {
    result = true;
  } else if ($q.isArray(value) || ($q.isObject(value) && value.jquery)) {
    if (!value.length) {
      result = true;
    }
  } else if (!value.toString().length) {
    result = true;
  }

  return result;
};

$q.isNullOrWhiteSpace = function isNullOrWhiteSpace(value) {
  let result = false;
  if ($q.isNull(value)) {
    result = true;
  } else if ($q.isArray(value) || ($q.isObject(value) && value.jquery)) {
    if (!value.length) {
      result = true;
    }
  } else if (!value.toString().trim().length) {
    result = true;
  }

  return result;
};

$q.toString = function convertToString(value, defaultValue) {
  let string;
  if ($q.isNull(value)) {
    string = $q.isNull(defaultValue) ? null : defaultValue;
  } else {
    string = value.toString();
  }

  return string;
};

$q.isString = function isString(value) {
  let result = false;
  if (!$q.isNull(value)) {
    result = typeof value.valueOf() === 'string';
  }

  return result;
};

// Подготавливает значение к преобразованию в число
// forCheck - признак, указывающий что преобразование используется при проверки типа</param>
$q._prepareNumber = function prepareNumber(value, forCheck) {
  let processedValue;
  let number = null;
  if (!$q.isNullOrEmpty(value)) {
    processedValue = value.toString().replace(/\s/igm, '').replace(',', '.').toLowerCase();
    if (!forCheck) {
      if (processedValue === 'true') {
        number = 1;
        return number;
      } else if (processedValue === 'false') {
        number = 0;
        return number;
      }
    }

    number = Number.parseLocale(processedValue);
  }

  return number;
};

$q.toInt = function toInt(value, defaultValue) {
  let result = value;
  if (result === 'true' || result === 'false') {
    result = result === 'true' || 0;
  }

  if (result === 0) {
    return 0;
  }

  return +result || defaultValue;
};

$q.toDate = function toDate(value, defaultValue) {
  let date = null;
  if (!$q.isNullOrEmpty(value)) {
    date = Date.parseLocale(value);
  }

  return date || defaultValue || null;
};

$q.isDate = function isDate(value) {
  let localDate;
  let result = false;
  if (!$q.isNullOrEmpty(value)) {
    localDate = Date.parseLocale(value);
    if (localDate !== null) {
      result = true;
    }
  }

  return result;
};

$q.isArray = function isArray(value) {
  let result = false;
  if (value) {
    result = typeof value === 'object' && value.join;
  }

  return result;
};

$q.isObject = function isObject(value) {
  let result = false;
  if (!$q.isNull(value)) {
    result = typeof value === 'object';
  }

  return result;
};

$q.isFunction = function isFunction(value) {
  let result = false;
  if (!$q.isNull(value)) {
    result = typeof value.valueOf() === 'function';
  }

  return result;
};

$q.toJQuery = function toJQuery(value) {
  if ($q.isObject(value)) {
    if (value.jquery) {
      return value;
    }

    return $(value);
  } else if ($q.isArray(value)) {
    return $(value);
  }

  return null;
};

$q.isInRange = function isInRange(value, lowerBound, lowerBoundType, upperBound, upperBoundType) {
  if (lowerBoundType === Quantumart.QP8.Enums.RangeBoundaryType.Inclusive) {
    if (value < lowerBound) {
      return false;
    }
  } else if (lowerBoundType === Quantumart.QP8.Enums.RangeBoundaryType.Exclusive) {
    if (value <= lowerBound) {
      return false;
    }
  }

  if (upperBoundType === Quantumart.QP8.Enums.RangeBoundaryType.Inclusive) {
    if (value > upperBound) {
      return false;
    }
  } else if (upperBoundType === Quantumart.QP8.Enums.RangeBoundaryType.Exclusive) {
    if (value >= upperBound) {
      return false;
    }
  }

  return true;
};

$q.compareValues = function compareValues(firstValue, secondValue, comparisonOperator) {
  let result = false;
  if (comparisonOperator === Quantumart.QP8.Enums.ComparisonOperator.Equal) {
    result = firstValue === secondValue;
  } else if (comparisonOperator === Quantumart.QP8.Enums.ComparisonOperator.NotEqual) {
    result = firstValue !== secondValue;
  } else if (comparisonOperator === Quantumart.QP8.Enums.ComparisonOperator.GreaterThan) {
    result = firstValue > secondValue;
  } else if (comparisonOperator === Quantumart.QP8.Enums.ComparisonOperator.GreaterThanEqual) {
    result = firstValue >= secondValue;
  } else if (comparisonOperator === Quantumart.QP8.Enums.ComparisonOperator.LessThan) {
    result = firstValue < secondValue;
  } else if (comparisonOperator === Quantumart.QP8.Enums.ComparisonOperator.LessThanEqual) {
    result = firstValue <= secondValue;
  }

  return result;
};

$q.getJsonFromUrl = function getJsonFromUrl(method, url, params, async, allowCaching, callbackSuccess, callbackError) {
  const methodName = method.toUpperCase();
  const data = methodName === 'POST' ? JSON.stringify(params) : params;
  const settings = {
    type: methodName,
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    url,
    data,
    async,
    cache: allowCaching,
    error: callbackError || $q.processGenericAjaxError
  };

  settings.success = $q.ajaxCallbackDecorator(callbackSuccess, settings);
  return $q.decorateDeferred($.ajax(settings), $q.ajaxCallbackDecorator, settings);
};

$q.getJsonPFromUrl = function getJsonPFromUrl(
  method,
  url,
  params,
  allowCaching,
  timeout,
  callbackSuccess,
  callbackError
) {
  const methodName = method.toUpperCase();
  let data = params;
  if (methodName.toUpperCase() === 'POST') {
    data = JSON.stringify(params);
  }

  return $.ajax({
    type: methodName,
    contentType: 'application/json; charset=utf-8',
    dataType: 'jsonp',
    url,
    data,
    cache: allowCaching,
    timeout,
    success: callbackSuccess,
    error: callbackError
  });
};

$q.getCustomActionJson = function getCustomActionJson(url, params, callbackSuccess, callbackError) {
  $q.getJsonFromUrl(
    'POST',
    `${window.CONTROLLER_URL_CUSTOM_ACTION}Proxy`,
    Object.assign(params, { url }),
    false,
    false,
    callbackSuccess,
    callbackError);
};

$q.getTextContentFromUrl = function getTextContentFromUrl(url, allowCaching) {
  let textContent = '';

  $.ajax({
    type: 'GET',
    dataType: 'text',
    url,
    async: false,
    cache: allowCaching,
    success(data) {
      textContent = data;
    },
    error() {
      textContent = '';
    }
  });

  return textContent;
};

$q.getHtmlContentFromUrl = function getHtmlContentFromUrl(url, allowCaching) {
  let htmlContent = '';
  $.ajax({
    type: 'GET',
    dataType: 'html',
    url,
    async: false,
    cache: allowCaching,
    success(data) {
      htmlContent = data;
    },
    error() {
      htmlContent = '';
    }
  });

  return htmlContent;
};

$q.postDataToUrl = function postDataToUrl(url, data, async, callbackSuccess, callbackError) {
  return $.ajax({
    type: 'POST',
    contentType: 'application/x-www-form-urlencoded',
    dataType: 'json',
    url,
    data,
    async,
    cache: false,
    success: callbackSuccess,
    error: callbackError
  });
};

$q.getJsonSync = function getJsonSync(url, params) {
  return $.parseJSON($.ajax({
    type: 'GET',
    contentType: 'application/json; charset=utf-8',
    dataType: 'json',
    url,
    data: params,
    async: false,
    cache: false,
    success: null,
    error: $q.processGenericAjaxError
  }).responseText);
};

// Преобразует название типа данных в название JSON-типа данных (для передачи объекта WCF-сервису)
// typeName - название типа данных
// Возвращает название JSON-типа данных
$q.getTypeNameForJson = function getTypeNameForJson(typeName) {
  let type, namespace, part, parts, partCount, partIndex;
  let jsonTypeName = typeName;
  if (typeName.length > 0) {
    if (typeName.indexOf('.') !== -1) {
      type = '';
      namespace = '';
      parts = typeName.split('.');
      partCount = parts.length;

      for (partIndex = 0; partIndex < partCount; partIndex++) {
        part = parts[partIndex];
        if (partIndex === partCount - 1) {
          type = part;
        } else if (partIndex > 0) {
          namespace += '.';
        }

        namespace += part;
      }

      jsonTypeName = String.format('{0}:#{1}', type, namespace);
    }
  }

  return jsonTypeName;
};

$q.processGenericAjaxError = function processGenericAjaxError(jqXHR) {
  let errorMessage = String.format($l.Common.ajaxGenericErrorMessage, status);
  if (status === 401 || jqXHR.getResponseHeader('QP-Not-Authenticated')) {
    errorMessage = $l.Common.ajaxUserSessionExpiredErrorMessage;
  } else if (status === 500) {
    errorMessage = $l.Common.ajaxDataReceivingErrorMessage;
  }

  window.alert(errorMessage);
};

$q.generateErrorMessageText = function generateErrorMessageText(httpStatus) {
  const html = new $.telerik.stringBuilder();
  if (httpStatus === 500 || httpStatus === 404) {
    html.cat('<div class="alignCenter" style="margin-top: 1.0em;">');
    html.cat('  <div class="alignCenterToLeft">');
    html.cat('    <div class="alignCenterToRight">');
    html.cat('      <table cellpadding="0" cellspacing="0" border="0" style="margin: 0 120px;">');
    html.cat('        <tr class="top">');
    if (httpStatus === 500) {
      html.cat(
        `          <td style="width: 110px;"><img src="${
         window.COMMON_IMAGE_FOLDER_URL_ROOT
         }/errors/bug.gif" width="100" height="118" border="0" style="margin: 0 5px;" /></td>`);

      html.cat('          <td style="padding: 0 10px;">');
      html.cat(`            <h1>${$l.EditingArea.error500Title}</h1>`);
      html.cat($l.EditingArea.error500Text);
      html.cat('          </td>');
    } else if (httpStatus === 404) {
      html.cat(
        `          <td style="width: 110px;"><img src="${
         window.COMMON_IMAGE_FOLDER_URL_ROOT
         }/errors/compass.png" width="100" height="115" border="0" alt="Компас" style="margin: 0 5px;" /></td>`);

      html.cat('          <td style="padding: 0 10px;">');
      html.cat(`            <h1>${$l.EditingArea.error404Title}</h1>`);
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

$q.ajaxCallbackDecorator = function ajaxCallbackDecorator(callback, settings) {
  if (callback) {
    return function ajaxDecorator(data, textStatus, jqXHR) {
      if (!Quantumart.QP8.BackendLogOnWindow.deferredExecution(data, jqXHR, callback, settings)) {
        return callback(data, textStatus, jqXHR);
      }

      return undefined;
    };
  }

  return callback;
};

$q.decorateDeferred = function decorateDeferred(deferred, decorator, settings) {
  const result = deferred;
  result._doneBase = deferred.done;
  result.done = function doneCb(callback) {
    const currentCallback = decorator(callback, settings);
    const currentDeferred = this._doneBase(currentCallback);

    return $q.decorateDeferred(currentDeferred);
  };

  return result;
};

/* Возвращает строковое представление числа без использования экспоненциальной записи,
 * и ровно с digits цифр после запятой. Число округляется при необходимости,
 * и дробная часть добивается нулями до нужной длины.
 * Если число целое, то дробная часть отбрасывается */
$q.toFixed = function toFixed(number, digits) {
  const defaultDigits = digits || 0;
  return number.toFixed(defaultDigits).replace(new RegExp(`\\.0{${defaultDigits}}`), '');
};

if (typeof String.prototype.left !== 'function') {
  String.prototype.left = function trimLeft(strLength) {
    if (!/\d+/.test(strLength)) {
      return this;
    }

    return this.substr(0, strLength);
  };
}

if (typeof String.prototype.right !== 'function') {
  String.prototype.right = function trimRight(strLength) {
    if (!/\d+/.test(strLength)) {
      return this;
    }

    return this.substr(this.length - strLength);
  };
}

$q.generateRandomString = function generateRandomString(stringLength) {
  let i, randomNumber, randomSymbol;
  const symbolString = 'QuantumArt98BCDEFGHIJKLMNOPRSTUVWXYZbcdefghijklopqsvwxyz01234567';
  const symbolStringLength = symbolString.length;
  let result = '';

  for (i = 0; i < stringLength; i++) {
    randomNumber = parseInt(symbolStringLength * Math.random(), 10);
    randomSymbol = symbolString.substr(randomNumber, 1);

    result += randomSymbol;
  }

  return result;
};

$q.htmlEncode = function htmlEncode(htmlContent, allowFormatText) {
  let processedContent = htmlContent;
  if (!$q.isNullOrWhiteSpace(htmlContent)) {
    processedContent = processedContent.toString()
      .replace(/&/igm, '&amp;')
      .replace(/"/igm, '&quot;')
      .replace(/</igm, '&lt;')
      .replace(/>/igm, '&gt;');

    if (!$q.isNull(allowFormatText) || allowFormatText) {
      processedContent = processedContent
        .replace(/\n/igm, '<br />\n')
      .replace(/\t/igm, '&nbsp;&nbsp;&nbsp;&nbsp;');
    }
  }

  return processedContent;
};

$q.cutShort = function cutShort(value, maxLength, endSymbol) {
  let result = $q.toString(value, '').trim();
  if (result.length > maxLength) {
    result = result.left(maxLength).trim() + (endSymbol || '…');
  }

  return result;
};

$q.middleCutShort = function middleCutShort(value, maxLength, dividingSymbol) {
  const divSymbol = dividingSymbol || '…';
  let halfMaxLength, leftPart, rightPart;
  let result = $q.toString(value, '').trim();
  if (result.length > maxLength) {
    halfMaxLength = Math.floor((maxLength - divSymbol.length) / 2);
    leftPart = value.left(halfMaxLength).trim();
    rightPart = value.right(halfMaxLength).trim();
    result = leftPart + divSymbol + rightPart;
  }

  return result;
};

$q.setPropertyValue = function setPropertyValue(object, propertyName, value) {
  let part, parts, partCount, newObject, partIndex;
  if (propertyName.indexOf('.') === -1) {
    // eslint-disable-next-line no-param-reassign
    object[propertyName] = value;
  } else {
    parts = propertyName.split('.');
    partCount = parts.length;
    newObject = object;
    for (partIndex = 0; partIndex < partCount; partIndex++) {
      part = parts[partIndex];
      if (partIndex === partCount - 1) {
        newObject[part] = value;
      } else {
        if ($q.isNull(newObject[part])) {
          newObject[part] = {};
        }

        newObject = newObject[part];
      }
    }
  }
};

$q.removeProperty = function removeProperty(object, propertyName) {
  if (object && object[propertyName]) {
    if ($.browser.msie && $.browser.version < 8) {
      jQuery(object).removeAttr(propertyName);
    } else {
      // eslint-disable-next-line no-param-reassign
      delete object[propertyName];
    }
  }
};

$q.hashToString = function hashToString(obj) {
  let key;
  let result = '';

  // eslint-disable-next-line guard-for-in, no-restricted-syntax
  for (key in obj) {
    result = `${result + key}=${obj[key]},`;
  }

  return result.substr(0, result.length - 1);
};

$q.getHashKeysCount = function getHashKeysCount(hash) {
  let key; // eslint-disable-line no-unused-vars
  let keysCount = 0;
  if (hash) {
    // eslint-disable-next-line guard-for-in, no-restricted-syntax
    for (key in hash) {
      keysCount += 1;
    }
  }

  return keysCount;
};

Array.distinct = function distinct(array) {
  let itemIndex, item;
  const uniqueArray = [];
  if ($q.isArray(array)) {
    for (itemIndex = 0; itemIndex < array.length; itemIndex++) {
      item = array[itemIndex];
      if (!Array.contains(uniqueArray, item)) {
        Array.add(uniqueArray, item);
      }
    }
  }

  return uniqueArray;
};

$q.clearArray = function clearArray(array) {
  if ($q.isArray(array)) {
    Array.clear(array);
  }

  // eslint-disable-next-line no-param-reassign
  array = null;
};

$q.callFunction = function callFunction(callback, context) {
  if ($q.isFunction(callback)) {
    if (context) {
      return callback.call(context);
    }

    return callback();
  }

  return undefined;
};

$q.preventDefaultFunction = function preventDefaultFunction(e) {
  e.preventDefault();
};

$q.hashToQueryString = function hashToQueryString(hash) {
  let param, value;
  let counter = 0;
  let result = '';
  if (hash) {
    // eslint-disable-next-line guard-for-in, no-restricted-syntax
    for (param in hash) {
      value = $q.toString(hash[param], '');
      if (counter > 0) {
        result += '&';
      }

      result += `${param}=${escape(value)}`;
      counter += 1;
    }
  }

  return result;
};

$q.arrayToQueryString = function arrayToQueryString(parameterName, array) {
  let i, value;
  let result = '';
  if (!$q.isNullOrWhiteSpace(parameterName) && !$q.isNullOrEmpty(array)) {
    for (i = 0; i < array.length; i++) {
      value = $q.toString(array[i], '');
      if (i > 0) {
        result += '&';
      }

      result += `${parameterName}=${escape(value)}`;
    }
  }

  return result;
};

$q.updateQueryStringParameter = function updateQueryStringParameter(uri, key, value) {
  const re = new RegExp(`([?|&])${key}=.*?(&|$)`, 'i');
  const separator = uri.indexOf('?') === -1 ? '?' : '&';
  if (uri.match(re)) {
    return uri.replace(re, `$1${key}=${value}$2`);
  }

  return `${uri + separator + key}=${value}`;
};

$q.collectGarbageInIE = function collectGarbageInIE() {
  if ($.browser.msie) {
    window.CollectGarbage();
  }
};

$q.addRemoveToArrUniq = function addRemoveToArrUniq(arrToModify, valToAddRemove, shouldBeExcluded) {
  const underscoreMethod = shouldBeExcluded ? 'difference' : 'union';
  return _[underscoreMethod](arrToModify, valToAddRemove);
};

$q.bindProxies = function bindProxies(listOfFnNames, fnPostfix) {
  const postfix = fnPostfix || 'Handler';
  [].forEach.call(listOfFnNames, function eachFn(fnName) {
    try {
      this[fnName + postfix] = this[fnName].bind(this);
    } catch (e) {
      window.console.error(`Failed to register: ${fnName}`, e);
    }
  }, this);
};

$q.dispose = function dispose(listOfObjs) {
  [].forEach.call(listOfObjs, function eachOb(obj) {
    try {
      if (this[obj]) {
        this[obj] = null;
      }
    } catch (e) {
      window.console.error(`Failed to dispose: ${obj}`, e);
    }
  }, this);
};
