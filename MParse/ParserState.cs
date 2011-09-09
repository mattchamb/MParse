using System;
using System.Collections.Generic;
using System.Linq;

namespace MParse
{
    public class ParserState : IEquatable<ParserState>
    {
        private readonly Dictionary<int, ParserState> _stateTransitions;
        private readonly List<Item> _itemsInState;

        public IEnumerable<Item> Items { get { return _itemsInState; } }

        public ParserState(IEnumerable<Item> items)
        {
            _stateTransitions = new Dictionary<int, ParserState>();
            _itemsInState = new List<Item>(items);
        }

        public ParserState AddTransition(int symbol, List<Item> items)
        {
            var newState = new ParserState(items);
            _stateTransitions.Add(symbol, newState);
            return newState;
        }

        public ParserState AddTransition(int symbol, ParserState transitionTo)
        {
            _stateTransitions[symbol] = transitionTo;
            return transitionTo;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ParserState)) return false;
            return Equals((ParserState) obj);
        }

        public bool Equals(ParserState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _itemsInState.SequenceEqual(other._itemsInState);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_stateTransitions.GetHashCode()*397) ^ _itemsInState.GetHashCode();
            }
        }
    }
}
