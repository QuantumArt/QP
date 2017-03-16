using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quantumart.QP8.Utils;

namespace QP8.WebMvc.NUnit.Tests.Utils
{
    [TestClass]
    public class GraphHeplerTest
    {
        [TestMethod]
        public void CheckCycleInGraph_GrachHasCycles_ReturnTrue()
        {
            var graph = new Dictionary<int, int[]>
            {
                {0, new[]{1,2}},
                {1, new[]{3}},
                {3, new[]{4}},
                {4, new[]{1,5}},
                {2, new[]{4, 5}}
            };

            Assert.IsTrue(GraphHepler.CheckCycleInGraph(graph));
        }

        [TestMethod]
        public void CheckCycleInGraph_GrachDoesntHaveCycles_ReturnFalse()
        {
            var graph = new Dictionary<int, int[]>
            {
                {0, new[]{1,2}},
                {1, new[]{3}},
                {2, new[]{4, 5, 1}}
            };

            Assert.IsFalse(GraphHepler.CheckCycleInGraph(graph));
            graph = new Dictionary<int, int[]>
            {
                {0, new[]{1,2}},
                {1, new[]{3, 2}},
                {2, new[]{4, 5, 3}}
            };

            Assert.IsFalse(GraphHepler.CheckCycleInGraph(graph));
        }
    }
}
