using PseudoEBNF.Lexing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    internal class LexemeList : IList<Lexeme>, IEquatable<IEnumerable<Lexeme>>
    {
        readonly List<Lexeme> data = new List<Lexeme>();

        public Lexeme this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }

        public int Count => data.Count;

        public bool IsReadOnly => ((IList<Lexeme>)data).IsReadOnly;

        public void Add(Lexeme item) => data.Add(item);

        public void Add(IToken token, string text, int index) => data.Add(new Lexeme(token, text, index));

        public void Clear() => data.Clear();

        public bool Contains(Lexeme item) => data.Contains(item);

        public void CopyTo(Lexeme[] array, int arrayIndex) => data.CopyTo(array, arrayIndex);

        public int IndexOf(Lexeme item) => data.IndexOf(item);

        public void Insert(int index, Lexeme item) => data.Insert(index, item);

        public bool Remove(Lexeme item) => data.Remove(item);

        public void RemoveAt(int index) => data.RemoveAt(index);

        public IEnumerator<Lexeme> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override bool Equals(object obj)
        {
            var list = (obj as IEnumerable<Lexeme>)?.ToList();

            if(list == null)
            { return false; }

            return Equals(list);
        }

        public bool Equals(IEnumerable<Lexeme> other)
        {
            var list = other as IList<Lexeme> ?? other.ToList();

            return Count == list.Count
                && data
                .Select((lexeme, i) =>
                {
                    var result = lexeme.Equals(list[i]);
                    return result;
                })
                .All(b => b);
        }

        public static bool operator ==(LexemeList a, LexemeList b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(LexemeList a, LexemeList b)
        {
            return !(a == b);
        }
    }
}