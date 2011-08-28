using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MParse
{
    public class Production
    {
        public int Head { get; private set; }
        public IList<int> Tail { get; private set; }

        public Production(int head, IList<int> tail)
        {
            Head = head;
            Tail = tail;
        }

        public IList<Item> GetItems()
        {
            throw new NotImplementedException();
        }
    }

    public class Item
    {
        public int CurrentPosition { get; private set; }

        public Production Production { get; private set; }

        public Item(Production production) : this(0, production) {}

        public Item(int currentPosition, Production production)
        {
            CurrentPosition = currentPosition;
            Production = production;
        }

        public Item AdvanceDot()
        {
            throw new NotImplementedException();
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
