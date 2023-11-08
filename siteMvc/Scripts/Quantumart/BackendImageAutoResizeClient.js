/* eslint-disable max-lines */
/* eslint-disable max-statements */
/* eslint-disable max-len */
import { $q } from 'Utils';

export const ImageAutoResizeClient = {};
ImageAutoResizeClient._cache = {};

// eslint-disable-next-line
(function ($, imgAutoResize) {
  let _parameters = null;

  const displayErrors = function (errors) {
    $q.alertFail(errors);
  };

  const _defaultParameters = {
    checkAutoResizeActionUrl: `${window.APPLICATION_ROOT_URL}Library/CheckForAutoResize/`,
    autoResizeActionUrl: `${window.APPLICATION_ROOT_URL}Library/AutoResize/`,
    sourceFilePath: ``
  };

  const _checkParameters = function () {
    if (!_parameters.checkAutoResizeActionUrl) {
      throw new Error("imgAutoResize: parameter 'checkAutoResizeActionUrl' not set");
    }

    if (!_parameters.autoResizeActionUrl) {
      throw new Error("imgAutoResize: parameter 'autoResizeActionUrl' not set");
    }
  };

  const _autoResizeCheckParameters = function (folderUrl, fileName, baseUrl) {

    const resizeCheckParams = { folderUrl, fileName, baseUrl };

    $q.getJsonFromUrl('POST', _parameters.checkAutoResizeActionUrl,
      resizeCheckParams
    ).done(response => {
      if (response.ok) {
        const resizeParams = {
          folderUrl, fileName, baseUrl,
          reduceSizes: response.reduceSizes,
          resizedImageTemplate: response.resizedImageTemplate
        };
        _autoresize(resizeParams);
      } else {
        const message = response.message || $l.Crop.defaultErrorMessage;
        displayErrors([message]);
      }
    }).fail(() => {
      displayErrors([$l.Crop.defaultErrorMessage]);
    });
  };

  const _autoresize = function(resizeParams)  {
    $q.getJsonFromUrl('POST', _parameters.autoResizeActionUrl, resizeParams).done(response => {
      if (!response.ok) {
        const message = response.message || $l.Crop.defaultErrorMessage;
        displayErrors([message]);
      } else if (typeof _parameters.onCompleteCallback === 'function') {
           _parameters.onCompleteCallback();
      }
    }).fail(() => {
      displayErrors([$l.Crop.defaultErrorMessage]);
    });
  }

  const autoResize = function(folderUrl, fileName, baseUrl) {
    if ($q.confirmMessage(String.format(window.AUTO_RESIZE_CONFIRM_MESSAGE, fileName))) {
      _autoResizeCheckParameters(folderUrl, fileName, baseUrl);
    }
  }

  // eslint-disable-next-line
  imgAutoResize.create = function (parameters) {
    _parameters = {};
    Object.assign(_parameters, _defaultParameters);
    Object.assign(_parameters, parameters);

    _checkParameters();

    return {
      type: 'Quantumart.QP8.ImageAutoResizeClient',
      displayErrors,
      autoResize,
      parameters: _parameters
    };
  };
}(jQuery, ImageAutoResizeClient));

Quantumart.QP8.ImageAutoResizeClient = ImageAutoResizeClient;
