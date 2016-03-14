; (function(PL, Moxie) {
  'use strict';

  function isValidMimeType(mimeType, skipMimeValidation) {
    return !skipMimeValidation || mimeType.split('/')[0] === 'image';
  }

  PL.addFileFilter('max_img_resolution', function(options, file, cb) {
    var img;
    var opts = $.extend({}, {
      enabled: true,
      imageResolution: 640 * 480,
      skipMimeValidation: false,
      getResolutionErrorSettings: function(imgRes) {
        return {
          code: PL.IMAGE_DIMENSIONS_ERROR,
          message: 'Resolution exceeds the allowed limit of ' + imgRes + ' pixels.'
        };
      },
      getNotAnImgErrorSettings: function() {
        return {
          code: PL.IMAGE_FORMAT_ERROR,
          message: 'Checking file mime type failed for file: "' + file.name + '".'
        };
      }
    }, options);

    var finalize = function(result, errorSettings) {
      if (img) {
        img.destroy();
        img = null;
      }

      if (!result) {
        this.trigger('Error', $.extend({ file: file }, errorSettings));
      }

      cb(result);
    }.bind(this);

    opts.prefilterAction();
    if (opts.enabled) {
      if (!isValidMimeType(file.type, opts.skipMimeValidation)) {
        finalize(false, opts.getNotAnImgErrorSettings());
      } else {
        img = new Moxie.Image();
        img.onload = function() {
          finalize(img.width * img.height < opts.imageResolution, opts.getResolutionErrorSettings());
        };

        img.onerror = function() {
          finalize(false, opts.getResolutionErrorSettings());
        };

        img.load(file.getSource());
      }
    } else {
      finalize(true);
    }
  });
}(window.plupload, window.mOxie));
