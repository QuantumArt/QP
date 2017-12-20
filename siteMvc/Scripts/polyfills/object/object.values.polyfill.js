if (!Object.values) {
  Object.values = function (target) {
    'use strict';
    if (target == null) {
      throw new TypeError('Cannot convert undefined or null to object');
    }

    target = Object(target);
    var values = [];
    for (var key in target) {
      if (Object.prototype.hasOwnProperty.call(target, key)) {
        values.push(target[key]);
      }
    }

    return values;
  };
}
