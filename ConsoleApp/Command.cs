namespace ConsoleApp
{
    internal class Command
    {
        public string Name { get; }
        public Parameters Parameters { get; }

        public Command(string commandName, Parameters parameters)
        {
            Name = commandName;
            Parameters = parameters;
        }
    }
}
