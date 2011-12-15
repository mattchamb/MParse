using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using MParse.Core;
using MParse.Core.GrammarElements;
using MParse.Core.Interfaces;
using MParse.GrammarProviders;
using MParse.OutputGenerators;
using MParse.TokenProviders;

namespace MParseFront
{
    class MParseFront
    {
        static void Main(string[] args)
        {
            IGrammarProvider grammarProvider = new GGrammarProvider();
            var grammar = grammarProvider.GetGrammar();
            IGrammarOperator grammarOperator = new GrammarOperator(grammar);

            IOutputGenerator dotOutputGenerator = new DotOutputGenerator();
            IOutputGenerator exec = new SimpleExecutor();
            IOutputGenerator viewer = new ExecutionViewer();

            ITokenStream tokenStream = new TestTokenStream();

            var table = new TransitionTable(grammar, grammarOperator);

            //exec.GenerateOutput(table, tokenStream);
            //viewer.GenerateOutput(table, tokenStream);

            //CreateClasses(grammar.Productions, grammar.Symbols, table);

            dotOutputGenerator.GenerateOutput(table, tokenStream);
            //viewer.GenerateOutput(table, tokenStream);
            //tokenStream.Reset();
            //Console.WriteLine(exec.GenerateOutput(table, tokenStream) ? "Valid" : "Invalid");

        }



        private static void CreateClasses(IEnumerable<Production> productions, IEnumerable<GrammarSymbol> symbols, TransitionTable tt)
        {
            //var parserBuilder = new ParserBuilder("TestNamespace", productions, symbols, tt);
            //var s = parserBuilder.GetCode(new CSharpCodeProvider(), new CodeGeneratorOptions());
            ParserTemplate pt = new ParserTemplate();
            pt.Init("GrammarParser", productions, symbols, tt);
            File.WriteAllText("grammargrammar.cs", pt.TransformText());
        }
    }
}
