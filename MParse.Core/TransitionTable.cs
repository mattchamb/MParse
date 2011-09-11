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
            ConstructTable();
        }

        private void ConstructTable()
        {
            foreach (var state in _states)
            {
                var dict = new Dictionary<GrammarSymbol, TransitionAction>();
                _table.Add(state, dict);
                foreach (var item in state.Items)
                {
                    if(item.HasNextToken && item.NextToken is Terminal)
                    {
                        dict.Add(item.NextToken, new TransitionAction(ParserAction.Shift, state.StateTransitions[item.NextToken]));
                    }
                    if (!item.HasNextToken)
                    {
                        var followSet = _grammarOperator.FollowSet(item.ItemProduction.Head);
                        foreach (var symbol in followSet)
                        {
                            dict.Add(symbol, new TransitionAction(ParserAction.Reduce, item.ItemProduction));
                        }
                    }
                }
                if (state.Items.Contains(_grammarProvider.GetAugmentedState().AdvanceDot()))
                {
                    dict[new EndOfStream()] = new TransitionAction(ParserAction.Accept);
                }
                var symbols = _grammarProvider.GetGrammarSymbols();
                foreach (var symbol in symbols)
                {
                    if (symbol is NonTerminal && state.StateTransitions.ContainsKey(symbol))
                        dict.Add(symbol, new TransitionAction(ParserAction.Goto, state.StateTransitions[symbol]));
                    else if(!dict.ContainsKey(symbol))
                        dict.Add(symbol, new TransitionAction(ParserAction.Error));
                }
            }
        }

        //This has pretty crappy output, but it helps to debug.
        public void Print()
        {
            foreach (var val in _table)
            {
                Console.Write(val.Key.UniqueName.Substring(0, 4).PadRight(5));
                foreach (var value in val.Value.OrderBy(x => x.ToString()))
                {
                    var output = new StringBuilder();
                    output.Append(value.Key.ToString());
                    output.Append(": ");
                    switch (value.Value.Action)
                    {
                        case ParserAction.Accept:
                            output.AppendFormat("{0}", "ac");
                            break;
                        case ParserAction.Error:
                            output.AppendFormat("{0}", "er");
                            break;
                        case ParserAction.Goto:
                            output.AppendFormat("go {0}", value.Value.NextState.UniqueName.Substring(0, 4));
                            break;
                        case ParserAction.Reduce:
                            output.AppendFormat("re {0}", value.Value.ReduceByProduction);
                            break;
                        case ParserAction.Shift:
                            output.AppendFormat("sh {0}", value.Value.NextState.UniqueName.Substring(0, 4));
                            break;
                    }
                    
                    Console.Write(output.ToString().PadRight(50));
                }
                Console.WriteLine();
            }
        }
    }
}