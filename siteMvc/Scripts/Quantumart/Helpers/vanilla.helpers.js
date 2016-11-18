(function(Global) {
  'use strict';

  /**
   * a function, that, as long as it continues to be invoked, will not
   * be triggered. The function will be called after it stops being called for
   * n milliseconds.
   * @param  {Function} fn            a func to be debounced
   * @param  {Number}   delay         milliseconds for debounce
   * @param  {Boolean}  isImmediate   if is passed, trigger the function on the leading edge,
   *                                  instead of the trailing
   */
  Global.debounce = function(fn, delay, isImmediate) {
    var timeout;
    return function() {
      var _this = this;
      var args = arguments;
      function later() {
        timeout = null;
        if (!isImmediate) {
          fn.apply(_this, args);
        }
      }

      var callNow = isImmediate && !timeout;
      clearTimeout(timeout);
      timeout = setTimeout(later, delay);
      if (callNow) {
        fn.apply(_this, args);
      }
    };
  };

  function getUrlHelpers() {
    var rootPath;
    return {
      Content: function(relativeUrl) {
        if (relativeUrl.substring(0, 1) === '~') {
          relativeUrl = relativeUrl.substring(1);
        }

        if (relativeUrl.substring(0, 1) === '/') {
          relativeUrl = relativeUrl.substring(1);
        }

        return rootPath + relativeUrl;
      },
      SetRootPath: function(rootUrl) {
        rootPath = rootUrl;
      }
    };
  }

  window.Url = window.Global.UrlHelpers = getUrlHelpers();
}(window.Global = window.Global || {}));
