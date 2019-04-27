using System;
using System.Collections;
using System.Collections.Generic;

namespace ConsoleApp
{
    internal class Parameters : IEnumerable<Parameter>
    {
        private List<Parameter> data = new List<Parameter>();

        private Dictionary<char, Parameter> shortOptions = new Dictionary<char, Parameter>();

        private Dictionary<string, Parameter> longOptions = new Dictionary<string, Parameter>();

        public Parameters()
        {
        }

        public void Add(Parameter param)
        {
            data.Add(param);

            if (param.ShortOption != null)
            { shortOptions.Add(param.ShortOption.Value, param); }

            if (param.LongOption != null)
            { longOptions.Add(param.LongOption, param); }
        }

        internal Parameter GetParameter(string longOption)
        {
            longOptions.TryGetValue(longOption, out Parameter result);
            return result;
        }

        internal Parameter GetParameter(char shortOption)
        {
            shortOptions.TryGetValue(shortOption, out Parameter result);
            return result;
        }

        public IEnumerator<Parameter> GetEnumerator() => data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}