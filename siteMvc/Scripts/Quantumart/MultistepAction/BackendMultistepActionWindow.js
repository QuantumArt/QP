var EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELING = "OnMultistepActionWindowCanceling";
var EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELED = "OnMultistepActionWindowCanceled";
var EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CLOSED = "OnMultistepActionWindowClosed";

Quantumart.QP8.BackendMultistepActionWindow = function (actionName, shortActionName) {
	Quantumart.QP8.BackendMultistepActionWindow.initializeBase(this);

	this._actionName = actionName;
	this._shortActionName = shortActionName;

	this._startStageTime = new Date();
};

Quantumart.QP8.BackendMultistepActionWindow.prototype = {
    _actionName: "", //  название действия
    _shortActionName : "", // краткое название действия
	_stagesCount: 0, // Всего этапов в действии
	_stagesRemaining: 0, // осталось этапов

	_stageName: "", // имя текущего этапа
	_stageStepsCount: 0, // Количество шагов на текущем этапе
	_stageStepsRemaining: 0, // Осталось шагов на текущем этапе
	_stageItemsCount: 0, // Всего элементов на текущем этапе
	_stageItemsRemaining: 0, // Осталось элементов на этапе

	_startStageTime: null, // используеться для вычисления времени

	_popupWindowElement: null, // DOM-элемент, образующий всплывающее окно
	_popupWindowComponent: null, // компонент "Всплывающее окно"

	_progressBarComponent: null, // progress bar
	_progressBarElement: null,

	_stageNameElement: null,
	_stageRemainingElement: null,
	_stageItemsElement: null,
	_stageElapsedTimeElement: null,
	_stageTimeRemainingElement: null,
	_stageAdditionalInfoElement: null,

    _additionalInfo: null,
	_windowTitle: $l.MultistepAction.progressWindowTitle,
	_windowWidth: 500,
	_windowHeight: 250,
	_zIndex: undefined,
    _parentId: null,

	initialize: function () {
		this._popupWindowComponent = this._createWindow();
		this._popupWindowElement = this._popupWindowComponent.element;

		var $mainContainer = jQuery(".lop-main", this._popupWindowElement);

		$mainContainer.find(".lop-action-name").text(this._shortActionName == null ? this._actionName : this._shortActionName);

		var $stageNameElement = $mainContainer.find(".lop-stage-name dd");
		$stageNameElement.text($l.MultistepAction.setupStageName);
		this._stageNameElement = $stageNameElement.get(0);

		this._stageRemainingElement = $mainContainer.find(".lop-stage-remaining dd").get(0);
		this._stageItemsElement = $mainContainer.find(".lop-stage-items dd").get(0);
		this._stageElapsedTimeElement = $mainContainer.find(".lop-elapsed-time dd").get(0);
		this._stageTimeRemainingElement = $mainContainer.find(".lop-time-remaining dd").get(0);
		this._stageAdditionalInfoElement = $mainContainer.find(".lop-additional-info dd").get(0);
		var $progressBarElement = $mainContainer.find(".lop-pbar");
		this._progressBarElement = $progressBarElement.get(0);
		$progressBarElement.backendProgressBar();
		this._progressBarComponent = $progressBarElement.data("backendProgressBar");

		this._cancelButtonElement = $mainContainer.find(".lop-cancel-button")
			.click(jQuery.proxy(this._onCancelClicked, this))
			.get(0);

		$progressBarElement = null;
		$mainContainer = null;
	},

	_refreshView: function () {
		this._progressBarComponent.total(this._stageItemsCount);
		this._progressBarComponent.value(this._stageItemsCount - this._stageItemsRemaining);

		this._progressBarComponent.refresh();

		jQuery(this._stageNameElement).text(this._stageName);

		if (this._additionalInfo != null) {
		    if (this._additionalInfo.indexOf('.csv') > 0) {
		        jQuery(this._stageAdditionalInfoElement).html(
                "<a href='javascript:void(0);'>" + $l.MultistepAction.linkForDownloadFile + "</a>");
		        $(this._stageAdditionalInfoElement).children('a').on("click", jQuery.proxy(this._createDownloadLink, this));
		    } else {
		        jQuery(this._stageAdditionalInfoElement).html(this._additionalInfo);

		        if (this._stageAdditionalInfoElement.scrollWidth > this._stageAdditionalInfoElement.clientWidth) {
		            jQuery(this._stageAdditionalInfoElement).append('<div class="tooltip">' + this._additionalInfo + '</div>')
		        }
		    }
		}


		jQuery(this._stageRemainingElement).text(String.format(
			$l.MultistepAction.stageRemainingTemplate,
			this._stagesRemaining,
			this._stagesCount
		));

		jQuery(this._stageItemsElement).text(String.format(
			$l.MultistepAction.stageItemsRemainingTemplate,
			this._stageItemsRemaining,
			this._stageItemsCount
		));

		var elapsedTime = new Date().getTime() - this._startStageTime.getTime();
		jQuery(this._stageElapsedTimeElement).text(this._formatTimeRange(elapsedTime));

		// Вычисление TimeRemaining
		var timeRemaining = 0;
		if (this._stageStepsCount == 0) {
			jQuery(this._stageTimeRemainingElement).text('');
		}
		else if (this._stageStepsCount > this._stageStepsRemaining) {
			timeRemaining = Math.ceil(elapsedTime * this._stageStepsRemaining / (this._stageStepsCount - this._stageStepsRemaining));
			if (timeRemaining <= 1000) { timeRemaining = 1000; }
			jQuery(this._stageTimeRemainingElement).text(this._formatTimeRange(timeRemaining));
		}
		else {
			jQuery(this._stageTimeRemainingElement).text(this._formatTimeRange(0));
		}

	},

	_formatTimeRange: function (milliseconds) {
		var secontsspan = Math.floor(milliseconds / 1000);
		var hours = Math.floor(secontsspan / 3600);
		var minutes = Math.floor((secontsspan - hours * 3600) / 60);
		var seconds = Math.round(secontsspan - hours * 3600 - minutes * 60);
		if (minutes < 10) { minutes = "0" + minutes; }
		if (seconds < 10) { seconds = "0" + seconds; }
		return hours + ":" + minutes + ":" + seconds;
	},

	startAction: function (stageCount) {
		this._stagesCount = stageCount;
		this._stagesRemaining = stageCount;
	},

	// Начать этап
	startStage: function (stageName, stepsCount, itemsCount) {
		this._stageName = stageName;

		this._stageStepsCount = stepsCount;
		this._stageStepsRemaining = stepsCount;

		this._stageItemsCount = itemsCount;
		this._stageItemsRemaining = itemsCount;

		this._startStageTime = new Date();

		this._refreshView();
	},

	// Закончить этап
	completeStage: function () {
		this._stagesRemaining -= 1;
		if (this._stagesRemaining < 0) {
			this._stagesRemaining = 0;
		}
		this._stageItemsCount = 0;
		this._stageItemsRemaining = 0;

		this._stageStepsCount = 0;
		this._stageStepsRemaining = 0;

		this._refreshView();
	},
	_createDownloadLink: function () {
    if (!$q.isNullOrWhiteSpace(this._additionalInfo)) {
      var urlParams = { id: this._parentId, fileName: encodeURIComponent(this._additionalInfo) };
      var url = Quantumart.QP8.BackendLibrary.generateActionUrl("ExportFileDownload", urlParams);
      $c.downloadFileWithChecking(url, this._additionalInfo);
    }

    return false;
	},
	// Закончить шаг этапа
	completeStep: function (processedItemsCount, additionalInfo, parentId) {
	    this._stageItemsRemaining -= processedItemsCount;
	    this._additionalInfo = additionalInfo;
	    this._parentId = parentId;
		if (this._stageItemsRemaining < 0) {
			this._stageItemsRemaining = 0;
		}
		this._stageStepsRemaining--;
		if (this._stageStepsRemaining < 0) {
			this._stageStepsRemaining = 0;
		}

		this._refreshView();
	},

	_isInProcess: true,

	_stop: function () {
		this._isInProcess = false;

		jQuery(this._cancelButtonElement).prop(
		{
			value: $l.MultistepAction.close,
			title: $l.MultistepAction.closeTitle
		});

		jQuery([
			this._stageNameElement,
			this._stageItemsElement,
			this._stageTimeRemainingElement,
			this._stageElapsedTimeElement,
			this._stageRemainingElement
		]).text("");
	},

	setError: function () {
		this._stop();
		this._progressBarComponent.setColor('red');
		this._progressBarComponent.setText($l.MultistepAction.errorStatus);
	},

	setCancel: function () {
		this._stop();
		this._progressBarComponent.setColor('#BDBDBD');
		this._progressBarComponent.setText($l.MultistepAction.canceledStatus);
	},

	setComplete: function () {
		this._stop();
		this._progressBarComponent.setColor('green');
		this._progressBarComponent.setText($l.MultistepAction.completeStatus);
	},

	_createWindow: function () {
		var windowContentHtml = new $.telerik.stringBuilder();

	    windowContentHtml
		.cat('<div class="lop-main">')

			.cat('<div class="lop-action-name"></div>')

			.cat('<div class="lop-info">')

				.cat('<dl class="lop-stage-remaining">')
					.cat('<dt>' + $l.MultistepAction.stageRemainingLabel + '</dt>')
					.cat('<dd></dd>')
				.cat('</dl>')

				.cat('<dl class="lop-stage-name">')
					.cat('<dt>' + $l.MultistepAction.stageNameLabel + '</dt>')
					.cat('<dd></dd>')
				.cat('</dl>')

				.cat('<dl class="lop-stage-items">')
					.cat('<dt>' + $l.MultistepAction.stageItemsRemainingLabel + '</dt>')
					.cat('<dd></dd>')
				.cat('</dl>')

				.cat('<dl class="lop-elapsed-time">')
					.cat('<dt>' + $l.MultistepAction.stageElapsedTimeLabel + '</dt>')
					.cat('<dd></dd>')
				.cat('</dl>')

				.cat('<dl class="lop-time-remaining">')
					.cat('<dt>' + $l.MultistepAction.stageTimeRemaningLabel + '</dt>')
					.cat('<dd></dd>')
				.cat('</dl>')

                .cat('<dl class="lop-additional-info">')
					.cat('<dt>' + $l.MultistepAction.additionalInfoLabel + '</dt>')
					.cat('<dd class="brief"></dd>')
				.cat('</dl>')

				.cat('<div class="lop-pbar-container">')
					.cat('<div class="lop-pbar"></div>')
				.cat('</div>')

				.cat('<div class="lop-cancel-container">')
					.cat('<input type="button" class="lop-cancel-button" value="')
						.cat($l.MultistepAction.cancel)
						.cat('" title="').cat($l.MultistepAction.cancelTitle)
					  .cat('" />')
				.cat('</div>')

			.cat('</div>')
		.cat('</div>');

		var popupWindowComponent = $.telerik.window.create({
			title: this._windowTitle,
			html: windowContentHtml.string(),
			width: this._windowWidth,
			height: this._windowHeight,
			modal: true,
			resizable: false,
			draggable: false,
			onClose: jQuery.proxy(this._onWindowClose, this)
		}).data("tWindow").center();

		var $popupWindow = jQuery(popupWindowComponent.element);
		if (this._zIndex)
			$popupWindow.css("z-index", this._windowZIndex);

		var $content = $popupWindow.find("DIV.t-window-content:first");
		var bottomPaddingFix = 0;
		if (jQuery.support.borderRadius) {
			bottomPaddingFix = 15;
		}
		else {
			bottomPaddingFix = 10;
		}
		$content.css("padding-bottom", bottomPaddingFix + "px");

		return popupWindowComponent;
	},

	_onWindowClose: function () {
		if (this._isInProcess) {
			this._cancel();
			return false;
		}
		else {
			this.notify(EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CLOSED, {});
		}
	},

	_onCancelClicked: function () {
		this._popupWindowComponent.close();
	},

	_cancel: function () {
		var eventArgs = new Quantumart.QP8.BackendMultistepActionWindowEventArgs();
		this.notify(EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELING, eventArgs);
		if (eventArgs.getCancel() === true) {
			this.notify(EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELED, eventArgs);
		}
		else {
			return false;
		}
	},

	dispose: function () {
		this._progressBarComponent = null;
		if (this._progressBarElement) {
			jQuery(this._progressBarElement).backendProgressBar("dispose");
		}
		this._progressBarElement = null;


		if (this._cancelButtonElement) {
			jQuery(this._cancelButtonElement).off("click");
		}
		this._cancelButtonElement = null;

		if (this._popupWindowComponent) {
			var popupWindowComponent = this._popupWindowComponent;
			$c.destroyPopupWindow(popupWindowComponent);

			popupWindowComponent = null;
			this._popupWindowComponent = null;
		}
		this._popupWindowElement = null;
	}
};

Quantumart.QP8.BackendMultistepActionWindow.registerClass("Quantumart.QP8.BackendMultistepActionWindow", Quantumart.QP8.Observable);

Quantumart.QP8.BackendMultistepActionWindowEventArgs = function () {
	Quantumart.QP8.BackendMultistepActionWindowEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendMultistepActionWindowEventArgs.prototype = {
	_cancel: false,

	getCancel: function(){return this._cancel;},
	setCancel: function(val) {this._cancel = val;}
};

Quantumart.QP8.BackendMultistepActionWindowEventArgs.registerClass("Quantumart.QP8.BackendMultistepActionWindowEventArgs", Sys.EventArgs);
