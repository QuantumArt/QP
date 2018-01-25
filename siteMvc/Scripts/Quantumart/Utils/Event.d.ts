/**
 * Factory that creates event function.
 * Which delegates it's execution to list of handlers.
 */
export declare function event<TFunction = (...args: any[]) => void>(): Delegate<TFunction>;
export declare function event<TData>(
  dataType: new () => TData,
): Delegate<(data: TData) => void>;
export declare function event<TSender, TData>(
  senderType: new () => TSender,
  dataType: new () => TData,
): Delegate<(sender: TSender, data: TData) => void>;
export declare function event<TSender, TData, TExtra>(
  senderType: new () => TSender,
  dataType: new () => TData,
  extraType: new () => TExtra,
): Delegate<(sender: TSender, data: TData, extra: TExtra) => void>;

/** Represents a general typed event */
export type Delegate<TFunction> = TFunction & {
  /**
   * Add the specified handler function to the list of event handlers.
   */
  attach(handler: TFunction): void;
  /**
   * Removes handler, previously registered with `.attach()`, from the event handlers list.
   */
  detach(handler: TFunction): void;
}

/** Represents the typed event when the event provides sender and data. */
export type EventHandler<TEventArgs> = Delegate<(
  sender: object,
  eventArgs: TEventArgs,
) => void>;
