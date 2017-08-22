if (Function.prototype.bind && window.console && typeof window.console.log === 'object') {
  ['log', 'info', 'warn', 'error', 'assert', 'dir', 'clear', 'profile', 'profileEnd'].forEach(function (method) {
    window.console[method] = this.bind(window.console[method], window.console);
  }, Function.prototype.call);
}
