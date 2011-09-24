using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using MParse.Core.GrammarElements;
using MParse.Core.Interfaces;

namespace MParse.GrammarProviders
{
    public class XmlGrammarProvider : IGrammarProvider
    {
        private Production[] _productions;
        private GrammarSymbol[] _grammarSymbols;

        private string _input;

        public void SetInput(string inputData)
        {
            _input = inputData;
        }

        public Production[] GetProductions()
        {
            if(_productions == null)
                ParseInputData();
            return _productions;
        }

        public GrammarSymbol[] GetGrammarSymbols()
        {
            if (_grammarSymbols == null)
                ParseInputData();
            return _grammarSymbols;
        }

        public Item GetAugmentedState()
        {
            if (_productions == null)
                ParseInputData();
            return new Item(0, _productions[0]);
        }

        private void ParseInputData()
        {
            var xs = new XmlSerializer(typeof(XmlGrammar));
            var grammarData = (XmlGrammar)xs.Deserialize(XElement.Parse(_input).CreateReader());
        }

       

    }
}
