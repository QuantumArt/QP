window.HISTORY_PREVENT_NAVIGATION_STATE = 'HISTORY_PREVENT_NAVIGATION_STATE';
window.HISTORY_INITIAL_STATE = 'HISTORY_INITIAL_STATE';

export class BackendBrowserHistoryManager {
  static preventBrowserNavigateBack() {
    if (window.history.state === null) {
      window.history.replaceState(window.HISTORY_PREVENT_NAVIGATION_STATE, document.title);
      window.history.pushState(window.HISTORY_INITIAL_STATE, document.title);
    }
    window.addEventListener('popstate', () => {
      if (window.history.state === window.HISTORY_PREVENT_NAVIGATION_STATE) {
        window.history.forward();
      }
    });
  }
}

Quantumart.QP8.BackendBrowserHistoryManager = BackendBrowserHistoryManager;
