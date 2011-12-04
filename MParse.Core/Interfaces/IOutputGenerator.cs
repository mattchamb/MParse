using System;
using System.Collections.Generic;

namespace MParse.Core.Interfaces
{
    public interface IOutputGenerator
    {
        /// <summary>
        /// Creates the output based on the data in the transition table.
        /// </summary>
        /// <param name="transitionTable"></param>
        /// <param name="tokenStream"></param>
        /// <returns>true if the output generation was successful, otherwise false.</returns>
        bool GenerateOutput(TransitionTable transitionTable, ITokenStream tokenStream);
    }
}
