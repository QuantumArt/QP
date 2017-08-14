Quantumart.QP8.BackendPagePropertiesMediator = function (rootElementId) {
    var $componentElem = $('#' + rootElementId);
    var $cacheHours = $componentElem.find('.cache-hours-container');
    var $displayCacheHours = $componentElem.find('.display-cache-hours');

    manageCacheHours();

    $displayCacheHours.change(manageCacheHours);

    function manageCacheHours() {
        if ($displayCacheHours.is(":checked")) {
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
