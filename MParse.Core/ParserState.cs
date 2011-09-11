using System;
using System.Collections.Generic;
using System.Text;
using MParse.GrammarElements;

namespace MParse
{
    public class ParserState : IEquatable<ParserState>
    {

        private readonly Guid _stateId;
        private readonly Dictionary<GrammarSymbol, ParserState> _stateTransitions;
        private readonly HashSet<Item> _itemsInState;

        //Describes the transition from this state to another state under the given input symbol.
        public Dictionary<GrammarSymbol, ParserState> StateTransitions
        {
            get
            {
                return _stateTransitions;
            }
        }

        public HashSet<Item> Items
        {
            get
            {
                return _itemsInState;
            }
        }

        public ParserState(IEnumerable<Item> items)
        {
            _stateTransitions = new Dictionary<GrammarSymbol, ParserState>();
            _itemsInState = new HashSet<Item>(items);
            _stateId = Guid.NewGuid();
        }

        public ParserState AddTransition(GrammarSymbol symbol, List<Item> items)
        {
            var newState = new ParserState(items);
            _stateTransitions.Add(symbol, newState);
            return newState;
        }

        public ParserState AddTransition(GrammarSymbol symbol, ParserState transitionTo)
        {
            if (_stateTransitions.ContainsKey(symbol))
                throw new Exception("Conflict");
            _stateTransitions[symbol] = transitionTo;
            return transitionTo;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(ParserState)) return false;
            return Equals((ParserState)obj);
        }

        public bool Equals(ParserState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _itemsInState.SetEquals(other._itemsInState);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_stateTransitions.GetHashCode() * 397) ^ _itemsInState.GetHashCode();
            }
        }
        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var item in Items)
            {
                result.Append(item.ToString());
                result.AppendLine("||");
            }
            return result.ToString();
        }

        public string UniqueName
        {
            get
            {
                return _stateId.ToString();
            }
        }
    }
}
