using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MParse.Core.GrammarElements
{
    /// <summary>
    /// Used in the agumented production of a grammar to indicate the final reduction of the parser.
    /// </summary>
    public class Start : NonTerminal
    {
        private static readonly Start Singleton = new Start();

        /// <summary>
        /// A singleton instance of the start symbol.
        /// </summary>
        public static Start Instance
        {
            get { return Singleton; }
        }

        private Start() : base(-1, "$start")
        {
        }

        public override bool Equals(object other)
        {
            return other is Start;
        }

        public override bool Equals(GrammarSymbol other)
        {
            return other is Start;
        }

        public bool Equals(Start other)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
