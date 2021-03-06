import { BackendBrowserHistoryManager } from '../Managers/BackendBrowserHistoryManager';
import { BackendLibrary } from '../Library/BackendLibrary';
import { Observable } from '../Common/Observable';
import { $c } from '../ControlHelpers';
import { $q } from '../Utils';
import { Url } from '../Helpers/vanilla.helpers';

window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELING = 'OnMultistepActionWindowCanceling';
window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELED = 'OnMultistepActionWindowCanceled';
window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CLOSED = 'OnMultistepActionWindowClosed';

export class BackendMultistepActionWindow extends Observable {
  _backendBrowserHistoryManager = BackendBrowserHistoryManager.getInstance();

  constructor(actionName, shortActionName) {
    super();

    this._actionName = actionName;
    this._shortActionName = shortActionName;
    this._startStageTime = new Date();
    this._traceResult = [];
  }

  // название действия
  _actionName = '';

  // краткое название действия
  _shortActionName = '';

  // Всего этапов в действии
  _stagesCount = 0;

  // осталось этапов
  _stagesRemaining = 0;

  // имя текущего этапа
  _stageName = '';

  // Количество шагов на текущем этапе
  _stageStepsCount = 0;

  // Осталось шагов на текущем этапе
  _stageStepsRemaining = 0;

  // Всего элементов на текущем этапе
  _stageItemsCount = 0;

  // Осталось элементов на этапе
  _stageItemsRemaining = 0;

  // используеться для вычисления времени
  _startStageTime = null;

  // DOM-элемент, образующий всплывающее окно
  _popupWindowElement = null;

  // компонент "Всплывающее окно"
  _popupWindowComponent = null;

  // progress bar
  _progressBarComponent = null;
  _progressBarElement = null;

  _stageNameElement = null;
  _stageRemainingElement = null;
  _stageItemsElement = null;
  _stageElapsedTimeElement = null;
  _stageTimeRemainingElement = null;
  _stageAdditionalInfoElement = null;

  _additionalInfo = null;
  _windowTitle = $l.MultistepAction.progressWindowTitle;
  _windowWidth = 500;
  _windowHeight = 250;
  _zIndex = undefined;
  _parentId = null;
  _traceResult = null;

  initialize() {
    this._popupWindowComponent = this._createWindow();
    this._popupWindowElement = this._popupWindowComponent.element;

    const $mainContainer = $('.lop-main', this._popupWindowElement);
    $mainContainer
      .find('.lop-action-name')
      .text(this._shortActionName === null ? this._actionName : this._shortActionName);

    const $stageNameElement = $mainContainer.find('.lop-stage-name dd');
    $stageNameElement.text($l.MultistepAction.setupStageName);

    this._stageNameElement = $stageNameElement.get(0);
    this._stageRemainingElement = $mainContainer.find('.lop-stage-remaining dd').get(0);
    this._stageItemsElement = $mainContainer.find('.lop-stage-items dd').get(0);
    this._stageElapsedTimeElement = $mainContainer.find('.lop-elapsed-time dd').get(0);
    this._stageTimeRemainingElement = $mainContainer.find('.lop-time-remaining dd').get(0);
    this._stageAdditionalInfoElement = $mainContainer.find('.lop-additional-info dd').get(0);

    const $progressBarElement = $mainContainer.find('.lop-pbar');
    this._progressBarElement = $progressBarElement.get(0);
    $progressBarElement.backendProgressBar();

    this._progressBarComponent = $progressBarElement.data('backendProgressBar');
    this._cancelButtonElement = $mainContainer
      .find('.lop-cancel-button')
      .click($.proxy(this._onCancelClicked, this))
      .get(0);
  }

  _loadTraceScript(url) {
    if (url) {
      $(this._stageAdditionalInfoElement).append(`<script src="${url}" type="text/javascript"></script>`);
    }
  }

  _refreshView() {
    let timeRemaining;

    this._progressBarComponent.total(this._stageItemsCount);
    this._progressBarComponent.value(this._stageItemsCount - this._stageItemsRemaining);
    this._progressBarComponent.refresh();

    $(this._stageNameElement).text(this._stageName);
    if (this._additionalInfo !== null) {
      if (this._additionalInfo.indexOf('.csv') > 0) {
        $(this._stageAdditionalInfoElement).html(
          `<a href='javascript:void(0);'>${$l.MultistepAction.linkForDownloadFile}</a>`);
        $(this._stageAdditionalInfoElement).children('a').on('click', $.proxy(this._createDownloadLink, this));
      } else {
        $(this._stageAdditionalInfoElement).html(this._additionalInfo);


        if (this._stageAdditionalInfoElement.scrollWidth > this._stageAdditionalInfoElement.clientWidth) {
          $(this._stageAdditionalInfoElement).append(`<div class="tooltip">${this._additionalInfo}</div>`);
        }
      }
    }


    $(this._stageRemainingElement).text(String.format(
      $l.MultistepAction.stageRemainingTemplate,
      this._stagesRemaining,
      this._stagesCount
    ));

    $(this._stageItemsElement).text(String.format(
      $l.MultistepAction.stageItemsRemainingTemplate,
      this._stageItemsRemaining,
      this._stageItemsCount
    ));

    const elapsedTime = new Date().getTime() - this._startStageTime.getTime();
    $(this._stageElapsedTimeElement).text(this._formatTimeRange(elapsedTime));

    timeRemaining = 0;
    if (this._stageStepsCount === 0) {
      $(this._stageTimeRemainingElement).text('');
    } else if (this._stageStepsCount > this._stageStepsRemaining) {
      timeRemaining = Math.ceil(
        elapsedTime * this._stageStepsRemaining / (this._stageStepsCount - this._stageStepsRemaining)
      );

      if (timeRemaining <= 1000) {
        timeRemaining = 1000;
      }

      $(this._stageTimeRemainingElement).text(this._formatTimeRange(timeRemaining));
    } else {
      $(this._stageTimeRemainingElement).text(this._formatTimeRange(0));
    }
  }

  _formatTimeRange(milliseconds) {
    const secondsSpan = Math.floor(milliseconds / 1000);
    const hours = Math.floor(secondsSpan / 3600);

    /** @type {string | number} */
    let minutes = Math.floor((secondsSpan - (hours * 3600)) / 60);

    /** @type {string | number} */
    let seconds = Math.round(secondsSpan - (hours * 3600) - (minutes * 60));

    if (minutes < 10) {
      minutes = `0${minutes}`;
    }

    if (seconds < 10) {
      seconds = `0${seconds}`;
    }

    return `${hours}:${minutes}:${seconds}`;
  }

  startAction(stageCount) {
    this._stagesCount = stageCount;
    this._stagesRemaining = stageCount;
  }

  // Начать этап
  startStage(stageName, stepsCount, itemsCount) {
    this._stageName = stageName;
    this._stageStepsCount = stepsCount;
    this._stageStepsRemaining = stepsCount;
    this._stageItemsCount = itemsCount;
    this._stageItemsRemaining = itemsCount;
    this._startStageTime = new Date();
    this._refreshView();
  }

  // Закончить этап
  completeStage(entityId) {
    this._stagesRemaining -= 1;
    if (this._stagesRemaining < 0) {
      this._stagesRemaining = 0;
    }

    this._stageItemsCount = 0;
    this._stageItemsRemaining = 0;

    this._stageStepsCount = 0;
    this._stageStepsRemaining = 0;

    this._refreshView();

    this.showTraceResult(entityId);
  }

  _createDownloadLink() {
    let url, urlParams;
    if (!$q.isNullOrWhiteSpace(this._additionalInfo)) {
      urlParams = { id: this._parentId, fileName: encodeURIComponent(this._additionalInfo) };
      url = BackendLibrary.generateActionUrl('ExportFileDownload', urlParams);
      $c.downloadFileWithChecking(url, this._additionalInfo);
    }

    return false;
  }

  completeStep(processedItemsCount, additionalInfo, traceResult, parentId) {
    this._stageItemsRemaining -= processedItemsCount;
    this._additionalInfo = additionalInfo;
    this._parentId = parentId;
    if (this._stageItemsRemaining < 0) {
      this._stageItemsRemaining = 0;
    }

    this._stageStepsRemaining -= 1;
    if (this._stageStepsRemaining < 0) {
      this._stageStepsRemaining = 0;
    }

    if (traceResult) {
      this._traceResult.push(traceResult);
    }

    this._refreshView();
  }

  showTraceResult(entityId) {
    if (entityId && this._traceResult && this._traceResult.length > 0) {
      // eslint-disable-next-line new-cap
      const url = Url.Content(`~/Content/GetTraceImportScript/${entityId}`);
      this._loadTraceScript(url);

      const wnd = Quantumart.QP8.BackendMultistepActionWindow;
      if (typeof wnd.processTraceResult === 'function') {
        wnd.processTraceResult(this._traceResult);
      }
    }
  }

  _isInProcess = true;
  _stop() {
    this._isInProcess = false;
    $(this._cancelButtonElement).prop({
      value: $l.MultistepAction.close,
      title: $l.MultistepAction.closeTitle
    });

    $([
      this._stageNameElement,
      this._stageItemsElement,
      this._stageTimeRemainingElement,
      this._stageElapsedTimeElement,
      this._stageRemainingElement
    ]).text('');
  }

  setError() {
    this._stop();
    this._progressBarComponent.setColor('red');
    this._progressBarComponent.setText($l.MultistepAction.errorStatus);
  }

  setCancel() {
    this._stop();
    this._progressBarComponent.setColor('#BDBDBD');
    this._progressBarComponent.setText($l.MultistepAction.canceledStatus);
  }

  setComplete() {
    this._stop();
    this._progressBarComponent.setColor('green');
    this._progressBarComponent.setText($l.MultistepAction.completeStatus);
  }

  _createWindow() {
    let bottomPaddingFix;
    const windowContentHtml = new $.telerik.stringBuilder();

    windowContentHtml
      .cat('<div class="lop-main">')
      .cat('<div class="lop-action-name"></div>')
      .cat('<div class="lop-info">')

      .cat('<dl class="lop-stage-remaining">')
      .cat(`<dt>${$l.MultistepAction.stageRemainingLabel}</dt>`)
      .cat('<dd></dd>')
      .cat('</dl>')

      .cat('<dl class="lop-stage-name">')
      .cat(`<dt>${$l.MultistepAction.stageNameLabel}</dt>`)
      .cat('<dd></dd>')
      .cat('</dl>')

      .cat('<dl class="lop-stage-items">')
      .cat(`<dt>${$l.MultistepAction.stageItemsRemainingLabel}</dt>`)
      .cat('<dd></dd>')
      .cat('</dl>')

      .cat('<dl class="lop-elapsed-time">')
      .cat(`<dt>${$l.MultistepAction.stageElapsedTimeLabel}</dt>`)
      .cat('<dd></dd>')
      .cat('</dl>')

      .cat('<dl class="lop-time-remaining">')
      .cat(`<dt>${$l.MultistepAction.stageTimeRemaningLabel}</dt>`)
      .cat('<dd></dd>')
      .cat('</dl>')

      .cat('<dl class="lop-additional-info">')
      .cat(`<dt>${$l.MultistepAction.additionalInfoLabel}</dt>`)
      .cat('<dd class="brief"></dd>')
      .cat('</dl>')

      .cat('<div class="lop-pbar-container">')
      .cat('<div class="lop-pbar"></div>')
      .cat('</div>')

      .cat('<div class="lop-cancel-container">')
      .cat('<input type="button" class="lop-cancel-button" value="')
      .cat($l.MultistepAction.cancel)
      .cat('" title="')
      .cat($l.MultistepAction.cancelTitle)
      .cat('" />')
      .cat('</div>')

      .cat('</div>')
      .cat('</div>');

    const popupWindowComponent = $.telerik.window.create({
      title: this._windowTitle,
      html: windowContentHtml.string(),
      width: this._windowWidth,
      height: this._windowHeight,
      modal: true,
      resizable: false,
      draggable: false,
      onOpen: this._backendBrowserHistoryManager.handleModalWindowOpen,
      onClose: $.proxy(this._onWindowClose, this)
    }).data('tWindow').center();

    const $popupWindow = $(popupWindowComponent.element);
    if (this._zIndex) {
      // @ts-ignore FIXME
      $popupWindow.css('z-index', this._windowZIndex);
    }

    const $content = $popupWindow.find('DIV.t-window-content:first');
    bottomPaddingFix = 0;
    if ($.support.borderRadius) {
      bottomPaddingFix = 15;
    } else {
      bottomPaddingFix = 10;
    }

    $content.css('padding-bottom', `${bottomPaddingFix}px`);
    return popupWindowComponent;
  }

  _onWindowClose() {
    if (this._isInProcess) {
      this._cancel();
      return false;
    }

    this._backendBrowserHistoryManager.handleModalWindowClose();
    this.notify(window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CLOSED, {});
    return undefined;
  }

  _onCancelClicked() {
    this._popupWindowComponent.close();
  }

  _cancel() {
    // eslint-disable-next-line no-use-before-define
    const eventArgs = new BackendMultistepActionWindowEventArgs();
    this.notify(window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELING, eventArgs);
    if (eventArgs.getCancel()) {
      this.notify(window.EVENT_TYPE_MULTISTEP_ACTION_WINDOW_CANCELED, eventArgs);
    } else {
      return false;
    }

    return undefined;
  }

  dispose() {
    let popupWindowComponent;
    this._progressBarComponent = null;
    if (this._progressBarElement) {
      $(this._progressBarElement).backendProgressBar('dispose');
    }

    this._progressBarElement = null;
    if (this._cancelButtonElement) {
      $(this._cancelButtonElement).off('click');
    }

    this._cancelButtonElement = null;
    if (this._popupWindowComponent) {
      popupWindowComponent = this._popupWindowComponent;
      $c.destroyPopupWindow(popupWindowComponent);
    }
  }
}


export class BackendMultistepActionWindowEventArgs extends Sys.EventArgs {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor() {
    super();
  }

  _cancel = false;

  getCancel() {
    return this._cancel;
  }

  setCancel(val) {
    this._cancel = val;
  }
}


Quantumart.QP8.BackendMultistepActionWindow = BackendMultistepActionWindow;
Quantumart.QP8.BackendMultistepActionWindowEventArgs = BackendMultistepActionWindowEventArgs;
