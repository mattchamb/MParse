using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MParse.GrammarElements;
using MParse.Interfaces;

namespace MParse
{
    public class TransitionTable
    {
        private readonly Dictionary<ParserState, Dictionary<GrammarSymbol, TransitionAction>> _table;
        private readonly IEnumerable<ParserState> _states; 
        private readonly IGrammarProvider _grammarProvider;
        private readonly IGrammarOperator _grammarOperator;

        public IEnumerable<ParserState> States
        {
            get { return _states; }
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

        public TransitionTable(IGrammarProvider grammarProvider, IGrammarOperator grammarOperator, IEnumerable<ParserState> parserStates)
        {
            if (grammarProvider == null)
                throw new ArgumentNullException("grammarProvider");
            if (grammarOperator == null)
                throw new ArgumentNullException("grammarOperator");
            if (parserStates == null)
                throw new ArgumentNullException("parserStates");

            _states = parserStates;
            _table = new Dictionary<ParserState, Dictionary<GrammarSymbol, TransitionAction>>();
            _grammarProvider = grammarProvider;
            _grammarOperator = grammarOperator;
            foreach (var state in _states)
            {
                ConstructActionsForState(state);
            }
        }


        private void ConstructActionsForState(ParserState state)
        {
            var stateActions = new Dictionary<GrammarSymbol, TransitionAction>();
            _table.Add(state, stateActions);
            var symbols = _grammarProvider.GetGrammarSymbols();
            foreach (var symbol in symbols)
            {
                if (state.StateTransitions.ContainsKey(symbol))
                {
                    if (symbol is Terminal)
                    {
                        stateActions.Add(symbol,
                                         new TransitionAction(TransitionAction.ParserAction.Shift, state.StateTransitions[symbol]));
                    }
                    else
                    {
                        stateActions.Add(symbol,
                                         new TransitionAction(TransitionAction.ParserAction.Goto, state.StateTransitions[symbol]));
                    }
                }
            }
            foreach (var item in state.Items)
            {
                if (item.ItemProduction != _grammarProvider.GetAugmentedState().ItemProduction && !item.HasNextToken)
                {
                    var followSet = _grammarOperator.FollowSet(item.ItemProduction.Head);
                    foreach (var terminal in followSet)
                    {
                        stateActions.Add(terminal, new TransitionAction(TransitionAction.ParserAction.Reduce, item.ItemProduction));
                    }
                }
            }
        }

        
    }
}