Quantumart.QP8.Cache = function () {
};

Quantumart.QP8.Cache._itemInfos = {};
Quantumart.QP8.Cache.getItem = function (key) {
  let item = null;
  if (Quantumart.QP8.Cache._itemInfos[key]) {
    item = Quantumart.QP8.Cache._itemInfos[key].Value;
  }

  return item;
};

Quantumart.QP8.Cache.addItem = function (key, value) {
  let itemInfo = { Value: value };
  Quantumart.QP8.Cache._itemInfos[key] = itemInfo;
};

Quantumart.QP8.Cache.removeItem = function (key) {
  $q.removeProperty(Quantumart.QP8.Cache._itemInfos, key);
};

Quantumart.QP8.Cache.clear = function () {
  for (let key in Quantumart.QP8.Cache._itemInfos) {
    Quantumart.QP8.Cache.removeItem(key);
  }
};

Quantumart.QP8.Cache.dispose = function () {
  if ($q.getHashKeysCount(Quantumart.QP8.Cache._itemInfos) > 0) {
    Quantumart.QP8.Cache.clear();
  }

  Quantumart.QP8.Cache._itemInfos = null;
};

Quantumart.QP8.Cache.registerClass('Quantumart.QP8.Cache');
