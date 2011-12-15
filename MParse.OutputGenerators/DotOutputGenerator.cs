using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MParse.Core;
using MParse.Core.Interfaces;

namespace MParse.OutputGenerators
{
    public class DotOutputGenerator : IOutputGenerator
    {
        public bool GenerateOutput(TransitionTable transitionTable, ITokenStream tokenStream)
        {
            var builder = new StringBuilder();
            builder.AppendLine("digraph G {");
            builder.AppendLine("node[shape=box];");
            WriteTransitions(builder, transitionTable);
            builder.AppendLine("}");
            Console.WriteLine(builder);
            return true;
        }

        private void WriteTransitions(StringBuilder output, TransitionTable table)
        {
            var visited = table.States.ToDictionary(state => state, state => false);
            var plan = new Queue<ParserState>();
            plan.Enqueue(table.States.First());
            while (plan.Count > 0)
            {
                var currState = plan.Dequeue();
                visited[currState] = true;
                if (table.StateMap.FromStateExists(currState))
                {
                    foreach (var stateTransition in table.StateMap[currState])
                    {
                        output.AppendFormat("\"{0}\" -> \"{1}\" [label=\"{2}\"];",
                                            StateToDotString(currState),
                                            StateToDotString(stateTransition.Value),
                                            stateTransition.Key);
                        output.AppendLine();
                        if (!visited[stateTransition.Value] && !plan.Contains(stateTransition.Value))
                            plan.Enqueue(stateTransition.Value);
                    }
                }
            }
        }

        private string StateToDotString(ParserState state)
        {
            var result = new StringBuilder();
            foreach (var item in state.Items)
            {
                result.Append(item);
                result.Append("\\l");
            }
            return result.ToString();
        }
    }
}