using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MParse.GrammarProviders.FileProvider
{
    public class ItemListVisitor : IParseTreeVisitor
    {
        public List<string> Items { get; private set; }

        public ItemListVisitor()
        {
            Items = new List<string>();
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
            Items.Add(nonTerminal.T0.Text);
        }

        #region Not Required

        public void Visit(_S_grammar_EOF nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_grammar_segment_list nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_segment_list_segment_list_segment nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_segment_list_segment nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_segment_attribute_segment nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_segment_production_segment nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_segment_token_segment nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_attribute_segment_ATTRIBUTE_SEGMENT_IDENTIFIER_attribute_list_ATTRIBUTE_SEGMENT_IDENTIFIER nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_production_segment_PRODUCTION_SEGMENT_IDENTIFIER_production_list_PRODUCTION_SEGMENT_IDENTIFIER nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_token_segment_TOKEN_SEGMENT_IDENTIFIER_token_list_TOKEN_SEGMENT_IDENTIFIER nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_production_list_production_list_production_statement nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_production_list_production_statement nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_production_statement_production_head_production_RHS_list_SEMICOLON nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_token_list_token_list_token_statement nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_token_list_token_statement nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_token_statement_ID nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_attribute_list_attribute_list_attribute_statement nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_attribute_list_attribute_statement nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_attribute_statement_attrib_id_EQUALS_attrib_value nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_attrib_id_ID nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_attrib_value_ID nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_production_head_ID nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_production_RHS_list_production_RHS_list_production_RHS nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_production_RHS_list_production_RHS nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_production_RHS_PROD_EQUALS_production_tail nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(_production_tail_item_list nonTerminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(EQUALS terminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(PROD_EQUALS terminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(ATTRIBUTE_SEGMENT_IDENTIFIER terminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(TOKEN_SEGMENT_IDENTIFIER terminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(PRODUCTION_SEGMENT_IDENTIFIER terminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(SEMICOLON terminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(ID terminal)
        {
            throw new NotImplementedException();
        }

        public void Visit(EOF terminal)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
