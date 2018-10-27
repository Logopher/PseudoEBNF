using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Tests
{
    internal class Resources
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string LoadString(string path)
        {
            var assembly = Assembly.GetCallingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(path))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
