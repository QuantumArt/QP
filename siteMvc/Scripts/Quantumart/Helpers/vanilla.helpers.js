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

window.Global = window.Global || {};
window.Global.UrlHelpers = new UrlHelpers();
window.Url = window.Global.UrlHelpers;

export default UrlHelpers;
