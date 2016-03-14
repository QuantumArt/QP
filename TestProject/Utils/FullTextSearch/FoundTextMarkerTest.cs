using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.Utils.FullTextSearch;

namespace WebMvc.Test.Utils.FullTextSearch
{
	[TestClass]
	public class FoundTextMarkerTest
	{
		[TestMethod]
		public void FindWordFormPositionDictionaryTest()
		{			
			IEnumerable<string> forms = new[] {"тарифы",
												"тарифу",
												"тарифом",
												"тарифов",
												"тарифе",
												"тарифах",
												"тарифами",
												"тарифам",
												"тарифа",
												"тариф",
												"интернету",
												"интернетом",
												"интернете",
												"интернета",
												"интернет"};

			var res = FoundTextMarker.GetRelevantMarkedText("ЭТОТ ТАРИФ НА ИНТЕРНЕТ ОЧЕНЬ ВЫГОДЕН", forms, 10, "<b>", "</b>");
			Assert.AreEqual("ЭТОТ <b>ТАРИФ</b> НА <b>ИНТЕРНЕТ</b>", res);

			res = FoundTextMarker.GetRelevantMarkedText("Есть много новых тарифов которые стоят не дорого.", forms, 10, "<b>", "</b>");
			Assert.AreEqual("много новых <b>тарифов</b> которые стоят", res);

			res = FoundTextMarker.GetRelevantMarkedText("Сейчас очень быстрый доступ в ИНТЕРНЕТ.", forms, 10, "<b>", "</b>");
			Assert.AreEqual("быстрый доступ в <b>ИНТЕРНЕТ</b>.", res);

			res = FoundTextMarker.GetRelevantMarkedText("Вообще левая строка", forms, 10, "<b>", "</b>");
			Assert.AreEqual("Вообще левая", res);

			res = FoundTextMarker.GetRelevantMarkedText("", forms, 10, "<b>", "</b>");
			Assert.AreEqual(String.Empty, res);

			res = FoundTextMarker.GetRelevantMarkedText(null, forms, 10, "<b>", "</b>");
			Assert.IsNull(res);

			res = FoundTextMarker.GetRelevantMarkedText("этот тариф на интернет очень выгоден", Enumerable.Empty<string>(), 10, "<b>", "</b>");
			Assert.AreEqual("этот тариф", res);
		}
	}
}
