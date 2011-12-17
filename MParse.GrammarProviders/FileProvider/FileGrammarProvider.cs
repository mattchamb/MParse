using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MParse.Core;
using MParse.Core.GrammarElements;
using MParse.Core.Interfaces;

namespace MParse.GrammarProviders.FileProvider
{
    public class FileGrammarProvider : IGrammarProvider
    {
        private int _symbolId;
        private int _productionId;
        private readonly string _fileName;

        private const string AugmentedStart = "$start";

        public FileGrammarProvider(string fileName)
        {
            _fileName = fileName;
            _symbolId = 0;
            _productionId = 0;
        }

        public Grammar GetGrammar()
        {
            string fileContents = File.ReadAllText(_fileName);

            var tokenStream = Lexer.Run(fileContents);

            var parser = new Parser();

            var parseTree = parser.Parse(tokenStream);
            var grammarCreator = new GrammarTreeVisitor();

            parseTree.AcceptVisitor(grammarCreator);

            var isValid = ValidateGrammar(grammarCreator.Tokens, grammarCreator.Productions);

            
            var terminals = (from symbolName in grammarCreator.Tokens
                             select new Terminal(_symbolId++, symbolName))
                             .ToDictionary(terminal => terminal.Name);


            var nonTerminals = grammarCreator.Productions
                                .Select(production => production.ProductionHead)
                                .Distinct()
                                .Select(symbolName => new NonTerminal(_symbolId++, symbolName))
                                .ToDictionary(nonTerminal => nonTerminal.Name);

            var productions = new List<Core.GrammarElements.Production>();

            foreach (var nonTerminal in nonTerminals.Keys)
            {
                foreach (var production in grammarCreator.Productions.Where(x => x.ProductionHead == nonTerminal))
                {
                    var tail = new GrammarSymbol[production.ProductionTail.Count];
                    for (int i = 0; i < tail.Length; i++)
                    {
                        var productionToken = production.ProductionTail[i];
                        if (terminals.ContainsKey(productionToken))
                        {
                            tail[i] = terminals[productionToken];
                        }
                        else if(nonTerminals.ContainsKey(productionToken))
                        {
                            tail[i] = nonTerminals[productionToken];
                        }
                        else
                        {
                            throw new Exception("Unknown production head.");
                        }
                    }
                    productions.Add(new Core.GrammarElements.Production(_productionId++, nonTerminals[nonTerminal], tail));
                }
            }

            

            var endOfStream = new EndOfStream();
            terminals.Add(endOfStream.Name, endOfStream);

            var startAugmentedSymbol = new NonTerminal(_symbolId++, AugmentedStart);
            nonTerminals.Add(AugmentedStart, startAugmentedSymbol);

            var root = GetGrammarRoot(productions);
            var augmentedProduction = GetAugmentedProduction(startAugmentedSymbol, root, endOfStream);

            productions.Add(augmentedProduction);

            return new Grammar(productions, terminals.Values.Cast<GrammarSymbol>().Concat(nonTerminals.Values).ToList(), new Item(augmentedProduction));
        }

        private Core.GrammarElements.Production GetAugmentedProduction(NonTerminal start, GrammarSymbol root, GrammarSymbol endOfStream)
        {
            
            return new Core.GrammarElements.Production(_productionId++,
                                                       start,
                                                       new[]
                                                       {
                                                           root, endOfStream
                                                       });
        }

        private GrammarSymbol GetGrammarRoot(IEnumerable<Core.GrammarElements.Production> productions)
        {
            var productionHeads = productions.Select(x => x.Head);
            var productionBodies = productions.SelectMany(x => x.Tail);
            
            //the root is a production head that never appears in a production body.
            return productionHeads.Except(productionBodies).Single();
        }

        private bool ValidateGrammar(IList<string> terminals, IList<Production> productions)
        {
            var declaredNonTerminals = productions.Select(x => x.ProductionHead).Distinct().ToList();
            var grammarSymbolsInTails = productions.SelectMany(x => x.ProductionTail).Distinct().ToList();

            //check if there are any declared terminals that are used
            //as the head of a production.
            var v = terminals.Intersect(declaredNonTerminals).ToList();
            if(v.Count > 0)
            {
                return false;
            }

            //check if there are any symbols used in a production that are not a terminal
            //and are not the head of some production.
            v = grammarSymbolsInTails.Except(terminals).Except(declaredNonTerminals).ToList();
            if(v.Count > 0)
            {
                return false;
            }

            //check if there are any declared terminals that are not used in any production.
            v = terminals.Except(grammarSymbolsInTails).ToList();
            if(v.Count > 0)
            {
                return false;
            }

            //check that there is only one nonterminal that does not appear in a production
            //this nonterminal is the root of the grammar.
            v = declaredNonTerminals.Except(grammarSymbolsInTails).ToList();
            if(v.Count != 1)
            {
                return false;
            }

            return false;
        }
    }

    public class Parser
    {
        public TreeNode Parse(IEnumerable<_Terminal> tokens)
        {
            var tokenEnumerator = tokens.GetEnumerator();
            if (!tokenEnumerator.MoveNext())
                throw new ArgumentException("Cannot access tokens.", "tokens");

            var stateStack = new Stack<int>();
            var parseStack = new Stack<TreeNode>();
            stateStack.Push(0);

            while (true)
            {
                var t = tokenEnumerator.Current;
                var result = t.Action(stateStack, parseStack);
                if ((result == ActionResult.Accept))
                {
                    break;
                }
                if (result == ActionResult.ShiftContinue)
                {
                    tokenEnumerator.MoveNext();
                    continue;
                }
                if (result == ActionResult.ReduceContinue)
                {
                    var reducer = ((IReducer)(parseStack.Peek()));
                    reducer.Goto(stateStack);
                    continue;
                }
                if (result == ActionResult.Error)
                {
                    throw new Exception("Parsing Failed.");
                }
            }
            return (TreeNode)parseStack.Pop();
        }
    }

    public interface IParseTreeVisitor
    {
        void Visit(_S_grammar_EOF nonTerminal);
        void Visit(_grammar_segment_list nonTerminal);
        void Visit(_segment_list_segment_list_segment nonTerminal);
        void Visit(_segment_list_segment nonTerminal);
        void Visit(_segment_attribute_segment nonTerminal);
        void Visit(_segment_production_segment nonTerminal);
        void Visit(_segment_token_segment nonTerminal);
        void Visit(_attribute_segment_ATTRIBUTE_SEGMENT_IDENTIFIER_attribute_list_ATTRIBUTE_SEGMENT_IDENTIFIER nonTerminal);
        void Visit(_production_segment_PRODUCTION_SEGMENT_IDENTIFIER_production_list_PRODUCTION_SEGMENT_IDENTIFIER nonTerminal);
        void Visit(_token_segment_TOKEN_SEGMENT_IDENTIFIER_token_list_TOKEN_SEGMENT_IDENTIFIER nonTerminal);
        void Visit(_production_list_production_list_production_statement nonTerminal);
        void Visit(_production_list_production_statement nonTerminal);
        void Visit(_production_statement_production_head_production_RHS_list_SEMICOLON nonTerminal);
        void Visit(_token_list_token_list_token_statement nonTerminal);
        void Visit(_token_list_token_statement nonTerminal);
        void Visit(_token_statement_ID nonTerminal);
        void Visit(_attribute_list_attribute_list_attribute_statement nonTerminal);
        void Visit(_attribute_list_attribute_statement nonTerminal);
        void Visit(_attribute_statement_attrib_id_EQUALS_attrib_value nonTerminal);
        void Visit(_attrib_id_ID nonTerminal);
        void Visit(_attrib_value_ID nonTerminal);
        void Visit(_production_head_ID nonTerminal);
        void Visit(_production_RHS_list_production_RHS_list_production_RHS nonTerminal);
        void Visit(_production_RHS_list_production_RHS nonTerminal);
        void Visit(_production_RHS_PROD_EQUALS_production_tail nonTerminal);
        void Visit(_production_tail_item_list nonTerminal);
        void Visit(_item_list_item_list_item nonTerminal);
        void Visit(_item_list_item nonTerminal);
        void Visit(_item_ID nonTerminal);
        void Visit(EQUALS terminal);
        void Visit(PROD_EQUALS terminal);
        void Visit(ATTRIBUTE_SEGMENT_IDENTIFIER terminal);
        void Visit(TOKEN_SEGMENT_IDENTIFIER terminal);
        void Visit(PRODUCTION_SEGMENT_IDENTIFIER terminal);
        void Visit(SEMICOLON terminal);
        void Visit(ID terminal);
        void Visit(EOF terminal);
    }

    public abstract class TreeNode
    {
        private Guid _nodeId;

        public TreeNode()
        {
            _nodeId = Guid.NewGuid();
        }

        public virtual void AcceptVisitor(IParseTreeVisitor visitor)
        {
            //No default implementation
        }
    }

    public abstract class _Terminal : TreeNode
    {
        private string _text;

        protected _Terminal(string text)
        {
            _text = text;
        }

        public string Text
        {
            get
            {
                return _text;
            }
        }

        public abstract ActionResult Action(Stack<int> stateStack, Stack<TreeNode> parseStack);

        public override string ToString()
        {
            return _text;
        }
    }

    public enum ActionResult
    {
        Error,
        Accept,
        ShiftContinue,
        ReduceContinue,
    }

    public interface IReducer
    {
        void Goto(Stack<int> stateStack);
    }

    public abstract class S : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class grammar : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 0)
            {
                stateStack.Push(1);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class segment_list : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 0)
            {
                stateStack.Push(2);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class segment : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 0)
            {
                stateStack.Push(3);
                return;
            }
            if (currentState == 2)
            {
                stateStack.Push(11);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class attribute_segment : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 0)
            {
                stateStack.Push(4);
                return;
            }
            if (currentState == 2)
            {
                stateStack.Push(4);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class production_segment : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 0)
            {
                stateStack.Push(5);
                return;
            }
            if (currentState == 2)
            {
                stateStack.Push(5);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class token_segment : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 0)
            {
                stateStack.Push(6);
                return;
            }
            if (currentState == 2)
            {
                stateStack.Push(6);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class production_list : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 9)
            {
                stateStack.Push(19);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class production_statement : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 9)
            {
                stateStack.Push(20);
                return;
            }
            if (currentState == 19)
            {
                stateStack.Push(28);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class token_list : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 8)
            {
                stateStack.Push(16);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class token_statement : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 8)
            {
                stateStack.Push(17);
                return;
            }
            if (currentState == 16)
            {
                stateStack.Push(26);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class attribute_list : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 7)
            {
                stateStack.Push(12);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class attribute_statement : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 7)
            {
                stateStack.Push(13);
                return;
            }
            if (currentState == 12)
            {
                stateStack.Push(23);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class attrib_id : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 7)
            {
                stateStack.Push(14);
                return;
            }
            if (currentState == 12)
            {
                stateStack.Push(14);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class attrib_value : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 25)
            {
                stateStack.Push(33);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class production_head : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 9)
            {
                stateStack.Push(21);
                return;
            }
            if (currentState == 19)
            {
                stateStack.Push(21);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class production_RHS_list : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 21)
            {
                stateStack.Push(30);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class production_RHS : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 21)
            {
                stateStack.Push(31);
                return;
            }
            if (currentState == 30)
            {
                stateStack.Push(35);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class production_tail : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 32)
            {
                stateStack.Push(37);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class item_list : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 32)
            {
                stateStack.Push(38);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public abstract class item : TreeNode, IReducer
    {
        public void Goto(Stack<int> stateStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 32)
            {
                stateStack.Push(39);
                return;
            }
            if (currentState == 38)
            {
                stateStack.Push(41);
                return;
            }
            throw new Exception("Unexpected GOTO operation.");
        }
    }

    public partial class EQUALS : _Terminal
    {
        public EQUALS(string text)
            : base(text)
        {
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override ActionResult Action(Stack<int> stateStack, Stack<TreeNode> parseStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 14)
            {
                stateStack.Push(25);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 15)
            {
                parseStack.Push(_attrib_id_ID.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public partial class PROD_EQUALS : _Terminal
    {
        public PROD_EQUALS(string text)
            : base(text)
        {
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override ActionResult Action(Stack<int> stateStack, Stack<TreeNode> parseStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 21)
            {
                stateStack.Push(32);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 22)
            {
                parseStack.Push(_production_head_ID.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 30)
            {
                stateStack.Push(32);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 31)
            {
                parseStack.Push(_production_RHS_list_production_RHS.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 35)
            {
                parseStack.Push(_production_RHS_list_production_RHS_list_production_RHS.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 37)
            {
                parseStack.Push(_production_RHS_PROD_EQUALS_production_tail.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 38)
            {
                parseStack.Push(_production_tail_item_list.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 39)
            {
                parseStack.Push(_item_list_item.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 40)
            {
                parseStack.Push(_item_ID.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 41)
            {
                parseStack.Push(_item_list_item_list_item.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public partial class ATTRIBUTE_SEGMENT_IDENTIFIER : _Terminal
    {
        public ATTRIBUTE_SEGMENT_IDENTIFIER(string text)
            : base(text)
        {
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override ActionResult Action(Stack<int> stateStack, Stack<TreeNode> parseStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 0)
            {
                stateStack.Push(7);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 2)
            {
                stateStack.Push(7);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 3)
            {
                parseStack.Push(_segment_list_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 4)
            {
                parseStack.Push(_segment_attribute_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 5)
            {
                parseStack.Push(_segment_production_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 6)
            {
                parseStack.Push(_segment_token_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 11)
            {
                parseStack.Push(_segment_list_segment_list_segment.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 12)
            {
                stateStack.Push(24);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 13)
            {
                parseStack.Push(_attribute_list_attribute_statement.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 23)
            {
                parseStack.Push(_attribute_list_attribute_list_attribute_statement.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 24)
            {
                parseStack.Push(_attribute_segment_ATTRIBUTE_SEGMENT_IDENTIFIER_attribute_list_ATTRIBUTE_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 27)
            {
                parseStack.Push(_token_segment_TOKEN_SEGMENT_IDENTIFIER_token_list_TOKEN_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 29)
            {
                parseStack.Push(_production_segment_PRODUCTION_SEGMENT_IDENTIFIER_production_list_PRODUCTION_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 33)
            {
                parseStack.Push(_attribute_statement_attrib_id_EQUALS_attrib_value.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 34)
            {
                parseStack.Push(_attrib_value_ID.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public partial class TOKEN_SEGMENT_IDENTIFIER : _Terminal
    {
        public TOKEN_SEGMENT_IDENTIFIER(string text)
            : base(text)
        {
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override ActionResult Action(Stack<int> stateStack, Stack<TreeNode> parseStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 0)
            {
                stateStack.Push(8);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 2)
            {
                stateStack.Push(8);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 3)
            {
                parseStack.Push(_segment_list_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 4)
            {
                parseStack.Push(_segment_attribute_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 5)
            {
                parseStack.Push(_segment_production_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 6)
            {
                parseStack.Push(_segment_token_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 11)
            {
                parseStack.Push(_segment_list_segment_list_segment.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 16)
            {
                stateStack.Push(27);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 17)
            {
                parseStack.Push(_token_list_token_statement.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 18)
            {
                parseStack.Push(_token_statement_ID.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 24)
            {
                parseStack.Push(_attribute_segment_ATTRIBUTE_SEGMENT_IDENTIFIER_attribute_list_ATTRIBUTE_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 26)
            {
                parseStack.Push(_token_list_token_list_token_statement.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 27)
            {
                parseStack.Push(_token_segment_TOKEN_SEGMENT_IDENTIFIER_token_list_TOKEN_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 29)
            {
                parseStack.Push(_production_segment_PRODUCTION_SEGMENT_IDENTIFIER_production_list_PRODUCTION_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public partial class PRODUCTION_SEGMENT_IDENTIFIER : _Terminal
    {
        public PRODUCTION_SEGMENT_IDENTIFIER(string text)
            : base(text)
        {
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override ActionResult Action(Stack<int> stateStack, Stack<TreeNode> parseStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 0)
            {
                stateStack.Push(9);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 2)
            {
                stateStack.Push(9);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 3)
            {
                parseStack.Push(_segment_list_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 4)
            {
                parseStack.Push(_segment_attribute_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 5)
            {
                parseStack.Push(_segment_production_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 6)
            {
                parseStack.Push(_segment_token_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 11)
            {
                parseStack.Push(_segment_list_segment_list_segment.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 19)
            {
                stateStack.Push(29);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 20)
            {
                parseStack.Push(_production_list_production_statement.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 24)
            {
                parseStack.Push(_attribute_segment_ATTRIBUTE_SEGMENT_IDENTIFIER_attribute_list_ATTRIBUTE_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 27)
            {
                parseStack.Push(_token_segment_TOKEN_SEGMENT_IDENTIFIER_token_list_TOKEN_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 28)
            {
                parseStack.Push(_production_list_production_list_production_statement.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 29)
            {
                parseStack.Push(_production_segment_PRODUCTION_SEGMENT_IDENTIFIER_production_list_PRODUCTION_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 36)
            {
                parseStack.Push(_production_statement_production_head_production_RHS_list_SEMICOLON.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public partial class SEMICOLON : _Terminal
    {
        public SEMICOLON(string text)
            : base(text)
        {
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override ActionResult Action(Stack<int> stateStack, Stack<TreeNode> parseStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 30)
            {
                stateStack.Push(36);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 31)
            {
                parseStack.Push(_production_RHS_list_production_RHS.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 35)
            {
                parseStack.Push(_production_RHS_list_production_RHS_list_production_RHS.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 37)
            {
                parseStack.Push(_production_RHS_PROD_EQUALS_production_tail.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 38)
            {
                parseStack.Push(_production_tail_item_list.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 39)
            {
                parseStack.Push(_item_list_item.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 40)
            {
                parseStack.Push(_item_ID.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 41)
            {
                parseStack.Push(_item_list_item_list_item.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public partial class ID : _Terminal
    {
        public ID(string text)
            : base(text)
        {
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override ActionResult Action(Stack<int> stateStack, Stack<TreeNode> parseStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 7)
            {
                stateStack.Push(15);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 8)
            {
                stateStack.Push(18);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 9)
            {
                stateStack.Push(22);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 12)
            {
                stateStack.Push(15);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 13)
            {
                parseStack.Push(_attribute_list_attribute_statement.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 16)
            {
                stateStack.Push(18);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 17)
            {
                parseStack.Push(_token_list_token_statement.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 18)
            {
                parseStack.Push(_token_statement_ID.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 19)
            {
                stateStack.Push(22);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 20)
            {
                parseStack.Push(_production_list_production_statement.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 23)
            {
                parseStack.Push(_attribute_list_attribute_list_attribute_statement.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 25)
            {
                stateStack.Push(34);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 26)
            {
                parseStack.Push(_token_list_token_list_token_statement.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 28)
            {
                parseStack.Push(_production_list_production_list_production_statement.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 32)
            {
                stateStack.Push(40);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 33)
            {
                parseStack.Push(_attribute_statement_attrib_id_EQUALS_attrib_value.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 34)
            {
                parseStack.Push(_attrib_value_ID.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 36)
            {
                parseStack.Push(_production_statement_production_head_production_RHS_list_SEMICOLON.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 38)
            {
                stateStack.Push(40);
                parseStack.Push(this);
                return ActionResult.ShiftContinue;
            }
            if (currentState == 39)
            {
                parseStack.Push(_item_list_item.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 40)
            {
                parseStack.Push(_item_ID.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 41)
            {
                parseStack.Push(_item_list_item_list_item.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public partial class EOF : _Terminal
    {
        public EOF(string text)
            : base(text)
        {
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override ActionResult Action(Stack<int> stateStack, Stack<TreeNode> parseStack)
        {
            int currentState = stateStack.Peek();
            if (currentState == 1)
            {
                return ActionResult.Accept;
            }
            if (currentState == 2)
            {
                parseStack.Push(_grammar_segment_list.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 3)
            {
                parseStack.Push(_segment_list_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 4)
            {
                parseStack.Push(_segment_attribute_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 5)
            {
                parseStack.Push(_segment_production_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 6)
            {
                parseStack.Push(_segment_token_segment.ReduceBy(parseStack));
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 11)
            {
                parseStack.Push(_segment_list_segment_list_segment.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 24)
            {
                parseStack.Push(_attribute_segment_ATTRIBUTE_SEGMENT_IDENTIFIER_attribute_list_ATTRIBUTE_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 27)
            {
                parseStack.Push(_token_segment_TOKEN_SEGMENT_IDENTIFIER_token_list_TOKEN_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            if (currentState == 29)
            {
                parseStack.Push(_production_segment_PRODUCTION_SEGMENT_IDENTIFIER_production_list_PRODUCTION_SEGMENT_IDENTIFIER.ReduceBy(parseStack));
                stateStack.Pop();
                stateStack.Pop();
                stateStack.Pop();
                return ActionResult.ReduceContinue;
            }
            return ActionResult.Error;
        }
    }

    public partial class _S_grammar_EOF : S
    {
        public grammar T0 { get; private set; }
        public EOF T1 { get; private set; }

        private _S_grammar_EOF(IList<TreeNode> nodes)
        {
            if (nodes.Count != 2)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (grammar)nodes[0];
            T1 = (EOF)nodes[1];
        }

        public static _S_grammar_EOF ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _S_grammar_EOF(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _grammar_segment_list : grammar
    {
        public segment_list T0 { get; private set; }

        private _grammar_segment_list(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (segment_list)nodes[0];
        }

        public static _grammar_segment_list ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _grammar_segment_list(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _segment_list_segment_list_segment : segment_list
    {
        public segment_list T0 { get; private set; }
        public segment T1 { get; private set; }

        private _segment_list_segment_list_segment(IList<TreeNode> nodes)
        {
            if (nodes.Count != 2)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (segment_list)nodes[0];
            T1 = (segment)nodes[1];
        }

        public static _segment_list_segment_list_segment ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _segment_list_segment_list_segment(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _segment_list_segment : segment_list
    {
        public segment T0 { get; private set; }

        private _segment_list_segment(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (segment)nodes[0];
        }

        public static _segment_list_segment ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _segment_list_segment(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _segment_attribute_segment : segment
    {
        public attribute_segment T0 { get; private set; }

        private _segment_attribute_segment(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (attribute_segment)nodes[0];
        }

        public static _segment_attribute_segment ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _segment_attribute_segment(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _segment_production_segment : segment
    {
        public production_segment T0 { get; private set; }

        private _segment_production_segment(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (production_segment)nodes[0];
        }

        public static _segment_production_segment ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _segment_production_segment(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _segment_token_segment : segment
    {
        public token_segment T0 { get; private set; }

        private _segment_token_segment(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (token_segment)nodes[0];
        }

        public static _segment_token_segment ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _segment_token_segment(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _attribute_segment_ATTRIBUTE_SEGMENT_IDENTIFIER_attribute_list_ATTRIBUTE_SEGMENT_IDENTIFIER : attribute_segment
    {
        public ATTRIBUTE_SEGMENT_IDENTIFIER T0 { get; private set; }
        public attribute_list T1 { get; private set; }
        public ATTRIBUTE_SEGMENT_IDENTIFIER T2 { get; private set; }

        private _attribute_segment_ATTRIBUTE_SEGMENT_IDENTIFIER_attribute_list_ATTRIBUTE_SEGMENT_IDENTIFIER(IList<TreeNode> nodes)
        {
            if (nodes.Count != 3)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (ATTRIBUTE_SEGMENT_IDENTIFIER)nodes[0];
            T1 = (attribute_list)nodes[1];
            T2 = (ATTRIBUTE_SEGMENT_IDENTIFIER)nodes[2];
        }

        public static _attribute_segment_ATTRIBUTE_SEGMENT_IDENTIFIER_attribute_list_ATTRIBUTE_SEGMENT_IDENTIFIER ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _attribute_segment_ATTRIBUTE_SEGMENT_IDENTIFIER_attribute_list_ATTRIBUTE_SEGMENT_IDENTIFIER(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _production_segment_PRODUCTION_SEGMENT_IDENTIFIER_production_list_PRODUCTION_SEGMENT_IDENTIFIER : production_segment
    {
        public PRODUCTION_SEGMENT_IDENTIFIER T0 { get; private set; }
        public production_list T1 { get; private set; }
        public PRODUCTION_SEGMENT_IDENTIFIER T2 { get; private set; }

        private _production_segment_PRODUCTION_SEGMENT_IDENTIFIER_production_list_PRODUCTION_SEGMENT_IDENTIFIER(IList<TreeNode> nodes)
        {
            if (nodes.Count != 3)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (PRODUCTION_SEGMENT_IDENTIFIER)nodes[0];
            T1 = (production_list)nodes[1];
            T2 = (PRODUCTION_SEGMENT_IDENTIFIER)nodes[2];
        }

        public static _production_segment_PRODUCTION_SEGMENT_IDENTIFIER_production_list_PRODUCTION_SEGMENT_IDENTIFIER ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _production_segment_PRODUCTION_SEGMENT_IDENTIFIER_production_list_PRODUCTION_SEGMENT_IDENTIFIER(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _token_segment_TOKEN_SEGMENT_IDENTIFIER_token_list_TOKEN_SEGMENT_IDENTIFIER : token_segment
    {
        public TOKEN_SEGMENT_IDENTIFIER T0 { get; private set; }
        public token_list T1 { get; private set; }
        public TOKEN_SEGMENT_IDENTIFIER T2 { get; private set; }

        private _token_segment_TOKEN_SEGMENT_IDENTIFIER_token_list_TOKEN_SEGMENT_IDENTIFIER(IList<TreeNode> nodes)
        {
            if (nodes.Count != 3)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (TOKEN_SEGMENT_IDENTIFIER)nodes[0];
            T1 = (token_list)nodes[1];
            T2 = (TOKEN_SEGMENT_IDENTIFIER)nodes[2];
        }

        public static _token_segment_TOKEN_SEGMENT_IDENTIFIER_token_list_TOKEN_SEGMENT_IDENTIFIER ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _token_segment_TOKEN_SEGMENT_IDENTIFIER_token_list_TOKEN_SEGMENT_IDENTIFIER(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _production_list_production_list_production_statement : production_list
    {
        public production_list T0 { get; private set; }
        public production_statement T1 { get; private set; }

        private _production_list_production_list_production_statement(IList<TreeNode> nodes)
        {
            if (nodes.Count != 2)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (production_list)nodes[0];
            T1 = (production_statement)nodes[1];
        }

        public static _production_list_production_list_production_statement ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _production_list_production_list_production_statement(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _production_list_production_statement : production_list
    {
        public production_statement T0 { get; private set; }

        private _production_list_production_statement(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (production_statement)nodes[0];
        }

        public static _production_list_production_statement ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _production_list_production_statement(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _production_statement_production_head_production_RHS_list_SEMICOLON : production_statement
    {
        public production_head T0 { get; private set; }
        public production_RHS_list T1 { get; private set; }
        public SEMICOLON T2 { get; private set; }

        private _production_statement_production_head_production_RHS_list_SEMICOLON(IList<TreeNode> nodes)
        {
            if (nodes.Count != 3)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (production_head)nodes[0];
            T1 = (production_RHS_list)nodes[1];
            T2 = (SEMICOLON)nodes[2];
        }

        public static _production_statement_production_head_production_RHS_list_SEMICOLON ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _production_statement_production_head_production_RHS_list_SEMICOLON(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _token_list_token_list_token_statement : token_list
    {
        public token_list T0 { get; private set; }
        public token_statement T1 { get; private set; }

        private _token_list_token_list_token_statement(IList<TreeNode> nodes)
        {
            if (nodes.Count != 2)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (token_list)nodes[0];
            T1 = (token_statement)nodes[1];
        }

        public static _token_list_token_list_token_statement ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _token_list_token_list_token_statement(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _token_list_token_statement : token_list
    {
        public token_statement T0 { get; private set; }

        private _token_list_token_statement(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (token_statement)nodes[0];
        }

        public static _token_list_token_statement ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _token_list_token_statement(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _token_statement_ID : token_statement
    {
        public ID T0 { get; private set; }

        private _token_statement_ID(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (ID)nodes[0];
        }

        public static _token_statement_ID ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _token_statement_ID(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _attribute_list_attribute_list_attribute_statement : attribute_list
    {
        public attribute_list T0 { get; private set; }
        public attribute_statement T1 { get; private set; }

        private _attribute_list_attribute_list_attribute_statement(IList<TreeNode> nodes)
        {
            if (nodes.Count != 2)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (attribute_list)nodes[0];
            T1 = (attribute_statement)nodes[1];
        }

        public static _attribute_list_attribute_list_attribute_statement ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _attribute_list_attribute_list_attribute_statement(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _attribute_list_attribute_statement : attribute_list
    {
        public attribute_statement T0 { get; private set; }

        private _attribute_list_attribute_statement(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (attribute_statement)nodes[0];
        }

        public static _attribute_list_attribute_statement ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _attribute_list_attribute_statement(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _attribute_statement_attrib_id_EQUALS_attrib_value : attribute_statement
    {
        public attrib_id T0 { get; private set; }
        public EQUALS T1 { get; private set; }
        public attrib_value T2 { get; private set; }

        private _attribute_statement_attrib_id_EQUALS_attrib_value(IList<TreeNode> nodes)
        {
            if (nodes.Count != 3)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (attrib_id)nodes[0];
            T1 = (EQUALS)nodes[1];
            T2 = (attrib_value)nodes[2];
        }

        public static _attribute_statement_attrib_id_EQUALS_attrib_value ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _attribute_statement_attrib_id_EQUALS_attrib_value(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _attrib_id_ID : attrib_id
    {
        public ID T0 { get; private set; }

        public string Text { get; private set; }

        private _attrib_id_ID(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (ID)nodes[0];
            Text = T0.Text;
        }

        public static _attrib_id_ID ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _attrib_id_ID(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _attrib_value_ID : attrib_value
    {
        public ID T0 { get; private set; }

        public string Text { get; private set; }

        private _attrib_value_ID(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (ID)nodes[0];
            Text = T0.Text;
        }

        public static _attrib_value_ID ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _attrib_value_ID(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _production_head_ID : production_head
    {
        public ID T0 { get; private set; }

        private _production_head_ID(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (ID)nodes[0];
        }

        public static _production_head_ID ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _production_head_ID(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _production_RHS_list_production_RHS_list_production_RHS : production_RHS_list
    {
        public production_RHS_list T0 { get; private set; }
        public production_RHS T1 { get; private set; }

        private _production_RHS_list_production_RHS_list_production_RHS(IList<TreeNode> nodes)
        {
            if (nodes.Count != 2)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (production_RHS_list)nodes[0];
            T1 = (production_RHS)nodes[1];
        }

        public static _production_RHS_list_production_RHS_list_production_RHS ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _production_RHS_list_production_RHS_list_production_RHS(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _production_RHS_list_production_RHS : production_RHS_list
    {
        public production_RHS T0 { get; private set; }

        private _production_RHS_list_production_RHS(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (production_RHS)nodes[0];
        }

        public static _production_RHS_list_production_RHS ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _production_RHS_list_production_RHS(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _production_RHS_PROD_EQUALS_production_tail : production_RHS
    {
        public PROD_EQUALS T0 { get; private set; }
        public production_tail T1 { get; private set; }

        private _production_RHS_PROD_EQUALS_production_tail(IList<TreeNode> nodes)
        {
            if (nodes.Count != 2)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (PROD_EQUALS)nodes[0];
            T1 = (production_tail)nodes[1];
        }

        public static _production_RHS_PROD_EQUALS_production_tail ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _production_RHS_PROD_EQUALS_production_tail(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _production_tail_item_list : production_tail
    {
        public item_list T0 { get; private set; }

        private _production_tail_item_list(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (item_list)nodes[0];
        }

        public static _production_tail_item_list ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _production_tail_item_list(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _item_list_item_list_item : item_list
    {
        public item_list T0 { get; private set; }
        public item T1 { get; private set; }

        private _item_list_item_list_item(IList<TreeNode> nodes)
        {
            if (nodes.Count != 2)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (item_list)nodes[0];
            T1 = (item)nodes[1];
        }

        public static _item_list_item_list_item ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _item_list_item_list_item(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _item_list_item : item_list
    {
        public item T0 { get; private set; }

        private _item_list_item(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (item)nodes[0];
        }

        public static _item_list_item ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _item_list_item(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public partial class _item_ID : item
    {
        public ID T0 { get; private set; }

        private _item_ID(IList<TreeNode> nodes)
        {
            if (nodes.Count != 1)
            {
                throw new ArgumentException("The list of nodes given is of the wrong length.", "nodes");
            }
            T0 = (ID)nodes[0];
        }

        public static _item_ID ReduceBy(Stack<TreeNode> parseStack)
        {
            //reverse the nodes because they are popped off in the inverted order.
            var nodes = new TreeNode[]
            {
                parseStack.Pop(),
            }.Reverse().ToArray();
            return new _item_ID(nodes);
        }

        public override void AcceptVisitor(IParseTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

}

