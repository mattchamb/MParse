using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using MParse;

namespace MParse.Tests
{
    [TestFixture]
    public class GrammarProviderTests
    {
        public GrammarProvider Grammar;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            Grammar = new DummyGrammarProvider();
            //Grammar = new AnotherDummy();
        }

        [Test]
        public void ProductionsNotNullTest()
        {
            var productions = Grammar.GetProductions();
            Assert.IsNotNull(productions);
        }

        [Test]
        public void ClosureTest()
        {
            var itemForTest = Grammar.GetProductions()[0].GetItems()[0];
            var closure = Grammar.GetClosure(new List<Item> {itemForTest});
            foreach (var item in closure)
            {
                Console.WriteLine(item);
            }
        }

        [Test]
        public void NonTerminalFirstSetTest()
        {
            var expected = new int[] {0, 1};
            var productionForTest = Grammar.GetProductions()[0];
            var result = Grammar.FirstSet(productionForTest.Head);
            Assert.IsTrue(result.SequenceEqual(expected));
        }

        [Test]
        public void TerminalFirstSetTest()
        {
            var expected = new int[] { 0 };
            var result = Grammar.FirstSet(0);
            Assert.IsTrue(result.SequenceEqual(expected));
        }

        [Test]
        public void EmptyFollowSetTest()
        {
            var expected = new int[] { };
            var result = Grammar.FollowSet(6);
            Assert.IsTrue(result.SequenceEqual(expected));
        }

        [Test]
        public void FollowSetTest()
        {
            var expected = new int[] { 2, 3 };
            var result = Grammar.FollowSet(4);
            Assert.IsTrue(result.SequenceEqual(expected));
        }

        [Test]
        public void StatesTest()
        {
            var states = Grammar.CreateStates();
            var transitionTable = Grammar.CreateTransitionTable(states);
        }
    }
}
