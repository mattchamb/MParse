using System;
using System.Text;
using MParse.GrammarElements;

namespace MParse
{
    public class TransitionAction
    {
        public enum ParserAction
        {
            Shift,
            Reduce,
            Accept
        }

        public ParserAction Action { get; private set; }
        public ParserState NextState { get; private set; }
        public Production ReduceByProduction { get; private set; }

        public TransitionAction(ParserAction action)
        {
            Action = action;
        }

        public TransitionAction(ParserAction action, ParserState nextState) : this(action)
        {
            if(action != ParserAction.Shift)
                throw new Exception("Can only define the next state for the shift action.");
            NextState = nextState;
        }

        public TransitionAction(ParserAction action, Production reduceByProduction) :this(action)
        {
            if (action != ParserAction.Reduce)
                throw new Exception("Can only define the production to reduce by for the reduce action.");
            ReduceByProduction = reduceByProduction;
        }
        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine(Action.ToString());
            switch (Action)
            {
                case ParserAction.Shift:
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