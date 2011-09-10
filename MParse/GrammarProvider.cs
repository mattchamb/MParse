using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MParse
{
    public abstract class GrammarProvider
    {
        /// <summary>
        /// This function provides the productions that define the grammar.
        /// </summary>
        /// <returns>
        /// An array of productions as defined by the underlying method
        /// of representing a grammar.
        /// </returns>
        public abstract Production[] GetProductions();

        
        /// <summary>
        /// This function returns all of the grammar symbols in the grammar.
        /// </summary>
        /// <returns></returns>
        public abstract int[] GetGrammarSymbols();

        /// <summary>
        /// Returns the item that represents the starting point of the grammar.
        /// </summary>
        /// <returns></returns>
        public abstract Item GetAugmentedState();

        /// <summary>
        /// Takes a symbol and tells you if the symbol is a Terminal.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the symbol passed is neither a terminal nor a non-terminal.
        /// </exception>
        public abstract bool IsTerminal(int symbol);

        private readonly Dictionary<int, List<int>> _firstSetCache;
        private readonly Dictionary<int, List<int>> _followSetCache;
        

        protected GrammarProvider()
        {
            _firstSetCache = new Dictionary<int, List<int>>();
            _followSetCache = new Dictionary<int, List<int>>();
        }


        /// <summary>
        /// Returns the set of items that can be transitioned to from inputItems
        /// under input "symbol".
        /// Refer Dragon Book pg 246
        /// </summary>
        /// <param name="inputItems"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public virtual List<Item> Goto(IEnumerable<Item> inputItems, int symbol)
        {
            var items = inputItems.Where(inputItem => inputItem.HasNextToken && inputItem.NextToken == symbol);
            items = items.Select(inputItem => inputItem.AdvanceDot());
            return GetClosure(items);
        }

        public virtual TransitionTable CreateTransitionTable(List<ParserState> states)
        {
            var transitionTable = new TransitionTable(states[0]);

            for (int i = 0; i < states.Count; i++)
            {
                DoFunc(transitionTable, states[i]);
            }

            return transitionTable;
        }

        private void DoFunc(TransitionTable tt, ParserState state)
        {
            var stateActions = new Dictionary<int, TransitionAction>();
            tt._table.Add(state, stateActions);
            var symbols = GetGrammarSymbols();
            foreach (var symbol in symbols)
            {
                if (state.StateTransitions.ContainsKey(symbol))
                {
                    if (IsTerminal(symbol))
                    {
                        stateActions.Add(symbol,
                                         new TransitionAction(ParserAction.Shift, state.StateTransitions[symbol]));
                    }
                }
            }
            foreach (var item in state.Items)
            {
                if(!item.HasNextToken)
                {
                    var followSet = FollowSet(item.ItemProduction.Head);
                    foreach (var terminal in followSet)
                    {
                        stateActions.Add(terminal, new TransitionAction(ParserAction.Reduce, item.ItemProduction));
                    }
                }
            }
        }

        /// <summary>
        /// Create the list of states, which internally contain the transitions
        /// out of that state based upon symbol inputs.
        /// Refer Dragon Book pg 246
        /// </summary>
        /// <returns></returns>
        public virtual List<ParserState> CreateStates()
        {
            //The first state is the closure of the starting symbol.
            var states = new List<ParserState>
                             {
                                 new ParserState(GetClosure(GetAugmentedState()))
                             };
            var grammarSymbols = GetGrammarSymbols();
            //iterate through the states list.
            //  this list grows as we iterate
            for (int i = 0; i < states.Count; i++)
            {
                var state = states[i];
                foreach (var grammarSymbol in grammarSymbols)
                {
                    //get the set of items to transition to for this input
                    var gotoForSymbol = Goto(state.Items, grammarSymbol);
                    //if the state exists
                    if(gotoForSymbol.Count > 0)
                    {
                        ParserState transitionTo;
                        if(!StateAlreadyExists(states, gotoForSymbol, out transitionTo))
                        {
                            //if there is not already a state with this set of items
                            //  then create a new state with that set of items
                            //  and add it to the states list
                            transitionTo = new ParserState(gotoForSymbol);
                            states.Add(transitionTo);
                        }
                        //add the state transition from the current state 
                        //  to the state with the goto set.
                        state.AddTransition(grammarSymbol, transitionTo);
                    }
                }
            }
            return states;
        }


        /// <summary>
        /// If there exists a state in states with the same set of Items as items
        /// then return true and assign state to the reference of the existing state.
        /// otherwise return false
        /// </summary>
        /// <param name="states"></param>
        /// <param name="items"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool StateAlreadyExists(IEnumerable<ParserState> states, IEnumerable<Item> items, out ParserState state)
        {
            foreach (var parserState in states)
            {
                bool areEqual = items.All(item => parserState.Items.Contains(item)) && parserState.Items.Count() == items.Count();
                if(areEqual)
                {
                    state = parserState;
                    return true;
                }
            }
            state = null;
            return false;
        }

        private List<Item> GetClosure(Item item)
        {
            return GetClosure(new[] {item});
        }

        /// <summary>
        /// Returns the Closure of the set of items passed to this function.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public virtual List<Item> GetClosure(IEnumerable<Item> items)
        {
            var productions = GetProductions();

            var closure = new List<Item>();
            //the closure contains all of the original items
            closure.AddRange(items);
            //This loop is repeated until no more items are added.
            bool itemAddedToClosure;
            do
            {
                itemAddedToClosure = false;
                for (int j = 0; j < closure.Count; j++)
                {
                    var item = closure[j];
                    //for each production that has the non-terminal after the dot as its head
                    foreach (var prod in productions.Where(h => item.HasNextToken && h.Head == item.NextToken))
                    {
                        var newItem = new Item(prod);
                        if (!closure.Contains(newItem))
                        {
                            //add the item to the closure
                            closure.Add(newItem);
                            //signal to continue the loop
                            itemAddedToClosure = true;
                        }
                    }
                }
            } while (itemAddedToClosure);

            return closure;
        }

        /// <summary>
        /// Returns FIRST(X) which is defined as the set of terminals that
        /// can start a string of symbols derived from X.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public virtual List<int> FirstSet(int symbol)
        {
            //have we already calculated the FIRST set of this symbol?
            if (_firstSetCache.ContainsKey(symbol))
                return _firstSetCache[symbol];

            //The FIRST set of a terminal is itself.
            if (IsTerminal(symbol))
                return new List<int> {symbol};

            var result = new HashSet<int>();
            var productions = GetProductions();

            //Get the starting symbol of a production where:
            //  The production has a valid tail (doesn't derive to an empty string)
            //  The head of the production is the symbol we are interested in
            //  The production isn't left recursive (so we don't get infinite recursion)
            var productionStartValues =
                productions.Where(x => x.Length > 0 && x.Head == symbol && x[0] != x.Head)
                .Select(x => x[0]);

            foreach (var value in productionStartValues)
            {
                if (IsTerminal(value))
                    result.Add(value);
                else
                    //Recursively get the FIRST set of the non-terminals
                    result.UnionWith(FirstSet(value));
            }

            var resultList = result.ToList();
            _firstSetCache.Add(symbol, resultList);
            return resultList;
        }


        /// <summary>
        /// Returns FOLLOW(X) which is defined as the set of symbols that
        /// can appear immediately after some string of symbols derived from X.
        /// </summary>
        /// <param name="nonTerminal"></param>
        /// <returns></returns>
        public virtual List<int> FollowSet(int nonTerminal)
        {
            if (_followSetCache.ContainsKey(nonTerminal))
                return _followSetCache[nonTerminal];

            if(IsTerminal(nonTerminal))
                throw new ArgumentException("Cannot calculate the FOLLOW set for a Terminal.", "nonTerminal");

            var result = new HashSet<int>();
            var productions = GetProductions();
            
            foreach (var production in productions)
            {
                for (int i = 0; i < production.Length; i++)
                {
                    //Find the position of the nonterminal in the production
                    if(production[i] == nonTerminal)
                    {
                        //If the index of the nonterminal is at the end of the
                        //  production's tail, then we add the FOLLOW set
                        //  of the production's head to the result.
                        //Otherwise, we find the FIRST set of the following 
                        //  element in the production, and add it to the FOLLOW set.
                        if(i == production.Length - 1)
                        {
                            result.UnionWith(FollowSet(production.Head));
                        }
                        else
                        {
                            result.UnionWith(FirstSet(production[i + 1]));
                        }
                    }
                }
            }
            var resultList = result.ToList();
            _followSetCache.Add(nonTerminal, resultList);
            return resultList;
        }
    }

    public class TransitionTable
    {
        public readonly ParserState _startingState;

        public readonly Dictionary<ParserState, Dictionary<int, TransitionAction>> _table;

        public TransitionTable(ParserState startingState)
        {
            if(startingState == null)
                throw new ArgumentNullException("startingState");
            _startingState = startingState;
            _table = new Dictionary<ParserState, Dictionary<int, TransitionAction>>();
        }

    }

    public class TransitionAction
    {
        public ParserAction Action { get; private set; }
        public ParserState NextState { get; private set; }
        public Production ReduceByProduction { get; private set; }

        public TransitionAction(ParserAction action)
        {
            Action = action;
        }

        public TransitionAction(ParserAction action, ParserState nextState) : this(action)
        {
            if(action != ParserAction.Shift)
                throw new Exception("Can only define the next state for the shift action.");
            NextState = nextState;
        }

        public TransitionAction(ParserAction action, Production reduceByProduction) :this(action)
        {
            if (action != ParserAction.Reduce)
                throw new Exception("Can only define the production to reduce by for the reduce action.");
            ReduceByProduction = reduceByProduction;
        }
        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine(Action.ToString());
            switch (Action)
            {
                case ParserAction.Shift:
                    result.Append(NextState);
                    break;
                case ParserAction.Reduce:
                    result.Append(ReduceByProduction);
                    break;
            }
            return result.ToString();
        }
    }

    public enum ParserAction
    {
        Shift,
        Reduce,
        Accept
    }
   
}