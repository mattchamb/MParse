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
            IGrammarProvider grammarProvider = new TestGrammarProvider();
            IGrammarOperator grammarOperator = new GrammarOperator(grammarProvider);
            /*IOutputGenerator dotOutputGenerator = new DotOutputGenerator();
            IOutputGenerator exec = new SimpleExecutor();
            IOutputGenerator viewer = new ExecutionViewer();
            ITokenStream tokenStream = new TestTokenStream();
            IOutputGenerator mon = new ParserMonad();

            dotOutputGenerator.Initialize(args, new Dictionary<string, string>{{"outFile", "graph.dot"}});
            exec.Initialize(args, null);
            viewer.Initialize(args, null);
            mon.Initialize(args, null);*/

            CreateClasses(grammarProvider.GetProductions(), grammarProvider.GetGrammarSymbols());
            /*var table = new TransitionTable(grammarProvider, grammarOperator);

            //dotOutputGenerator.GenerateOutput(table, tokenStream);
            //viewer.GenerateOutput(table, tokenStream);
            //tokenStream.Reset();
            //Console.WriteLine(exec.GenerateOutput(table, tokenStream) ? "Valid" : "Invalid");
            mon.GenerateOutput(table, tokenStream);

            dotOutputGenerator.Dispose();
            exec.Dispose();*/
        }

        

        private static void CreateClasses(Production[] production, GrammarSymbol[] symbols)
        {
            var cb = new ClassBuilder("TestNamespace");
            foreach (var symbol in symbols)
            {
                if (symbol is Terminal)
                {
                    cb.AddSymbol(symbol as Terminal);
                }
                else
                {
                    cb.AddSymbol(symbol as NonTerminal, production.Where(x => x.Head.Name == symbol.Name));
                }
            }
            File.WriteAllText("out.cs", cb.GetCode(new CSharpCodeProvider(), new CodeGeneratorOptions()));

        }
    }
}
