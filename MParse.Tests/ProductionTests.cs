using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MParse.Tests
{
    [TestFixture]
    class ProductionTests
    {

        public Production Prod;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            Prod = new Production(0, new[] {1, 2, 3});
        }

        [Test]
        public void GetItemsTest()
        {
            var result = Prod.GetItems();
            var expected = new List<Item>
                               {
                                   new Item(0, Prod),
                                   new Item(1, Prod),
                                   new Item(2, Prod),
                                   new Item(3, Prod)
                               };
            Assert.IsTrue(result.SequenceEqual(expected));
        }
    }
}
