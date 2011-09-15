using System.Collections.Generic;
using MParse.Core.GrammarElements;

namespace MParse.Core.Interfaces
{
    public interface ITokenStream : IEnumerator<Terminal>, IEnumerable<Terminal>
    {

    }
}
