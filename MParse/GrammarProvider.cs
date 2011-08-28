using System.Collections.Generic;
using System.Linq;

namespace MParse
{
    public abstract class GrammarProvider
    {
        public abstract Production[] GetProductions();
        public abstract bool IsTerminal(int token);

        public virtual List<Item> GetClosure(List<Item> items)
        {
            var productions = GetProductions();

            var closure = new List<Item>();
            closure.AddRange(items);
            int initialSize;
            do
            {
                initialSize = closure.Count;
                for (int j = 0; j < closure.Count; j++)
                {
                    var item = closure[j];
                    foreach (var prod in productions.Where(h => h.Head == item.NextToken))
                    {
                        var newItem = new Item(prod);
                        if (!closure.Contains(newItem))
                            closure.Add(newItem);
                    }
               
                }

            } while (initialSize < closure.Count);

            return closure;
        }

        public virtual List<int> FirstSet(Production production)
        {
            var productions = GetProductions();
            return null;
        }

        public virtual List<int> FollowSet(Production production)
        {
            var productions = GetProductions();
            return null;
        }
    }
}