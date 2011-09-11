using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MParse;
using MParse.GrammarProviders;
using MParse.Interfaces;
using MParse.OutputProviders;

namespace MParseFront
{
    class MParseFront
    {
        static void Main(string[] args)
        {
            IGrammarProvider grammarProvider = new TestGrammarProvider();
            IGrammarOperator grammarOperator = new GrammarOperator(grammarProvider);
            IOutputGenerator outputGenerator = new DotOutputGenerator();

            outputGenerator.Initialize(null, null);

            var states = grammarOperator.CreateStates();

            var table = new TransitionTable(grammarProvider, grammarOperator, states);

            outputGenerator.GenerateOutput(table);
        }
    }
}
