﻿using System;

namespace MParse
{
    public abstract class GrammarSymbol : IEquatable<GrammarSymbol>
    {
        public int SymbolId { get; private set; }

        public string Name { get; private set; }

        protected GrammarSymbol(int symbolId)
        {
            SymbolId = symbolId;
        }

        protected GrammarSymbol(int symbolId, string symbolName) : this(symbolId)
        {
            Name = symbolName ?? symbolId.ToString();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GrammarSymbol)) return false;
            return Equals((GrammarSymbol)obj);
        }

        public bool Equals(GrammarSymbol other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.SymbolId == SymbolId;
        }

        public override int GetHashCode()
        {
            return SymbolId.GetHashCode();
        }

        public static bool operator ==(GrammarSymbol left, GrammarSymbol right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GrammarSymbol left, GrammarSymbol right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Terminal : GrammarSymbol
    {
        public Terminal(int symbolId) : base(symbolId)
        {
        }

        public Terminal(int symbolId, string symbolName) 
            : base(symbolId, symbolName)
        {
        }
    }

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
