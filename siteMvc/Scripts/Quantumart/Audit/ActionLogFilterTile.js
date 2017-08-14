window.EVENT_TYPE_FILTER_TILE_CLOSE = "Quantumart.QP8.ActionLogFilterTile.onFilterTileClose";

Quantumart.QP8.ActionLogFilterTile = function (containerElement, options) {
	Quantumart.QP8.ActionLogFilterTile.initializeBase(this);
	this._containerElement = containerElement;
	this._options = Object.assign({
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
	$closeButton: null,
	$windowOpenLink: null,

	_popupWindowComponent: null,
	_filterComponent: null,

	_currentValue: null,

	initialize: function () {
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

		this.$tile = $(containerHeaderHtml.string());
		this.$closeButton = this.$tile.find(".closeButton");
		this.$closeButton.on("click", $.proxy(this._onCloseTileClick, this));
		this.$windowOpenLink = $(".open-link", this.$tile);
		this.$windowOpenLink.on("click", $.proxy(this._onOpenFilterWndClick, this));
		this.$filterDetailsSpanElement = $(".filter-details", this.$tile);

		$(this._containerElement).append(this.$tile);
	},

	get_value: function () {
		if (this._currentValue) {
			return this._currentValue;
		}
	},

	get_options: function () {
 return this._options;
},

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

		$(".closeFilter", this._popupWindowComponent.element).click($.proxy(this._onCloseFilterWndClick, this));
		$(".closeAndApplyFilter", this._popupWindowComponent.element).click($.proxy(this._onCloseAndApplyFilterWndClick, this));
		$("form", this._popupWindowComponent.element).submit($.proxy(this._onFilterFormSubmitted, this));


		this._filterComponent = this._options.createFilter($(".filterContainer", this._popupWindowComponent.element));
		this._filterComponent.initialize();
	},

	_onCloseTileClick: function () {
		this.notify(window.EVENT_TYPE_FILTER_TILE_CLOSE, { type: this._options.type });
	},

	_onOpenFilterWndClick: function () {
		if (this._popupWindowComponent) {
			this._popupWindowComponent.open();
			this._filterComponent.onOpen();
		} else {
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
	    $(this._containerElement).closest('form').find(".alSearchButton").trigger("click");
	},

	_onFilterFormSubmitted: function (e) {
	    e.preventDefault();
	    $(".closeAndApplyFilter", this._popupWindowComponent.element).trigger("click");
	    return false;
	},

	dispose: function () {
		Quantumart.QP8.ActionLogFilterTile.callBaseMethod(this, "dispose");

		if (this.$closeButton) {
			this.$closeButton.off("click");
		}

		if (this.$windowOpenLink) {
			this.$windowOpenLink.off("click");
		}

		if (this._filterComponent) {
			this._filterComponent.dispose();
		}

		if (this._popupWindowComponent) {
		    $(".closeFilter", this._popupWindowComponent.element).off("click");
		    $(".closeAndApplyFilter", this._popupWindowComponent.element).off("click");
		    $("form", this._popupWindowComponent.element).off("submit");
        $c.destroyPopupWindow(this._popupWindowComponent);
		}

		this.$tile.empty().remove();
	}
};

Quantumart.QP8.ActionLogFilterTile.registerClass("Quantumart.QP8.ActionLogFilterTile", Quantumart.QP8.Observable);
