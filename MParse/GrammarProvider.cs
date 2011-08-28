﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MParse
{
    public abstract class GrammarProvider
    {
        /// <summary>
        /// Implemented by derived classes.
        /// This function provides the productions that define the grammar.
        /// </summary>
        /// <returns>
        /// An array of productions as defined by the underlying method
        /// of representing a grammar.
        /// </returns>
        public abstract Production[] GetProductions();

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
        /// Returns the Closure of the set of items passed to this function.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public virtual List<Item> GetClosure(List<Item> items)
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
                    foreach (var prod in productions.Where(h => h.Head == item.NextToken))
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
}