using System;
using System.Linq;

namespace MParse
{

    public class AnotherDummy : GrammarProvider
    {

        public enum T
        {
            S,
            E,
            T,
            F,
            Plus,
            Times,
            Id,
            Lparen,
            Rparen
        }
        public override Production[] GetProductions()
        {
            return new[]
                       {
                           new Production((int) T.S, new[] {(int) T.E}),
                           new Production((int) T.E, new[] {(int) T.E, (int) T.Plus, (int) T.T}),
                           new Production((int) T.E, new[] {(int) T.T}), 
                           new Production((int) T.T, new[] {(int) T.T, (int) T.Times, (int) T.F}), 
                           new Production((int) T.T, new[] {(int) T.F}), 
                           new Production((int) T.F, new[] {(int) T.Lparen, (int) T.E, (int) T.Rparen}), 
                           new Production((int) T.F, new[] {(int) T.Id})
                       };
        }

        public override int[] GetGrammarSymbols()
        {
            return
                new[]
                    {
                        (int) T.S,
                        (int) T.E,
                        (int) T.T,
                        (int) T.F,
                        (int) T.Plus,
                        (int) T.Times,
                        (int) T.Id,
                        (int) T.Lparen,
                        (int) T.Rparen,
                    };
        }

        public override Item GetAugmentedState()
        {
            return new Item(new Production((int) T.S, new[] {(int) T.E}));
        }

        public override bool IsTerminal(int symbol)
        {
            return
                new[]
                    {
                        (int) T.Plus,
                        (int) T.Times,
                        (int) T.Id,
                        (int) T.Lparen,
                        (int) T.Rparen
                    }.Any(x => x == symbol);
        }
    }

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