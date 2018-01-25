interface JQuery {
  backendProgressBar(options?: {
    total?: number,
    value?: number,
    digits?: number,
  }): void;

  backendProgressBar(method: 'init', options?: {
    total?: number,
    value?: number,
    digits?: number,
  }): void;

  backendProgressBar(method: 'dispose'): void;
}
