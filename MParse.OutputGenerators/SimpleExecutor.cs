using System;
using System.Collections.Generic;
using System.Linq;
using MParse.Core;
using MParse.Core.Interfaces;

namespace MParse.OutputGenerators
{
    public class SimpleExecutor : IOutputGenerator
    {
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
                    bool b = tokenStream.MoveNext();
                    if(!b)
                        throw null;
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
    }
}
