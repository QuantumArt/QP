/**
 * Factory that creates event function.
 * Which delegates it's execution to list of handlers.
 */
export declare function event<TSender>(
  sender: TSender,
): Event<
  (...args: any[]) => void,
  (sender: TSender, ...args: any[]) => void
>;

export declare function event<TSender>(
  sender: TSender,
  dataType: new () => String,
): Event<
  (data: string) => void,
  (sender: TSender, data: string) => void
>;

export declare function event<TSender>(
  sender: TSender,
  dataType: new () => Number,
): Event<
  (data: number) => void,
  (sender: TSender, data: number) => void
>;

export declare function event<TSender>(
  sender: TSender,
  dataType: new () => Boolean,
): Event<
  (data: boolean) => void,
  (sender: TSender, data: boolean) => void
>;

export declare function event<TSender, TData>(
  sender: TSender,
  dataType: new () => TData,
): Event<
  (data: TData) => void,
  (sender: TSender, data: TData) => void
>;

/**
 * Factory that creates async event function.
 * Which delegates it's execution to list of handlers.
 */
export declare function asyncEvent<TSender>(
  sender: TSender,
): Event<
  (...args: any[]) => Promise<void>,
  (sender: TSender, ...args: any[]) => Promise<void> | void
>;

export declare function asyncEvent<TSender>(
  sender: TSender,
  dataType: new () => String,
): Event<
  (data: string) => Promise<void>,
  (sender: TSender, data: string) => Promise<void> | void
>;

export declare function asyncEvent<TSender>(
  sender: TSender,
  dataType: new () => Number,
): Event<
  (data: number) => Promise<void>,
  (sender: TSender, data: number) => Promise<void> | void
>;

export declare function asyncEvent<TSender>(
  sender: TSender,
  dataType: new () => Boolean,
): Event<
  (data: boolean) => Promise<void>,
  (sender: TSender, data: boolean) => Promise<void> | void
>;

export declare function asyncEvent<TSender, TData>(
  sender: TSender,
  dataType: new () => TData,
): Event<
  (data: TData) => Promise<void>,
  (sender: TSender, data: TData) => Promise<void> | void
>;

/** Represents a general typed event */
export type Event<TMethod, THandler> = TMethod & {
  /**
   * Add the specified handler function to the list of event handlers.
   */
  attach(handler: THandler): Event<TMethod, THandler>;
  /**
   * Removes handler, previously registered with `.attach()`, from the event handlers list.
   */
  detach(handler: THandler): Event<TMethod, THandler>;
}
