Quantumart.QP8.BackendPagePropertiesMediator = function (rootElementId) {
  let $componentElem = $(`#${rootElementId}`);
  let $cacheHours = $componentElem.find('.cache-hours-container');
  let $displayCacheHours = $componentElem.find('.display-cache-hours');

  let manageCacheHours = function () {
    if ($displayCacheHours.is(':checked')) {
      $cacheHours.show();
    } else {
      $cacheHours.hide();
    }
  };

  manageCacheHours();
  $displayCacheHours.change(manageCacheHours);

  function dispose() {
  }

  return {
    dispose: dispose
  };
};
