using System.Collections;
using System.Collections.Generic;
using MParse.Core.GrammarElements;
using MParse.Core.Interfaces;

namespace MParse.TokenProviders
{
    public class TestTokenStream : ITokenStream
    {
        private readonly IEnumerator<Terminal> _enumerator;

        private static readonly IEnumerable<Terminal> Tokens = new[]
                                                    {
                                                        new Terminal(6, "Id"),
                                                        new Terminal(5, "*"),
                                                        new Terminal(6, "Id"),
                                                        new EndOfStream()
                                                    };

        public TestTokenStream()
        {
            _enumerator = Tokens.GetEnumerator();
        }

        public void Dispose()
        {

        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        /// <filterpriority>2</filterpriority>
        public void Reset()
        {
            _enumerator.Reset();
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>
        /// The element in the collection at the current position of the enumerator.
        /// </returns>
        public Terminal Current
        {
            get { return _enumerator.Current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public IEnumerator<Terminal> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
