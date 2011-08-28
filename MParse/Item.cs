using System;
using System.Text;

namespace MParse
{
    public class Item
    {
        public Item(Production production) : this(0, production) {}

        public Item(int currentPosition, Production production)
        {
            if(currentPosition < 0)
            {
                throw new ArgumentException("currentPosition cannot be less than zero.", "currentPosition");
            }
            CurrentPosition = currentPosition;
            ItemProduction = production;
        }

        public int CurrentPosition { get; private set; }
        public Production ItemProduction { get; private set; }

        public bool CanAdvanceDot
        {
            get { return CurrentPosition < ItemProduction.Length; }
        }

        public int NextToken
        {
            get { return ItemProduction.Tail[CurrentPosition]; }
        }

        public Item AdvanceDot()
        {
            if(!CanAdvanceDot)
                throw new InvalidOperationException("Cannot advance the dot any more in this production.");
            return new Item(CurrentPosition + 1, ItemProduction);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is Item)) return false;
            return Equals((Item)obj);
        }

        public bool Equals(Item other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.CurrentPosition == CurrentPosition && Equals(other.ItemProduction, ItemProduction);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CurrentPosition*397) ^ (ItemProduction != null ? ItemProduction.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append(ItemProduction.Head);
            result.Append(" -> ");
            for (int i = 0; i < ItemProduction.Length; i++)
            {
                if (i == CurrentPosition)
                    result.Append(". ");
                result.Append(ItemProduction.Tail[i]);
                result.Append(" ");
            }
            return result.ToString();
        }

    }
}