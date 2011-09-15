namespace MParse.Core.GrammarElements
{
    public class Terminal : GrammarSymbol
    {
        public Terminal(int symbolId)
            : base(symbolId)
        {
        }

        public Terminal(int symbolId, string symbolName)
            : base(symbolId, symbolName)
        {
        }
    }
}