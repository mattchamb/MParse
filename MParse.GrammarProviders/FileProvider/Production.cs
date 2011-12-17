using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MParse.GrammarProviders.FileProvider
{
    public class Production
    {
        public string ProductionHead { get; private set; }
        public IList<string> ProductionTail { get; private set; }

        public Production(string productionHead, IList<string> productionTail )
        {
            ProductionHead = productionHead;
            ProductionTail = productionTail;
        }
    }
}
