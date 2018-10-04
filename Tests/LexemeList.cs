using PseudoEBNF.Common;
using PseudoEBNF.Lexing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    internal class LexemeList : Compatible, IList<Lexeme>, IEquatable<IEnumerable<Lexeme>>
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        readonly List<Lexeme> data = new List<Lexeme>();

        public LexemeList(Compatible c)
            : base(c)
        {
        }

        public Lexeme this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }

        public int Count => data.Count;

        public bool IsReadOnly => ((IList<Lexeme>)data).IsReadOnly;

        public void Add(Lexeme item)
        {
            if (item.CompatibilityGuid != CompatibilityGuid)
            { throw new Exception(); }

            data.Add(item);
        }

        public void Add(Token token, string text, int index)
        {
            Add(new Lexeme(this, token, text, index));
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool Contains(Lexeme item)
        {
            return data.Contains(item);
        }

        public void CopyTo(Lexeme[] array, int arrayIndex)
        {
            data.CopyTo(array, arrayIndex);
        }

        public int IndexOf(Lexeme item)
        {
            return data.IndexOf(item);
        }

        public void Insert(int index, Lexeme item)
        {
            if (item.CompatibilityGuid != CompatibilityGuid)
            { throw new Exception(); }

            data.Insert(index, item);
        }

        public bool Remove(Lexeme item)
        {
            return data.Remove(item);
        }

        public void RemoveAt(int index)
        {
            data.RemoveAt(index);
        }

        public IEnumerator<Lexeme> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            var list = (obj as IEnumerable<Lexeme>)?.ToList();

            if (list == null)
            { return false; }

            return Equals(list);
        }

        public bool Equals(IEnumerable<Lexeme> other)
        {
            var list = other as IList<Lexeme> ?? other.ToList();

            if (list is LexemeList lexemeList && lexemeList.CompatibilityGuid != CompatibilityGuid)
            { throw new Exception(); }

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