using System;
using System.Collections.Generic;
using System.Linq;

namespace MParse
{
    public interface IGrammarProvider
    {
        /// <summary>
        /// This function provides the productions that define the grammar.
        /// </summary>
        /// <returns>
        /// An array of productions as defined by the underlying method
        /// of representing a grammar.
        /// </returns>
        Production[] GetProductions();

        /// <summary>
        /// This function returns all of the grammar symbols in the grammar.
        /// </summary>
        /// <returns></returns>
        GrammarSymbol[] GetGrammarSymbols();

        /// <summary>
        /// Returns the item that represents the starting point of the grammar.
        /// </summary>
        /// <returns></returns>
        Item GetAugmentedState();
    }

    public interface IGrammarOperator
    {
        /// <summary>
        /// Returns the set of items that can be transitioned to from inputItems
        /// under input "symbol".
        /// Refer Dragon Book pg 246
        /// </summary>
        /// <param name="inputItems"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        List<Item> Goto(IEnumerable<Item> inputItems, GrammarSymbol symbol);

        /// <summary>
        /// Create the list of states, which internally contain the transitions
        /// out of that state based upon symbol inputs.
        /// Refer Dragon Book pg 246
        /// </summary>
        /// <returns></returns>
        List<ParserState> CreateStates();

        /// <summary>
        /// Returns the Closure of the set of items passed to this function.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        List<Item> GetClosure(IEnumerable<Item> items);

        /// <summary>
        /// Returns FIRST(X) which is defined as the set of terminals that
        /// can start a string of symbols derived from X.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        List<GrammarSymbol> FirstSet(GrammarSymbol symbol);

        /// <summary>
        /// Returns FOLLOW(X) which is defined as the set of symbols that
        /// can appear immediately after some string of symbols derived from X.
        /// </summary>
        /// <remarks>
        /// Cannot calculate the FOLLOW set for a Terminal.
        /// </remarks>
        /// <param name="nonTerminal"></param>
        /// <returns></returns>
        List<GrammarSymbol> FollowSet(GrammarSymbol nonTerminal);
    }

    public class GrammarOperator : IGrammarOperator
    {

        /// <summary>
        /// Takes a symbol and tells you if the symbol is a Terminal.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the symbol passed is neither a terminal nor a non-terminal.
        /// </exception>
        //public abstract bool IsTerminal(int symbol);

        private readonly Dictionary<GrammarSymbol, List<GrammarSymbol>> _firstSetCache;
        private readonly Dictionary<GrammarSymbol, List<GrammarSymbol>> _followSetCache;

        private readonly IGrammarProvider _grammarProvider;

        public GrammarOperator(IGrammarProvider grammarProvider)
        {
            _grammarProvider = grammarProvider;
            _firstSetCache = new Dictionary<GrammarSymbol, List<GrammarSymbol>>();
            _followSetCache = new Dictionary<GrammarSymbol, List<GrammarSymbol>>();
        }

        public GrammarOperator() : this(new TestGrammarProvider())
        {
            
        }
        

        /// <summary>
        /// Returns the set of items that can be transitioned to from inputItems
        /// under input "symbol".
        /// Refer Dragon Book pg 246
        /// </summary>
        /// <param name="inputItems"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public virtual List<Item> Goto(IEnumerable<Item> inputItems, GrammarSymbol symbol)
        {
            var items = inputItems.Where(inputItem => inputItem.HasNextToken && inputItem.NextToken == symbol);
            items = items.Select(inputItem => inputItem.AdvanceDot());
            return GetClosure(items);
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
                                 new ParserState(GetClosure(_grammarProvider.GetAugmentedState()))
                             };
            var grammarSymbols = _grammarProvider.GetGrammarSymbols();
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
                if (areEqual)
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
            return GetClosure(new[] { item });
        }

        /// <summary>
        /// Returns the Closure of the set of items passed to this function.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public virtual List<Item> GetClosure(IEnumerable<Item> items)
        {
            var productions = _grammarProvider.GetProductions();

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
        public virtual List<GrammarSymbol> FirstSet(GrammarSymbol symbol)
        {
            //have we already calculated the FIRST set of this symbol?
            if (_firstSetCache.ContainsKey(symbol))
                return _firstSetCache[symbol];

            //The FIRST set of a terminal is itself.
            if (symbol is Terminal)
                return new List<GrammarSymbol> { symbol };

            var result = new HashSet<GrammarSymbol>();
            var productions = _grammarProvider.GetProductions();

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
        /// <remarks>
        /// Cannot calculate the FOLLOW set for a Terminal.
        /// </remarks>
        /// <param name="nonTerminal"></param>
        /// <returns></returns>
        public virtual List<GrammarSymbol> FollowSet(GrammarSymbol nonTerminal)
        {
            if (_followSetCache.ContainsKey(nonTerminal))
                return _followSetCache[nonTerminal];

            if (nonTerminal is Terminal)
                throw new ArgumentException("Cannot calculate the FOLLOW set for a Terminal.", "nonTerminal");

            var result = new HashSet<GrammarSymbol>();
            var productions = _grammarProvider.GetProductions();

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