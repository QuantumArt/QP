import { $q } from './Utils';

export class GlobalCache {
  static _itemInfos = {};

  static getItem(key) {
    let item = null;
    if (GlobalCache._itemInfos[key]) {
      item = GlobalCache._itemInfos[key].Value;
    }

    return item;
  }

  static addItem(key, value) {
    const itemInfo = { Value: value };
    GlobalCache._itemInfos[key] = itemInfo;
  }

  static removeItem(key) {
    $q.removeProperty(GlobalCache._itemInfos, key);
  }

  static clear() {
    Object.keys(GlobalCache._itemInfos).forEach(key => {
      GlobalCache.removeItem(key);
    });
  }

  static dispose() {
    if ($q.getHashKeysCount(GlobalCache._itemInfos) > 0) {
      GlobalCache.clear();
    }

    GlobalCache._itemInfos = null;
  }
}

Quantumart.QP8.GlobalCache = GlobalCache;
