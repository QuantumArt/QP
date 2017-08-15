Quantumart.QP8.ImageCropResizeClient = Quantumart.QP8.ImageCropResizeClient || {};
Quantumart.QP8.ImageCropResizeClient.Cache = Quantumart.QP8.ImageCropResizeClient.Cache || {};
(function ($, imgCropResize) {
  const parameters = null;
  const $windowElement = null;
  const $rootElement = null;
  const $x = null;
  const $y = null;
  const $width = null;
  const $height = null;
  const imgAreaSelect = null;
  const $btnSave = null;
  const $saveForm = null;
  const slider = null;
  const $sliderValueSpan = null;
  const defaultErrorMessage = $l.Crop.defaultErrorMessage;
  const $nouislider = null;
  const $errors = null;
  const $errorsContainer = null;
  const $radioSaveCopy = null;
  const $radioOverwrite = null;
  const $inputUserFileName = null;
  const $radioInputs = null;
  const $img = null;
  const $aside = null;
  const $imageContainerDiv = null;
  const $wincontent = null;
  let _telerikWindow = null;
  const $imgArea = null;
  const imgInitWidth = 0;
  const imgInitHeight = 0;

  const _saveMode = {
    overwrite: 'overwrite',
    saveAs: 'saveAs'
  };

  const _removeErrors = function () {
    _$errors.empty();
    _$errorsContainer.hide();
  };

  const _setSize = function ($img) {
    if (_$aside && $img) {
      const padding = 10;
      const width = _$aside.width() + $img.outerWidth() + (padding * 3);
      const height = Math.max(_$aside.height(), $img.outerHeight() + padding) + (padding * 2);
      const winWidth = Math.min(width, $(window).width() * 0.8);
      const winHeight = Math.min(height, $(window).height() * 0.8);
      const imgContainerWidth = winWidth - _$aside.width() - padding;
      const imgContainerHeight = winHeight - padding;
      _$windowElement.find('.t-content').css('width', `${winWidth}px`);
      _$windowElement.find('.t-content').css('height', `${winHeight}px`);
      _$windowElement.find('.image').css('width', `${imgContainerWidth}px`);
      _$windowElement.find('.image').css('height', `${imgContainerHeight}px`);
    }
  };

  const displayErrors = function (errors) {
    _removeErrors();
    for (const i in errors) {
      _$errors.append($(`<li>${errors[i]}</li>`));
    }

    _$errorsContainer.show();
    _setSize(_$img);
  };

  const _getSaveMode = function () {
    if (!_parameters.allowFileRewrite || _$radioSaveCopy.attr('checked')) {
      return _saveMode.saveAs;
    }

    return _saveMode.overwrite;
  };

  const _endsWith = function (suffix, str) {
    return str.indexOf(suffix, str.length - suffix.length) !== -1;
  };

  const _getFolder = function (url) {
    return url.substring(0, url.lastIndexOf('/'));
  };

  const _getExtension = function (url) {
    const index = url.lastIndexOf('.');
    return index == -1 ? '' : url.substr(index, url.length - index);
  };

  const readData = function () {
    let targetFileUrl = '';
    const overwriteFile = _getSaveMode() == _saveMode.overwrite;

    if (overwriteFile) {
      targetFileUrl = _parameters.sourceImageUrl;
    } else {
      let value = $.trim(_$inputUserFileName.val());
      if (value != '' && value.lastIndexOf('.') == -1) {
        value += _getExtension(_parameters.sourceImageUrl);
      }

      if (_parameters.resultImageFolder) {
        targetFileUrl = _endsWith('/', _parameters.resultImageFolder)
          ? _parameters.resultImageFolder + value : `/${value}`;
      } else {
        targetFileUrl = _getFolder(_parameters.sourceImageUrl) + value;
      }
    }

    const result = {
      overwriteFile,
      targetFileUrl,
      sourceFileUrl: _parameters.sourceImageUrl,
      resize: +_$nouislider.val()
    };

    const select = _imgAreaSelect.getSelection();
    if (select.width || select.height) {
      Object.assign(result, {
        left: Math.max(select.x1, 0),
        top: Math.max(select.y1, 0),
        width: select.width,
        height: select.height
      });
    }

    return result;
  };

  const show = function () {
    if (_telerikWindow) {
      _telerikWindow.center();
      _removeErrors();
    }
  };

  const close = function () {
    _removeErrors();
    _telerikWindow.close();
  };

  const dispose = function () {
    _$imgArea.imgAreaSelect('destroy');
    _telerikWindow.destroy();
    _$btnSave.off('click');
    _$saveForm.off('submit');
    _$nouislider.off('slide');
    _$nouislider.off('_setSize');
    _$nouislider.off('mousewheel DOMMouseScroll');
    _$radioInputs.off('change');
  };

  const _defaultParameters = {
    sourceImageUrl: '',
    resultImageFolder: '',
    crop: {
      height: 100,
      width: 100,
      left: 0,
      top: 0
    },
    allowResizeCropArea: true,
    allowFileRewrite: true,
    checkFileNameActionUrl: `${window.APPLICATION_ROOT_URL}Library/CheckForCrop/`,
    cropResizeActionUrl: `${window.APPLICATION_ROOT_URL}Library/Crop/`,
    resizeRange: { max: 3.0, min: 0.1 },
    onCompleteCallback () {
      Sys.Debug.trace('imgCropResize: finished');
    }
  };

  const _fieldsetCropHtml = `<fieldset class='crop'>\
                <legend>${$l.Crop.selection}</legend>\
                <dl>\
                    <dt>${$l.Crop.left}</dt>\
                    <dd>\
                        <span class='selection-x'></span>\
                    </dd>\
                </dl>\
                <dl>\
        <dl>\
                    <dt>${$l.Crop.top}</dt>\
                    <dd>\
                        <span class='selection-y' />\
                    </dd>\
                </dl>\
                <dl>\
                    <dt>${$l.Crop.width}</dt>\
                    <dd>\
                        <span class='selection-width' />\
                    </dd>\
                </dl>\
                <dl>\
                    <dt>${$l.Crop.height}</dt>\
                    <dd>\
                        <span class='selection-height' />\
                    </dd>\
                </dl>\
            </fieldset>`;

  const _fieldsetSliderHtml = `<fieldset class='slider'>\
                <legend>${$l.Crop.changeSize}</legend>\
                <div>\
                    <span class='slider-value'>x1</span>\
                    <div class='right'>\
                        <div class='nouislider' />\
                    </div>\
                </div>\
            </fieldset>`;

  const _fieldsetSaveHtml = `<form class='saveForm'><fieldset class='save'>\
                <legend>${$l.Crop.save}</legend>\
                <dl class='row'>\
                    <dd class='save-items'>\
                        <input class='button-save' type='button' value='${$l.Crop.save}' />\
                    </dd>\
                </dl>\
            </fieldset></form>`;

  const _saveOverwriteOrSaveAsHtml = `<div class='save-overwrite'>\
                            <ul class='formItem'>\
                                <li>\
                                    <input id='overwrite_0' class='image-overwrite' name='overwrite' checked='checked' type='radio' value='true' />\
                                    <label for='overwrite_0'>${$l.Crop.overwrite}</label>\
                                </li>\
                                <li>\
                                    <input id='overwrite_1' class='image-save-copy' name='overwrite' type='radio' value='false' />\
                                    <label for='overwrite_1'>${$l.Crop.saveAs}</label>\
                                </li>\
                                <li>\
                                    <input class='filename-input' type='text' disabled='disabled' />\
                                </li>\
                            </ul>\
                        </div>`;

  const _saveOverwriteHtml = `<div class='save-overwrite'>\
                            <ul class='formItem'>\
                                <li>\
                                    <label>${$l.Crop.saveAs}</label>\
                                </li>\
                                <li>\
                                    <input class='filename-input' type='text' />\
                                </li>\
                            </ul>\
                        </div>`;

  const _fieldsetErrorsContainerHtml = `<fieldset class='errorsContainer' style='display: none;'>\
                <legend>${$l.Crop.errors}</legend>\
                <ul class='errors'>\
                </ul>\
            </fieldset>`;

  const _ensureSelection = function (selection) {
    setTimeout(() => {
      if (_imgAreaSelect != null && (selection.width != _parameters.crop.width || selection.height != _parameters.crop.height)) {
        _imgAreaSelect.setSelection(selection.x1, selection.y1, selection.x1 + _parameters.crop.width, selection.y2 + _parameters.crop.height);
        _imgAreaSelect.update();
      }
    }, 0);
  };

  const _resizeImage = function (resizeWidth) {
    _$img[0].style.width = `${Math.floor(resizeWidth)}px`;
    _setSize(_$img);
    setTimeout(_imgAreaSelect.update(), 0);
  };

  const _onSlide = function (resize) {
    _$sliderValueSpan.text(`x${resize}`);

    const resizeWidth = _imgInitWidth * resize;
    const resizeHeight = _imgInitHeight * resize;
    if (!_parameters.allowResizeCropArea) {
      const dx = resizeWidth - _parameters.crop.width;
      const dy = resizeHeight - _parameters.crop.height;
      if (dx > 0 && dy > 0) {
        _resizeImage(resizeWidth);
      }

      const selection = _imgAreaSelect.getSelection();
      _ensureSelection(selection);
    } else {
      _resizeImage(resizeWidth);
    }
  };

  const _createSlider = function () {
    _slider = _$nouislider.noUiSlider({
      range: {
        min: _parameters.resizeRange.min,
        '50%': 1,
        max: _parameters.resizeRange.max
      },
      start: 1
    });

    _$nouislider.on({
      slide (slider, val) {
        _onSlide(val);
      },
      set (slider, val) {
        _$sliderValueSpan.text(`x${val}`);
        _onSlide(val);
      }
    });

    _$nouislider.on('mousewheel DOMMouseScroll', function (e) {
      const o = e.originalEvent;
      const delta = o && (o.wheelDelta || (o.detail && -o.detail));

      if (delta) {
        e.preventDefault();
        const currentvalue = parseFloat($(this).val());
        let step = 1;
        step *= delta < 0
          ? currentvalue < 1 ? -0.25 : -0.5
          : currentvalue < 1 ? 0.25 : 0.5;

        const newValue = currentvalue + step;
        if (newValue >= 0 && newValue <= 3.5) {
          $(this).val(newValue, { set: true });
        }
      }
    });
  };

  const _displaySelectionCoordinates = function (selection, needRedrawWidthAndHeight) {
    setTimeout(() => {
      if ((isNaN(selection.width) && isNaN(selection.height)) || (selection.width === 0 && selection.height === 0)) {
        _$x.text('');
        _$y.text('');
        _$width.text('');
        _$height.text('');
      } else {
        if (needRedrawWidthAndHeight) {
          _$width.text(selection.width);
          _$height.text(selection.height);
        }
        _$x.text(Math.max(selection.x1, 0));
        _$y.text(Math.max(selection.y1, 0));
      }
    }, 0);
  };

  const _createImgAreaSelect = function () {
    const parameters = {
      persistent: !_parameters.allowResizeCropArea,
      instance: true,
      resizable: _parameters.allowResizeCropArea,
      movable: true,
      x1: _parameters.crop.left,
      y1: _parameters.crop.top,
      x2: _parameters.crop.left + _parameters.crop.width,
      y2: _parameters.crop.top + _parameters.crop.height,
      parent: _$windowElement.find('.image'),
      onSelectChange (img, selection) {
        if (!_parameters.allowResizeCropArea) {
          _ensureSelection(selection);
        }

        _displaySelectionCoordinates(selection, _parameters.allowResizeCropArea);
      },
      onInit (img, selection) {
        _displaySelectionCoordinates(selection, true);
      }
    };

    if (!_parameters.allowResizeCropArea) {
      Object.assign(parameters,
        {
          minHeight: _parameters.crop.height,
          minWidth: _parameters.crop.width,
          maxHeight: _parameters.crop.height,
          maxWidth: _parameters.crop.width
        });
    }

    _imgAreaSelect = _$imgArea.imgAreaSelect(parameters);
  };

  const _createControls = function () {
    _createImgAreaSelect();
    _createSlider();
  };

  const _createWindow = function () {
    _telerikWindow = $.telerik.window.create({
      title: $l.Crop.title,
      actions: ['Close'],

      modal: true,
      resizable: false,
      draggable: true,
      scrollable: true,
      onOpen (evt) {

      },
      onClose () {
        _telerikWindow.destroy();
      },
      onActivate () {

      },
      onLoad () {

      }
    }).data('tWindow');

    _$windowElement = jQuery(_telerikWindow.element);
    _$windowElement.addClass('imgCropResizeWindow');
    _$wincontent.appendTo(_$windowElement.find('.t-content'));
  };

  const _assignRootElement = function () {
    _$rootElement = _$windowElement;
  };

  const _createShortlinks = function () {
    _$x = _$rootElement.find('.selection-x');
    _$y = _$rootElement.find('.selection-y');
    _$width = _$rootElement.find('.selection-width');
    _$height = _$rootElement.find('.selection-height');
    _$sliderValueSpan = _$rootElement.find('.slider-value');
    _$btnSave = _$rootElement.find('.button-save');
    _$saveForm = _$rootElement.find('.saveForm');
    _$nouislider = _$rootElement.find('.nouislider');
    _$errors = _$rootElement.find('.errors');
    _$errorsContainer = _$rootElement.find('.errorsContainer');
    _$radioSaveCopy = _$rootElement.find('.image-save-copy');
    _$radioOverwrite = _$rootElement.find('.image-overwrite');
    _$inputUserFileName = _$rootElement.find('.filename-input');
    _$radioInputs = _$rootElement.find('input[name=overwrite]:radio');
    _$imgArea = _$rootElement.find('img.img');
  };

  const _checkPrameters = function () {
    if (!_parameters.sourceImageUrl) {
      throw new Error("imgCropResize: parameter 'sourceImageUrl' not set");
    }

    if (!_parameters.checkFileNameActionUrl) {
      throw new Error("imgCropResize: parameter 'checkFileNameActionUrl' not set");
    }

    if (!_parameters.cropResizeActionUrl) {
      throw new Error("imgCropResize: parameter 'cropResizeActionUrl' not set");
    }

    if (_parameters.resizeRange.min < 0 || _parameters.resizeRange.max < 0) {
      throw new Error('imgCropResize: resize range must be positive');
    }

    if (_parameters.resizeRange.min > _parameters.resizeRange.max) {
      throw new Error('imgCropResize: resizeRange.min must be less than resizeRange.max');
    }
  };

  const _getFromCache = function () {
    return Quantumart.QP8.ImageCropResizeClient.Cache[_parameters.sourceImageUrl];
  };

  const _removeFromCache = function () {
    delete Quantumart.QP8.ImageCropResizeClient.Cache[_parameters.sourceImageUrl];
  };

  const _saveToCache = function () {
    const sendData = readData();
    if (sendData.width && sendData.height) {
      Quantumart.QP8.ImageCropResizeClient.Cache[_parameters.sourceImageUrl]
          = { crop: { top: sendData.top, left: sendData.left, width: sendData.width, height: sendData.height } };
    }
  };

  const _serverOperateImage = function () {
    let sendData = readData();
    sendData = JSON.stringify(sendData);

    $.ajax(_parameters.cropResizeActionUrl, {

      contentType: 'application/json',
      data: sendData,
      type: 'POST',
      dataType: 'json'

    }).success(response => {
      if (!response.ok) {
        const message = response.message || _defaultErrorMessage;
        displayErrors([message]);
      } else if (typeof _parameters.onCompleteCallback == 'function') {
        if (_getSaveMode() == _saveMode.saveAs) {
          _saveToCache();
        } else {
          _removeFromCache();
        }

        _parameters.onCompleteCallback();
      }
    }).fail(error => {
      displayErrors([_defaultErrorMessage]);
    });
  };

  const _serverCheckFileName = function (userInputFileName) {
    $.ajax(_parameters.checkFileNameActionUrl, {
      data: { targetFileUrl: userInputFileName },
      type: 'POST',
      dataType: 'json'
    }).success(response => {
      if (!response.ok) {
        const message = response.message || _defaultErrorMessage;
        displayErrors([message]);
      } else {
        _serverOperateImage();
      }
    }).fail(error => {
      displayErrors([_defaultErrorMessage]);
    });
  };

  const _createContent = function () {
    _$wincontent = $("<div class='imgCropResize content-container' />");
    _$aside = $(" <div class='aside'></div>");
    _$imageContainerDiv = $("<div class='image'/>");
    const url = `${_parameters.sourceImageUrl}?t=${new Date().getTime()}`;
    _$img = $(`<img class='img' src='${url}' alt='изображение' />`);
    _$img.load(function () {
      _imgInitWidth = _$img.width();
      _imgInitHeight = _$img.height();
      _setSize($(this));
      _telerikWindow.center();
    });
    _$img.appendTo(_$imageContainerDiv);

    const $fieldsetSave = $(_fieldsetSaveHtml);
    if (_parameters.allowFileRewrite) {
      $fieldsetSave.find('.save-items').prepend($(_saveOverwriteOrSaveAsHtml));
    } else {
      $fieldsetSave.find('.save-items').prepend($(_saveOverwriteHtml));
    }

    _$wincontent.append(
      _$aside.append(
        $(_fieldsetCropHtml),
        $(_fieldsetSliderHtml),
        $fieldsetSave,
        $(_fieldsetErrorsContainerHtml)
      ),
      _$aside,
      _$imageContainerDiv);
  };

  const _validate = function () {
    const saveMode = _getSaveMode();
    if (saveMode == _saveMode.saveAs && !_$inputUserFileName.val()) {
      displayErrors([$l.Crop.enterFileName]);
      return false;
    }
    _removeErrors();
    return true;
  };

  const _attachHandlers = function () {
    _$btnSave.click(() => {
      if (_validate()) {
        _removeErrors();
        if (_getSaveMode() == _saveMode.overwrite) {
          _serverOperateImage();
        } else {
          const formOptions = readData();
          _serverCheckFileName(formOptions.targetFileUrl);
        }
      }
    });

    _$saveForm.submit(e => {
      e.preventDefault();
      _$btnSave.trigger('click');
      return false;
    });

    _$radioInputs.change(radio => {
      const saveCopy = _$radioSaveCopy.attr('checked');
      if (saveCopy) {
        _$inputUserFileName.removeAttr('disabled');
      } else {
        _$inputUserFileName.attr('disabled', 'disabled');
      }
    });
  };

  imgCropResize.create = function (parameters) {
    _parameters = {};
    Object.assign(_parameters, _defaultParameters);
    Object.assign(_parameters, parameters);
    Object.assign(_parameters, _getFromCache());

    _checkPrameters();
    _createContent();
    _createWindow();
    _assignRootElement();
    _createShortlinks();
    _createControls();
    _attachHandlers();

    return {
      type: 'Quantumart.QP8.ImageCropResizeClient',
      displayErrors,
      getOptions: readData,
      openWindow: show,
      closeWindow: close,
      dispose,
      parameters: _parameters,
      window: _$windowElement
    };
  };
}(jQuery, Quantumart.QP8.ImageCropResizeClient));
