Quantumart.QP8.BackendPagePropertiesMediator = function (rootElementId) {
  const $componentElem = $(`#${rootElementId}`);
  const $cacheHours = $componentElem.find('.cache-hours-container');
  const $displayCacheHours = $componentElem.find('.display-cache-hours');

  const manageCacheHours = function () {
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
    dispose
  };
};
