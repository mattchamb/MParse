using System;
using System.Collections.Generic;
using System.Text;
using MParse.Core.GrammarElements;

namespace MParse.Core
{
    public class ParserState : IEquatable<ParserState>
    {

        private readonly int _stateId;
        private readonly HashSet<Item> _itemsInState;

        public HashSet<Item> Items
        {
            get { return _itemsInState; }
        }

        public ParserState(int stateId, IEnumerable<Item> items)
        {
            
            _itemsInState = new HashSet<Item>(items);
            _stateId = stateId;
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
            return _itemsInState.GetHashCode();
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

        public int StateId
        {
            get
            {
                return _stateId;
            }
        }
    }
}
