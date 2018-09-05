namespace PseudoEBNF.Common
{
    public class Match<T>
    {
        public T Result { get; }

        public bool Success { get; }

        public Match(T result, bool success)
        {
            Result = result;
            Success = success;
        }
    }
}