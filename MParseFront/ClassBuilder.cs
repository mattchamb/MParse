using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using MParse.Core.GrammarElements;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;

namespace MParseFront
{
    class ClassBuilder
    {
        private CodeNamespace _codeNamespace;
        private CodeCompileUnit _compileUnit;
        public ClassBuilder(string codeNamespace)
        {
            _compileUnit = new CodeCompileUnit();
            _codeNamespace = new CodeNamespace(codeNamespace);
            _compileUnit.Namespaces.Add(_codeNamespace);
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

        public void AddSymbol(NonTerminal head, IEnumerable<Production> productions)
        {
            var nonTermAbstractBase = new CodeTypeDeclaration(head.Name);
            nonTermAbstractBase.TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public;
            _codeNamespace.Types.Add(nonTermAbstractBase);
            var concreteTypes = GetConcreteTypes(nonTermAbstractBase, productions);
            _codeNamespace.Types.AddRange(concreteTypes.ToArray());
        }

        private IEnumerable<CodeTypeDeclaration> GetConcreteTypes(CodeTypeDeclaration baseType, IEnumerable<Production> productions)
        {
            var concreteTypes = new List<CodeTypeDeclaration>();
            int classCount = 1;
            foreach (var prod in productions)
            {
                var concreteType = new CodeTypeDeclaration(string.Format("_{0}_{1}", prod.Head.Name, classCount++));
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
            }
            return concreteTypes;
        }

        public void AddSymbol(Terminal term)
        {
            var argName = "text";
            var termType = new CodeTypeDeclaration(term.Name);
            _codeNamespace.Types.Add(termType);
            var ctor = new CodeConstructor();
            termType.Members.Add(ctor);
            ctor.Attributes = MemberAttributes.Public;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), argName));
            var backingField = AddPropertyToClass("System.String", "Text", termType);
            AddFieldAssignmentToConstructor(backingField, ctor, argName);
        }

        private CodeMemberField AddPropertyToClass(string propertyType, string propertyName, CodeTypeDeclaration classDeclaration)
        {
            var typeRef = new CodeTypeReference(propertyType);

            var classField = new CodeMemberField(typeRef, "_" + propertyName.ToLower());
            classDeclaration.Members.Add(classField);

            CodeMemberProperty property = new CodeMemberProperty()
            {
                Name = propertyName,
                HasSet = false,
                Type = new CodeTypeReference(propertyType),
                Attributes = MemberAttributes.Public,
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
