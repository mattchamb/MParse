using System.Collections.Generic;
using System.Linq;
using MParse.Core.GrammarElements;
using MParse.Core.Interfaces;
using MParse.Core;

namespace MParse.GrammarProviders
{
    public class GGrammarProvider : IGrammarProvider
    {
        public enum T
        {
            S,
            grammar,
            segment_list,
            segment,
            attribute_segment,
            production_segment,
            token_segment,
            production_list,
            production_statement,
            token_list,
            token_statement,
            attribute_list,
            attribute_statement,
            attrib_id,
            attrib_value,
            production_head,
            production_RHS_list,
            production_RHS,
            production_tail,
            item_list,
            item,
            EQUALS,
            SEMICOLON,
            PROD_EQUALS,
            ATTRIBUTE_SEGMENT_IDENTIFIER,
            TOKEN_SEGMENT_IDENTIFIER,
            PRODUCTION_SEGMENT_IDENTIFIER,
            ID,
            EOF
        }

        private readonly Dictionary<T, GrammarSymbol> _symbols;
        private readonly List<Production> _productions;

        public GGrammarProvider()
        {
            _symbols = new Dictionary<T, GrammarSymbol>
            {
                {T.S, new NonTerminal ((int) T.S, T.S.ToString())},
                {T.grammar, new NonTerminal ((int) T.grammar, T.grammar.ToString())},
                {T.segment_list, new NonTerminal((int) T.segment_list, T.segment_list.ToString())},
                {T.segment, new NonTerminal((int) T.segment, T.segment.ToString())},
                {T.attribute_segment, new NonTerminal((int) T.attribute_segment, T.attribute_segment.ToString())},
                {T.production_segment, new NonTerminal((int) T.production_segment, T.production_segment.ToString())},
                {T.token_segment, new NonTerminal((int) T.token_segment, T.token_segment.ToString())},
                {T.production_list, new NonTerminal((int) T.production_list, T.production_list.ToString())},
                {T.production_statement, new NonTerminal((int) T.production_statement, T.production_statement.ToString())},
                {T.token_list, new NonTerminal((int) T.token_list, T.token_list.ToString())},
                {T.token_statement, new NonTerminal((int) T.token_statement, T.token_statement.ToString())},
                {T.attribute_list, new NonTerminal((int) T.attribute_list, T.attribute_list.ToString())},
                {T.attribute_statement, new NonTerminal((int) T.attribute_statement, T.attribute_statement.ToString())},
                {T.attrib_id, new NonTerminal((int) T.attrib_id, T.attrib_id.ToString())},
                {T.attrib_value, new NonTerminal((int) T.attrib_value, T.attrib_value.ToString())},
                {T.production_head, new NonTerminal((int) T.production_head, T.production_head.ToString())},
                {T.production_RHS_list, new NonTerminal((int) T.production_RHS_list, T.production_RHS_list.ToString())},
                {T.production_RHS, new NonTerminal((int) T.production_RHS, T.production_RHS.ToString())},
                {T.production_tail, new NonTerminal((int) T.production_tail, T.production_tail.ToString())},
                {T.item_list, new NonTerminal((int) T.item_list, T.item_list.ToString())},
                {T.item, new NonTerminal((int) T.item, T.item.ToString())},


                {T.EQUALS, new Terminal ((int) T.EQUALS, T.EQUALS.ToString())},
                {T.PROD_EQUALS, new Terminal ((int) T.PROD_EQUALS, T.PROD_EQUALS.ToString())},
                {T.ATTRIBUTE_SEGMENT_IDENTIFIER, new Terminal ((int) T.ATTRIBUTE_SEGMENT_IDENTIFIER, T.ATTRIBUTE_SEGMENT_IDENTIFIER.ToString())},
                {T.TOKEN_SEGMENT_IDENTIFIER, new Terminal ((int) T.TOKEN_SEGMENT_IDENTIFIER, T.TOKEN_SEGMENT_IDENTIFIER.ToString())},
                {T.PRODUCTION_SEGMENT_IDENTIFIER, new Terminal ((int) T.PRODUCTION_SEGMENT_IDENTIFIER, T.PRODUCTION_SEGMENT_IDENTIFIER.ToString())},
                {T.SEMICOLON, new Terminal ((int) T.SEMICOLON, T.SEMICOLON.ToString())},
                {T.ID, new Terminal ((int) T.ID, T.ID.ToString())},
                {T.EOF, new EndOfStream ()}
            };
            _productions = new List<Production>
            {
                new Production(28, _symbols[T.S], new[] {_symbols[T.grammar], _symbols[T.EOF]}),
                new Production(0, _symbols[T.grammar], new[] { _symbols[T.segment_list] }),
                new Production(1, _symbols[T.segment_list], new[] { _symbols[T.segment_list], _symbols[T.segment] }),
                new Production(2, _symbols[T.segment_list], new[] {	_symbols[T.segment] }),
                new Production(3, _symbols[T.segment], new[] { _symbols[T.attribute_segment] }),
                new Production(4, _symbols[T.segment], new[] {  _symbols[T.production_segment] }),
                new Production(5, _symbols[T.segment], new[] {  _symbols[T.token_segment] }),
                new Production(6, _symbols[T.production_segment], new[] { _symbols[T.PRODUCTION_SEGMENT_IDENTIFIER], _symbols[T.production_list], _symbols[T.PRODUCTION_SEGMENT_IDENTIFIER] }),
                new Production(7, _symbols[T.production_list], new[] { _symbols[T.production_list], _symbols[T.production_statement] }),
                new Production(8, _symbols[T.production_list], new[] { _symbols[T.production_statement] }),
                new Production(9, _symbols[T.token_segment], new[] { _symbols[T.TOKEN_SEGMENT_IDENTIFIER], _symbols[T.token_list ],_symbols[T.TOKEN_SEGMENT_IDENTIFIER] }),
                new Production(10, _symbols[T.token_list], new[] { _symbols[T.token_list], _symbols[T.token_statement] }),
                new Production(11, _symbols[T.token_list], new[] { _symbols[T.token_statement] }),
                new Production(12, _symbols[T.token_statement], new[] { _symbols[T.ID] }),
                new Production(13, _symbols[T.attribute_segment], new[] { _symbols[T.ATTRIBUTE_SEGMENT_IDENTIFIER], _symbols[T.attribute_list ],_symbols[T.ATTRIBUTE_SEGMENT_IDENTIFIER] }),
                new Production(14, _symbols[T.attribute_list], new[] { _symbols[T.attribute_list], _symbols[T.attribute_statement] }),
                new Production(15, _symbols[T.attribute_list], new[] { _symbols[T.attribute_statement] }),
                new Production(16, _symbols[T.attribute_statement], new[] { _symbols[T.attrib_id], _symbols[T.EQUALS], _symbols[T.attrib_value] }),
                new Production(17, _symbols[T.production_statement], new[] { _symbols[T.production_head], _symbols[T.production_RHS_list], _symbols[T.SEMICOLON] }),
                new Production(18, _symbols[T.production_RHS_list], new[] { _symbols[T.production_RHS_list], _symbols[T.production_RHS] }),
                new Production(19, _symbols[T.production_RHS_list], new[] { _symbols[T.production_RHS] }),
                new Production(20, _symbols[T.production_RHS], new[] { _symbols[T.PROD_EQUALS], _symbols[T.production_tail] }),
                new Production(21, _symbols[T.production_tail], new[] { _symbols[T.item_list] }),
                new Production(22, _symbols[T.item_list], new[] { _symbols[T.item_list], _symbols[T.item] }),
                new Production(23, _symbols[T.item_list], new[] {	_symbols[T.item] }),
                new Production(24, _symbols[T.item], new[] { _symbols[T.ID] }),
                new Production(25, _symbols[T.production_head], new[] { _symbols[T.ID] }),
                new Production(26, _symbols[T.attrib_id], new[] { _symbols[T.ID] }),
                new Production(27, _symbols[T.attrib_value], new[] { _symbols[T.ID] }),
            };
        }

        private Production[] GetProductions()
        {
            return _productions.ToArray();
        }

        private GrammarSymbol[] GetGrammarSymbols()
        {
            return _symbols.Select(x => x.Value).ToArray();
        }

        private Item GetAugmentedState()
        {
            return new Item(_productions[0]);
        }

        public Grammar GetGrammar()
        {
            return new Grammar(GetProductions(), GetGrammarSymbols(), GetAugmentedState());
        }
    }
}