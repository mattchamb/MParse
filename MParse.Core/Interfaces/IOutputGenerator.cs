using System;
using System.Collections.Generic;

namespace MParse.Interfaces
{
    public interface IOutputGenerator : IDisposable
    {
        /// <summary>
        /// Used to pass configuration values to the OutputGenerator.
        /// Called before GenerateOutput.
        /// </summary>
        /// <param name="commandLineArgs"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        void Initialize(string[] commandLineArgs, Dictionary<string, string> settings);

        /// <summary>
        /// Creates the output based on the data in the transition table.
        /// </summary>
        /// <param name="transitionTable"></param>
        /// <param name="tokenStream"></param>
        /// <returns>true if the output generation was successful, otherwise false.</returns>
        bool GenerateOutput(TransitionTable transitionTable, ITokenStream tokenStream);
    }
}
