// eslint-disable-next-line no-extra-semi
; (function init(PL, Moxie) {
  let isValidMimeType = function (mimeType, skipMimeValidation) {
    return !skipMimeValidation || mimeType.split('/')[0] === 'image';
  };

  PL.addFileFilter('max_img_resolution', function filterCb(options, file, cb) {
    let img;
    let opts = Object.assign({}, {
      enabled: true,
      imageResolution: 640 * 480,
      skipMimeValidation: false,
      getResolutionErrorSettings: function (imgRes) {
        return {
          code: PL.IMAGE_DIMENSIONS_ERROR,
          message: `Resolution exceeds the allowed limit of ${imgRes} pixels.`
        };
      },
      getNotAnImgErrorSettings: function () {
        return {
          code: PL.IMAGE_FORMAT_ERROR,
          message: `Checking file mime type failed for file: "${file.name}".`
        };
      }
    }, options);

    let finalize = function finalize(result, errorSettings) {
      if (img) {
        img.destroy();
        img = null;
      }

      if (!result) {
        this.trigger('Error', Object.assign({ file: file }, errorSettings));
      }

      cb(result);
    }.bind(this);

    opts.prefilterAction();
    if (opts.enabled) {
      if (isValidMimeType(file.type, opts.skipMimeValidation)) {
        img = new Moxie.Image();

        img.onload = function () {
          finalize(img.width * img.height < opts.imageResolution, opts.getResolutionErrorSettings());
        };

        img.onerror = function () {
          finalize(false, opts.getResolutionErrorSettings());
        };

        img.load(file.getSource());
      } else {
        finalize(false, opts.getNotAnImgErrorSettings());
      }
    } else {
      finalize(true);
    }
  });
}(window.plupload, window.mOxie));
