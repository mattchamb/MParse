using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MParse.GrammarElements;
using MParse.Interfaces;

namespace MParse.OutputProviders
{
    public class ExecutionViewer : IOutputGenerator
    {
        public void Initialize(string[] commandLineArgs, Dictionary<string, string> settings)
        {
            
        }

        public bool GenerateOutput(TransitionTable transitionTable)
        {
            var input = new GrammarSymbol[]
                                        {
                                            new Terminal(6, "Id"),
                                            new Terminal(5, "*"),
                                            new Terminal(6, "Id"),
                                            new Terminal(4, "+"),
                                            new Terminal(6, "Id"),
                                            new EndOfStream()
                                        };
            var stack = new Stack<ParserState>();

            stack.Push(transitionTable.States.First());
            var index = 0;
            var current = input[index];
            while(true)
            {
                current = input[index];
                var currentState = stack.Peek();
                Console.WriteLine("---------------");
                Console.WriteLine("input = " + current);
                Console.WriteLine("stacksize = " + stack.Count);
                var action = transitionTable[currentState, current];
                if(action.Action == ParserAction.Shift)
                {
                    Console.WriteLine("Shift");
                    stack.Push(action.NextState);
                    index++;
                    continue;
                }
                else if (action.Action == ParserAction.Reduce)
                {
                    Console.WriteLine("Reduce by: " + action.ReduceByProduction);
                    for (int i = 0; i < action.ReduceByProduction.Length; i++)
                    {
                        stack.Pop();
                    }
                    currentState = stack.Peek();
                    var nextAction = transitionTable[currentState, action.ReduceByProduction.Head];
                    Debug.Assert(nextAction.Action == ParserAction.Goto);
                    stack.Push(nextAction.NextState);
                    continue;
                }
                else if (action.Action == ParserAction.Error)
                {
                    Console.WriteLine("Error");
                    return false;
                }
                else if (action.Action == ParserAction.Accept)
                {
                    Console.WriteLine("Accept");
                    return true;
                }
            }
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
