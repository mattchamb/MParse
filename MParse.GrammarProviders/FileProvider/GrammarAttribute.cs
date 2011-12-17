namespace MParse.GrammarProviders.FileProvider
{
    public class GrammarAttribute
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public GrammarAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
