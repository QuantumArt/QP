// #region class ActionLogFilterTile
var EVENT_TYPE_FILTER_TILE_CLOSE = "Quantumart.QP8.ActionLogFilterTile.onFilterTileClose";

Quantumart.QP8.ActionLogFilterTile = function (containerElement, options) {
	Quantumart.QP8.ActionLogFilterTile.initializeBase(this);
	this._containerElement = containerElement;
	this._options = jQuery.extend(
	{
		title: 'Undefined',
		type: 0,
		windowSize: { w: 350, h: 125 },
		createFilter: function ($filterContainer) {
			return new Quantumart.QP8.ActionLogFilterBase($filterContainer);
		}
	}, options);
};

Quantumart.QP8.ActionLogFilterTile.prototype = {
	_containerElement: null,
	_options: null,

	$tile: null,
	$closeButton : null,
	$windowOpenLink: null,

	_popupWindowComponent: null,
	_filterComponent: null,

	_currentValue: null,

	initialize: function () {

		// создать заголовок контейнера
		var containerHeaderHtml = new $.telerik.stringBuilder();
		containerHeaderHtml
			.cat('<div class="filterTile">')
				.cat('<div class="filterTileContent group">')
					.cat('<div class="title"><a href="javascript:void(0)" class="open-link js">')
						.cat(this._options.title)
						.cat('<span class="filter-details"></span>')
					.cat('</a></div>')
					.cat('<div ')
						.cat(' class="closeButton"')
						.cat(' title="' + $q.htmlEncode($l.SearchBlock.closeFieldSearchContainerButtonText) + '"')
					.cat('>')
					.cat('</div>')
				.cat("</div>")
			.cat("</div>");
		this.$tile = jQuery(containerHeaderHtml.string());

		this.$closeButton = this.$tile.find(".closeButton");
		this.$closeButton.on("click", jQuery.proxy(this._onCloseTileClick, this));

		this.$windowOpenLink = jQuery(".open-link", this.$tile);
		this.$windowOpenLink.on("click", jQuery.proxy(this._onOpenFilterWndClick, this));

		this.$filterDetailsSpanElement = jQuery(".filter-details", this.$tile);


		// добавить на страницу
		jQuery(this._containerElement).append(this.$tile);
	},

	get_value: function () {
		if (this._currentValue) {
			return this._currentValue;
		}
	},

	get_options: function () { return this._options; },

	_createFilter: function () {
	    var html = new $.telerik.stringBuilder()
				.cat('<form class="formLayout alFilter">')
				.cat('<div class="filterContainer"></div>')
                .cat('<div>')
		    	.cat('<input class="button closeAndApplyFilter" type="button" value="' + $l.SearchBlock.closeAndApplyWndButtonText + '">')
	            .cat('<input class="button closeFilter" type="button" value="' + $l.SearchBlock.closeWndButtonText + '">')
		    	.cat('</div>')
				.cat('</form>')
				.string();

		this._popupWindowComponent = $.telerik.window.create({
			title: $l.SearchBlock.filterSettings + this._options.title,
			html: html,
			width: this._options.windowSize.w,
			height: this._options.windowSize.h,
			modal: true,
			resizable: false,
			draggable: false,
			visible: true
		}).data("tWindow").center();

		jQuery(".closeFilter", this._popupWindowComponent.element).click(jQuery.proxy(this._onCloseFilterWndClick, this));
		jQuery(".closeAndApplyFilter", this._popupWindowComponent.element).click(jQuery.proxy(this._onCloseAndApplyFilterWndClick, this));
		jQuery("form", this._popupWindowComponent.element).submit(jQuery.proxy(this._onFilterFormSubmitted, this));


		this._filterComponent = this._options.createFilter(jQuery(".filterContainer", this._popupWindowComponent.element));
		this._filterComponent.initialize();
	},

	_onCloseTileClick: function () {
		this.notify(EVENT_TYPE_FILTER_TILE_CLOSE, { "type": this._options.type });
	},

	_onOpenFilterWndClick: function () {
		if (this._popupWindowComponent) {
			this._popupWindowComponent.open();
			this._filterComponent.onOpen();
		}
		else {
			this._createFilter();
		}
	},

	_onCloseFilterWndClick: function () {
		this._currentValue = this._filterComponent.get_value();
		this.$filterDetailsSpanElement.html(": " + this._filterComponent.get_filterDetails());
		this._popupWindowComponent.close();
	},

	_onCloseAndApplyFilterWndClick: function () {
	    this._onCloseFilterWndClick();
	    jQuery(this._containerElement).closest('form').find(".alSearchButton").trigger("click");
	},

	_onFilterFormSubmitted: function (e) {
	    e.preventDefault();
	    jQuery(".closeAndApplyFilter", this._popupWindowComponent.element).trigger("click");
	    return false;
	},


	dispose: function () {
		Quantumart.QP8.ActionLogFilterTile.callBaseMethod(this, "dispose");

		if(this.$closeButton) {
			this.$closeButton.off("click");
			this.$closeButton = null;
		}

		if (this.$windowOpenLink) {
			this.$windowOpenLink.off("click");
			this.$windowOpenLink = null;
		}

		if (this._filterComponent) {
			this._filterComponent.dispose();
			this._filterComponent = null;
		}

		if (this._popupWindowComponent) {
		    jQuery(".closeFilter", this._popupWindowComponent.element).off("click");
		    jQuery(".closeAndApplyFilter", this._popupWindowComponent.element).off("click");
		    jQuery("form", this._popupWindowComponent.element).off("submit");
			$c.destroyPopupWindow(this._popupWindowComponent);
			this._popupWindowComponent = null;
		}

		this.$tile.empty().remove();
		this.$tile = null;
	}
};

Quantumart.QP8.ActionLogFilterTile.registerClass("Quantumart.QP8.ActionLogFilterTile", Quantumart.QP8.Observable);
// #endregion
