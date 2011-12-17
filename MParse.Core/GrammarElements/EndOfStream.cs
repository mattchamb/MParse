namespace MParse.Core.GrammarElements
{
    /// <summary>
    /// Used in the agumented production of a grammar to indicate the end of the stream.
    /// </summary>
    public class EndOfStream : Terminal
    {
        private static readonly EndOfStream Singleton = new EndOfStream();

        /// <summary>
        /// A singleton instance of the end of stream symbol.
        /// </summary>
        public static EndOfStream Instance
        {
            get { return Singleton; }
        }

        private EndOfStream() : base(-1, "$accept")
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
