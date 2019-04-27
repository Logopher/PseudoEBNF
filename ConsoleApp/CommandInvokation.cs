using System.Collections.Generic;

namespace ConsoleApp
{
    internal class CommandInvokation
    {
        public string Path { get; }
        public IReadOnlyDictionary<string, string> Arguments { get; }

        public CommandInvokation(string path, Dictionary<string, string> arguments)
        {
            Path = path;
            Arguments = arguments;
        }
    }
}
