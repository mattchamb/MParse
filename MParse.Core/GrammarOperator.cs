using System;
using System.Collections.Generic;
using System.Linq;
using MParse.Core.GrammarElements;
using MParse.Core.Interfaces;

namespace MParse.Core
{
    public class GrammarOperator : IGrammarOperator
    {
        private readonly Dictionary<GrammarSymbol, List<Terminal>> _firstSetCache;
        private readonly Dictionary<GrammarSymbol, List<GrammarSymbol>> _followSetCache;

        private readonly Grammar _grammar;

        public GrammarOperator(Grammar grammar)
        {
            if (grammar == null)
                throw new ArgumentNullException("grammar");

            _grammar = grammar;
            _firstSetCache = new Dictionary<GrammarSymbol, List<Terminal>>();
            _followSetCache = new Dictionary<GrammarSymbol, List<GrammarSymbol>>();
        }

        /// <summary>
        /// Returns the set of items that can be transitioned to from inputItems
        /// under input <paramref name="symbol"/>.
        /// Refer Dragon Book pg 246
        /// </summary>
        /// <param name="inputItems"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public IList<Item> Goto(IEnumerable<Item> inputItems, GrammarSymbol symbol)
        {
            if (inputItems == null)
                throw new ArgumentNullException("inputItems");
            if (symbol == null)
                throw new ArgumentNullException("symbol");

            var items = inputItems.Where(inputItem => inputItem.HasNextToken && inputItem.NextToken == symbol);
            items = items.Select(inputItem => inputItem.AdvanceDot());
            return GetClosure(items);

        }

        /// <summary>
        /// Create a tuple of the states, and a map of the transitions
        /// out of each state based upon symbol inputs.
        /// Refer Dragon Book pg 246
        /// </summary>
        /// <returns></returns>
        public Tuple<IList<ParserState>, StateTransitionMap> CreateStates()
        {
            var stateMap = new StateTransitionMap();
            int currentStateNumber = 0;
            //The first state is the closure of the starting symbol.
            IList<ParserState> states = new List<ParserState>
            {
                new ParserState(currentStateNumber++, GetClosure(_grammar.AugmentedState))
            };
            var grammarSymbols = _grammar.Symbols;
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
                    if (gotoForSymbol.Count > 0)
                    {
                        ParserState transitionTo;
                        if (!StateAlreadyExists(states, gotoForSymbol, out transitionTo))
                        {
                            //if there is not already a state with this set of items
                            //  then create a new state with that set of items
                            //  and add it to the states list
                            transitionTo = new ParserState(currentStateNumber++, gotoForSymbol);
                            states.Add(transitionTo);
                        }
                        //add the state transition from the current state 
                        //  to the state of the goto set.
                        //  We do this here so that it is not necessary to 
                        //  calculate it again at a later stage.
                        stateMap.AddTransition(state, grammarSymbol, transitionTo);
                    }
                }
            }
            return Tuple.Create(states, stateMap);
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
            var itemsArray = items.ToArray();
            foreach (var parserState in states)
            {
                if (parserState.Items.SetEquals(itemsArray))
                {
                    state = parserState;
                    return true;
                }
            }
            state = null;
            return false;
        }

        public IList<Item> GetClosure(Item item)
        {
            return GetClosure(new[] { item });
        }

        /// <summary>
        /// Returns the Closure of the set of items passed to this function.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public IList<Item> GetClosure(IEnumerable<Item> items)
        {
            var productions = _grammar.Productions;

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
        public IList<Terminal> FirstSet(GrammarSymbol symbol)
        {
            if (symbol == null)
                throw new ArgumentNullException("symbol");

            //have we already calculated the FIRST set of this symbol?
            if (_firstSetCache.ContainsKey(symbol))
                return _firstSetCache[symbol];

            //The FIRST set of a terminal is itself.
            if (symbol is Terminal)
                return new List<Terminal> { symbol as Terminal };

            var result = new HashSet<Terminal>();
            var productions = _grammar.Productions;

            //Get the starting symbol of a production where:
            //  The production has a valid tail (doesn't derive to an empty string)
            //  The head of the production is the symbol we are interested in
            //  The production isn't left recursive (so we don't get infinite recursion)
            var productionStartValues =
                productions.Where(prod => prod.Length > 0 && prod.Head == symbol && prod[0] != prod.Head)
                .Select(prod => prod[0]);

            foreach (var value in productionStartValues)
            {
                if (value is Terminal)
                    result.Add(value as Terminal);
                else
                    //Recursively get the FIRST set of the non-terminals
                    result.UnionWith(FirstSet(value));
            }

            var resultList = result.ToList();
            _firstSetCache.Add(symbol, resultList);
            return resultList;
        }


        /// <summary>
        /// Returns FOLLOW(X) which is defined as the set of terminals that
        /// can appear immediately after some string of symbols derived from X.
        /// </summary>
        /// <remarks>
        /// Cannot calculate the FOLLOW set for a Terminal.
        /// </remarks>
        /// <param name="nonTerminal"></param>
        /// <returns></returns>
        public IList<GrammarSymbol> FollowSet(GrammarSymbol nonTerminal)
        {

            if (nonTerminal == null)
                throw new ArgumentNullException("nonTerminal");

            if (_followSetCache.ContainsKey(nonTerminal))
                return _followSetCache[nonTerminal];

            if (nonTerminal is Terminal)
                throw new ArgumentException("Cannot calculate the FOLLOW set for a Terminal.", "nonTerminal");

            var result = new HashSet<GrammarSymbol>();
            var productions = _grammar.Productions;

            foreach (var production in productions)
            {
                for (int i = 0; i < production.Length; i++)
                {
                    //Find the position of the nonterminal in the production
                    if (production[i] == nonTerminal)
                    {
                        //If the index of the nonterminal is at the end of the
                        //  production's tail, then we add the FOLLOW set
                        //  of the production's head to the result.
                        //Otherwise, we find the FIRST set of the following 
                        //  element in the production, and add it to the FOLLOW set.
                        if (i == production.Length - 1)
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
}