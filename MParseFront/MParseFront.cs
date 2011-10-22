using System;
using System.Collections.Generic;
using MParse.Core;
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
            IOutputGenerator dotOutputGenerator = new DotOutputGenerator();
            IOutputGenerator exec = new SimpleExecutor();
            IOutputGenerator viewer = new ExecutionViewer();
            ITokenStream tokenStream = new TestTokenStream();
            IOutputGenerator mon = new ParserMonad();

            dotOutputGenerator.Initialize(args, new Dictionary<string, string>{{"outFile", "graph.dot"}});
            exec.Initialize(args, null);
            viewer.Initialize(args, null);
            mon.Initialize(args, null);

            var table = new TransitionTable(grammarProvider, grammarOperator);

            //dotOutputGenerator.GenerateOutput(table, tokenStream);
            //viewer.GenerateOutput(table, tokenStream);
            //tokenStream.Reset();
            //Console.WriteLine(exec.GenerateOutput(table, tokenStream) ? "Valid" : "Invalid");
            mon.GenerateOutput(table, tokenStream);

            dotOutputGenerator.Dispose();
            exec.Dispose();

            Console.ReadLine();
        }
    }
}
