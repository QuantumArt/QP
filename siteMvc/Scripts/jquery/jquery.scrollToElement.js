/*
 * ScrollToElement 1.0
 * Copyright (c) 2009 Lauri Huovila, Neovica Oy
 *  lauri.huovila@neovica.fi
 *  http://www.neovica.fi
 *
 * Dual licensed under the MIT and GPL licenses.
 */

(function($) {
    $.scrollToElement = function( $element, speed ) {

        speed = speed || 750;

        $("html, body").animate({
            scrollTop: $element.offset().top,
            scrollLeft: $element.offset().left
        }, speed);
        return $element;
    };

    $.fn.scrollTo = function( speed ) {
        speed = speed || "normal";
        return $.scrollToElement( this, speed );
    };
})(jQuery);