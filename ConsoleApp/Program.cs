using System;
using System.Collections.Generic;
using System.Linq;
using PseudoEBNF;
using PseudoEBNF.Parsing.Nodes;
using PseudoEBNF.Semantics;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var commandLine = Environment.CommandLine;

            var parser = new CommandParser(new Parameters
            {
                new Parameter{ Name = "path" }
            });

            parser.Lock();

            var invokation = parser.Parse(commandLine);

            var path = invokation.Arguments["path"];

            
        }
    }
}
