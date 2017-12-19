class UrlHelpers {
  constructor(rootPath = '/') {
    this.rootPath = rootPath;
  }

  Content(relativeUrl) {
    let normalizedUrl = relativeUrl;
    if (normalizedUrl.substring(0, 1) === '~') {
      normalizedUrl = normalizedUrl.substring(1);
    }

    if (normalizedUrl.substring(0, 1) === '/') {
      normalizedUrl = normalizedUrl.substring(1);
    }

    return this.rootPath + normalizedUrl;
  }

  SetRootPath(rootUrl) {
    this.rootPath = rootUrl;
  }
}

// eslint-disable-next-line no-shadow
const Url = new UrlHelpers();

window.Url = Url;
