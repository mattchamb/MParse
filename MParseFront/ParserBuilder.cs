using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MParse.Core;
using MParse.Core.GrammarElements;

namespace MParseFront
{
    class ParserBuilder
    {
        private readonly CodeNamespace _codeNamespace;
        private readonly CodeCompileUnit _compileUnit;
        private readonly CodeTypeDeclaration _terminalBase;
        private readonly IEnumerable<Production> _productions;
        private readonly IEnumerable<GrammarSymbol> _symbols;
        private readonly TransitionTable _transitionTable;

        public ParserBuilder(string codeNamespace, IEnumerable<Production> productions, IEnumerable<GrammarSymbol> symbols, TransitionTable transTable)
        {
            _compileUnit = new CodeCompileUnit();
            _codeNamespace = new CodeNamespace(codeNamespace);
            _codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            _codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            _compileUnit.Namespaces.Add(_codeNamespace);
            _productions = productions;
            _symbols = symbols;
            _transitionTable = transTable;

            _terminalBase = new CodeTypeDeclaration("_Terminal")
            {
                TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public,
            };
            _terminalBase.Members.Add(GetAbstractActionMethod());
            _codeNamespace.Types.Add(_terminalBase);
            _codeNamespace.Types.Add(GetActionResultEnum());
            _codeNamespace.Types.Add(GetIReducerInterface());

            AddTerminalTextProperty(_terminalBase);
            AddTerminalToString(_terminalBase);

            foreach (var symbol in symbols)
            {
                if (symbol is Terminal)
                {
                    AddSymbol(symbol as Terminal);
                }
                else
                {
                    AddSymbol(symbol as NonTerminal, productions.Where(x => x.Head.Name == symbol.Name));
                }
            }
            var parserMethod = AddParserClass("Parser", productions.First().Head.Name);

            AddParserFunctionBody(parserMethod);
        }

        private CodeTypeDeclaration GetIReducerInterface()
        {
            var result = new CodeTypeDeclaration("IReducer")
            {
                IsInterface = true
            };
            var gotoMethod = new CodeMemberMethod()
            {
                Name = "Goto",
                ReturnType = new CodeTypeReference(typeof(void)),
            };
            gotoMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Stack<int>)), "states"));
            result.Members.Add(gotoMethod);
            return result;
        }

        private void AddParserFunctionBody(CodeMemberMethod parserMethod)
        {
            parserMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(Stack<int>)), "stateStack", new CodeObjectCreateExpression(new CodeTypeReference(typeof(Stack<int>)))));
            var stateStack = new CodeVariableReferenceExpression("stateStack");

            parserMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(Stack<object>)), "parseStack", new CodeObjectCreateExpression(new CodeTypeReference(typeof(Stack<object>)))));
            var parseStack = new CodeVariableReferenceExpression("parseStack");

            parserMethod.Statements.Add(new CodeMethodInvokeExpression(stateStack, "Push", new CodePrimitiveExpression(0)));

            parserMethod.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(int)), "tokenPos", new CodePrimitiveExpression(0)));

            var loop = new CodeIterationStatement(new CodeSnippetStatement(), new CodePrimitiveExpression(true), new CodeSnippetStatement());
            parserMethod.Statements.Add(loop);

            var tokenPos = new CodeVariableReferenceExpression("tokenPos");
            loop.Statements.Add(new CodeLabeledStatement("startLoop", new CodeVariableDeclarationStatement(new CodeTypeReference("_Terminal"), "t", new CodeIndexerExpression(new CodeArgumentReferenceExpression("tokens"), tokenPos))));
            

            var actionResultRef = new CodeTypeReference("ActionResult");
            loop.Statements.Add(new CodeVariableDeclarationStatement(actionResultRef, "result", new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("t"), "Action"), stateStack, parseStack)));

            var resultVar = new CodeVariableReferenceExpression("result");

            var ifAccept = new CodeConditionStatement(new CodeBinaryOperatorExpression(resultVar, CodeBinaryOperatorType.ValueEquality, new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("ActionResult"), "Accept")));
            loop.Statements.Add(ifAccept);
            ifAccept.TrueStatements.Add(new CodeGotoStatement("breakLoop"));

            var ifShiftContinue = new CodeConditionStatement(new CodeBinaryOperatorExpression(resultVar, CodeBinaryOperatorType.ValueEquality, new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("ActionResult"), "ShiftContinue")));
            loop.Statements.Add(ifShiftContinue);
            ifShiftContinue.TrueStatements.Add(new CodeAssignStatement(tokenPos, new CodeBinaryOperatorExpression(tokenPos, CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))));
            ifShiftContinue.TrueStatements.Add(new CodeGotoStatement("startLoop"));

            var ifReduceContinue = new CodeConditionStatement(new CodeBinaryOperatorExpression(resultVar, CodeBinaryOperatorType.ValueEquality, new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("ActionResult"), "ReduceContinue")));
            loop.Statements.Add(ifReduceContinue);
            ifReduceContinue.TrueStatements.Add(new CodeVariableDeclarationStatement("IReducer", "reducer", new CodeCastExpression("IReducer", new CodeMethodInvokeExpression(parseStack, "Peek"))));
            ifReduceContinue.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("reducer"), "Goto", stateStack));
            ifReduceContinue.TrueStatements.Add(new CodeGotoStatement("startLoop"));

            var ifError = new CodeConditionStatement(new CodeBinaryOperatorExpression(resultVar, CodeBinaryOperatorType.ValueEquality, new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("ActionResult"), "Error")));
            loop.Statements.Add(ifError);
            ifError.TrueStatements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(new CodeTypeReference(typeof(Exception)), new CodePrimitiveExpression("Parsing Failed."))));

            parserMethod.Statements.Add(new CodeLabeledStatement("breakLoop", new CodeMethodReturnStatement(new CodeCastExpression(parserMethod.ReturnType, new CodeMethodInvokeExpression(parseStack, "Pop")))));
        }

        private CodeTypeDeclaration GetActionResultEnum()
        {
            var result = new CodeTypeDeclaration("ActionResult")
            {
                IsEnum = true
            };
            result.Members.Add(new CodeMemberField("ActionResult", "Error"));
            result.Members.Add(new CodeMemberField("ActionResult", "Accept"));
            result.Members.Add(new CodeMemberField("ActionResult", "ShiftContinue"));
            result.Members.Add(new CodeMemberField("ActionResult", "ReduceContinue"));
            return result;
        }

        private CodeMemberMethod GetAbstractActionMethod()
        {
            var result = new CodeMemberMethod
            {
                Name = "Action",
                Attributes = MemberAttributes.Public | MemberAttributes.Abstract,
                ReturnType = new CodeTypeReference("ActionResult")
            };
            
            result.Parameters.Add(new CodeParameterDeclarationExpression("Stack<int>", "states"));
            result.Parameters.Add(new CodeParameterDeclarationExpression("Stack<object>", "parseStack"));
            return result;
        }

        private void AddTerminalTextProperty(CodeTypeDeclaration terminalBase)
        {
            var ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Family;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "text"));
            var backingField = AddPropertyToClass("System.String", "Text", terminalBase);
            AddFieldAssignmentToConstructor(backingField, ctor, "text");
            terminalBase.Members.Add(ctor);
        }

        private void AddTerminalToString(CodeTypeDeclaration type)
        {
            var toString = new CodeMemberMethod()
            {
                Name = "ToString",
                ReturnType = new CodeTypeReference(typeof(string)),
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
            };
            type.Members.Add(toString);
            //const string argName = "indent";
            //toString.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), argName));
            toString.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_text")));
        }

        public string GetCode(CodeDomProvider codeProdiver, CodeGeneratorOptions generatorOptions)
        {
            using(var codeStream = new StringWriter())
            using(var indentedWriter = new IndentedTextWriter(codeStream, "    "))
            {
                codeProdiver.GenerateCodeFromCompileUnit(_compileUnit, indentedWriter, generatorOptions);
                return codeStream.ToString();
            }
        }

        private CodeMemberMethod AddParserClass(string name, string parseTreeRootType)
        {
            var parserClass = new CodeTypeDeclaration(name);
            _codeNamespace.Types.Add(parserClass);

            var parserMethod = new CodeMemberMethod()
            {
                Name = "Parse",
                ReturnType = new CodeTypeReference(parseTreeRootType),
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
            };
            parserMethod.Parameters.Add(new CodeParameterDeclarationExpression("IList<_Terminal>", "tokens"));
            parserClass.Members.Add(parserMethod);
            return parserMethod;
        }

        private void AddSymbol(NonTerminal head, IEnumerable<Production> productions)
        {
            var nonTermAbstractBase = new CodeTypeDeclaration(head.Name);
            nonTermAbstractBase.BaseTypes.Add(new CodeTypeReference("IReducer"));
            nonTermAbstractBase.TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public;
            nonTermAbstractBase.Members.Add(ImplementReducerGoto(head));
            var concreteTypes = GetConcreteTypesForProductions(nonTermAbstractBase, productions);
            _codeNamespace.Types.Add(nonTermAbstractBase);
            _codeNamespace.Types.AddRange(concreteTypes.ToArray());
        }

        private CodeTypeMember ImplementReducerGoto(NonTerminal head)
        {
            var result = new CodeMemberMethod()
            {
                Name = "Goto",
                ReturnType = new CodeTypeReference(typeof(void)),
                Attributes = MemberAttributes.Public | MemberAttributes.Final

            };
            result.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(Stack<int>)), "states"));
            var states = new CodeArgumentReferenceExpression("states");
            result.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(int)), "currentState", new CodeMethodInvokeExpression(states, "Peek")));

            foreach (var state in _transitionTable.States)
            {
                var action = _transitionTable[state, head];
                if (action.Action == ParserAction.Goto)
                {
                    var ifState = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("currentState"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(state.StateId)));
                    ifState.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(states, "Push"), new CodePrimitiveExpression(action.NextState.StateId)));
                    ifState.TrueStatements.Add(new CodeMethodReturnStatement());
                    result.Statements.Add(ifState);
                }
            }
            result.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(Exception), new CodePrimitiveExpression("Unexpected GOTO operation."))));
            return result;
        }

        private IEnumerable<CodeTypeDeclaration> GetConcreteTypesForProductions(CodeTypeDeclaration baseType, IEnumerable<Production> productions)
        {
            var concreteTypes = new List<CodeTypeDeclaration>();
            foreach (var prod in productions)
            {
                var concreteType = new CodeTypeDeclaration(GetClassName(prod));
                concreteType.BaseTypes.Add(new CodeTypeReference(baseType.Name));
                concreteTypes.Add(concreteType);
                int argCount = 1;
                var ctor = new CodeConstructor();
                ctor.Attributes = MemberAttributes.Public;
                concreteType.Members.Add(ctor);
                foreach (var symbol in prod.Tail)
                {
                    var argName = string.Format("t{0}", argCount++);
                    ctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(symbol.Name), argName));
                    var backingField = AddPropertyToClass(symbol.Name, argName, concreteType);
                    AddFieldAssignmentToConstructor(backingField, ctor, argName);
                }
                var reduceByMethod = GetReduceByMethod(prod);
                concreteType.Members.Add(reduceByMethod);
            }
            return concreteTypes;
        }

        private CodeMemberMethod GetReduceByMethod(Production prod)
        {
            var result = new CodeMemberMethod()
            {
                Name = "ReduceBy",
                Attributes = MemberAttributes.Public | MemberAttributes.Static,
                ReturnType = new CodeTypeReference(GetClassName(prod)),
            };
            result.Parameters.Add(new CodeParameterDeclarationExpression("Stack<object>", "parseTree"));

            for (int i = 0; i < prod.Tail.Length; i++)
            {
                string varName = "o" + i;
                var decl = new CodeVariableDeclarationStatement(typeof(object), varName, new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("parseTree"), "Pop"));
                result.Statements.Add(decl);
            }
            var ctorArgs = new CodeExpression[prod.Tail.Length];
            int count = 0;
            for (int i = prod.Tail.Length - 1; i >= 0; i--)
            {
                ctorArgs[count] = new CodeCastExpression(new CodeTypeReference(prod.Tail[count].Name), new CodeVariableReferenceExpression("o" + i));
                count++;
            }
            result.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(GetClassName(prod), ctorArgs)));
            return result;
        }

        private string GetClassName(Production prod)
        {
            var sb = new StringBuilder();
            sb.Append('_');
            sb.Append(prod.Head.Name);
            foreach (var v in prod.Tail)
            {
                sb.Append('_');
                sb.Append(v.Name);
            }
            return sb.ToString();
        }

        private void AddSymbol(Terminal term)
        {
            const string argName = "text";
            var termType = new CodeTypeDeclaration(term.Name);
            termType.BaseTypes.Add(new CodeTypeReference(new CodeTypeParameter(_terminalBase.Name)));
            termType.Members.Add(GetOverrideActionMethod(term, _transitionTable));
            _codeNamespace.Types.Add(termType);
            var ctor = new CodeConstructor();
            termType.Members.Add(ctor);
            ctor.Attributes = MemberAttributes.Public;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), argName));
            ctor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression(argName));
        }

        private CodeMemberMethod GetOverrideActionMethod(GrammarSymbol currToken, TransitionTable table)
        {
            var result = new CodeMemberMethod
            {
                Name = "Action",
                Attributes = MemberAttributes.Public | MemberAttributes.Override,
                ReturnType = new CodeTypeReference("ActionResult")
            };
            result.Parameters.Add(new CodeParameterDeclarationExpression("Stack<int>", "states"));
            result.Parameters.Add(new CodeParameterDeclarationExpression("Stack<object>", "parseStack"));

            var states = new CodeArgumentReferenceExpression("states");
            var parseStack = new CodeArgumentReferenceExpression("parseStack");

            result.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(int)), "currentState", new CodeMethodInvokeExpression(states, "Peek")));
            foreach (var state in table.States)
            {
                var ifState = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("currentState"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(state.StateId)));
                var action = table[state, currToken];
                if (action.Action == ParserAction.Shift)
                {
                    ifState.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(states, "Push"), new CodePrimitiveExpression(action.NextState.StateId)));
                    ifState.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(parseStack, "Push"), new CodeThisReferenceExpression()));
                    ifState.TrueStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("ActionResult"), "ShiftContinue")));
                }
                if (action.Action == ParserAction.Reduce)
                {
                    ifState.TrueStatements.Add(new CodeMethodInvokeExpression(parseStack, "Push", new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(GetClassName(action.ReduceByProduction)), "ReduceBy"), parseStack)));
                    for (int i = 0; i < action.ReduceByProduction.Length; i++)
                    {
                        ifState.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(states, "Pop")));
                    }
                    ifState.TrueStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("ActionResult"), "ReduceContinue")));
                }
                if (action.Action == ParserAction.Accept)
                {
                    ifState.TrueStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("ActionResult"), "Accept")));
                }
                if (action.Action == ParserAction.Error)
                {
                    ifState.TrueStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("ActionResult"), "Error")));
                }
                result.Statements.Add(ifState);
            }
            result.Statements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("ActionResult"), "Error")));
            return result;
        }

        private CodeMemberField AddPropertyToClass(string propertyType, string propertyName, CodeTypeDeclaration classDeclaration)
        {
            var typeRef = new CodeTypeReference(propertyType);
            var classField = new CodeMemberField(typeRef, "_" + propertyName.ToLower());
            classDeclaration.Members.Add(classField);
            var property = new CodeMemberProperty()
            {
                Name = propertyName,
                HasSet = false,
                Type = new CodeTypeReference(propertyType),
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
            };
            classDeclaration.Members.Add(property);
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), classField.Name)));
            return classField;
        }

        private void AddFieldAssignmentToConstructor(CodeMemberField field, CodeConstructor ctor, string argName)
        {
            ctor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name), new CodeArgumentReferenceExpression(argName)));
        }
    }
}
