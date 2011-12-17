using System.Collections.Generic;

namespace MParse.GrammarProviders.FileProvider
{
    public class GrammarTreeVisitor : IParseTreeVisitor
    {
        public List<GrammarAttribute> Attributes { get; private set; }
        public List<string> Tokens { get; private set; }
        public List<Production> Productions { get; private set; }

        public GrammarTreeVisitor()
        {
            Attributes = new List<GrammarAttribute>();
            Tokens = new List<string>();
            Productions = new List<Production>();
        }

        public void Visit(_S_grammar_EOF nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
            nonTerminal.T1.AcceptVisitor(this);
        }
        public void Visit(_grammar_segment_list nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_segment_list_segment_list_segment nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
            nonTerminal.T1.AcceptVisitor(this);
        }
        public void Visit(_segment_list_segment nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_segment_attribute_segment nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_segment_production_segment nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_segment_token_segment nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_attribute_segment_ATTRIBUTE_SEGMENT_IDENTIFIER_attribute_list_ATTRIBUTE_SEGMENT_IDENTIFIER nonTerminal)
        {
            var attribVisitor = new AttributeListVisitor();
            nonTerminal.T1.AcceptVisitor(attribVisitor);
            Attributes.AddRange(attribVisitor.Attributes);

        }
        public void Visit(_production_segment_PRODUCTION_SEGMENT_IDENTIFIER_production_list_PRODUCTION_SEGMENT_IDENTIFIER nonTerminal)
        {
            nonTerminal.T1.AcceptVisitor(this);
        }

        public void Visit(_token_segment_TOKEN_SEGMENT_IDENTIFIER_token_list_TOKEN_SEGMENT_IDENTIFIER nonTerminal)
        {
            var tokenListVisitor = new TokenListVisitor();
            nonTerminal.T1.AcceptVisitor(tokenListVisitor);
            Tokens.AddRange(tokenListVisitor.Terminals);

        }
        public void Visit(_production_list_production_list_production_statement nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
            nonTerminal.T1.AcceptVisitor(this);
        }
        public void Visit(_production_list_production_statement nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_production_statement_production_head_production_RHS_list_SEMICOLON nonTerminal)
        {
            var prodHead = ((_production_head_ID) nonTerminal.T0).T0.Text;
            var rhsListVisitor = new ProductionRHSListVisitor();
            nonTerminal.T1.AcceptVisitor(rhsListVisitor);
            foreach(var tail in rhsListVisitor.ProductionTails)
            {
                Productions.Add(new Production(prodHead, tail));
            }

        }
        public void Visit(_token_list_token_list_token_statement nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
            nonTerminal.T1.AcceptVisitor(this);
        }
        public void Visit(_token_list_token_statement nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_token_statement_ID nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_attribute_list_attribute_list_attribute_statement nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
            nonTerminal.T1.AcceptVisitor(this);
        }
        public void Visit(_attribute_list_attribute_statement nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_attribute_statement_attrib_id_EQUALS_attrib_value nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
            nonTerminal.T1.AcceptVisitor(this);
            nonTerminal.T2.AcceptVisitor(this);
        }
        public void Visit(_attrib_id_ID nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_attrib_value_ID nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_production_head_ID nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_production_RHS_list_production_RHS_list_production_RHS nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
            nonTerminal.T1.AcceptVisitor(this);
        }
        public void Visit(_production_RHS_list_production_RHS nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_production_RHS_PROD_EQUALS_production_tail nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
            nonTerminal.T1.AcceptVisitor(this);
        }
        public void Visit(_production_tail_item_list nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_item_list_item_list_item nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
            nonTerminal.T1.AcceptVisitor(this);
        }
        public void Visit(_item_list_item nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }
        public void Visit(_item_ID nonTerminal)
        {
            nonTerminal.T0.AcceptVisitor(this);
        }

        public void Visit(EQUALS terminal)
        {

        }

        public void Visit(PROD_EQUALS terminal)
        {

        }

        public void Visit(ATTRIBUTE_SEGMENT_IDENTIFIER terminal)
        {

        }

        public void Visit(TOKEN_SEGMENT_IDENTIFIER terminal)
        {

        }

        public void Visit(PRODUCTION_SEGMENT_IDENTIFIER terminal)
        {

        }

        public void Visit(SEMICOLON terminal)
        {

        }

        public void Visit(ID terminal)
        {

        }

        public void Visit(EOF terminal)
        {

        }
    }
}