using System.Collections.Generic;
using MParse.Core.GrammarElements;
using System;

namespace MParse.Core.Interfaces
{
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
        IList<Item> Goto(IEnumerable<Item> inputItems, GrammarSymbol symbol);

        /// <summary>
        /// Create a tuple of the states, and a map of the transitions
        /// out of each state based upon symbol inputs.
        /// Refer Dragon Book pg 246
        /// </summary>
        /// <returns></returns>
        Tuple<IList<ParserState>, StateTransitionMap> CreateStates();

        /// <summary>
        /// Returns the Closure of the set of items passed to this function.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        IList<Item> GetClosure(IEnumerable<Item> items);

        /// <summary>
        /// Returns the Closure of the given item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        IList<Item> GetClosure(Item item);

        /// <summary>
        /// Returns FIRST(X) which is defined as the set of terminals that
        /// can start a string of symbols derived from X.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        IList<Terminal> FirstSet(GrammarSymbol symbol);

        /// <summary>
        /// Returns FOLLOW(X) which is defined as the set of symbols that
        /// can appear immediately after some string of symbols derived from X.
        /// </summary>
        /// <remarks>
        /// Cannot calculate the FOLLOW set for a Terminal.
        /// </remarks>
        /// <param name="nonTerminal"></param>
        /// <returns></returns>
        IList<GrammarSymbol> FollowSet(GrammarSymbol nonTerminal);
    }
}