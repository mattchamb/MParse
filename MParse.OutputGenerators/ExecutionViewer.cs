using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MParse.Core;
using MParse.Core.Interfaces;

namespace MParse.OutputGenerators
{
    public class ExecutionViewer : IOutputGenerator
    {

        private class History
        {
            public ParserState BeforeState { get; private set; }
            public ParserState AfterState { get; private set; }
            public string NamedState { get; set; }
            public string Action { get; private set; }

            public History(ParserState beforeState, ParserState afterState, string action)
            {
                BeforeState = beforeState;
                AfterState = afterState;
                Action = action;
            }
            public History(ParserState beforeState, string namedState, string action)
            {
                BeforeState = beforeState;
                NamedState = namedState;
                Action = action;
            }
        }

        public void Initialize(string[] commandLineArgs, Dictionary<string, string> settings)
        {
            
        }

        public bool GenerateOutput(TransitionTable transitionTable, ITokenStream tokenStream)
        {
            if (!tokenStream.MoveNext())
                throw new ArgumentException("Cannot get state for tokenStream", "tokenStream");
            var history = new List<History>();
            var stack = new Stack<ParserState>();
            int actionNumber = 0;
            stack.Push(transitionTable.States.First());
            while(true)
            {
                var currentState = stack.Peek();
                //Console.WriteLine("---------------");
                //Console.WriteLine("input = " + tokenStream.Current);
                //Console.WriteLine("stacksize = " + stack.Count);
                var action = transitionTable[currentState, tokenStream.Current];
                if(action.Action == ParserAction.Shift)
                {
                    //Console.WriteLine("Shift");
                    history.Add(new History(currentState, action.NextState, string.Format("{0}. In: {1}. Shift", actionNumber++, tokenStream.Current)));
                    stack.Push(action.NextState);
                    tokenStream.MoveNext();
                    continue;
                }
                if (action.Action == ParserAction.Reduce)
                {
                    //Console.WriteLine("Reduce by: " + action.ReduceByProduction);
                    for (int i = 0; i < action.ReduceByProduction.Length; i++)
                    {
                        stack.Pop();
                    }
                    history.Add(new History(currentState, stack.Peek(), string.Format("{0}. Red. {1}", actionNumber++, action.ReduceByProduction)));
                    currentState = stack.Peek();
                    var nextAction = transitionTable[currentState, action.ReduceByProduction.Head];
                    stack.Push(nextAction.NextState);
                    history.Add(new History(currentState, stack.Peek(), string.Format("{0}. GOTO", actionNumber++)));
                    continue;
                }
                if (action.Action == ParserAction.Error)
                {
                    history.Add(new History(currentState, "Error", string.Format("{0}. In: {1}.", actionNumber++, tokenStream.Current)));
                    //Console.WriteLine("Error");
                    break;
                }
                if (action.Action == ParserAction.Accept)
                {
                    history.Add(new History(currentState, "Accept", string.Format("{0}. In: {1}.", actionNumber++, tokenStream.Current)));
                    //Console.WriteLine("Accept");
                    break;
                }
            }
            var output = new StringBuilder();
            var states = history.Select(x => x.AfterState).Union(history.Select(x => x.BeforeState)).Where(x => x != null).ToList();
            output.AppendLine("digraph G {");

            foreach (var state in states)
            {
                output.Append(TidyStateName(state));
                output.AppendFormat(" [shape=box,label=\"{0}\"]", StateToDotString(state));
                output.AppendLine(";");
            }

            foreach (var h in history)
            {
                if(h.AfterState != null)
                    output.AppendFormat("{0} -> {1} [label=\"{2}\"]", TidyStateName(h.BeforeState), TidyStateName(h.AfterState), h.Action);
                else
                    output.AppendFormat("{0} -> {1} [label=\"{2}\"]", TidyStateName(h.BeforeState), h.NamedState, h.Action);
                output.AppendLine();
            }
            output.AppendLine("}");
            File.WriteAllText("history.dot", output.ToString());
            return true;
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

        public void Dispose()
        {
            
        }
    }
}
