using MParse.GrammarElements;

namespace MParse.Interfaces
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
}