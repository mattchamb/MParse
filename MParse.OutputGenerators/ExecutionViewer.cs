using System;
using System.Collections.Generic;
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

        public void GenerateOutput(TransitionTable transitionTable)
        {
            var input = new[]
                                              {
                                                  new Terminal(6, "Id"),
                                                  new Terminal(5, "*"),
                                                  new Terminal(6, "Id"),
                                                  new Terminal(4, "+"),
                                                  new Terminal(6, "Id")
                                              };
            var stack = new Stack<ParserState>();

            stack.Push(transitionTable.States.First());

            for (int index = 0; index < input.Length; index++)
            {
                GrammarSymbol symbol = input[index];
                var currentState = stack.Peek();
                Console.WriteLine("---------------");
                Console.WriteLine("input = " + symbol);
                Console.WriteLine("stacksize = " + stack.Count);
                var action = transitionTable[currentState, symbol];
                switch (action.Action)
                {
                    case TransitionAction.ParserAction.Shift:
                        stack.Push(action.NextState);
                        Console.WriteLine("Shift");
                        break;
                    case TransitionAction.ParserAction.Reduce:
                        Console.WriteLine("Reducing by production: " + action.ReduceByProduction);
                        for (int i = 0; i < action.ReduceByProduction.Length; i++)
                        {
                            stack.Pop();
                        }
                        currentState = stack.Peek();
                        stack.Push(transitionTable[currentState, action.ReduceByProduction.Head].NextState);
                        break;
                    case TransitionAction.ParserAction.Goto:
                        stack.Push(action.NextState);
                        break;
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
