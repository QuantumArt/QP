// @ts-ignore
const SUPER_METHODS = Symbol ? Symbol('__superMethods__') : '__superMethods__';

/**
 * Forces invocations of this function to always have `this` refer to the class instance,
 * even if the function is passed around or would otherwise lose its `this` context.
 * @param {Object} target
 * @param {string} key
 * @param {object} descriptor
 * @returns {object} PropertyDescriptor
 */
export const bind = function (target, key, descriptor) {
  const { constructor } = target;
  const {
    value: method,
    configurable,
    enumerable
  } = descriptor || Object.getOwnPropertyDescriptor(target, key);

  return {
    configurable,
    enumerable,
    get() {
      // Class.prototype.key lookup
      if (!(this instanceof this.constructor)) {
        return method;
      }

      // Bound method calling super.sameMethod() which is also bound and so on
      if (this.constructor !== constructor) {
        let superMethods = this[SUPER_METHODS];
        if (!superMethods) {
          Object.defineProperty(this, SUPER_METHODS, {
            value: superMethods = []
          });
        }
        let methodEntry = superMethods.find(pair => pair.key === method);
        if (!methodEntry) {
          superMethods.push(methodEntry = {
            key: method,
            value: method.bind(this)
          });
        }
        return methodEntry.value;
      }

      const bound = method.bind(this);

      Object.defineProperty(this, key, {
        configurable: true,
        writable: true,
        enumerable: false,
        value: bound
      });

      return bound;
    },
    set(newMethod) {
      Object.defineProperty(this, key, {
        configurable: true,
        writable: true,
        enumerable: true,
        value: newMethod
      });

      return newMethod;
    }
  };
};
