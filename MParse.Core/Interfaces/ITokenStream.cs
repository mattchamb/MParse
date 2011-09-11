using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MParse.GrammarElements;

namespace MParse.Interfaces
{
    public interface ITokenStream : IEnumerator<Terminal>, IEnumerable<Terminal>
    {

    }
}
