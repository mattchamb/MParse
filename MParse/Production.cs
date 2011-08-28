using System;
using System.Collections.Generic;
using System.Text;

namespace MParse
{
    public class Production
    {
        public int Head { get; private set; }
        public int[] Tail { get; private set; }

        private List<Item> _items;

        public Production(int head, int[] tail)
        {
            if(tail == null || tail.Length == 0)
                throw new ArgumentException("The tail must be a valid Integer array.", "tail");
            Head = head;
            Tail = tail;
        }

        public List<Item> GetItems()
        {
            if (_items != null)
                return _items;

            var result = new List<Item>();
            var currentItem = new Item(this);

            do
            {
                result.Add(currentItem);
                currentItem = currentItem.AdvanceDot();
            } while (currentItem.CanAdvanceDot);

            _items = result;
            return result;
        }

        public int Length
        {
            get { return Tail.Length; }
        }

        public int this[int index]
        {
            get { return Tail[index]; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is Production)) return false;
            return Equals((Production)obj);
        }

        public bool Equals(Production other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Head == Head && Equals(other.Tail, Tail);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Head*397) ^ (Tail != null ? Tail.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append(Head);
            result.Append(" -> ");
            for (int i = 0; i < Length; i++)
            {
                result.Append(Tail[i]);
                result.Append(" ");
            }
            return result.ToString();
        }
    }
}
