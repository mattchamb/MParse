using System;
using System.Collections.Generic;
using MParse.Core.GrammarElements;

namespace MParse.Core
{
    public class TransitionTable
    {
        private readonly Dictionary<ParserState, Dictionary<GrammarSymbol, TransitionAction>> _table;
        private readonly StateTransitionMap _stateMap;
        private readonly Grammar _grammar;

        public StateTransitionMap StateMap
        {
            get { return _stateMap; }
        }

        public TransitionAction this[ParserState state, GrammarSymbol symbol]
        {
            get
            {
                if(!_table.ContainsKey(state))
                    throw new InvalidOperationException("The given state is unknown for this transition table.");
                var tableEntry = _table[state];
                if (!tableEntry.ContainsKey(symbol))
                    throw new InvalidOperationException("There is no transition from the given state under the given input.");
                return tableEntry[symbol];
            }
        }

        public TransitionTable(Grammar grammar)
        {
            if (grammar == null)
                throw new ArgumentNullException("grammar");

            _table = new Dictionary<ParserState, Dictionary<GrammarSymbol, TransitionAction>>();
            _grammar = grammar;
            _stateMap = grammar.CreateStateMap();
            ConstructTable();
        }

        private void ConstructTable()
        {

            foreach (var state in _stateMap.States)
            {
                var dict = new Dictionary<GrammarSymbol, TransitionAction>();
                _table.Add(state, dict);
                foreach (var item in state.Items)
                {
                    if(item.HasNextToken && item.NextToken is Terminal)
                    {
                        dict.Add(item.NextToken, new TransitionAction(ParserAction.Shift, _stateMap[state, item.NextToken]));
                    }
                    if (!item.HasNextToken)
                    {
                        var followSet = _grammar.FollowSet(item.ItemProduction.Head);
                        foreach (var symbol in followSet)
                        {
                            dict.Add(symbol, new TransitionAction(ParserAction.Reduce, item.ItemProduction));
                        }
                    }
                }
                if (state.Items.Contains(_grammar.AugmentedState.AdvanceDot()))
                {
                    dict[EndOfStream.Instance] = new TransitionAction(ParserAction.Accept);
                }
                var symbols = _grammar.Symbols;
                foreach (var symbol in symbols)
                {
                    if (symbol is NonTerminal && _stateMap.TransitionExists(state, symbol))
                        dict.Add(symbol, new TransitionAction(ParserAction.Goto, _stateMap[state, symbol]));
                    else if(!dict.ContainsKey(symbol))
                        dict.Add(symbol, new TransitionAction(ParserAction.Error));
                }
            }
        }
    }
}