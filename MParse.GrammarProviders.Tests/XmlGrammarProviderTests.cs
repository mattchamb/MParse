using System.Collections.Generic;
using System.Linq;
using MParse.Core.GrammarElements;
using NUnit.Framework;
using MParse.Core.Interfaces;

namespace MParse.GrammarProviders.Tests
{
    [TestFixture]
    public class XmlGrammarProviderTests
    {
        IGrammarProvider _provider;

        [SetUp]
        public void Setup()
        {
            const string inputXml =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Grammar>
  <Production>
    <Head>E</Head>
    <Body>
      <Symbol>id</Symbol>
    </Body>
  </Production>
</Grammar>
";
            _provider = new XmlGrammarProvider();
            _provider.SetInput(inputXml);
        }

        [Test]
        public void TestGetProductions()
        {
            var expected = new[]
                               {
                                   new Production(new NonTerminal(0, "_START_"), new GrammarSymbol[]
                                                                                     {
                                                                                         new NonTerminal(1, "E"),
                                                                                         new EndOfStream()
                                                                                     }),
                                   new Production(new NonTerminal(1, "E"), new GrammarSymbol[]
                                                                               {
                                                                                   new Terminal(2, "id")
                                                                               })
                               };
            var result = _provider.GetProductions();
            Assert.IsTrue(expected.SequenceEqual(result));
        }

        [Test]
        public void TestGetGrammarSymbols()
        {
            var expected = new HashSet<GrammarSymbol>
                               {
                                   new NonTerminal(0, "_START_"),
                                   new NonTerminal(1, "E"),
                                   new Terminal(2, "id"),
                                   new EndOfStream()
                               };
            var result = _provider.GetGrammarSymbols();
            Assert.IsTrue(expected.SetEquals(result));
        }

        [Test]
        public void TestGetAugmentedState()
        {
            var expected = new Item(0,
                                    new Production(new NonTerminal(0, "_START_"),
                                                   new GrammarSymbol[]
                                                       {
                                                           new NonTerminal(1, "E"),
                                                           new EndOfStream()
                                                       }));
            var result = _provider.GetAugmentedState();
            Assert.AreEqual(expected, result);
        }
    }
}
