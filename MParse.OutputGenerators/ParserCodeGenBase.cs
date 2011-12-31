using System.Collections.Generic;
using System.Text;
using MParse.Core.GrammarElements;
using MParse.Core;
using Microsoft.VisualStudio.TextTemplating;

namespace MParse.OutputGenerators
{
    public abstract class ParserCodeGenBase : TextTransformation
    {
        protected static class Constants
        {
            public const string TerminalBaseClassName = "_Terminal";
            public const string TerminalActionMethodName = "Action";
            public const string ReductionInterfaceName = "IReducer";
            public const string ReduceByMethodName = "ReduceBy";
            public const string TreeNode = "TreeNode";
        }

        protected string GetClassName(Production prod)
        {
            var sb = new StringBuilder();
            sb.Append('_');
            sb.Append(prod.Head.Name);
            foreach (var v in prod.Tail)
            {
                sb.Append('_');
                sb.Append(v.Name);
            }
            return sb.ToString();
        }

        protected IEnumerable<Production> _productions;
        protected IEnumerable<GrammarSymbol> _symbols;
        protected TransitionTable _table;
        protected string _codeNamespace;

        public void Init(string codeNamespace, IEnumerable<Production> productions, IEnumerable<GrammarSymbol> symbols, TransitionTable transTable)
        {
            _productions = productions;
            _symbols = symbols;
            _table = transTable;
            _codeNamespace = codeNamespace;
        }

        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
    }
}
