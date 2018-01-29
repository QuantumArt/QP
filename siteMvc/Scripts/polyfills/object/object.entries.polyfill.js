if (!Object.entries) {
  Object.entries = function (target) {
    'use strict';
    if (target == null) {
      throw new TypeError('Cannot convert undefined or null to object');
    }

    target = Object(target);
    var entries = [];
    for (var key in target) {
      if (Object.prototype.hasOwnProperty.call(target, key)) {
        entries.push([key, target[key]]);
      }
    }

    return entries;
  };
}
