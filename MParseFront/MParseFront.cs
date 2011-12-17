using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FileProvider = MParse.GrammarProviders.FileProvider;
using Microsoft.CSharp;
using MParse.Core;
using MParse.Core.GrammarElements;
using MParse.Core.Interfaces;
using MParse.GrammarProviders;
using MParse.OutputGenerators;

namespace MParseFront
{
    class MParseFront
    {
        static void Main(string[] args)
        {
            //IGrammarProvider grammarProvider = new GGrammarProvider();
            IGrammarProvider grammarProvider = new FileProvider.FileGrammarProvider(@"..\..\..\MParse.GrammarProviders\FileProvider\grammar.gm");
            var grammar = grammarProvider.GetGrammar();
            
            var dotOutputGenerator = new DotOutputGenerator();

            var table = new TransitionTable(grammar);



            CreateClasses(grammar.Productions, grammar.Symbols, table);
        }



        private static void CreateClasses(IEnumerable<Production> productions, IEnumerable<GrammarSymbol> symbols, TransitionTable tt)
        {
            //var parserBuilder = new ParserBuilder("TestNamespace", productions, symbols, tt);
            //var s = parserBuilder.GetCode(new CSharpCodeProvider(), new CodeGeneratorOptions());
            var pt = new ParserTemplate();
            pt.Init("GrammarParser", productions, symbols, tt);
            File.WriteAllText("grammargrammar.cs", pt.TransformText());
        }
    }
}
