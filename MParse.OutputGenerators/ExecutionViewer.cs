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
                var action = transitionTable[currentState, tokenStream.Current];
                if(action.Action == ParserAction.Shift)
                {
                    history.Add(new History(currentState, action.NextState, string.Format("{0}. In: {1}. Shift", actionNumber++, tokenStream.Current)));
                    stack.Push(action.NextState);
                    tokenStream.MoveNext();
                    continue;
                }
                if (action.Action == ParserAction.Reduce)
                {
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
            foreach (var h in history)
            {
                if(h.AfterState != null)
                    output.AppendFormat("\"{3}\\n{0}\" -> \"{4}\\n{1}\" [label=\"{2}\"]", StateToDotString(h.BeforeState), StateToDotString(h.AfterState), h.Action, h.BeforeState.StateId, h.AfterState.StateId);
                else
                    output.AppendFormat("\"{3}\\n{0}\" -> \"{1}\" [label=\"{2}\"]", StateToDotString(h.BeforeState), h.NamedState, h.Action, h.BeforeState.StateId);
                output.AppendLine();
            }
            output.AppendLine("}");
            Console.WriteLine(output);
            return true;
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
