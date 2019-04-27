namespace ConsoleApp
{
    internal class Parameter
    {
        public string Name { get; set; }

        public char? ShortOption { get; set; }

        public string LongOption { get; set; }

        public bool IsPositional { get; set; } = true;

        public bool IsRequired { get; set; } = true;
    }
}