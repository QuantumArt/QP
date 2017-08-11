// #region event types of pager
// === Типы событий пейджера ===
var EVENT_TYPE_PAGE_NUMBER_CHANGED = "OnPageNumberChanged";

// #endregion

// #region class BackendPagerEventArgs
// === Класс "Аргументы события, вызванного пейджером" ===
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

Quantumart.QP8.BackendPagerEventArgs.registerClass("Quantumart.QP8.BackendPagerEventArgs", Sys.EventArgs);

// #endregion

// #region class Pager
// === Компонент Pager
Quantumart.QP8.BackendPager = function (pagerElement) {
	Quantumart.QP8.BackendPager.initializeBase(this);

	this._pagerElement = pagerElement;

	this._onPageClickHandler = jQuery.proxy(this._onPageClick, this);
	this._onInHoverHandler = jQuery.proxy(this._onInHover, this);
	this._onOutHoverHandler = jQuery.proxy(this._onOutHover, this);

};

Quantumart.QP8.BackendPager.prototype = {
	_pagerElement: null, // корневой dom-элемент компонента
	_arrowFirstElement: null,
	_arrowPrevElement: null,
	_arrowLastElement: null,
	_arrowNextElement: null,
	_pageFrameElement: null,
	_statusTextElement: null,

	_totalCount: 0, // общее количество записей
	_pageSize: 10, // размер страницы
	_pageFrameSize: 10, // размер фрейма страниц (максимальное количество страниц показываемое в пейджере одновременно)

	_pageCount: 0, // количество страниц
	_pageFrameCount: 0, // количество фреймов

	_currentPageNumber: 0, // номер выбранной страницы
	_currentPageFrameNumber: 0, // номер тукущего фрейма

	_currentFrameStartPageNumber: 0,
	_currentFrameEndPageNumber: 0,

	_pageFrameRedraw: function () {
		var $pageFrameElement = jQuery(this._pageFrameElement);
		$pageFrameElement.children().remove();

		var html = new $.telerik.stringBuilder();

		// Если текущий фрейм не первый, то вставляем вначале "..."
		html.catIf('<a class="t-link qp-link-prev-frame qp-page-link">...</a>', this._currentPageFrameNumber > 0);

		// Генерируем номера страниц
		if (this._totalCount > 0) {
			for (var pn = this._currentFrameStartPageNumber; pn <= this._currentFrameEndPageNumber; pn++) {
				if (pn != this._currentPageNumber) {
 html.cat('<a class="t-link qp-page-link">').cat((pn + 1)).cat('</a>'); 
} else {
 html.cat('<span class="t-state-active">').cat((pn + 1)).cat('</span>'); 
}
			}
		}

		// Если текущий фрейм не последний, то вставляем конце "..."
		html.catIf('<a class="t-link qp-link-next-frame qp-page-link">...</a>', this._currentPageFrameNumber < this._pageFrameCount - 1);

		// визуализация
		$pageFrameElement.html(html.string());

		$pageFrameElement = null;
		html = null;
	},

 // перерисовать фрейм страниц
	_setPageNumber: function (pageNumber) {
		this.set({ currentPageNumber: pageNumber });
		this.redraw();
	},

 // установить номер страницы


	initialize: function () {
		var $pagerElement = jQuery(this._pagerElement);

		$pagerElement.addClass("t-widget t-grid");

		var innerHtml = '<div class="t-grid-pager t-grid-bottom">' +
							'<div class="t-pager t-reset">' +
								'<a href="#" class="t-link qp-link-arrow-first qp-page-link"><span class="t-icon t-arrow-first">first</span></a>' +
								'<a href="#" class="t-link qp-link-arrow-prev qp-page-link"><span class="t-icon t-arrow-prev">prev</span></a>' +
								'<div class="t-numeric"></div>' +
								'<a href="#" class="t-link qp-link-arrow-next qp-page-link"><span class="t-icon t-arrow-next">next</span></a>' +
								'<a href="#" class="t-link qp-link-arrow-last qp-page-link"><span class="t-icon t-arrow-last">last</span></a>' +
							'</div>' +
							'<div class="t-status-text">' +
						'</div>';
		$pagerElement.html(innerHtml);

		this._arrowFirstElement = $pagerElement.find('span.t-arrow-first').closest('a.t-link').get(0);
		this._arrowPrevElement = $pagerElement.find('span.t-arrow-prev').closest('a.t-link').get(0);
		this._arrowNextElement = $pagerElement.find('span.t-arrow-next').closest('a.t-link').get(0);
		this._arrowLastElement = $pagerElement.find('span.t-arrow-last').closest('a.t-link').get(0);

		// привязываем события
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

 // начальная инициализация
	set: function (options) {
		if ($q.isObject(options)) {
			// установитьновые значения свойств
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

		// Посчитать количество страниц
		this._pageCount = Math.floor(this._totalCount / this._pageSize) + (this._totalCount % this._pageSize == 0 ? 0 : 1);

		// нормализовать номер текущей страницы
		if (this._currentPageNumber >= this._pageCount) {
 this._currentPageNumber = this._pageCount - 1; 
}
		if (this._currentPageNumber < 0) {
 this._currentPageNumber = 0; 
}

		// посчитать количество фреймов
		this._pageFrameCount = Math.floor(this._pageCount / this._pageFrameSize) + (this._pageCount % this._pageFrameSize == 0 ? 0 : 1);

		// почитать номер текущего фрейма
		this._currentPageFrameNumber = Math.floor(this._currentPageNumber / this._pageFrameSize);

		// первая страница текущего фрейма
		this._currentFrameStartPageNumber = Quantumart.QP8.BackendPager.getFrameStartPageNumber(this._currentPageFrameNumber, this._pageFrameSize);

		// последняя страница текущего фрейма
		this._currentFrameEndPageNumber = Quantumart.QP8.BackendPager.getFrameEndPageNumber(this._currentPageFrameNumber, this._pageFrameSize, this._pageCount);

	},

 // устанавливает новое состояние пейджера
	redraw: function () {
		// если текущая страница - первая - то дизейблим соответствующие arrow
		if (this._currentPageNumber == 0) {
			jQuery(this._arrowFirstElement).addClass("t-state-disabled").removeClass('t-state-hover');
			jQuery(this._arrowPrevElement).addClass("t-state-disabled").removeClass('t-state-hover'); ;
		} else {
			jQuery(this._arrowFirstElement).removeClass("t-state-disabled");
			jQuery(this._arrowPrevElement).removeClass("t-state-disabled");
		}

		// если текущая страница - последняя - то дизейблим соответствующие arrow
		if (this._currentPageNumber >= this._pageCount - 1) {
			jQuery(this._arrowLastElement).addClass("t-state-disabled").removeClass('t-state-hover'); ;
			jQuery(this._arrowNextElement).addClass("t-state-disabled").removeClass('t-state-hover'); ;
		} else {
			jQuery(this._arrowLastElement).removeClass("t-state-disabled");
			jQuery(this._arrowNextElement).removeClass("t-state-disabled");
		}

		// вывести статус
		if (this._totalCount > 0) {
 jQuery(this._statusTextElement).html(String.format($l.Pager.statusTextTemplate,
				this._currentPageNumber * this._pageSize + 1,
				Math.min(this._currentPageNumber * this._pageSize + this._pageSize, this._totalCount),
				this._totalCount)); 
} else {
 jQuery(this._statusTextElement).html(String.format($l.Pager.statusTextTemplate, 0, 0, 0)); 
}

		// нарисовать фрейм страниц
		this._pageFrameRedraw();
	},

 // перерисовать пейджер
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
		var $e = jQuery(e.currentTarget);
		if (!$e.hasClass('t-state-disabled')) {
 $e.addClass("t-state-hover"); 
}
	},

	_onOutHover: function (e) {
		jQuery(e.currentTarget).removeClass("t-state-hover");
	},

	_onPageClick: function (e) {
		var $selectedPage = jQuery(e.currentTarget);

		if (!$selectedPage.hasClass('t-state-disabled')) {
			var newPageNumber = 0;

			if ($selectedPage.hasClass("qp-link-arrow-first")) {
 newPageNumber = 0; 
} else if ($selectedPage.hasClass("qp-link-arrow-prev")) {
 newPageNumber = this._currentPageNumber - 1; 
} else if ($selectedPage.hasClass("qp-link-arrow-next")) {
 newPageNumber = this._currentPageNumber + 1; 
} else if ($selectedPage.hasClass("qp-link-arrow-last")) {
 newPageNumber = this._pageCount - 1; 
} else if ($selectedPage.hasClass("qp-link-prev-frame")) {
 newPageNumber = Quantumart.QP8.BackendPager.getFrameEndPageNumber(this._currentPageFrameNumber - 1, this._pageFrameSize, this._pageCount); 
} else if ($selectedPage.hasClass("qp-link-next-frame")) {
 newPageNumber = Quantumart.QP8.BackendPager.getFrameStartPageNumber(this._currentPageFrameNumber + 1, this._pageFrameSize); 
} else {
 newPageNumber = $q.toInt($selectedPage.html()) - 1; 
}

			var eventArgs = new Quantumart.QP8.BackendPagerEventArgs(newPageNumber);
			this.notify(EVENT_TYPE_PAGE_NUMBER_CHANGED, eventArgs);
			eventArgs = null;
		}

		$selectedPage = null;
	},


	dispose: function () {
		var $pagerElement = jQuery(this._pagerElement);
		$pagerElement.find("a.qp-page-link").undelegate();

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
	} // dispose
};

Quantumart.QP8.BackendPager.getFrameStartPageNumber = function (pageFrameNumber, pageFrameSize) {
	return pageFrameNumber * pageFrameSize;
};

Quantumart.QP8.BackendPager.getFrameEndPageNumber = function (pageFrameNumber, pageFrameSize, pageCount) {
	return Math.min(pageFrameNumber * pageFrameSize + (pageFrameSize == 0 ? 0 : pageFrameSize - 1), pageCount == 0 ? 0 : pageCount - 1);
};

Quantumart.QP8.BackendPager.registerClass("Quantumart.QP8.BackendPager", Quantumart.QP8.Observable);

// #endregion
