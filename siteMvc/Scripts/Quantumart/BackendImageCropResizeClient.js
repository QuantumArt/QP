// / <reference path="jquery-1.7.1.intellisense.js" />
// / <reference path="jquery-1.7.1.js" />
Quantumart.QP8.ImageCropResizeClient = Quantumart.QP8.ImageCropResizeClient || {};
Quantumart.QP8.ImageCropResizeClient.Cache = Quantumart.QP8.ImageCropResizeClient.Cache || {};
(function ($, imgCropResize) {
    var

        // #region control parameters
        _defaultParameters = {
            sourceImageUrl: "",                                         //	URL исходного изображения
            resultImageFolder: "",                                      //	URL для размещения итогового изображения (папка, при пустом значении или отсутствии размещаем там же, где исходное)
            crop: {                                                     //	Начальная настройка кроппинга (ширина и высота)
                height: 100,
                width: 100,
                left: 0,
				top: 0
            },
            allowResizeCropArea: true,                                  //	Признак, разрешается ли изменение настроек кроппинга (ширины и высоты)
            allowFileRewrite: true,                                     //	Признак, разрешается ли перезапись исходного файла
            checkFileNameActionUrl: APPLICATION_ROOT_URL + "Library/CheckForCrop/",                   //	Серверный URL для проверки имени файла
            cropResizeActionUrl: APPLICATION_ROOT_URL + "Library/Crop/",                         //	Серверный URL для выполнения кроппинга и ресайза
            resizeRange: { max: 3.0, min: 0.1 },                             //  Минимальное и саксимальное значения коэффициента ресайза
            onCompleteCallback: function () {
            	Sys.Debug.trace("imgCropResize: finished");
            }                                                           //	Callback, вызываемый при завершении работы
        },

        // #endregion

        // #region Content html
        _fieldsetCropHtml = "<fieldset class='crop'>\
                <legend>" + $l.Crop.selection + "</legend>\
                <dl>\
                    <dt>" + $l.Crop.left + "</dt>\
                    <dd>\
                        <span class='selection-x'></span>\
                    </dd>\
                </dl>\
                <dl>\
				<dl>\
                    <dt>" + $l.Crop.top + "</dt>\
                    <dd>\
                        <span class='selection-y' />\
                    </dd>\
                </dl>\
                <dl>\
                    <dt>" + $l.Crop.width + "</dt>\
                    <dd>\
                        <span class='selection-width' />\
                    </dd>\
                </dl>\
                <dl>\
                    <dt>" + $l.Crop.height + "</dt>\
                    <dd>\
                        <span class='selection-height' />\
                    </dd>\
                </dl>\
            </fieldset>",

        _fieldsetSliderHtml = "<fieldset class='slider'>\
                <legend>" + $l.Crop.changeSize + "</legend>\
                <div>\
                    <span class='slider-value'>x1</span>\
                    <div class='right'>\
                        <div class='nouislider' />\
                    </div>\
                </div>\
            </fieldset>",

        _fieldsetSaveHtml = "<form class='saveForm'><fieldset class='save'>\
                <legend>" + $l.Crop.save + "</legend>\
                <dl class='row'>\
                    <dd class='save-items'>\
                        <input class='button-save' type='button' value='" + $l.Crop.save + "' />\
                    </dd>\
                </dl>\
            </fieldset></form>",

        _saveOverwriteOrSaveAsHtml = "<div class='save-overwrite'>\
                            <ul class='formItem'>\
                                <li>\
                                    <input id='overwrite_0' class='image-overwrite' name='overwrite' checked='checked' type='radio' value='true' />\
                                    <label for='overwrite_0'>" + $l.Crop.overwrite + "</label>\
                                </li>\
                                <li>\
                                    <input id='overwrite_1' class='image-save-copy' name='overwrite' type='radio' value='false' />\
                                    <label for='overwrite_1'>" + $l.Crop.saveAs + "</label>\
                                </li>\
                                <li>\
                                    <input class='filename-input' type='text' disabled='disabled' />\
                                </li>\
                            </ul>\
                        </div>",

         _saveOverwriteHtml = "<div class='save-overwrite'>\
                            <ul class='formItem'>\
                                <li>\
                                    <label>" + $l.Crop.saveAs + "</label>\
                                </li>\
                                <li>\
                                    <input class='filename-input' type='text' />\
                                </li>\
                            </ul>\
                        </div>",

        _fieldsetErrorsContainerHtml = "<fieldset class='errorsContainer' style='display: none;'>\
                <legend>" + $l.Crop.errors + "</legend>\
                <ul class='errors'>\
                </ul>\
            </fieldset>",

        // #endregion

        // #region private fields
		_parameters = null,
         _$windowElement = null,
        _$rootElement = null,
        _$x = null,
        _$y = null,
        _$width = null,
        _$height = null,
        _imgAreaSelect = null,
        _$btnSave = null,
		_$saveForm = null,
        _slider = null,
        _$sliderValueSpan = null,
        _defaultErrorMessage = $l.Crop.defaultErrorMessage,
        _$nouislider = null,
        _$errors = null,
        _$errorsContainer = null,
        _$radioSaveCopy = null,
        _$radioOverwrite = null,
        _$inputUserFileName = null,
        _$radioInputs = null,
        _$img = null,
        _$aside = null,
        _$imageContainerDiv = null,
        _$wincontent = null,
        _window = null,
        _$imgArea = null,
        _imgInitWidth = 0,
        _imgInitHeight = 0,

        // #endregion

        _ensureSelection = function (selection) {
            setTimeout(function () {
                if (_imgAreaSelect != null && (selection.width != _parameters.crop.width || selection.height != _parameters.crop.height)) {
                    _imgAreaSelect.setSelection(selection.x1, selection.y1, selection.x1 + _parameters.crop.width, selection.y2 + _parameters.crop.height);
                    _imgAreaSelect.update();
                }
            }, 0);
        },

        // #region Slider
        _onSlide = function (resize) {
            _$sliderValueSpan.text("x" + resize);

            var resizeWidth = _imgInitWidth * resize;
            var resizeHeight = _imgInitHeight * resize;
            if (!_parameters.allowResizeCropArea) {
            	var dx = resizeWidth - _parameters.crop.width;
            	var dy = resizeHeight - _parameters.crop.height;
                if (dx > 0 && dy > 0) {
                	_resizeImage(resizeWidth);
                }

                var selection = _imgAreaSelect.getSelection();
                _ensureSelection(selection);
            } else {
            	_resizeImage(resizeWidth);
            }
        },

        _resizeImage = function (resizeWidth) {
        	_$img[0].style.width = Math.floor(resizeWidth) + "px";
        	_setSize(_$img);
        	setTimeout(_imgAreaSelect.update(), 0);
        },

        _createSlider = function () {
            _slider = _$nouislider.noUiSlider({
                range: {
                    // size down
                    min: _parameters.resizeRange.min,

                    // default
                    '50%': 1,

                    // size up
                    max: _parameters.resizeRange.max
                },
                start: 1
            });

            _$nouislider.on({
                slide: function (slider, val) {
                    _onSlide(val);
                },
                set: function (slider, val) {
                    _$sliderValueSpan.text("x" + val);
                    _onSlide(val);
                }
            });

            _$nouislider.on('mousewheel DOMMouseScroll', function (e) {
                var o = e.originalEvent;
                var delta = o && (o.wheelDelta || (o.detail && -o.detail));

                if (delta) {
                    e.preventDefault();
                    var currentvalue = parseFloat($(this).val());
                    var step = 1;
                    step *= delta < 0 // mouse down
                        ? currentvalue < 1 ? -0.25 : -0.5 // slide left
                        : currentvalue < 1 ? 0.25 : 0.5; // slide right

                    var newValue = currentvalue + step;
                    if (newValue >= 0 && newValue <= 3.5) {
                        $(this).val(newValue, { set: true });
                    }
                }
            });
        },

        // #endregion

        // #region ImgAreaSelect
        _createImgAreaSelect = function () {
            var parameters = {
                persistent: !_parameters.allowResizeCropArea,
                instance: true,
                resizable: _parameters.allowResizeCropArea,
                movable: true,
                x1: _parameters.crop.left,
                y1: _parameters.crop.top,
                x2: _parameters.crop.left + _parameters.crop.width,
                y2: _parameters.crop.top + _parameters.crop.height,
                parent: _$windowElement.find(".image"),
                onSelectChange: function (img, selection) {
                    if (!_parameters.allowResizeCropArea) {
                        _ensureSelection(selection);
                    }
                    _displaySelectionCoordinates(selection, _parameters.allowResizeCropArea);
                },
                onInit: function (img, selection) {
                    _displaySelectionCoordinates(selection, true);
                }
            };

            if (!_parameters.allowResizeCropArea) {
                $.extend(parameters,
                    {
                        minHeight: _parameters.crop.height,
                        minWidth: _parameters.crop.width,
                        maxHeight: _parameters.crop.height,
                        maxWidth: _parameters.crop.width
                    });
            }

            _imgAreaSelect = _$imgArea.imgAreaSelect(parameters);
        },
        _displaySelectionCoordinates = function (selection, needRedrawWidthAndHeight) {
            setTimeout(function () {
                if (isNaN(selection.width) && isNaN(selection.height) || selection.width == 0 && selection.height == 0) {
                    _$x.text("");
                    _$y.text("");
                    _$width.text("");
                    _$height.text("");
                } else {
                    if (needRedrawWidthAndHeight) {
                        _$width.text(selection.width);
                        _$height.text(selection.height);
                    }
                    _$x.text(Math.max(selection.x1, 0));
                    _$y.text(Math.max(selection.y1, 0));
                }
            }, 0);
        },

        // #endregion

        // #region Create conrols
        _createControls = function () {
            _createImgAreaSelect();

            _createSlider();
        },

        // #endregion

        _setSize = function ($img) {
            if (_$aside && $img) {
            	var padding = 10;
            	var width = _$aside.width() + $img.outerWidth() + padding * 3;
            	var height = Math.max(_$aside.height(), $img.outerHeight() + padding) + padding * 2;
            	var winWidth = Math.min(width, $(window).width() * 0.8);
            	var winHeight = Math.min(height, $(window).height() * 0.8);
            	var imgContainerWidth = winWidth - _$aside.width() - padding;
            	var imgContainerHeight = winHeight - padding;
            	_$windowElement.find(".t-content").css("width", winWidth + "px");
            	_$windowElement.find(".t-content").css("height", winHeight + "px");
            	_$windowElement.find(".image").css("width", imgContainerWidth + "px");
            	_$windowElement.find(".image").css("height", imgContainerHeight + "px");
            }
        },
        _createWindow = function () {
            _window = $.telerik.window.create({
                title: $l.Crop.title,
                actions: ["Close"],

                modal: true,
                resizable: false,
                draggable: true,
                scrollable: true,
                onOpen: function (evt) {

                },
                onClose: function () {
                	_window.destroy();
                },
                onActivate: function () {

                },
                onLoad: function () {

                }
            }).data("tWindow");

            _$windowElement = jQuery(_window.element);
            _$windowElement.addClass("imgCropResizeWindow");
			_$wincontent.appendTo(_$windowElement.find(".t-content"));
        },
        _assignRootElement = function () {
            _$rootElement = _$windowElement;
        },

        // #endregion

        // #region Assign internal controls
        _createShortlinks = function () {
            _$x = _$rootElement.find(".selection-x");
            _$y = _$rootElement.find(".selection-y");
            _$width = _$rootElement.find(".selection-width");
            _$height = _$rootElement.find(".selection-height");
            _$sliderValueSpan = _$rootElement.find(".slider-value");
            _$btnSave = _$rootElement.find(".button-save");
            _$saveForm = _$rootElement.find(".saveForm");
            _$nouislider = _$rootElement.find(".nouislider");
            _$errors = _$rootElement.find(".errors");
            _$errorsContainer = _$rootElement.find(".errorsContainer");
            _$radioSaveCopy = _$rootElement.find(".image-save-copy");
            _$radioOverwrite = _$rootElement.find(".image-overwrite");
            _$inputUserFileName = _$rootElement.find(".filename-input");
            _$radioInputs = _$rootElement.find("input[name=overwrite]:radio");
            _$imgArea = _$rootElement.find("img.img");
        },

        // #endregion

        // #region Misc methods
        _endsWith = function (suffix, str) {
            return str.indexOf(suffix, str.length - suffix.length) !== -1;
        },
        _getFolder = function (url) {
            return url.substring(0, url.lastIndexOf("/"));
        },
		_getExtension = function (url) {
			var index = url.lastIndexOf(".");
			return (index == -1) ? "" : url.substr(index, url.length - index);
		},
        _removeErrors = function () {
            _$errors.empty();
            _$errorsContainer.hide();
        },
        _checkPrameters = function () {
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
 throw new Error("imgCropResize: resize range must be positive");
}

            if (_parameters.resizeRange.min > _parameters.resizeRange.max) {
 throw new Error("imgCropResize: resizeRange.min must be less than resizeRange.max");
}
        },

		_getFromCache = function () {
			return Quantumart.QP8.ImageCropResizeClient.Cache[_parameters.sourceImageUrl];
		},

		_removeFromCache = function () {
			delete Quantumart.QP8.ImageCropResizeClient.Cache[_parameters.sourceImageUrl];
		},

		_saveToCache = function () {
			var sendData = readData();
			if (sendData.width && sendData.height) {
 Quantumart.QP8.ImageCropResizeClient.Cache[_parameters.sourceImageUrl]
					= { crop: { top: sendData.top, left: sendData.left, width: sendData.width, height: sendData.height } };
}
		},

        // #endregion

        // #region Ajax methods
        _serverCheckFileName = function (userInputFileName) {
            $.ajax(_parameters.checkFileNameActionUrl, {

                data: { targetFileUrl: userInputFileName },
                type: "POST",
                dataType: "json"

            }).success(function (response) {
                if (!response.ok) {
                    var message = response.message || _defaultErrorMessage;
                    displayErrors([message]);
                } else {
                    _serverOperateImage();
                }
            }).fail(function (error) {
                displayErrors([_defaultErrorMessage]);
            });
        },
        _serverOperateImage = function () {
            var sendData = readData();
            sendData = JSON.stringify(sendData);

            $.ajax(_parameters.cropResizeActionUrl, {

                contentType: "application/json",
                data: sendData,
                type: "POST",
                dataType: "json"

            }).success(function (response) {
                if (!response.ok) {
                    var message = response.message || _defaultErrorMessage;
                    displayErrors([message]);
                } else {
                	if (typeof _parameters.onCompleteCallback == "function") {
                		if (_getSaveMode() == _saveMode.saveAs) {
                			_saveToCache();
                		} else {
                			_removeFromCache();
                		}
                        _parameters.onCompleteCallback();
                    }
                }
            }).fail(function (error) {
                displayErrors([_defaultErrorMessage]);
            });
        },

        // #endregion

        // #region Create content

        _createContent = function () {
            // main content
            _$wincontent = $("<div class='imgCropResize content-container' />");

            // controls
            _$aside = $(" <div class='aside'></div>");

            // image
            _$imageContainerDiv = $("<div class='image'/>");
            var url = _parameters.sourceImageUrl + "?t=" + new Date().getTime();
            _$img = $("<img class='img' src='" + url  + "' alt='изображение' />");
            _$img.load(function () {
            	_imgInitWidth = _$img.width();
            	_imgInitHeight = _$img.height();
            	_setSize($(this));
            	_window.center();
            });
            _$img.appendTo(_$imageContainerDiv);



            // save overwrite options
            var $fieldsetSave = $(_fieldsetSaveHtml);
            if (_parameters.allowFileRewrite) {
                $fieldsetSave.find(".save-items").prepend($(_saveOverwriteOrSaveAsHtml));
            } else {
                $fieldsetSave.find(".save-items").prepend($(_saveOverwriteHtml));
            }

            // all
            _$wincontent.append(
                _$aside.append(
                    $(_fieldsetCropHtml),
                    $(_fieldsetSliderHtml),
                    $fieldsetSave,
                    $(_fieldsetErrorsContainerHtml)
                ),
                _$aside,
                _$imageContainerDiv);
        },

        _saveMode = {
            overwrite: "overwrite",
            saveAs: "saveAs"
        },

        _getSaveMode = function () {
            if (!_parameters.allowFileRewrite || _$radioSaveCopy.attr("checked")) {
                return _saveMode.saveAs;
            }
            return _saveMode.overwrite;
        },

        _validate = function () {
            var saveMode = _getSaveMode();
            if (saveMode == _saveMode.saveAs && !_$inputUserFileName.val()) {
                displayErrors([$l.Crop.enterFileName]);
                return false;
            }
            _removeErrors();
            return true;
        },

        _attachHandlers = function () {
            _$btnSave.click(function () {
                if (_validate()) {
                	_removeErrors();
                	if (_getSaveMode() == _saveMode.overwrite) {
                		_serverOperateImage();
                	} else {
                		var formOptions = readData();
                		_serverCheckFileName(formOptions.targetFileUrl);
                	}
                }
            });

            _$saveForm.submit(function (e) {
            	e.preventDefault();
            	_$btnSave.trigger("click");
            	return false;
            });

            _$radioInputs.change(function (radio) {
                var saveCopy = _$radioSaveCopy.attr("checked");
                if (saveCopy) {
 _$inputUserFileName.removeAttr("disabled");
} else {
 _$inputUserFileName.attr("disabled", "disabled");
}
            });
        }

    // #endregion
    ;

    // #region Create Control method
    imgCropResize.create = function (parameters) {
    	_parameters = {};
    	$.extend(_parameters, _defaultParameters);
    	$.extend(_parameters, parameters);
    	$.extend(_parameters, _getFromCache());

        _checkPrameters();
        _createContent();
        _createWindow();
        _assignRootElement();
        _createShortlinks();
        _createControls();
        _attachHandlers();

        return {
            type: "Quantumart.QP8.ImageCropResizeClient",
            displayErrors: displayErrors,
            getOptions: readData,
            openWindow: show,
            closeWindow: close,
            dispose: dispose,
            parameters: _parameters,
            window: _$windowElement
        };
    };

    // #endregion

    // #region Public API
    var displayErrors = function (errors) {
        _removeErrors();
        for (var i in errors) {
            _$errors.append($("<li>" + errors[i] + "</li>"));
        }

        _$errorsContainer.show();
        _setSize(_$img);
    };
    var readData = function () {
        var targetFileUrl = "";
        var overwriteFile = _getSaveMode() == _saveMode.overwrite;

        if (overwriteFile) {
            targetFileUrl = _parameters.sourceImageUrl;
        } else {
        	var value = $.trim(_$inputUserFileName.val());
        	if (value != "" && value.lastIndexOf(".") == -1) {
 value += _getExtension(_parameters.sourceImageUrl);
}

        	if (_parameters.resultImageFolder) {
                targetFileUrl = _endsWith("/", _parameters.resultImageFolder)
                    ? _parameters.resultImageFolder + value : "/" + value;
            } else {
                targetFileUrl = _getFolder(_parameters.sourceImageUrl) + value;
            }
        }

        var result = {
            overwriteFile: overwriteFile,
            targetFileUrl: targetFileUrl,
            sourceFileUrl: _parameters.sourceImageUrl,
            resize: +_$nouislider.val()
        };

        var select = _imgAreaSelect.getSelection();
        if (select.width || select.height) {
            $.extend(result, {
                left: Math.max(select.x1, 0),
                top: Math.max(select.y1, 0),
                width: select.width,
                height: select.height
            });
        }

        return result;
    };
    var show = function () {
        if (_window) {
        	_window.center();
            _removeErrors();
        }
    };
    var close = function () {
        _removeErrors();
        _window.close();
    };
    var dispose = function () {
        _$imgArea.imgAreaSelect("destroy");
        _$imgArea = null;
        _window.destroy();
        _$windowElement = null;
        _$rootElement = null;
        _$x = null;
        _$y = null;
        _$width = null;
        _$height = null;
        _imgAreaSelect = null;
        _$btnSave.off("click");
		_$saveForm.off("submit");
		_$btnSave = null;
		_$saveForm = null;
        _$nouislider.off("slide");
        _$nouislider.off("_setSize");
        _$nouislider.off('mousewheel DOMMouseScroll');
        _$nouislider = null;
        _slider = null;
        _$sliderValueSpan = null;
        _$errors = null;
        _$errorsContainer = null;
        _$radioSaveCopy = null;
        _$radioOverwrite = null;
        _$inputUserFileName = null;
        _$radioInputs.off("change");
        _$radioInputs = null;
        _$img = null;
        _$aside = null;
        _$imageContainerDiv = null;
        _$wincontent = null;
        _window = null;
        _parameters = null;

        delete this.displayErrors;
        delete this.getOprions;
        delete this.openWindow;
        delete this.closeWindow;
        delete this.dispose;
        delete this.parameters;
        delete this.window;
    };

    // #endregion
}(jQuery, Quantumart.QP8.ImageCropResizeClient));
