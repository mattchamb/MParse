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

            IState s = new StartingState(tokens, transitionTable);

            Func<IState, IParser<IState>> p = (IState x) => x.ToParser();

            Func<IState, IParser<IState>> f = (IState y) =>
            {
                if (y is StartingState)
                {
                    return (y as StartingState).NextState().ToParser();
                }
                if (y is State)
                {
                    return (y as State).NextState().ToParser();
                }
                return y.ToParser();
            };

            var g = p.Compose(f).Compose(f).Compose(f).Compose(f).Compose(f);

            var result = g(s);

            if (!(result.Value is AcceptState))
            {
                result = g(result.Value);
            }

            if (!(result.Value is AcceptState))
            {
                result = g(result.Value);
            }

            return true;
        }

        public void Dispose()
        {
           
        }
    }

    public interface IState { }

    public class ErrorState : IState
    {
        public int ErrorPosition { get; private set; }
        public ErrorState(int errorPosition)
        {
            ErrorPosition = errorPosition;
        }
    }

    public class AcceptState : IState
    {

    }

    public class StartingState : IState
    {
        private Stack<ParserState> _stateStack;
        private int _currentTokenPosition;
        private IList<Terminal> _tokenStream;
        private TransitionTable _transitionTable;

        public StartingState(IList<Terminal> tokenStream, TransitionTable transitionTable)
        {
            _stateStack = new Stack<ParserState>();
            _stateStack.Push(transitionTable.States.First());
            _currentTokenPosition = 0;
            _tokenStream = tokenStream;
            _transitionTable = transitionTable;
        }

        public IState NextState()
        {
            return new State(_tokenStream, _transitionTable, new Stack<ParserState>(_stateStack));
        }
    }

    public class State : IState
    {
        private Stack<ParserState> _stateStack;
        private int _currentTokenPosition;
        private IList<Terminal> _tokenStream;
        private TransitionTable _transitionTable;

        public State(IList<Terminal> tokenStream, TransitionTable transitionTable, Stack<ParserState> stateStack)
        {
            _stateStack = stateStack;
            _currentTokenPosition = 0;
            _tokenStream = tokenStream;
            _transitionTable = transitionTable;
        }

        private State(State s)
	    {
            _stateStack = new Stack<ParserState>(s._stateStack.Reverse());
            _currentTokenPosition = s._currentTokenPosition;
            _tokenStream = s._tokenStream;
            _transitionTable = s._transitionTable;
	    }

        private State Clone()
        {
            return new State(this);
        }

        public IState NextState()
        {

            var currentState = _stateStack.Peek();
            var action = _transitionTable[currentState, _tokenStream[_currentTokenPosition]];
            
            if (action.Action == ParserAction.Shift)
            {
                var nextState = Clone();
                nextState._stateStack.Push(action.NextState);
                nextState._currentTokenPosition++;
                return nextState;
            }
            if (action.Action == ParserAction.Reduce)
            {
                var nextState = Clone();
                for (int i = 0; i < action.ReduceByProduction.Length; i++)
                {
                    nextState._stateStack.Pop();
                }
                currentState = nextState._stateStack.Peek();
                var nextAction = nextState._transitionTable[currentState, action.ReduceByProduction.Head];
                nextState._stateStack.Push(nextAction.NextState);
                return nextState;
            }
            if (action.Action == ParserAction.Error)
            {
                return new ErrorState(_currentTokenPosition);
            }
            if (action.Action == ParserAction.Accept)
            {
                return new AcceptState();
            }
            return new ErrorState(_currentTokenPosition);
        }
    }

    public interface IParser <TState>
    {
        TState Value { get; set; }
    }

    public class Parser<TState> : IParser<TState>
    {
        public TState Value { get; set; }
    }

   
    public static class MyClass
    {
        public static IParser<T> ToParser<T>(this T value)
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