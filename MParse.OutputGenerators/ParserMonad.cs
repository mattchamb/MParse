using System;
using System.Collections.Generic;
using System.Linq;
using MParse.Core;
using MParse.Core.Interfaces;
using MParse.Core.GrammarElements;
using System.Linq.Expressions;

namespace MParse.OutputGenerators
{
    public class ParserMonad : IOutputGenerator
    {

        public void Initialize(string[] commandLineArgs, Dictionary<string, string> settings)
        {
           
        }

        public bool GenerateOutput(TransitionTable transitionTable, ITokenStream tokenStream)
        {
            var tokens = tokenStream.ToList();
            

            IParser<State> startingParserState = new StartingState(tokens, transitionTable).ToParser<State>();
            
            Func<State, IParser<State>> g = (State y) => y.NextState().ToParser();

            g = g.Compose(g);
            g = g.Compose(g);
            g = g.Compose(g);
            g = g.Compose(g);
            g = g.Compose(g);
            g = g.Compose(g);
            g = g.Compose(g);


            var result = startingParserState.Bind(g);
            

            if (!(result.Value is AcceptState))
            {
                result = result.Bind(g);
            }

            if (!(result.Value is AcceptState))
            {
                result = result.Bind(g);
            }

            if (!(result.Value is AcceptState))
            {
                result = result.Bind(g);
            }

            if (!(result.Value is AcceptState))
            {
                result = result.Bind(g);
            }

            if (!(result.Value is AcceptState))
            {
                result = result.Bind(g);
            }

            if (!(result.Value is AcceptState))
            {
                result = result.Bind(g);
            }

            return true;
        }

        public void Dispose()
        {
           
        }
    }

    public abstract class State
    {
        protected Stack<ParserState> _stateStack;
        protected int _currentTokenPosition;
        protected IList<Terminal> _tokenStream;
        protected TransitionTable _transitionTable;

        public State()
        {

        }

        public State(IList<Terminal> tokenStream, TransitionTable transitionTable, Stack<ParserState> stateStack)
        {
            _stateStack = new Stack<ParserState>(stateStack.Reverse());
            _currentTokenPosition = 0;
            _tokenStream = tokenStream;
            _transitionTable = transitionTable;
        }

        public State(IList<Terminal> tokenStream, TransitionTable transitionTable) 
            : this(tokenStream, transitionTable, new Stack<ParserState>())
        {
        }

        protected State(State s)
        {
            _stateStack = new Stack<ParserState>(s._stateStack.Reverse());
            _currentTokenPosition = s._currentTokenPosition;
            _tokenStream = s._tokenStream;
            _transitionTable = s._transitionTable;
        }

        protected virtual State Clone()
        {
            return this;
        }
        
        public virtual State NextState()
        {
            var currentState = _stateStack.Peek();
            var action = _transitionTable[currentState, _tokenStream[_currentTokenPosition]];
            State result;
            switch(action.Action)
            {
                case ParserAction.Shift:
                    result = new ShiftState(this, action.NextState);
                    break;

                case ParserAction.Reduce:
                    result = new ReduceState(this, action.ReduceByProduction);
                    break;

                case ParserAction.Accept:
                    result = new AcceptState();
                    break;

                case ParserAction.Error:
                default:
                    result = new ErrorState(_currentTokenPosition);
                    break;
            }
            return result;
        }
    }

    public class ErrorState : State
    {
        public int ErrorPosition { get; private set; }

        public ErrorState(int errorPosition)
        {
            ErrorPosition = errorPosition;
        }

        public override State NextState()
        {
            return this;
        }
    }

    public class AcceptState : State
    {
        public override State NextState()
        {
            return this;
        }
    }

    public class StartingState : State
    {

        public StartingState(IList<Terminal> tokenStream, TransitionTable transitionTable) : base(tokenStream, transitionTable)
        {
            _stateStack.Push(transitionTable.States.First());
        }

        public StartingState(State s) : base(s)
        {

        }

        public override State NextState()
        {
            return new ShiftState(this);
        }
    }

    public class GotoState : State
    {
        public GotoState(State s, Production reduceByProduction)
            : base(s)
        {
            var currentState = _stateStack.Peek();
            var nextAction = _transitionTable[currentState, reduceByProduction.Head];
            _stateStack.Push(nextAction.NextState);
        }
    }

    public class ShiftState : State
    {
        public ShiftState(State s) : base(s)
        {

        }

        public ShiftState(State s, ParserState nextState)
            : base(s)
        {
            _currentTokenPosition++;
            _stateStack.Push(nextState);
        }
    }

    public class ReduceState : State
    {
        private Production _reduceByProduction;
        public ReduceState(State s, Production reduceByProduction)
            : base(s)
        {
            _reduceByProduction = reduceByProduction;

            for (int i = 0; i < reduceByProduction.Length; i++)
            {
                _stateStack.Pop();
            }
        }

        public override State NextState()
        {
            return new GotoState(this, _reduceByProduction);
        }
    }

    public interface IParser <TState>
    {
        TState Value { get; set; }
    }

    public class Parser<TState> : IParser<TState> where TState : State
    {
        public TState Value { get; set; }
    }

    public static class MyClass
    {
        public static IParser<T> ToParser<T>(this T value) where T : State
        {
            return new Parser<T>() { Value = value };
        }

        public static IParser<V> Bind<U, V>(this IParser<U> m, Func<U, IParser<V>> k)
        {
            return k(m.Value);
        }

        public static Func<T, IParser<V>> Compose<T, U, V>(this Func<U, IParser<V>> f, Func<T, IParser<U>> g)
        {
            return x => Bind(g(x), f);
        }
    }
}