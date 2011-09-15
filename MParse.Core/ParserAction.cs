namespace MParse.Core
{
    public enum ParserAction
    {
        Shift,
        Reduce,
        Goto,
        Accept,
        Error
    }
}