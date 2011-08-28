using System;
using System.Collections.Generic;

namespace MParse
{
    public class Production
    {
        public int Head { get; private set; }
        public int[] Tail { get; private set; }

        public Production(int head, int[] tail)
        {
            Head = head;
            Tail = tail;
        }

        public IList<Item> GetItems()
        {
            throw new NotImplementedException();
        }

        public int Length
        {
            get { return Tail.Length; }
        }
    }

  
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

        public Item AdvanceDot()
        {
            if(!CanAdvanceDot)
                throw new InvalidOperationException("Cannot advance the dot any more in this production.");
            return new Item(CurrentPosition + 1, ItemProduction);
        }
    }

    public interface IGrammarProvider
    {
        IList<Production> GetProductions();
        bool IsTerminal(int token);
        IList<Item> GetClosure(IList<Item> items);
        IList<int> FirstSet(Production production);
        IList<int> FollowSet(Production production);
    }
}
