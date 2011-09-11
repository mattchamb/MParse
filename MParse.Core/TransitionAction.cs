using System;
using System.Text;
using MParse.GrammarElements;

namespace MParse
{
    public class TransitionAction
    {
        

        public ParserAction Action { get; private set; }

        public ParserState NextState { get; private set; }
        public Production ReduceByProduction { get; private set; }

        public TransitionAction(ParserAction action)
        {
            Action = action;
        }

        public TransitionAction(ParserAction action, ParserState nextState)
            : this(action)
        {
            if (action != ParserAction.Shift && action != ParserAction.Goto)
                throw new Exception("Can only define the next state for the shift or goto actions.");
            if (nextState == null)
                throw new ArgumentNullException("nextState");
            NextState = nextState;
        }

        public TransitionAction(ParserAction action, Production reduceByProduction)
            : this(action)
        {
            if (action != ParserAction.Reduce)
                throw new Exception("Can only define the production to reduce by for the reduce action.");
            if (reduceByProduction == null)
                throw new ArgumentNullException("reduceByProduction");
            ReduceByProduction = reduceByProduction;
        }
        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine(Action.ToString());
            switch (Action)
            {
                case ParserAction.Shift:
                case ParserAction.Goto:
                    result.Append(NextState);
                    break;
                case ParserAction.Reduce:
                    result.Append(ReduceByProduction);
                    break;
            }
            return result.ToString();
        }
    }
}