namespace MParse.Core.GrammarElements
{
    public class EndOfStream : Terminal
    {
        public EndOfStream() : base(-1, "EOF")
        {
        }

        public override bool Equals(object other)
        {
            return other is EndOfStream;
        }

        public override bool Equals(GrammarSymbol other)
        {
            return other is EndOfStream;
        }

        public bool Equals(EndOfStream other)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
