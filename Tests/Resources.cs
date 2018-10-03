using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal class Resources
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string LoadString(string path)
        {
            var assembly = Assembly.GetCallingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(path))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
