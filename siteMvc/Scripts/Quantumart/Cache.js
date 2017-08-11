// ****************************************************************************
// *** Компонент "Кэш"														***
// ****************************************************************************

// #region class Cache
// === Класс "Кэш" ===
Quantumart.QP8.Cache = function () {
};

Quantumart.QP8.Cache._itemInfos = {};

Quantumart.QP8.Cache.getItem = function (key) {
	// / <summary>
	// / Возвращает элемент из кэша
	// / </summary>
	// / <param name="key" type="String">ключ</param>
	// / <returns type="Object">элемент</returns>
	var item = null;
	if (Quantumart.QP8.Cache._itemInfos[key]) {
		item = Quantumart.QP8.Cache._itemInfos[key].Value;
	}

	return item;
};

Quantumart.QP8.Cache.addItem = function (key, value) {
	// / <summary>
	// / Добавляет элемент в кэш
	// / </summary>
	// / <param name="key" type="String">ключ</param>
	// / <param name="value" type="Object">элемент</param>
	var itemInfo = { Value: value };

	Quantumart.QP8.Cache._itemInfos[key] = itemInfo;
};

Quantumart.QP8.Cache.removeItem = function (key) {
	// / <summary>
	// / Удаляет элемент из кэша
	// / </summary>
	// / <param name="key" type="String">ключ</param>
	$q.removeProperty(Quantumart.QP8.Cache._itemInfos, key);
};

Quantumart.QP8.Cache.clear = function () {
	// / <summary>
	// / Очищает кэш
	// / </summary>
	for (var key in Quantumart.QP8.Cache._itemInfos) {
		Quantumart.QP8.Cache.removeItem(key);
	}
};

Quantumart.QP8.Cache.dispose = function () {
	// / <summary>
	// / Уничтожает кэш
	// / </summary>
	if ($q.getHashKeysCount(Quantumart.QP8.Cache._itemInfos) > 0) {
		Quantumart.QP8.Cache.clear();
	}
	Quantumart.QP8.Cache._itemInfos = null;
};

Quantumart.QP8.Cache.registerClass("Quantumart.QP8.Cache");

// #endregion

// Сокращенная запись
window.$cache = Quantumart.QP8.Cache;
