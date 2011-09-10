using System.Collections.Generic;
using MParse.GrammarElements;

namespace MParse.Interfaces
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
}