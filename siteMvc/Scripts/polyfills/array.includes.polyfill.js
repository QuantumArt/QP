if (!Array.prototype.includes) {
  Array.prototype.includes = function (searchElement /*, fromIndex*/) {
    'use strict';
    if (this == null) {
      throw new TypeError('Array.prototype.includes called on null or undefined');
    }

    var obj = Object(this);
    var len = parseInt(obj.length, 10) || 0;
    if (len === 0) {
      return false;
    }

    var k;
    var n = parseInt(arguments[1], 10) || 0;
    if (n >= 0) {
      k = n;
    } else {
      k = len + n;
      if (k < 0) { k = 0; }
    }

    var currentElement;
    while (k < len) {
      currentElement = obj[k];
      if (searchElement === currentElement || (searchElement !== searchElement && currentElement !== currentElement)) {
        return true;
      }

      k++;
    }
    return false;
  };
}
