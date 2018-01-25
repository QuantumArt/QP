/**
 * Factory that creates event function.
 * Which delegates it's execution to list of handlers.
 * @returns Event function
 */
export const event = function () {
  const listeners = [];

  const e = function (...args) {
    listeners.slice().forEach(handler => {
      handler(...args);
    });
  };

  e.attach = handler => {
    listeners.push(handler);
  };

  e.detach = handler => {
    // eslint-disable-next-line no-bitwise
    listeners.splice(listeners.indexOf(handler) >>> 0, 1);
  };

  return e;
};
