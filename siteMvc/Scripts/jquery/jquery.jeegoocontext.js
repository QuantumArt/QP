// Copyright (c) 2009 - 2010 Erik van den Berg (http://www.planitworks.nl)
// Dual licensed under the MIT (http://www.opensource.org/licenses/mit-license.php)
// and GPL (http://www.opensource.org/licenses/gpl-license.php) licenses.
//
// Thanks to Denis Evteev for some excellent improvements.
//
// Version: 1.2.2
// Requires jQuery 1.3.2+

// Version: 1.2.2q
// Updated 18.08.2010 by Andrey Taritsyn
(function ($) {
	var _global;
	var _menus;

	jeegoocontext = function (element, options) {
		var _self = this;

		this._id = options.menuElementId;
		this._menuElement = $("#" + this._id).get(0);
		this._targetElement = element;

		if (!_global) {
			_global = {};
		}

		if (!_menus) {
			_menus = {};
		}

		this._onDocumentClickingHandler = function (e) {
			_self.hide(e);
		};

		this._onDocumentHoveringHandler = function (e) {
			var menuId = _self._id;
			var menu = _menus[menuId];

			// Exit if menu not exist
			if (!menu) {
				return false;
			}

			// Remove hovers from last-opened submenu and hide any open relatedTarget submenu's after timeout
			if ($(e.relatedTarget).parents("#" + menuId).length > 0) {
				// Clear show submenu timeout
				window.clearTimeout(menu.show);

				var $li = $(e.relatedTarget).parent().find("LI");
				$li.add($li.find("> *")).removeClass(menu.hoverClass);

				// Clear last hovered menu-item
				_menus[_global.activeId].currentHover = null;

				// Set hide submenu timeout
				menu.hide = window.setTimeout(
                    function () {
			$li.find("UL").hide();
                    },
                    menu.delay
                );
			}
		};

		this._onDocumentKeyDownHandler = function (e) {
			var activeMenuId = _global.activeId;
			var activeMenu = _menus[activeMenuId];

			switch (e.which) {
				case 38: // keyup
					if (_global.keyUpDownStop) {
						return false;
					}

					_self._onDocumentKeyUpDownInnerHandler();
					_global.keyUpDown = window.setInterval(_self._onDocumentKeyUpDownInnerHandler, activeMenu.keyDelay);
					_global.keyUpDownStop = true;

					return false;
				case 39: // keyright
					if (activeMenu.currentHover) {
						activeMenu.currentHover.find("UL:visible:first LI:visible:first").trigger("mouseover");
					}
					else {
						var $visibleMenus = $("#" + activeMenuId + ", #" + activeMenuId + " UL:visible");
						if ($visibleMenus.length > 0) {
							$($visibleMenus[$visibleMenus.length - 1]).find(":visible:first").trigger("mouseover");
						}

						$visibleMenus = null;
					}

					return false;
				case 40: // keydown
					if (_global.keyUpDownStop) {
						return false;
					}
					_self._onDocumentKeyUpDownInnerHandler(true);
					_global.keyUpDown = window.setInterval(
                        function () {
				_self._onDocumentKeyUpDownInnerHandler(true);
                        },
                        activeMenu.keyDelay
                    );
					_global.keyUpDownStop = true;

					return false;
				case 37: // keyleft
					if (activeMenu.currentHover) {
						$(activeMenu.currentHover.parents("LI")[0]).trigger("mouseover");
					}
					else {
						var $hoveredLi = $("#" + activeMenuId + " LI." + activeMenu.hoverClass);
						if ($hoveredLi.length > 0) {
							$($hoveredLi[$hoveredLi.length - 1]).trigger("mouseover");
						}

						$hoveredLi = null;
					}

					return false;
				case 13: // enter
					if (activeMenu.currentHover) {
						activeMenu.currentHover.trigger("click");
					}
					else {
						$(document).trigger("click");
					}
					break;
				case 27: // escape
					$(document).trigger("click");
					break;
				default:
					break;
			}
		};

		this._onDocumentKeyUpHandler = function (e) {
			window.clearInterval(_global.keyUpDown);
			_global.keyUpDownStop = false;
		};

		this._onContextMenuShowingHandler = function (e) {
			_self.show(e, this);

			e.stopPropagation();
			return false;
		};

		this._onContextMenuItemHoveringHandler = function (e) {
			var menuId = _self._id;
			var menu = _menus[menuId];

			// Exit if menu not exist
			if (!menu) {
				return false;
			}

			var $this = menu.currentHover = $(this);

			// Clear hide and show timeouts
			window.clearTimeout(menu.show);
			window.clearTimeout(menu.hide);

			// Clear all hover state
			jQuery(_self._menuElement).find("*").removeClass(menu.hoverClass);

			// Set hover state on self, direct children, ancestors and ancestor direct children
			var $parents = $this.parents("LI");
			$this
                .add($this.find("> *"))
                .add($parents)
                .add($parents.find("> *"))
                .addClass(menu.hoverClass)
                ;

			// Invoke onHover callback if set, 'this' refers to the hovered list-item.
			// Discontinue default behavior if callback returns false.
			var continueDefault = true;
			if (menu.onHover) {
				if (menu.onHover.apply(this, [e, menu.context]) == false) {
					continueDefault = false;
				}
			}

			// Continue after timeout(timeout is reset on every mouseover)
			if (!menu.proceed) {
				menu.show = window.setTimeout(
                    function () {
			menu.proceed = true;
			$this.trigger("mouseover");
                    },
                    menu.delay
                );

				e.stopPropagation();
				return false;
			}
			menu.proceed = false;

			// Hide all sibling submenu's and deeper level submenu's
			$this.parent().find("UL").not($this.find("> UL")).hide();

			if (!continueDefault) {
				e.preventDefault();
				return false;
			}

			// Default behavior
			// =================================================== //

			// Position and fade-in submenu's
			var $submenu = $this.find("> UL");
			if ($submenu.length != 0) {
				var offSet = $this.offset();

				var overflow = _self._getOverflow(
                    (offSet.left + $this.parent().width() + menu.submenuLeftOffset + $submenu.width() + menu.widthOverflowOffset),
                    (offSet.top + menu.submenuTopOffset + $submenu.height() + menu.heightOverflowOffset)
                );
				var parentWidth = $submenu.parent().parent().width();
				var y = offSet.top - $this.parent().offset().top;
				$submenu.css(
                    {
			"left": (overflow.width > 0) ? (-parentWidth - menu.submenuLeftOffset + "px") : (parentWidth + menu.submenuLeftOffset + "px"),
			"top": (overflow.height > 0) ? (y - overflow.height + menu.submenuTopOffset) + "px" : y + menu.submenuTopOffset + "px"
                    }
                );

				overflow = null;
				offSet = null;

				$submenu.fadeIn(menu.fadeIn);
			}

			$submenu = null;
			$this = null;

			e.stopPropagation();
		};

		this._onContextMenuItemClickingHandler = function (e) {
			var menuId = _self._id;
			var menu = _menus[menuId];

			// Exit if menu not exist
			if (!menu) {
				return false;
			}

			// Invoke onSelect callback if set, 'this' refers to the selected listitem.
			// Discontinue default behavior if callback returns false.
			if (menu.onSelect) {
				if (menu.onSelect.apply(this, [e, menu.context]) == false) {
					e.stopPropagation();
					return false;
				}
			}

			// Default behavior
			//====================================================//

			// Hide menu
			_self.hide(e);

			e.stopPropagation();
		};

		this.initialize(options);
	};

	jeegoocontext.prototype = {
		_id: "",
		_menuElement: null,
		_targetElement: null,

		_onDocumentClickingHandler: null,
		_onDocumentHoveringHandler: null,
		_onDocumentKeyDownHandler: null,
		_onDocumentKeyUpHandler: null,
		_onDocumentKeyUpDownInnerHandler: function (down) {
			var activeMenuId = _global.activeId;
			var activeMenu = _menus[activeMenuId];

			if (activeMenu.currentHover) {
				// Hover the first visible menu-item from the next or previous siblings and skip any separator items
				var $currentHover = activeMenu.currentHover;
				var $prevNext = down ?
				    $currentHover.nextAll(":not(." + activeMenu.separatorClass + "):visible:first")
                    :
				    $currentHover.prevAll(":not(." + activeMenu.separatorClass + "):visible:first");

				// If nothing is found, hover the first or last visible sibling
				if ($prevNext.length == 0) {
					$prevNext = $currentHover.parent().find("> LI:visible");
					$prevNext = (down ? $($prevNext[0]) : $($prevNext[$prevNext.length - 1]));
				}

				$prevNext.trigger("mouseover");

				$currentHover = null;
				$prevNext = null;
			}
			else {
				// Current hover is null, select the last visible submenu
				var $visibleMenus = $("#" + activeMenuId + ", #" + activeMenuId + " UL").filter(
                    function () {
			return ($(this).is(":visible") && $(this).parents(":hidden").length == 0);
                    }
                );

				if ($visibleMenus.length > 0) {
					// Find all visible menu-items for this menu and hover the first or last visible sibling
					var $visibleItems = $($visibleMenus[$visibleMenus.length - 1]).find("> LI:visible");
					$($visibleItems[(down ? 0 : ($visibleItems.length - 1))]).trigger("mouseover");

					$visibleItems = null;
				}

				$visibleMenus = null;
			}
		},
		_onContextMenuShowingHandler: null,
		_onContextMenuItemHoveringHandler: null,
		_onContextMenuItemClickingHandler: null,

		initialize: function (options) {
			var menuId = this._id;

			// Always override _global.menuClass if value is provided by options
			if (options && options.menuClass) {
				_global.menuClass = options.menuClass;
			}

			// Only set _global.menuClass if not set
			if (!_global.menuClass) {
				_global.menuClass = "jeegoocontext";
			}

			// Always override _global.activeClass if value is provided by options
			if (options && options.activeClass) {
				_global.activeClass = options.activeClass;
			}

			// Only set _global.activeClass if not set
			if (!_global.activeClass) {
				_global.activeClass = "active";
			}

			var menu = _menus[menuId] = $.extend({}, $.fn["jeegoocontext"].defaults, options);

			// All context bound to this menu
			menu.allContext = this._targetElement.selector;

			// Auto add submenu arrows(spans) if set by options
			if (menu.autoAddSubmenuArrows) {
				$(this._menuElement)
                    .find("LI:has(UL)")
                    .not(":has(." + menu.submenuClass + ")")
                    .prepend("<span class=\"" + menu.submenuClass + "\"></span>")
                    ;
			}

			this.detachContextMenuClickedItemsEventHandlers();
			this.attachContextMenuClickedItemsEventHandlers();
			if (!menu.allowManualShowing) {
				this._attachContextMenuEventHandlers();
			}
		},

		show: function (e, targetElement) {
			var menuId = this._id;
			var menu = _menus[menuId];

			// Exit if menu not exist
			if (!menu) {
				return false;
			}

			var menuElement = $(this._menuElement);

			// Check for overflow and correct menu-position accordingly
			var menuLeft = e.pageX;
			var menuTop = e.pageY;
			var overflow = this._getOverflow(
                (e.pageX + menuElement.width() + menu.widthOverflowOffset),
                (e.pageY + menuElement.height() + menu.heightOverflowOffset)
            );

			if (overflow.width > 0) {
				menuLeft -= overflow.width;
			}

			if (overflow.height > 0) {
				menuTop -= overflow.height;
			}

			overflow = null;

			this.showAt(e, targetElement, menuLeft, menuTop);
		},

		showAt: function (e, targetElement, x, y) {
			var menuId = this._id;
			var menu = _menus[menuId];

			// Exit if menu not exist
			if (!menu) {
				return false;
			}

			// Save context(i.e. the current area to which the menu belongs)
			menu.context = targetElement;

			var $menu = $(this._menuElement);

			// Invoke onShow callback if set, 'this' refers to the menu.
			// Discontinue default behavior if callback returns false.
			if (menu.onShow) {
				if (menu.onShow.apply($menu, [e, menu.context]) == false) {
					return false;
				}
			}

			// Default behavior
			// =================================================== //

			// Reset last active menu
			this._resetMenu();

			// Set this id as active menu id
			_global.activeId = menuId;

			// Hide current menu and all submenus, on first page load this is neccesary for proper keyboard support
			$("#" + _global.activeId)
                .add("#" + _global.activeId + " UL")
                .hide()
                ;

			// Clear all active context on page
			this._clearActive();

			// Make this context active
			$(menu.context).addClass(_global.activeClass);

			// Clear all hover state
			$menu.find("LI, LI > *").removeClass(menu.hoverClass);

			// QP improvement
			if (menu.onTune) {
				if (menu.onTune.apply($menu, [e, menu.context]) == false) {
					return false;
				}
			}
			// ---------------------

			// Fade-in menu at clicked-position
			$menu
                .css(
                    {
			"left": x + "px",
			"top": y + "px"
                    }
                )
                .fadeIn(menu.fadeIn)
                ;

			// Bind mouseover, keyup/keydown and click events to the document
			this._attachDocumentEventHandlers();
		},

		hide: function (e) {
			var activeMenuId = _global.activeId;
			var activeMenu = null;
			if (activeMenuId) {
				activeMenu = _menus[activeMenuId]
			}

			// Invoke onHide callback if set, 'this' refers to the menu.
			// Discontinue default behavior if callback returns false.
			if (activeMenu && activeMenu.onHide) {
				if (activeMenu.onHide.apply($("#" + activeMenuId), [e, activeMenu.context]) == false) {
					return false;
				}
			}

			// Default behavior
			// =================================================== //

			// Clear active context
			this._clearActive();

			// Hide active menu
			this._resetMenu();

			if (activeMenu && activeMenu.onHid) {
				(activeMenu.onHid.apply($("#" + activeMenuId), [e, activeMenu.context]));
			}
		},

		getContextMenuEventType: function () {
			var menuId = this._id;
			var menu = _menus[menuId];

			// Event type is a namespaced event so it can be easily unbound later
			var contextMenuEventType = menu.event;
			if (!contextMenuEventType) {
				contextMenuEventType = $.browser.opera ? menu.operaEvent : "contextmenu";
			}

			return contextMenuEventType;
		},

		_getContextMenuItemClickedSelectors: function () {
			var menuId = this._id;
			var menu = _menus[menuId];
			var menuItemClickedSelectors = "LI:not(." + menu.separatorClass + ")";

			return menuItemClickedSelectors;
		},

		// Detect overflow
		_getOverflow: function (x, y) {
			return {
				width: (x && parseInt(x)) ? (x - $(window).width() - $(window).scrollLeft()) : 0,
				height: (y && parseInt(y)) ? (y - $(window).height() - $(window).scrollTop()) : 0
			};
		},

		// Clear all active context
		_clearActive: function () {
			for (menuId in _menus) {
				$(_menus[menuId].allContext).removeClass(_global.activeClass);
			}
		},

		// Reset menu
		_resetMenu: function () {
			// Hide active menu and it's submenus
			var activeMenuId = _global.activeId;

			if (activeMenuId) {
				$("#" + activeMenuId).add("#" + activeMenuId + " UL").hide();
			}

			// Stop key up/down interval
			window.clearInterval(_global.keyUpDown);
			_global.keyUpDownStop = false;

			// Clear current hover
			if (_menus[activeMenuId]) {
				_menus[activeMenuId].currentHover = null;
			}

			// Clear active menu
			_global.activeId = null;

			// Unbind click and mouseover functions bound to the document
			this._detachDocumentEventHandlers();
		},

		_attachContextMenuEventHandlers: function () {
			$(this._targetElement)
                .bind(this.getContextMenuEventType(), this._onContextMenuShowingHandler);
		},

		_detachContextMenuEventHandlers: function () {
			$(this._targetElement)
                .bind(this.getContextMenuEventType(), this._onContextMenuShowingHandler);
		},

		attachContextMenuClickedItemsEventHandlers: function () {
			var $menu = jQuery(this._menuElement);
			var menuItemClickedSelectors = this._getContextMenuItemClickedSelectors();

			$menu
			    .delegate(menuItemClickedSelectors, "mouseover", this._onContextMenuItemHoveringHandler)
			    .delegate(menuItemClickedSelectors, "click", this._onContextMenuItemClickingHandler)
			    ;
		},

		detachContextMenuClickedItemsEventHandlers: function () {
			var $menu = jQuery(this._menuElement);
			var menuItemClickedSelectors = this._getContextMenuItemClickedSelectors();

			$menu
			    .undelegate(menuItemClickedSelectors, "mouseover", this._onContextMenuItemHoveringHandler)
			    .undelegate(menuItemClickedSelectors, "click", this._onContextMenuItemClickingHandler)
			    ;
		},

		_attachDocumentEventHandlers: function () {
			$(document)
				.bind("mouseover", this._onDocumentHoveringHandler)
				.bind("click", this._onDocumentClickingHandler)
				.bind("keydown", this._onDocumentKeyDownHandler)
				.bind("keyup", this._onDocumentKeyUpHandler)
			    ;
		},

		_detachDocumentEventHandlers: function () {
			$(document)
				.unbind("mouseover", this._onDocumentHoveringHandler)
				.unbind("click", this._onDocumentClickingHandler)
				.unbind("keydown", this._onDocumentKeyDownHandler)
				.unbind("keyup", this._onDocumentKeyUpHandler)
			    ;
		},

		dispose: function () {
			var menuId = this._id;

			if (_global.activeId == menuId) {
				_global.activeId = "";
			}

			this._detachDocumentEventHandlers();
			this.detachContextMenuClickedItemsEventHandlers();
			this._detachContextMenuEventHandlers();

			if (menuId && _menus[menuId] instanceof Object) {
				delete _menus[menuId];
			}

			this._menuElement = null;
			this._targetElement = null;

			this._onDocumentClickingHandler = null;
			this._onDocumentHoveringHandler = null;
			this._onDocumentKeyDownHandler = null;
			this._onDocumentKeyUpHandler = null;
			this._onContextMenuShowingHandler = null;
			this._onContextMenuItemHoveringHandler = null;
			this._onContextMenuItemClickingHandler = null;

		}
	};

	// jQuery extender
	$.fn.jeegoocontext = function (options) {
		var componentName = "jeegoocontext_" + options.menuElementId;

		return this.each(function () {
			var $$ = $(this);

			if (!$$.data(componentName)) {
				var component = new jeegoocontext(this, options);
				$$.data(componentName, component);
			}
		});
	};

	// default options
	$.fn.jeegoocontext.defaults = {
		menuElementId: "",
		hoverClass: "hover",
		submenuClass: "submenu",
		separatorClass: "separator",
		operaEvent: "dblclick",
		fadeIn: 100,
		delay: 300,
		keyDelay: 100,
		widthOverflowOffset: 0,
		heightOverflowOffset: 0,
		submenuLeftOffset: 0,
		submenuTopOffset: 0,
		autoAddSubmenuArrows: true,
		allowManualShowing: false
	};

	$.fn.jeegoocontext.getContextMenuEventType = function () {
		var options = $.fn["jeegoocontext"].defaults;
		var contextMenuEventType = $.browser.opera ? options.operaEvent : "contextmenu";

		options = null;

		return contextMenuEventType;
	};

})(jQuery);
