using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.Utils;

namespace WebMvc.Test.Utils
{
	[TestClass]
	public class GraphHeplerTest
	{
		[TestMethod]
		public void CheckCycleInGraph_GrachHasCycles_ReturnTrue()
		{			
			Dictionary<int, int[]> graph = new Dictionary<int, int[]> 
			{
				{0, new int[]{1,2}},
				{1, new int[]{3}},
				{3, new int[]{4}},
				{4, new int[]{1,5}},
				{2, new int[]{4, 5}}
			};
			Assert.IsTrue(GraphHepler.CheckCycleInGraph(graph));
		}

		[TestMethod]
		public void CheckCycleInGraph_GrachDoesntHaveCycles_ReturnFalse()
		{

			Dictionary<int, int[]> graph = new Dictionary<int, int[]> 
			{
				{0, new int[]{1,2}},
				{1, new int[]{3}},
				{2, new int[]{4, 5, 1}}
			};
			Assert.IsFalse(GraphHepler.CheckCycleInGraph(graph));

			graph = new Dictionary<int, int[]> 
			{
				{0, new int[]{1,2}},
				{1, new int[]{3, 2}},
				{2, new int[]{4, 5, 3}}
			};
			Assert.IsFalse(GraphHepler.CheckCycleInGraph(graph));			
		}
	}
}
