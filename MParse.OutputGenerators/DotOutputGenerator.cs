using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MParse.Interfaces;

namespace MParse.OutputProviders
{
    public class DotOutputGenerator : IOutputGenerator
    {
        private Dictionary<string, string> _settings;
        private Stream _output;

        public void Initialize(string[] commandLineArgs, Dictionary<string, string> settings)
        {
            _settings = settings;
            _output = Console.OpenStandardOutput();
        }

        public void GenerateOutput(TransitionTable transitionTable)
        {
            var builder = new StringBuilder();
            builder.AppendLine("digraph G {");
            WriteStates(builder, transitionTable);
            WriteTransitions(builder, transitionTable);
            builder.AppendLine("}");
            var outputBytes = Encoding.ASCII.GetBytes(builder.ToString());
            _output.Write(outputBytes, 0, outputBytes.Length);
        }

        private void WriteTransitions(StringBuilder output, TransitionTable table)
        {
            var visited = table.States.ToDictionary(state => state.UniqueName, state => false);
            var plan = new Queue<ParserState>();
            plan.Enqueue(table.States.First());
            while (plan.Count > 0)
            {
                var currState = plan.Dequeue();
                visited[currState.UniqueName] = true;
                foreach (var stateTransition in currState.StateTransitions)
                {
                    output.AppendFormat("{0} -> {1} [label=\"{2}\"];", TidyStateName(currState), TidyStateName(stateTransition.Value),
                                        stateTransition.Key);
                    output.AppendLine();
                    if (!visited[stateTransition.Value.UniqueName])
                        plan.Enqueue(stateTransition.Value);
                }
            }
        }

        private void WriteStates(StringBuilder output, TransitionTable table)
        {
            foreach (var state in table.States)
            {
                output.Append(TidyStateName(state));
                output.AppendFormat(" [shape=box,label=\"{0}\"]", StateToDotString(state));
                output.AppendLine(";");
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

        private string TidyStateName(ParserState state)
        {
            //Dot does not like the hyphens or numbers in the node names.
            return state.UniqueName.Replace("-", "")
                .Replace('0', 'a')
                .Replace('1', 'b')
                .Replace('2', 'c')
                .Replace('3', 'd')
                .Replace('4', 'e')
                .Replace('5', 'f')
                .Replace('6', 'g')
                .Replace('7', 'h')
                .Replace('8', 'i')
                .Replace('9', 'j');
        }
    }
}