/**
 * Factory that creates event function.
 * Which delegates it's execution to list of handlers.
 * @param {object} sender Sender object
 * @returns Event function
 */
export const event = function (sender) {
  const listeners = [];

  const e = function (...args) {
    const promises = listeners
      .slice()
      .map(handler => handler(sender, ...args))
      .filter(promise => promise instanceof Promise);

    return Promise.all(promises);
  };

  e.attach = handler => {
    listeners.push(handler);
    return e;
  };

  e.detach = handler => {
    // eslint-disable-next-line no-bitwise
    listeners.splice(listeners.indexOf(handler) >>> 0, 1);
    return e;
  };

  return e;
};
