namespace MParse
{
    public class NonTerminal : GrammarSymbol
    {
        public NonTerminal(int symbolId) : base(symbolId)
        {
        }
        public NonTerminal(int symbolId, string symbolName) 
            : base(symbolId, symbolName)
        {
            
        }
    }
}