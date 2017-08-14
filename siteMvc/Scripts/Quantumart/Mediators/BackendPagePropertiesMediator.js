Quantumart.QP8.BackendPagePropertiesMediator = function (rootElementId) {
  let $componentElem = $(`#${rootElementId}`);
  let $cacheHours = $componentElem.find('.cache-hours-container');
  let $displayCacheHours = $componentElem.find('.display-cache-hours');

  manageCacheHours();

  $displayCacheHours.change(manageCacheHours);

  function manageCacheHours() {
    if ($displayCacheHours.is(':checked')) {
      $cacheHours.show();
    } else {
      $cacheHours.hide();
    }
  }

  function dispose() {
    $componentElem = null;
    $cacheHours = null;
    $displayCacheHours = null;
  }

  return {
    dispose: dispose
  };
};
