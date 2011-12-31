using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MParse.Core.GrammarElements;

namespace MParse.Core
{
    public class StateTransitionMap
    {
        private readonly Dictionary<ParserState, Dictionary<GrammarSymbol, ParserState>> _stateTransitions;
        private readonly HashSet<ParserState> _states;

        public StateTransitionMap()
        {
            _states = new HashSet<ParserState>();
            _stateTransitions = new Dictionary<ParserState, Dictionary<GrammarSymbol, ParserState>>();
        }

        public void AddTransition(ParserState transitionFrom, GrammarSymbol symbol, ParserState transitionTo)
        {
            if(!_stateTransitions.ContainsKey(transitionFrom))
                _stateTransitions.Add(transitionFrom, new Dictionary<GrammarSymbol,ParserState>());
            if (_stateTransitions[transitionFrom].ContainsKey(symbol))
                throw new Exception("State conflict. Trying to add a transition for a state when one already exists for the specified grammar symbol.");
            _stateTransitions[transitionFrom][symbol] = transitionTo;
            _states.Add(transitionFrom);
            _states.Add(transitionTo);
        }

        /// <summary>
        /// Describes the transition from the given state to another state under the given input symbol.
        /// </summary>
        public ParserState this[ParserState transitionFrom, GrammarSymbol symbol]
        {
            get { return _stateTransitions[transitionFrom][symbol]; }
        }

        public Dictionary<GrammarSymbol, ParserState> this[ParserState transitionFrom]
        {
            get { return _stateTransitions[transitionFrom]; }
        }

        public bool TransitionExists(ParserState transitionFrom, GrammarSymbol symbol)
        {
            return _stateTransitions.ContainsKey(transitionFrom) && _stateTransitions[transitionFrom].ContainsKey(symbol);
        }

        public bool FromStateExists(ParserState transitionFrom)
        {
            return _stateTransitions.ContainsKey(transitionFrom);
        }

        public IEnumerable<ParserState> States
        {
            get { return _states; }
        }
    }
}
