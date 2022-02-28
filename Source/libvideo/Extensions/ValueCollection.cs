using System;
using System.Collections;
using System.Collections.Generic;

namespace VideoLibrary.Extensions
{
    internal partial class Query
    {
        public class ValueCollection : ICollection<string>, IReadOnlyCollection<string>
        {
            private readonly Query _query;

            public ValueCollection(Query query)
            {
                this._query = query;
            }

            public int Count => _query.Count;

            public bool IsReadOnly => true;

            public void Add(string item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(string item)
            {
                for (int i = 0; i < _query.Count; i++)
                {
                    var pair = _query.Pairs[i];
                    if (item == pair.Value)
                        return true;
                }
                return false;
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                for (int i = 0; i < _query.Count; i++)
                    array[arrayIndex++] = _query.Pairs[i].Value;
            }

            public IEnumerator<string> GetEnumerator()
            {
                for (int i = 0; i < _query.Count; i++)
                    yield return _query.Pairs[i].Value;
            }

            public bool Remove(string item)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
