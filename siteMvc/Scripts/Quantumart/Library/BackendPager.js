window.EVENT_TYPE_PAGE_NUMBER_CHANGED = 'OnPageNumberChanged';
Quantumart.QP8.BackendPagerEventArgs = function (pageNumber) {
	Quantumart.QP8.BackendPagerEventArgs.initializeBase(this);
	this._pageNumber = pageNumber;
};

Quantumart.QP8.BackendPagerEventArgs.prototype = {
	_pageNumber: 0,
	get_PageNumber: function () {
		return this._pageNumber;
	}
};

Quantumart.QP8.BackendPagerEventArgs.registerClass('Quantumart.QP8.BackendPagerEventArgs', Sys.EventArgs);
Quantumart.QP8.BackendPager = function (pagerElement) {
	Quantumart.QP8.BackendPager.initializeBase(this);

	this._pagerElement = pagerElement;
	this._onPageClickHandler = $.proxy(this._onPageClick, this);
	this._onInHoverHandler = $.proxy(this._onInHover, this);
	this._onOutHoverHandler = $.proxy(this._onOutHover, this);
};

Quantumart.QP8.BackendPager.prototype = {
	_pagerElement: null,
	_arrowFirstElement: null,
	_arrowPrevElement: null,
	_arrowLastElement: null,
	_arrowNextElement: null,
	_pageFrameElement: null,
	_statusTextElement: null,

	_totalCount: 0,
	_pageSize: 10,
	_pageFrameSize: 10,

	_pageCount: 0,
	_pageFrameCount: 0,

	_currentPageNumber: 0,
	_currentPageFrameNumber: 0,

	_currentFrameStartPageNumber: 0,
	_currentFrameEndPageNumber: 0,

	_pageFrameRedraw: function () {
		var $pageFrameElement = $(this._pageFrameElement);
		$pageFrameElement.children().remove();

		var html = new $.telerik.stringBuilder();
		html.catIf('<a class="t-link qp-link-prev-frame qp-page-link">...</a>', this._currentPageFrameNumber > 0);

		if (this._totalCount > 0) {
			for (var pn = this._currentFrameStartPageNumber; pn <= this._currentFrameEndPageNumber; pn++) {
				if (pn != this._currentPageNumber) {
 html.cat('<a class="t-link qp-page-link">').cat(pn + 1).cat('</a>');
} else {
 html.cat('<span class="t-state-active">').cat(pn + 1).cat('</span>');
}
			}
		}

		html.catIf('<a class="t-link qp-link-next-frame qp-page-link">...</a>', this._currentPageFrameNumber < this._pageFrameCount - 1);

		$pageFrameElement.html(html.string());

		$pageFrameElement = null;
		html = null;
	},

	_setPageNumber: function (pageNumber) {
		this.set({ currentPageNumber: pageNumber });
		this.redraw();
	},

	initialize: function () {
		var $pagerElement = $(this._pagerElement);
		$pagerElement.addClass('t-widget t-grid');

		var innerHtml = '<div class="t-grid-pager t-grid-bottom">'
							+ '<div class="t-pager t-reset">'
								+ '<a href="#" class="t-link qp-link-arrow-first qp-page-link"><span class="t-icon t-arrow-first">first</span></a>'
								+ '<a href="#" class="t-link qp-link-arrow-prev qp-page-link"><span class="t-icon t-arrow-prev">prev</span></a>'
								+ '<div class="t-numeric"></div>'
								+ '<a href="#" class="t-link qp-link-arrow-next qp-page-link"><span class="t-icon t-arrow-next">next</span></a>'
								+ '<a href="#" class="t-link qp-link-arrow-last qp-page-link"><span class="t-icon t-arrow-last">last</span></a>'
							+ '</div>'
							+ '<div class="t-status-text">'
						+ '</div>';

		$pagerElement.html(innerHtml);

		this._arrowFirstElement = $pagerElement.find('span.t-arrow-first').closest('a.t-link').get(0);
		this._arrowPrevElement = $pagerElement.find('span.t-arrow-prev').closest('a.t-link').get(0);
		this._arrowNextElement = $pagerElement.find('span.t-arrow-next').closest('a.t-link').get(0);
		this._arrowLastElement = $pagerElement.find('span.t-arrow-last').closest('a.t-link').get(0);

		$pagerElement
			.delegate('a.qp-page-link', 'click', this._onPageClickHandler)
			.delegate('a.qp-page-link', 'mouseenter', this._onInHoverHandler)
			.delegate('a.qp-page-link', 'mouseleave', this._onOutHoverHandler);

		this._pageFrameElement = $pagerElement.find('div.t-numeric').get(0);
		this._statusTextElement = $pagerElement.find('div.t-status-text').get(0);

		this.set();
		this.redraw();

		$pagerElement = null;
	},

	set: function (options) {
		if ($q.isObject(options)) {
			if (!$q.isNull(options.totalCount)) {
 this._totalCount = $q.toInt(options.totalCount < 0 ? 0 : options.totalCount);
}
			if (!$q.isNull(options.pageSize)) {
 this._pageSize = $q.toInt(options.pageSize < 1 ? 1 : options.pageSize);
}
			if (!$q.isNull(options.pageFrameSize)) {
 this._pageFrameSize = $q.toInt(options.pageFrameSize < 1 ? 1 : options.pageFrameSize);
}
			if (!$q.isNull(options.currentPageNumber)) {
 this._currentPageNumber = $q.toInt(options.currentPageNumber < 0 ? 0 : options.currentPageNumber);
}
		}

		this._pageCount = Math.floor(this._totalCount / this._pageSize) + (this._totalCount % this._pageSize == 0 ? 0 : 1);

		if (this._currentPageNumber >= this._pageCount) {
 this._currentPageNumber = this._pageCount - 1;
}
		if (this._currentPageNumber < 0) {
 this._currentPageNumber = 0;
}

		this._pageFrameCount = Math.floor(this._pageCount / this._pageFrameSize) + (this._pageCount % this._pageFrameSize == 0 ? 0 : 1);
		this._currentPageFrameNumber = Math.floor(this._currentPageNumber / this._pageFrameSize);
		this._currentFrameStartPageNumber = Quantumart.QP8.BackendPager.getFrameStartPageNumber(this._currentPageFrameNumber, this._pageFrameSize);
		this._currentFrameEndPageNumber = Quantumart.QP8.BackendPager.getFrameEndPageNumber(this._currentPageFrameNumber, this._pageFrameSize, this._pageCount);
	},

	redraw: function () {
		if (this._currentPageNumber == 0) {
			$(this._arrowFirstElement).addClass('t-state-disabled').removeClass('t-state-hover');
			$(this._arrowPrevElement).addClass('t-state-disabled').removeClass('t-state-hover');
		} else {
			$(this._arrowFirstElement).removeClass('t-state-disabled');
			$(this._arrowPrevElement).removeClass('t-state-disabled');
		}

		if (this._currentPageNumber >= this._pageCount - 1) {
			$(this._arrowLastElement).addClass('t-state-disabled').removeClass('t-state-hover');
			$(this._arrowNextElement).addClass('t-state-disabled').removeClass('t-state-hover');
		} else {
			$(this._arrowLastElement).removeClass('t-state-disabled');
			$(this._arrowNextElement).removeClass('t-state-disabled');
		}

		if (this._totalCount > 0) {
 $(this._statusTextElement).html(
  String.format(
    $l.Pager.statusTextTemplate,
		(this._currentPageNumber * this._pageSize) + 1,
		Math.min((this._currentPageNumber * this._pageSize) + this._pageSize, this._totalCount),
		this._totalCount
  ));
} else {
 $(this._statusTextElement).html(String.format($l.Pager.statusTextTemplate, 0, 0, 0));
}

		this._pageFrameRedraw();
	},

	get_pageCount: function () {
		return this._pageCount;
	},

	get_pageNumber: function () {
		return this._currentPageNumber;
	},

	get_pageSize: function () {
		return this._pageSize;
	},


	_onInHover: function (e) {
		var $e = $(e.currentTarget);
		if (!$e.hasClass('t-state-disabled')) {
 $e.addClass('t-state-hover');
}
	},

	_onOutHover: function (e) {
		$(e.currentTarget).removeClass('t-state-hover');
	},

	_onPageClick: function (e) {
		var $selectedPage = $(e.currentTarget);

		if (!$selectedPage.hasClass('t-state-disabled')) {
			var newPageNumber = 0;

			if ($selectedPage.hasClass('qp-link-arrow-first')) {
 newPageNumber = 0;
} else if ($selectedPage.hasClass('qp-link-arrow-prev')) {
 newPageNumber = this._currentPageNumber - 1;
} else if ($selectedPage.hasClass('qp-link-arrow-next')) {
 newPageNumber = this._currentPageNumber + 1;
} else if ($selectedPage.hasClass('qp-link-arrow-last')) {
 newPageNumber = this._pageCount - 1;
} else if ($selectedPage.hasClass('qp-link-prev-frame')) {
 newPageNumber = Quantumart.QP8.BackendPager.getFrameEndPageNumber(this._currentPageFrameNumber - 1, this._pageFrameSize, this._pageCount);
} else if ($selectedPage.hasClass('qp-link-next-frame')) {
 newPageNumber = Quantumart.QP8.BackendPager.getFrameStartPageNumber(this._currentPageFrameNumber + 1, this._pageFrameSize);
} else {
 newPageNumber = $q.toInt($selectedPage.html()) - 1;
}

			var eventArgs = new Quantumart.QP8.BackendPagerEventArgs(newPageNumber);
			this.notify(window.EVENT_TYPE_PAGE_NUMBER_CHANGED, eventArgs);
			eventArgs = null;
		}

		$selectedPage = null;
	},

	dispose: function () {
		var $pagerElement = $(this._pagerElement);
		$pagerElement.find('a.qp-page-link').undelegate();

		$pagerElement = null;
		this._arrowFirstElement = null;
		this._arrowLastElement = null;
		this._arrowNextElement = null;
		this._arrowPrevElement = null;
		this._pageFrameElement = null;
		this._pagerElement = null;
		this._statusTextElement = null;

		this._onPageClickHandler = null;
		this._onInHoverHandler = null;
		this._onOutHoverHandler = null;
	}
};

Quantumart.QP8.BackendPager.getFrameStartPageNumber = function (pageFrameNumber, pageFrameSize) {
	return pageFrameNumber * pageFrameSize;
};

Quantumart.QP8.BackendPager.getFrameEndPageNumber = function (pageFrameNumber, pageFrameSize, pageCount) {
	return Math.min(
    (pageFrameNumber * pageFrameSize) + (pageFrameSize === 0 ? 0 : pageFrameSize - 1),
    pageCount === 0 ? 0 : pageCount - 1
  );
};

Quantumart.QP8.BackendPager.registerClass('Quantumart.QP8.BackendPager', Quantumart.QP8.Observable);
