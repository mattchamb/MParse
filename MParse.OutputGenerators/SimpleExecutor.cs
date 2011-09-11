using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MParse.GrammarElements;
using MParse.Interfaces;

namespace MParse.OutputProviders
{
    public class SimpleExecutor : IOutputGenerator
    {
        public void Initialize(string[] commandLineArgs, Dictionary<string, string> settings)
        {
            
        }

        public bool GenerateOutput(TransitionTable transitionTable, ITokenStream tokenStream)
        {
            if (!tokenStream.MoveNext())
                throw new ArgumentException("Cannot get state for tokenStream", "tokenStream");

            var stateStack = new Stack<ParserState>();
            stateStack.Push(transitionTable.States.First());
            while (true)
            {
                var currentState = stateStack.Peek();
                var action = transitionTable[currentState, tokenStream.Current];
                if (action.Action == ParserAction.Shift)
                {
                    stateStack.Push(action.NextState);
                    tokenStream.MoveNext();
                    continue;
                }
                if (action.Action == ParserAction.Reduce)
                {
                    for (int i = 0; i < action.ReduceByProduction.Length; i++)
                    {
                        stateStack.Pop();
                    }
                    currentState = stateStack.Peek();
                    var nextAction = transitionTable[currentState, action.ReduceByProduction.Head];
                    stateStack.Push(nextAction.NextState);
                    continue;
                }
                if (action.Action == ParserAction.Error)
                {
                    return false;
                }
                if (action.Action == ParserAction.Accept)
                {
                    return true;
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}
