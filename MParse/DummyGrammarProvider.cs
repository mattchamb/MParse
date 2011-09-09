using System;
using System.Linq;

namespace MParse
{
    public class DummyGrammarProvider : GrammarProvider
    {
        internal enum Tokens
        {
            Zero,
            One,
            Times,
            Plus,
            B,
            E,
            S
        }

        public override Production[] GetProductions()
        {
            return new[]
                       {
                           new Production((int) Tokens.S, new[] {(int) Tokens.E}),
                           new Production((int) Tokens.E, new[] {(int) Tokens.E, (int) Tokens.Times, (int) Tokens.B}),
                           new Production((int) Tokens.E, new[] {(int) Tokens.E, (int) Tokens.Plus, (int) Tokens.B}),
                           new Production((int) Tokens.E, new[] {(int) Tokens.B}),
                           new Production((int) Tokens.B, new[] {(int) Tokens.Zero}),
                           new Production((int) Tokens.B, new[] {(int) Tokens.One}),
                       };
        }

        public override int[] GetGrammarSymbols()
        {
            return
                new[]
                    {
                        (int) Tokens.S,
                        (int) Tokens.E,
                        (int) Tokens.B,
                        (int) Tokens.Plus,
                        (int) Tokens.Times,
                        (int) Tokens.Zero,
                        (int) Tokens.One
                    };
        }

        public override Item GetAugmentedState()
        {
            return new Item(new Production((int) Tokens.S, new[] {(int) Tokens.E}));
        }

        public override bool IsTerminal(int token)
        {
            return
                new []
                    {
                        (int) Tokens.Plus,
                        (int) Tokens.Times,
                        (int) Tokens.Zero,
                        (int) Tokens.One
                    }.Any(x => x == token);
        }
    }
}