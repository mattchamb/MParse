using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MParse.Core.GrammarElements;

namespace MParse.Core
{
    public class Grammar
    {
        public IList<Production> Productions { get; private set; }
        public IList<GrammarSymbol> Symbols { get; private set; }
        public Item AugmentedState { get; private set; }

        public Grammar(IList<Production> productions, IList<GrammarSymbol> symbols, Item augmentedState)
        {
            Productions = productions;
            Symbols = symbols;
            AugmentedState = augmentedState;
        }
    }
}
