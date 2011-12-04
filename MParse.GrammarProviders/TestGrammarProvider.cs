using System.Collections.Generic;
using System.Linq;
using MParse.Core.GrammarElements;
using MParse.Core.Interfaces;
using MParse.Core;

namespace MParse.GrammarProviders
{
    public class TestGrammarProvider : IGrammarProvider
    {
        public enum T
        {
            S,
            E,
            T,
            F,
            Plus,
            Times,
            Id,
            Lparen,
            Rparen,
            EOF
        }
        
        private readonly Dictionary<T, GrammarSymbol> _symbols;
        private readonly List<Production> _productions;

        public TestGrammarProvider()
        {
            _symbols = new Dictionary<T, GrammarSymbol>
                           {
                               {T.S,        new NonTerminal ((int) T.S,      T.S.ToString())},
                               {T.E,        new NonTerminal ((int) T.E,      T.E.ToString())},
                               {T.T,        new NonTerminal ((int) T.T,      T.T.ToString())},
                               {T.F,        new NonTerminal ((int) T.F,      T.F.ToString())},
                               {T.Plus,     new Terminal    ((int) T.Plus,   T.Plus.ToString())},
                               {T.Times,    new Terminal    ((int) T.Times,  T.Times.ToString())},
                               {T.Id,       new Terminal    ((int) T.Id,     T.Id.ToString())},
                               {T.Lparen,   new Terminal    ((int) T.Lparen, T.Lparen.ToString())},
                               {T.Rparen,   new Terminal    ((int) T.Rparen, T.Rparen.ToString())},
                               {T.EOF,   new EndOfStream    ()}
                           };
            _productions = new List<Production>
                               {
                                   new Production(0, _symbols[T.S], new[] {_symbols[T.E], _symbols[T.EOF]                              }),
                                   new Production(1, _symbols[T.E], new[] {_symbols[T.E],      _symbols[T.Plus],   _symbols[T.T]       }),
                                   new Production(2, _symbols[T.E], new[] {_symbols[T.T]                                               }),
                                   new Production(3, _symbols[T.T], new[] {_symbols[T.T],      _symbols[T.Times],  _symbols[T.F]       }),
                                   new Production(4, _symbols[T.T], new[] {_symbols[T.F]                                               }),
                                   new Production(5, _symbols[T.F], new[] {_symbols[T.Lparen], _symbols[T.E],      _symbols[T.Rparen]  }),
                                   new Production(6, _symbols[T.F], new[] {_symbols[T.Id]                                              })
                               };
        }

        private Production[] GetProductions()
        {
            return _productions.ToArray();
        }

        private GrammarSymbol[] GetGrammarSymbols()
        {
            return _symbols.Select(x => x.Value).ToArray();
        }

        private Item GetAugmentedState()
        {
            return new Item(_productions[0]);
        }

        public Grammar GetGrammar()
        {
            return new Grammar(GetProductions(), GetGrammarSymbols(), GetAugmentedState());
        }
    }
}