using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

namespace AD.Common.DataStructures
{
	public interface IIndexedSingle<TKey, TValue>
	{
		bool TryGetIndex(TValue value, out int index);
		bool TryGetIndex(TKey key, out int index);
		TValue this[TKey key] { get; }
		TKey GetKey(TValue value);
		IEnumerable<TKey> Keys();
	}

	public interface IIndexedEnumerable<TKey, TValue>
	{
		ISet<int> GetIndexes(TKey key);
		IEnumerable<TValue> this[TKey key] { get; }
		IEnumerable<KeyValuePair<TKey, int>> Count();
		int Count(TKey key);
	}

    public interface ISortedIndexedEnumerable<TKey, TValue> : IIndexedEnumerable<TKey, TValue>
    {
        IList<TKey> Keys { get; }
    }

    public interface IIndexedSortedEnumerable<TKey, TValue> : IIndexedEnumerable<TKey, TValue>
    {
        IEnumerable<TValue> Range(TValue from, TValue to);
    }

    public enum AddIndexReference : short
	{
		None = 0,
		Add = 1,
		Release = -1
	}

	public interface IIndex<T>
	{
		string Name { get; }
		void ReIndex(IList<T> values);
		void ApplyValue(T value, int index);
		void RemoveValue(T value, int index);
		int AddReference(AddIndexReference iref, int ownerHashCode);
		IEnumerable<T> Values();
	}

    public interface IObservableCollection<TValue> : IList<TValue>, IList
    {
        event Action<TValue, TValue, int> CollectionChanged;
    }

	public class StackedList<T> : IList<T>, IList
	{
		#region Constructors

		public StackedList()
		{
			m_stack = new Stack<int>();
			m_values = new List<T>();
		}

		public StackedList(int capasity)
		{
			m_stack = new Stack<int>();
			m_values = new List<T>(capasity);
		}

		public StackedList(IEnumerable<T> items)
		{
			m_stack = new Stack<int>();
			m_values = new List<T>(items);
		}

		#endregion

		#region IList

		public int Count
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get { return m_values.Count; }
		}

		public T this[int index]
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get { return m_values[index]; }
			set
			{
				if (index < 0 || index >= m_values.Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				SetItem(index, value);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public void Add(T item)
		{
			SetItem(Next, item);
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		protected int AddNext(T item)
		{
			int result = Next;
			SetItem(result, item);
			return result;
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public void Clear()
		{
			ClearItems();
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public void CopyTo(T[] array, int index)
		{
			m_values.CopyTo(array, index);
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public bool Contains(T item)
		{
			throw new NotSupportedException();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return m_values.OfType<T>().GetEnumerator();
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public int IndexOf(T item)
		{
			throw new NotSupportedException();
		}

		public void Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= m_values.Count)
			{
				//ThrowHelper.ThrowArgumentOutOfRangeException();
				throw new ArgumentOutOfRangeException();
			}

			RemoveItem(index);
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		protected virtual void ClearItems()
		{
			m_stack = new Stack<int>();
			m_values = new List<T>();
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		protected virtual void RemoveItem(int index)
		{
			m_values[index] = default(T);
			m_stack.Push(index);
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		protected virtual void SetItem(int index, T item)
		{
			m_values[index] = item;
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) m_values).GetEnumerator();
		}

		bool ICollection.IsSynchronized
		{
			get { return true; }
		}

		object ICollection.SyncRoot
		{
			get
			{
				if (m_syncRoot == null)
				{
					System.Threading.Interlocked.CompareExchange<object>(ref m_syncRoot, new Object(), null);
				}
				return m_syncRoot;
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException();
			}

			if (array.Rank != 1)
			{
				throw new ArgumentException();
			}

			if (array.GetLowerBound(0) != 0)
			{
				throw new ArgumentException();
			}

			if (index < 0)
			{
				throw new ArgumentOutOfRangeException();
			}

			if (array.Length - index < Count)
			{
				throw new ArgumentOutOfRangeException();
			}

			T[] tArray = array as T[];
			if (tArray != null)
			{
				m_values.CopyTo(tArray, index);
			}
			else
			{
				// 
				// Catch the obvious case assignment will fail.
				// We can found all possible problems by doing the check though. 
				// For example, if the element type of the Array is derived from T, 
				// we can't figure out if we can successfully copy the element beforehand.
				// 
				Type targetType = array.GetType().GetElementType();
				Type sourceType = typeof (T);
				if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
				{
					throw new ArgumentException();
				}

				// 
				// We can't cast array of value type to object[], so we don't support
				// widening of primitive types here. 
				//
				object[] objects = array as object[];
				if (objects == null)
				{
					throw new ArgumentNullException();
				}

				int count = m_values.Count;
				for (int i = 0; i < count; i++)
				{
					objects[index++] = m_values[i];
				}
			}
		}

		object IList.this[int index]
		{

			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get { return m_values[index]; }

			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			set
			{
				this[index] = (T) value;
			}
		}

		bool IList.IsReadOnly
		{
			get { return false; }
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		int IList.Add(object value)
		{
			return AddNext((T) value);
		}

		bool IList.Contains(object value)
		{
			if (IsCompatibleObject(value))
			{
				return Contains((T) value);
			}
			return false;
		}

		int IList.IndexOf(object value)
		{
			if (IsCompatibleObject(value))
			{
				return IndexOf((T) value);
			}
			return -1;
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		void IList.Remove(object value)
		{
			if (IsCompatibleObject(value))
			{
				Remove((T) value);
			}
		}

		private static bool IsCompatibleObject(object value)
		{
			// Non-null values are fine.  Only accept nulls if T is a class or Nullable<u>.
			// Note that default(T) is not equal to null for value types except when T is Nullable<u>. 
			return ((value is T) || (value == null && default(T) == null));
		}

		#endregion

		public int Next
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				if (m_stack.Count == 0)
				{
					m_values.Add(default(T));
					return m_values.Count - 1;
				}
				else
					return m_stack.Pop();
			}
		}

		private Stack<int> m_stack;
		private IList<T> m_values;
		private Object m_syncRoot;
	}

    #region EmptySet
    public static class EmptySet
    {
        static EmptySet()
        {
            _instance = new InternalEmptySet<int>();
        }

        static ISet<int> _instance;
        public static ISet<int> Instance { get { return _instance; } }

        class InternalEmptySet<T> : ISet<T>
        {
            public bool Add(T item)
            {
                throw new NotImplementedException();
            }

            public void ExceptWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public void IntersectWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsProperSubsetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsSubsetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool IsSupersetOf(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool Overlaps(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public bool SetEquals(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public void SymmetricExceptWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            public void UnionWith(IEnumerable<T> other)
            {
                throw new NotImplementedException();
            }

            void ICollection<T>.Add(T item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(T item)
            {
                return false;
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { return 0; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(T item)
            {
                return false;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return Enumerable.Empty<T>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
    #endregion

    public class IndexedList<TKey, TValue> : IIndex<TValue>, IIndexedEnumerable<TKey, TValue>
	{
		public string Name { get; protected set; }

		public int ReferenceCount
		{
			get { lock (m_references) return m_references.Count; }
		}

		public IndexedList(string name, Func<TValue, TKey> getIndexKey)
		{
			Name = name;
			GetIndexKey = getIndexKey;
			m_references = new HashSet<int>();
		}

		public void ReIndex(IList<TValue> values)
		{
			m_keys = new Dictionary<TKey, HashSet<int>>();
			m_values = values;
			lock (((IList) m_values).SyncRoot) ReIndexInternal();
		}

		public void ApplyValue(TValue value, int index)
		{
			TKey key;
			HashSet<int> indexSet;
			if (!m_keys.TryGetValue(key = GetIndexKey(value), out indexSet))
			{
				indexSet = new HashSet<int>();
				m_keys.Add(key, indexSet);
			}
			indexSet.Add(index);
		}

		public void RemoveValue(TValue value, int index)
		{

			HashSet<int> indexSet;
			if (m_keys.TryGetValue(GetIndexKey(value), out indexSet))
			{
				indexSet.Remove(index);
			}
		}

		private void ReIndexInternal()
		{
			for (int i = 0; i < m_values.Count; i++)
			{
				TValue value = m_values[i];
				if (value == null) continue;

				TKey key = GetIndexKey(value);

				HashSet<int> indexSet;
				if (!m_keys.TryGetValue(key, out indexSet))
				{
					indexSet = new HashSet<int>();
					m_keys.Add(key, indexSet);
				}
				indexSet.Add(i);
			}
		}

		public int AddReference(AddIndexReference iref, int ownerHashCode)
		{
			lock (m_references)
			{
				switch (iref)
				{
					case AddIndexReference.Release:
						m_references.Remove(ownerHashCode);
						break;
					default:
						m_references.Add(ownerHashCode);
						break;
				}

				return ReferenceCount;
			}
		}

		public IEnumerable<KeyValuePair<TKey, int>> Count()
		{
			lock (((IList) m_values).SyncRoot)
			{
				foreach (var key in m_keys)
				{
					yield return new KeyValuePair<TKey, int>(key.Key, key.Value.Count);
				}
			}
		}

		public int Count(TKey key)
		{
			lock (((IList) m_values).SyncRoot)
			{
				HashSet<int> indexSet;
				if (m_keys.TryGetValue(key, out indexSet))
					return indexSet.Count;
				return 0;
			}
		}

		public IEnumerable<TValue> this[TKey key]
		{
			get
			{
				lock (((IList) m_values).SyncRoot)
				{
					HashSet<int> indexSet;
					if (m_keys.TryGetValue(key, out indexSet))
					{
						foreach (TValue value in indexSet.Select(i => m_values[i]))
							yield return value;
					}
					else
						yield break;
				}
			}
		}
                
		public ISet<int> GetIndexes(TKey key)
		{
			lock (((IList) m_values).SyncRoot)
			{
				HashSet<int> result = null;
				if (m_keys.TryGetValue(key, out result))
					return result;
                return EmptySet.Instance;
			}
		}

		public IEnumerable<TValue> Values()
		{
			lock (((IList) m_values).SyncRoot)
			{
				foreach (var idx in m_keys.SelectMany(k => k.Value))
				{
					yield return m_values[idx];
				}
			}
		}

		private Dictionary<TKey, HashSet<int>> m_keys;
		private Func<TValue, TKey> GetIndexKey;
		private IList<TValue> m_values;
		private ISet<int> m_references;
	}

    public class IndexedMultiList<TKey, TValue> : IIndex<TValue>, IIndexedEnumerable<TKey, TValue>
    {
        public string Name { get; protected set; }

        public int ReferenceCount
        {
            get { lock (m_references) return m_references.Count; }
        }

        public IndexedMultiList(string name, Func<TValue, IEnumerable<TKey>> getIndexKeys)
        {
            Name = name;
            GetIndexKeys = getIndexKeys;
            m_references = new HashSet<int>();
        }

        public void ReIndex(IList<TValue> values)
        {
            m_keys = new Dictionary<TKey, HashSet<int>>();
            m_values = values;
            lock (((IList)m_values).SyncRoot) ReIndexInternal();
        }

        public void ApplyValue(TValue value, int index)
        {
            foreach (var key in GetIndexKeys(value))
            {
                HashSet<int> indexSet;
                if (!m_keys.TryGetValue(key, out indexSet))
                {
                    indexSet = new HashSet<int>();
                    m_keys.Add(key, indexSet);
                }
                indexSet.Add(index);
            }
        }

        public void RemoveValue(TValue value, int index)
        {
            foreach (var key in GetIndexKeys(value))
            {
                HashSet<int> indexSet;
                if (m_keys.TryGetValue(key, out indexSet))
                {
                    indexSet.Remove(index);
                }
            }
        }

        private void ReIndexInternal()
        {
            for (int i = 0; i < m_values.Count; i++)
            {
                TValue value = m_values[i];
                if (value == null) continue;

                foreach (var key in GetIndexKeys(value))
                {
                    HashSet<int> indexSet;
                    if (!m_keys.TryGetValue(key, out indexSet))
                    {
                        indexSet = new HashSet<int>();
                        m_keys.Add(key, indexSet);
                    }
                    indexSet.Add(i);
                }
            }
        }

        public int AddReference(AddIndexReference iref, int ownerHashCode)
        {
            lock (m_references)
            {
                switch (iref)
                {
                    case AddIndexReference.Release:
                        m_references.Remove(ownerHashCode);
                        break;
                    default:
                        m_references.Add(ownerHashCode);
                        break;
                }

                return ReferenceCount;
            }
        }

        public IEnumerable<KeyValuePair<TKey, int>> Count()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var key in m_keys)
                {
                    yield return new KeyValuePair<TKey, int>(key.Key, key.Value.Count);
                }
            }
        }

        public int Count(TKey key)
        {
            lock (((IList)m_values).SyncRoot)
            {
                HashSet<int> indexSet;
                if (m_keys.TryGetValue(key, out indexSet))
                    return indexSet.Count;
                return 0;
            }
        }

        public IEnumerable<TValue> this[TKey key]
        {
            get
            {
                lock (((IList)m_values).SyncRoot)
                {
                    HashSet<int> indexSet;
                    if (m_keys.TryGetValue(key, out indexSet))
                    {
                        foreach (TValue value in indexSet.Select(i => m_values[i]))
                            yield return value;
                    }
                    else
                        yield break;
                }
            }
        }

        public ISet<int> GetIndexes(TKey key)
        {
            lock (((IList)m_values).SyncRoot)
            {
                HashSet<int> result = null;
                if (m_keys.TryGetValue(key, out result))
                    return result;
                return EmptySet.Instance;
            }
        }

        public IEnumerable<TValue> Values()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var idx in m_keys.SelectMany(k => k.Value))
                {
                    yield return m_values[idx];
                }
            }
        }

        private Dictionary<TKey, HashSet<int>> m_keys;
        private Func<TValue, IEnumerable<TKey>> GetIndexKeys;
        private IList<TValue> m_values;
        private ISet<int> m_references;
    }

    public class SortedIndexedList<TKey, TValue> : IIndex<TValue>, ISortedIndexedEnumerable<TKey, TValue>
    {
        public string Name { get; protected set; }

        public int ReferenceCount
        {
            get { lock (m_references) return m_references.Count; }
        }

        public SortedIndexedList(string name, Func<TValue, TKey> getIndexKey, Func<TKey, TKey, int> compare)
        {
            Name = name;
            GetIndexKey = getIndexKey;
            m_comparer = new ValueComparer<TKey>(compare);
            m_references = new HashSet<int>();
        }

        public void ReIndex(IList<TValue> values)
        {
            m_keys = new SortedList<TKey, HashSet<int>>(m_comparer);
            m_values = values;
            lock (((IList)m_values).SyncRoot) ReIndexInternal();
        }

        public void ApplyValue(TValue value, int index)
        {
            TKey key;
            HashSet<int> indexSet;
            if (!m_keys.TryGetValue(key = GetIndexKey(value), out indexSet))
            {
                indexSet = new HashSet<int>();
                m_keys[key] = indexSet;
            }
            indexSet.Add(index);
        }

        public void RemoveValue(TValue value, int index)
        {
            TKey key;
            HashSet<int> indexSet;
            if (m_keys.TryGetValue(key = GetIndexKey(value), out indexSet))
            {
                indexSet.Remove(index);
            }
        }

        private void ReIndexInternal()
        {
            for (int i = 0; i < m_values.Count; i++)
            {
                TValue value = m_values[i];
                if (value == null) continue;

                TKey key = GetIndexKey(value);

                HashSet<int> indexSet;
                if (!m_keys.TryGetValue(key, out indexSet))
                {
                    indexSet = new HashSet<int>();
                    m_keys[key] = indexSet;
                }
                indexSet.Add(i);
            }
        }

        public int AddReference(AddIndexReference iref, int ownerHashCode)
        {
            lock (m_references)
            {
                switch (iref)
                {
                    case AddIndexReference.Release:
                        m_references.Remove(ownerHashCode);
                        break;
                    default:
                        m_references.Add(ownerHashCode);
                        break;
                }

                return ReferenceCount;
            }
        }

        public IEnumerable<KeyValuePair<TKey, int>> Count()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var key in m_keys)
                {
                    if (key.Value != null)
                        yield return new KeyValuePair<TKey, int>(key.Key, key.Value.Count);
                }
            }
        }

        public int Count(TKey key)
        {
            lock (((IList)m_values).SyncRoot)
            {
                HashSet<int> indexSet;
                if (m_keys.TryGetValue(key, out indexSet))
                    return indexSet.Count;
                return 0;
            }
        }

        public IEnumerable<TValue> this[TKey key]
        {
            get
            {
                lock (((IList)m_values).SyncRoot)
                {
                    HashSet<int> indexSet;
                    if (m_keys.TryGetValue(key, out indexSet))
                    {
                        foreach (TValue value in indexSet.Select(i => m_values[i]))
                            yield return value;
                    }
                    else
                        yield break;
                }
            }
        }

        public ISet<int> GetIndexes(TKey key)
        {
            lock (((IList)m_values).SyncRoot)
            {
                HashSet<int> result = null;
                if (m_keys.TryGetValue(key, out result))
                    return result;
                return EmptySet.Instance;
            }
        }

        public IEnumerable<TValue> Values()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var idx in m_keys.SelectMany(k => k.Value))
                {
                    yield return m_values[idx];
                }
            }
        }

        public IList<TKey> Keys
        {
            get
            {
                return m_keys.Keys;
            }
        }

        class ValueComparer<T> : IComparer<T>
        {
            public ValueComparer(Func<T, T, int> cmp)
            {
                m_cmp = cmp;
            }
            public int Compare(T x, T y)
            {
                return m_cmp(x, y);
            }
            private Func<T, T, int> m_cmp;
        }

        private SortedList<TKey, HashSet<int>> m_keys;
        private Func<TValue, TKey> GetIndexKey;
        private ValueComparer<TKey> m_comparer;
        private IList<TValue> m_values;
        private ISet<int> m_references;
    }

    public interface ISortedValue<TValue>
    {
        TValue MinValue { get; }
        TValue MaxValue { get; }

        IEnumerable<TValue> this[int form, int to] { get; }
    }

    public interface ISortedIndex<TKey, TValue> : IIndex<TValue>
    {
        ISortedValue<TValue> this[TKey key] { get; }     
    }

    public class IndexedSortedList<TKey, TValue> : ISortedIndex<TKey, TValue>, IIndexedEnumerable<TKey, TValue>
    {
        public string Name { get; protected set; }

        public int ReferenceCount
        {
            get { lock (m_references) return m_references.Count; }
        }

        public IndexedSortedList(string name, Func<TValue, TKey> getIndexKey, Func<TValue, TValue, int> compare)
        {
            Name = name;
            GetIndexKey = getIndexKey;
            m_comparer = new ValueComparer(compare);
            m_references = new HashSet<int>();
        }

        public void ReIndex(IList<TValue> values)
        {
            m_comparer.Initialize(values);
            m_keys = new Dictionary<TKey, SortedSet<int>>();
            m_values = values;
            lock (((IList)m_values).SyncRoot) ReIndexInternal();
        }

        public void ApplyValue(TValue value, int index)
        {
            TKey key;
            SortedSet<int> indexSet;
            if (!m_keys.TryGetValue(key = GetIndexKey(value), out indexSet))
            {
                indexSet = new SortedSet<int>(m_comparer);
                m_keys[key] = indexSet;
            }
            indexSet.Add(index);
        }

        public void RemoveValue(TValue value, int index)
        {
            TKey key;
            SortedSet<int> indexSet;
            if (m_keys.TryGetValue(key = GetIndexKey(value), out indexSet))
            {
                m_comparer.LastRemoveValue = value;
                indexSet.Remove(index);
            }
        }

        private void ReIndexInternal()
        {
            for (int i = 0; i < m_values.Count; i++)
            {
                TValue value = m_values[i];
                if (value == null) continue;

                TKey key = GetIndexKey(value);

                SortedSet<int> indexSet;
                if (!m_keys.TryGetValue(key, out indexSet))
                {
                    indexSet = new SortedSet<int>(m_comparer);
                    m_keys[key] = indexSet;
                }
                indexSet.Add(i);
            }
        }

        public int AddReference(AddIndexReference iref, int ownerHashCode)
        {
            lock (m_references)
            {
                switch (iref)
                {
                    case AddIndexReference.Release:
                        m_references.Remove(ownerHashCode);
                        break;
                    default:
                        m_references.Add(ownerHashCode);
                        break;
                }

                return ReferenceCount;
            }
        }

        public IEnumerable<KeyValuePair<TKey, int>> Count()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var key in m_keys)
                {
                    yield return new KeyValuePair<TKey, int>(key.Key, (key.Value ?? EmptySet.Instance).Count);
                }
            }
        }

        public int Count(TKey key)
        {
            lock (((IList)m_values).SyncRoot)
            {
                SortedSet<int> indexSet;
                if (m_keys.TryGetValue(key, out indexSet))
                    return indexSet.Count;
                return 0;
            }
        }

        public IEnumerable<TValue> this[TKey key]
        {
            get
            {
                lock (((IList)m_values).SyncRoot)
                {
                    SortedSet<int> indexSet;
                    if (m_keys.TryGetValue(key, out indexSet))
                    {
                        foreach (TValue value in indexSet.Select(i => m_values[i]))
                            yield return value;
                    }
                    else
                        yield break;
                }
            }
        }

        public ISet<int> GetIndexes(TKey key)
        {
            lock (((IList)m_values).SyncRoot)
            {
                SortedSet<int> result = null;
                if (m_keys.TryGetValue(key, out result))
                    return result;
                return EmptySet.Instance;
            }
        }

        public IEnumerable<TValue> Values()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var idx in m_keys.SelectMany(k => k.Value))
                {
                    yield return m_values[idx];
                }
            }
        }

        ISortedValue<TValue> ISortedIndex<TKey, TValue>.this[TKey key] 
        {
            get
            {
                SortedSet<int> values;
                if (m_keys.TryGetValue(key, out values) && values.Count > 0)
                    return new SortedValue(m_values[values.Min], m_values[values.Max], (f, t) => GetRange(key, f, t));

                return new SortedValue(default(TValue), default(TValue), null);
            }
        }

        IEnumerable<TValue> GetRange(TKey key, int from, int to)
        {
            SortedSet<int> values;
            if (m_keys.TryGetValue(key, out values) && values.Count > 0)
            {
                foreach (var i in values.GetViewBetween(from, to))
                {
                    yield return m_values[i];
                }
            }
        }

        class SortedValue : ISortedValue<TValue>
        {
            public SortedValue(TValue minValue, TValue maxValue, Func<int, int, IEnumerable<TValue>> valuesGetter)
            {
                MaxValue = maxValue; MinValue = minValue; ValuesGetter = valuesGetter;
            }
            public TValue MaxValue { get; protected set; }
            public TValue MinValue { get; protected set; }

            Func<int, int, IEnumerable<TValue>> ValuesGetter;
            public IEnumerable<TValue> this[int from, int to]
            {
                get
                {
                    return (ValuesGetter == null) ? Enumerable.Empty<TValue>() : ValuesGetter(from, to);
                }
            }
        }

        class ValueComparer : IComparer<int>
        {
            public ValueComparer(Func<TValue, TValue, int> cmp, IList<TValue> values = null)
            {
                m_values = values;
                m_cmp = cmp;
            }
            public void Initialize(IList<TValue> values)
            {
                m_values = values;
            }
            public int Compare(int x, int y)
            {
                var v1 = m_values[x]; var v2 = m_values[y];
                return m_cmp(v1 == null ? LastRemoveValue : v1, v2 == null ? LastRemoveValue : v2);
            }

            public TValue LastRemoveValue { get; set; }
            private IList<TValue> m_values;
            private Func<TValue, TValue, int> m_cmp;
        }

        private Dictionary<TKey, SortedSet<int>> m_keys;
        private Func<TValue, TKey> GetIndexKey;
        private ValueComparer m_comparer;
        private IList<TValue> m_values;
        private ISet<int> m_references;
    }

	public class PartiallyIndexedFilteredList<TKey, TValue> : IIndex<TValue>, IIndexedEnumerable<TKey, TValue>
	{
		public string Name { get; protected set; }

		public int ReferenceCount
		{
			get { lock (m_references) return m_references.Count; }
		}

        public PartiallyIndexedFilteredList(string name, Func<TValue, TKey[]> getIndexKey, Func<TValue, bool> checkFilterCondition)
		{
			Name = name;
			GetIndexKey = getIndexKey;
			CheckFilterCondition = checkFilterCondition;
			m_references = new HashSet<int>();
		}

		public void ReIndex(IList<TValue> values)
		{
			m_keys = new Dictionary<TKey, HashSet<int>>();
			m_values = values;
			lock (((IList) m_values).SyncRoot) ReIndexInternal();
		}

		public void ApplyValue(TValue value, int index)
		{
            var keys = GetIndexKey(value);
            for (int i = 0; i < keys.Length; i++)
            {
                HashSet<int> indexSet;
                if (!m_keys.TryGetValue(keys[i], out indexSet))
                {
                    if (!CheckFilterCondition(value))
                        return;

                    indexSet = new HashSet<int>();
                    m_keys.Add(keys[i], indexSet);
                    indexSet.Add(index);
                }
                else
                {
                    if (CheckFilterCondition(value))
                        indexSet.Add(index);
                    else
                        indexSet.Remove(index);
                }
            }
		}

		public void RemoveValue(TValue value, int index)
		{
            var keys = GetIndexKey(value);
            for (int i = 0; i < keys.Length; i++)
            {
                HashSet<int> indexSet;
                if (m_keys.TryGetValue(keys[i], out indexSet))
                {
                    indexSet.Remove(index);
                }
            }
		}

		private void ReIndexInternal()
		{
			for (int i = 0; i < m_values.Count; i++)
			{
				TValue value = m_values[i];
                if (value == null || !CheckFilterCondition(value)) continue;

                var keys = GetIndexKey(value);
                for (int j = 0; j < keys.Length; j++)
                {
                    HashSet<int> indexSet;
                    if (!m_keys.TryGetValue(keys[j], out indexSet))
                    {
                        indexSet = new HashSet<int>();
                        m_keys.Add(keys[j], indexSet);
                    }
                    indexSet.Add(i);
                }
			}
		}

		public int AddReference(AddIndexReference iref, int ownerHashCode)
		{
			lock (m_references)
			{
				switch (iref)
				{
					case AddIndexReference.Release:
						m_references.Remove(ownerHashCode);
						break;
					default:
						m_references.Add(ownerHashCode);
						break;
				}
				return ReferenceCount;
			}
		}

		public IEnumerable<KeyValuePair<TKey, int>> Count()
		{
			lock (((IList) m_values).SyncRoot)
			{
				foreach (var cnt in m_keys.Select(k => new KeyValuePair<TKey, int>(k.Key, k.Value.Count)))
					yield return cnt;
			}
		}

		public int Count(TKey key)
		{
			lock (((IList) m_values).SyncRoot)
			{
				HashSet<int> indexSet;
				if (m_keys.TryGetValue(key, out indexSet))
					return indexSet.Count;
				return 0;
			}
		}

		public IEnumerable<TValue> this[TKey key]
		{
			get
			{
				lock (((IList) m_values).SyncRoot)
				{
					HashSet<int> indexSet;
					if (m_keys.TryGetValue(key, out indexSet))
					{
						foreach (TValue value in indexSet.Select(i => m_values[i]))
							yield return value;
					}
					else
						yield break;
				}
			}
		}

		public IEnumerable<TValue> Values()
		{
			lock (((IList) m_values).SyncRoot)
			{
				foreach (var idx in m_keys.SelectMany((k) => k.Value))
				{
					yield return m_values[idx];
				}
			}
		}

		public ISet<int> GetIndexes(TKey key)
		{
			lock (((IList) m_values).SyncRoot)
			{
				HashSet<int> result = null;
				if (m_keys.TryGetValue(key, out result))
					return result;
                return EmptySet.Instance;
			}
		}

		private Dictionary<TKey, HashSet<int>> m_keys;
		private Func<TValue, TKey[]> GetIndexKey;
		private Func<TValue, bool> CheckFilterCondition;
		private IList<TValue> m_values;
		private ISet<int> m_references;
	}

    public class IndexedFilteredList<TKey, TValue> : IIndex<TValue>, IIndexedEnumerable<TKey, TValue>
    {
        public string Name { get; protected set; }

        public int ReferenceCount
        {
            get { lock (m_references) return m_references.Count; }
        }

        public IndexedFilteredList(string name, Func<TValue, TKey> getIndexKey, Func<TValue, bool> checkFilterCondition)
        {
            Name = name;
            GetIndexKey = getIndexKey;
            CheckFilterCondition = checkFilterCondition;
            m_references = new HashSet<int>();
        }

        public void ReIndex(IList<TValue> values)
        {
            m_keys = new Dictionary<TKey, HashSet<int>>();
            m_values = values;
            lock (((IList)m_values).SyncRoot) ReIndexInternal();
        }

        public void ApplyValue(TValue value, int index)
        {
            var key = GetIndexKey(value);
            HashSet<int> indexSet;
            if (!m_keys.TryGetValue(key, out indexSet))
            {
                if (!CheckFilterCondition(value))
                    return;

                indexSet = new HashSet<int>();
                m_keys.Add(key, indexSet);
                indexSet.Add(index);
            }
            else
            {
                if (CheckFilterCondition(value))
                    indexSet.Add(index);
                else
                    indexSet.Remove(index);
            }

        }

        public void RemoveValue(TValue value, int index)
        {
            HashSet<int> indexSet;
            if (m_keys.TryGetValue(GetIndexKey(value), out indexSet))
            {
                indexSet.Remove(index);
            }
        }

        private void ReIndexInternal()
        {
            for (int i = 0; i < m_values.Count; i++)
            {
                TValue value = m_values[i];
                if (value == null || !CheckFilterCondition(value)) continue;

                TKey key = GetIndexKey(value);

                HashSet<int> indexSet;
                if (!m_keys.TryGetValue(key, out indexSet))
                {
                    indexSet = new HashSet<int>();
                    m_keys.Add(key, indexSet);
                }

                indexSet.Add(i);
            }
        }

        public int AddReference(AddIndexReference iref, int ownerHashCode)
        {
            lock (m_references)
            {
                switch (iref)
                {
                    case AddIndexReference.Release:
                        m_references.Remove(ownerHashCode);
                        break;
                    default:
                        m_references.Add(ownerHashCode);
                        break;
                }
                return ReferenceCount;
            }
        }

        public IEnumerable<KeyValuePair<TKey, int>> Count()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var cnt in m_keys.Select((k) => new KeyValuePair<TKey, int>(k.Key, k.Value.Count)))
                    yield return cnt;
            }
        }

        public int Count(TKey key)
        {
            lock (((IList)m_values).SyncRoot)
            {
                HashSet<int> indexSet;
                if (m_keys.TryGetValue(key, out indexSet))
                    return indexSet.Count;
                return 0;
            }
        }

        public IEnumerable<TValue> this[TKey key]
        {
            get
            {
                lock (((IList)m_values).SyncRoot)
                {
                    HashSet<int> indexSet;
                    if (m_keys.TryGetValue(key, out indexSet))
                    {
                        foreach (TValue value in indexSet.Select((i) => m_values[i]))
                            yield return value;
                    }
                    else
                        yield break;
                }
            }
        }

        public IEnumerable<TValue> Values()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var idx in m_keys.SelectMany((k) => k.Value))
                {
                    yield return m_values[idx];
                }
            }
        }

        public ISet<int> GetIndexes(TKey key)
        {
            lock (((IList)m_values).SyncRoot)
            {
                HashSet<int> result = null;
                if (m_keys.TryGetValue(key, out result))
                    return result;
                return new HashSet<int>();
            }
        }

        private Dictionary<TKey, HashSet<int>> m_keys;
        private Func<TValue, TKey> GetIndexKey;
        private Func<TValue, bool> CheckFilterCondition;
        private IList<TValue> m_values;
        private ISet<int> m_references;
    }

    public class IndexedFilteredMultiList<TKey, TValue> : IIndex<TValue>, IIndexedEnumerable<TKey, TValue>
    {
        public string Name { get; protected set; }

        public int ReferenceCount
        {
            get { lock (m_references) return m_references.Count; }
        }

        public IndexedFilteredMultiList(string name, Func<TValue, IEnumerable<TKey>> getIndexKeys, Func<TKey, TValue, bool> checkFilterCondition)
        {
            Name = name;
            GetIndexKeys = getIndexKeys;
            CheckFilterCondition = checkFilterCondition;
            m_references = new HashSet<int>();
        }

        public void ReIndex(IList<TValue> values)
        {
            m_keys = new Dictionary<TKey, HashSet<int>>();
            m_values = values;
            lock (((IList)m_values).SyncRoot) ReIndexInternal();
        }

        public void ApplyValue(TValue value, int index)
        {
            foreach (var key in GetIndexKeys(value))
            {
                HashSet<int> indexSet;
                if (!m_keys.TryGetValue(key, out indexSet))
                {
                    if (!CheckFilterCondition(key, value))
                        return;

                    indexSet = new HashSet<int>();
                    m_keys.Add(key, indexSet);
                    indexSet.Add(index);
                }
                else
                {
                    if (CheckFilterCondition(key, value))
                        indexSet.Add(index);
                    else
                        indexSet.Remove(index);
                }
            }
        }

        public void RemoveValue(TValue value, int index)
        {
            foreach (var key in GetIndexKeys(value))
            {
                HashSet<int> indexSet;
                if (m_keys.TryGetValue(key, out indexSet))
                {
                    indexSet.Remove(index);
                }
            }
        }

        private void ReIndexInternal()
        {
            for (int i = 0; i < m_values.Count; i++)
            {
                TValue value = m_values[i];
                if (value == null) return;

                foreach (var key in GetIndexKeys(value))
                {
                    if (!CheckFilterCondition(key, value)) continue;

                    HashSet<int> indexSet;
                    if (!m_keys.TryGetValue(key, out indexSet))
                    {
                        indexSet = new HashSet<int>();
                        m_keys.Add(key, indexSet);
                    }

                    indexSet.Add(i);
                }
            }            
        }

        public int AddReference(AddIndexReference iref, int ownerHashCode)
        {
            lock (m_references)
            {
                switch (iref)
                {
                    case AddIndexReference.Release:
                        m_references.Remove(ownerHashCode);
                        break;
                    default:
                        m_references.Add(ownerHashCode);
                        break;
                }
                return ReferenceCount;
            }
        }

        public IEnumerable<KeyValuePair<TKey, int>> Count()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var cnt in m_keys.Select((k) => new KeyValuePair<TKey, int>(k.Key, k.Value.Count)))
                    yield return cnt;
            }
        }

        public int Count(TKey key)
        {
            lock (((IList)m_values).SyncRoot)
            {
                HashSet<int> indexSet;
                if (m_keys.TryGetValue(key, out indexSet))
                    return indexSet.Count;
                return 0;
            }
        }

        public IEnumerable<TValue> this[TKey key]
        {
            get
            {
                lock (((IList)m_values).SyncRoot)
                {
                    HashSet<int> indexSet;
                    if (m_keys.TryGetValue(key, out indexSet))
                    {
                        foreach (TValue value in indexSet.Select((i) => m_values[i]))
                            yield return value;
                    }
                    else
                        yield break;
                }
            }
        }

        public IEnumerable<TValue> Values()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var idx in m_keys.SelectMany((k) => k.Value))
                {
                    yield return m_values[idx];
                }
            }
        }

        public ISet<int> GetIndexes(TKey key)
        {
            lock (((IList)m_values).SyncRoot)
            {
                HashSet<int> result = null;
                if (m_keys.TryGetValue(key, out result))
                    return result;
                return new HashSet<int>();
            }
        }

        private Dictionary<TKey, HashSet<int>> m_keys;
        private Func<TValue, IEnumerable<TKey>> GetIndexKeys;
        private Func<TKey, TValue, bool> CheckFilterCondition;
        private IList<TValue> m_values;
        private ISet<int> m_references;
    }

    public class UniqueIndexedList<TKey, TValue> : IIndex<TValue>, IIndexedSingle<TKey, TValue>
	{
		public string Name { get; protected set; }

		public int ReferenceCount
		{
			get { lock (m_references) return m_references.Count; }
		}

		public UniqueIndexedList(string name, Func<TValue, TKey> getIndexKey)
		{
			Name = name;
			GetIndexKey = getIndexKey;
			m_references = new HashSet<int>();
		}

		public void ReIndex(IList<TValue> values)
		{
			m_keys = new Dictionary<TKey, int>();
			m_values = values;
			lock (((IList) m_values).SyncRoot) ReIndexInternal();
		}

		public void ApplyValue(TValue value, int index)
		{
			TKey key = GetIndexKey(value);
			m_keys[key] = index;
            //m_keys.Add(key, index);
		}

		public void RemoveValue(TValue value, int index)
		{
			TKey key = GetIndexKey(value);
			m_keys.Remove(key);
		}

		private void ReIndexInternal()
		{
			for (int i = 0; i < m_values.Count; i++)
			{
				TValue value = m_values[i];
				if (value == null) continue;

				TKey key = GetIndexKey(value);
				m_keys[key] = i;
                //m_keys.Add(key, i);
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				int index;
				if (m_keys.TryGetValue(key, out index))
					return m_values[index];
				return default(TValue);
			}
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			int index;
			bool result = m_keys.TryGetValue(key, out index);
			value = (result) ? m_values[index] : default(TValue);
			return result;
		}

		public bool TryGetIndex(TValue value, out int index)
		{
			if (!m_keys.TryGetValue(GetIndexKey(value), out index))
			{
				index = -1;
				return false;
			}
			return true;
		}

		public bool TryGetIndex(TKey key, out int index)
		{
			if (!m_keys.TryGetValue(key, out index))
			{
				index = -1;
				return false;
			}
			return true;
		}

		public TKey GetKey(TValue value)
		{
			return GetIndexKey(value);
		}

		public int AddReference(AddIndexReference iref, int ownerHashCode)
		{
			lock (m_references)
			{
				switch (iref)
				{
					case AddIndexReference.Release:
						m_references.Remove(ownerHashCode);
						break;
					default:
						m_references.Add(ownerHashCode);
						break;
				}
				return ReferenceCount;
			}
		}

		public IEnumerable<TValue> Values()
		{
			lock (((IList) m_values).SyncRoot)
			{
				foreach (var idx in m_keys.Values)
				{
					yield return m_values[idx];
				}
			}
		}

		public IEnumerable<TKey> Keys()
		{
			lock (((IList) m_values).SyncRoot)
			{
				foreach (var key in m_keys.Keys)
				{
					yield return key;
				}
			}
		}

		private Dictionary<TKey, int> m_keys;
		private Func<TValue, TKey> GetIndexKey;
		private IList<TValue> m_values;
		private ISet<int> m_references;
	}

    public class UniqueSortedIndexedList<TKey, TValue> : IIndex<TValue>, IIndexedSingle<TKey, TValue>
    {
        public string Name { get; protected set; }

        public int ReferenceCount
        {
            get { lock (m_references) return m_references.Count; }
        }

        public UniqueSortedIndexedList(string name, Func<TValue, TKey> getIndexKey, Func<TKey, TKey, int> compare)
        {
            Name = name;
            GetIndexKey = getIndexKey;
            m_comparer = new ValueComparer<TKey>(compare);
            m_references = new HashSet<int>();
        }

        public void ReIndex(IList<TValue> values)
        {
            m_keys = new SortedList<TKey, int>(values.Count, m_comparer);
            m_values = values;
            lock (((IList)m_values).SyncRoot) ReIndexInternal();
        }

        public void ApplyValue(TValue value, int index)
        {
            TKey key = GetIndexKey(value);
            m_keys[key] = index;
            //m_keys.Add(key, index);
        }

        public void RemoveValue(TValue value, int index)
        {
            TKey key = GetIndexKey(value);
            m_keys[key] = -1;
        }

        private void ReIndexInternal()
        {
            for (int i = 0; i < m_values.Count; i++)
            {
                TValue value = m_values[i];
                if (value == null) continue;

                TKey key = GetIndexKey(value);
                m_keys[key] = i;
                //m_keys.Add(key, i);
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                int index;
                if (m_keys.TryGetValue(key, out index) && index >= 0)
                    return m_values[index];
                return default(TValue);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index;
            bool result = m_keys.TryGetValue(key, out index);
            value = (result && index >= 0) ? m_values[index] : default(TValue);
            return result;
        }

        public bool TryGetIndex(TValue value, out int index)
        {
            if (!m_keys.TryGetValue(GetIndexKey(value), out index))
                index = -1;
            return index >= 0;
        }

        public bool TryGetIndex(TKey key, out int index)
        {
            if (!m_keys.TryGetValue(key, out index))
                index = -1;
            return index >= 0;
        }

        public TKey GetKey(TValue value)
        {
            return GetIndexKey(value);
        }

        public int AddReference(AddIndexReference iref, int ownerHashCode)
        {
            lock (m_references)
            {
                switch (iref)
                {
                    case AddIndexReference.Release:
                        m_references.Remove(ownerHashCode);
                        break;
                    default:
                        m_references.Add(ownerHashCode);
                        break;
                }
                return ReferenceCount;
            }
        }

        public IEnumerable<TValue> Values()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var idx in m_keys.Values)
                {
                    if (idx >= 0)
                        yield return m_values[idx];
                }
            }
        }

        public IEnumerable<TKey> Keys()
        {
            lock (((IList)m_values).SyncRoot)
            {
                foreach (var key in m_keys)
                {
                    if (key.Value >= 0)
                        yield return key.Key;
                }
            }
        }

        public void TryPack()
        {
            if (m_keys.Count / 10 > (m_keys.Count - m_values.Count))
            {
                ReIndex(m_values);
            }
        }

        class ValueComparer<TKey> : IComparer<TKey>
        {
            public ValueComparer(Func<TKey, TKey, int> cmp)
            {
                m_cmp = cmp;
            }
            public int Compare(TKey x, TKey y)
            {
                return m_cmp(x, y);
            }
            private Func<TKey, TKey, int> m_cmp;
        }

        private SortedList<TKey, int> m_keys;
        private Func<TValue, TKey> GetIndexKey;
        private ValueComparer<TKey> m_comparer;
        private IList<TValue> m_values;
        private ISet<int> m_references;
    }

	public class ObservableStackedList<TValue> : StackedList<TValue>, IObservableCollection<TValue>
	{
		#region Constructors

		public ObservableStackedList() : base()
		{
		}

		public ObservableStackedList(int capasity) : base(capasity)
		{
		}

		public ObservableStackedList(IEnumerable<TValue> items) : base(items)
		{
		}

		#endregion

		#region Protected Methods

		protected override void ClearItems()
		{
			base.ClearItems();
			OnCollectionChanged(default(TValue), default(TValue), -1);
		}

		protected override void RemoveItem(int index)
		{
			TValue removedItem = this[index];
			base.RemoveItem(index);
			OnCollectionChanged(removedItem, default(TValue), index);
		}

		protected override void SetItem(int index, TValue item)
		{
			TValue originalItem = this[index];
			base.SetItem(index, item);
			OnCollectionChanged(originalItem, item, index);
		}

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		protected virtual void OnCollectionChanged(TValue oldValue, TValue newValue, int index)
		{
			if (onCollectionChanged != null)
				onCollectionChanged(oldValue, newValue, index);
		}

		#endregion Protected Methods

        private Action<TValue, TValue, int> onCollectionChanged;
        public event Action<TValue, TValue, int> CollectionChanged
        {
            add
            {
                onCollectionChanged += value;
            }
            remove
            {
                onCollectionChanged -= value;
            }
        }
	}

	public struct IndexedUpdateHelper<TKey, TValue>
	{
		public readonly IIndexedSingle<TKey, TValue> Primary;
		public readonly IList<TValue> Values;

		private int m_index;

		public int Index
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get { return m_index; }
		}

		public TKey Key
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				if (m_index >= 0) return Primary.GetKey(Values[m_index]);
				return default(TKey);
			}
		}

		public TValue Value
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				if (m_index >= 0) return Values[m_index];
				return default(TValue);
			}
		}

		public IndexedUpdateHelper(IndexedListManager<TKey, TValue> manager, int index = -1)
		{
			Primary = manager.Primary;
			Values = manager.Values;
			m_index = index;
		}

		public void ApplyValue(TValue value)
		{
			if (m_index < 0 || !Primary.TryGetIndex(value, out m_index))
				m_index = ((IList) Values).Add(value);
		}

		public void ApplyValueWithReplace(TValue value)
		{
			if (m_index < 0 || !Primary.TryGetIndex(value, out m_index))
				m_index = ((IList) Values).Add(value);
			else
				Values[m_index] = value;
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public bool FindValue(TValue value)
		{
			return Primary.TryGetIndex(value, out m_index);
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public bool FindKey(TKey key)
		{
			return Primary.TryGetIndex(key, out m_index);
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public void RemoveValue()
		{
			Values.RemoveAt(m_index);
			m_index = -1;
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public void RemoveValue(TValue value)
		{
			if (Primary.TryGetIndex(value, out m_index))
			{
				Values.RemoveAt(m_index);
				m_index = -1;
			}
		}
	}

	public struct IndexedUpdateActionHelper<TKey, TValue>
	{
		public readonly IIndexedSingle<TKey, TValue> Primary;
		public readonly IList<TValue> Values;
		public readonly Action<TValue> OnNewValue;

		private int m_index;

		public int Index
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get { return m_index; }
		}

		public TKey Key
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				if (m_index >= 0) return Primary.GetKey(Values[m_index]);
				return default(TKey);
			}
		}

		public TValue Value
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				if (m_index >= 0) return Values[m_index];
				return default(TValue);
			}
		}

		public IndexedUpdateActionHelper(IndexedListManager<TKey, TValue> manager, Action<TValue> onNewValue, int index = -1)
		{
			if (onNewValue == null) throw new ArgumentNullException();
			Primary = manager.Primary;
			Values = manager.Values;
			OnNewValue = onNewValue;
			m_index = index;
		}

		public void ApplyValue(TValue value)
		{
			if (m_index < 0 || !Primary.TryGetIndex(value, out m_index))
			{
				m_index = ((IList) Values).Add(value);
				OnNewValue(value);
			}
		}

		public void ApplyValueWithReplace(TValue value)
		{
			if (m_index < 0 || !Primary.TryGetIndex(value, out m_index))
			{
				m_index = ((IList) Values).Add(value);
				OnNewValue(value);
			}
			else
				Values[m_index] = value;
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public bool FindValue(TValue value)
		{
			return Primary.TryGetIndex(value, out m_index);
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public bool FindKey(TKey key)
		{
			return Primary.TryGetIndex(key, out m_index);
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public void RemoveValue()
		{
			Values.RemoveAt(m_index);
			m_index = -1;
		}

		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public void RemoveValue(TValue value)
		{
			if (Primary.TryGetIndex(value, out m_index))
			{
				Values.RemoveAt(m_index);
				m_index = -1;
			}
		}
	}

	public class IndexedListManager<TKey, TValue>
	{
		public IndexedListManager()
            : this(new ObservableStackedList<TValue>())
		{
		}
        
		public IndexedListManager(int capasity)
            : this(new ObservableStackedList<TValue>(capasity))
		{
		}

		public IndexedListManager(IEnumerable<TValue> values)
            : this(new ObservableStackedList<TValue>(values))
		{
		}

		public IndexedListManager(IEnumerable<TValue> values, IEnumerable<IIndex<TValue>> indexes)
			: this(values)
		{
			foreach (var index in indexes) AddIndex(index);
		}

        public IndexedListManager(IObservableCollection<TValue> values, IEnumerable<IIndex<TValue>> indexes)
            : this(values)
        {
            foreach (var index in indexes) AddIndex(index);
        }

        public IndexedListManager(IObservableCollection<TValue> values)
        {
            m_values = values;
            m_indexes = new List<IIndex<TValue>>();
            m_values.CollectionChanged += OnCollectionChanged;
        }

		public void OnCollectionChanged(TValue oldValue, TValue newValue, int valueIndex)
		{
			if (oldValue != null)
			{
                for (int i = 0; i < m_indexes.Count; i++)
					m_indexes[i].RemoveValue(oldValue, valueIndex);
				//if (newValue == null) return;
			}

			if (newValue != null)
			{
				int idx;
				if (!m_primary.TryGetIndex(newValue, out idx))
				{
					m_primary.ApplyValue(newValue, valueIndex);
				}
				for(int i = 1; i < m_indexes.Count; i++)
					m_indexes[i].ApplyValue(newValue, valueIndex);
				return;
			}

			if (oldValue == null && newValue == null)
                for (int i = 0; i < m_indexes.Count; i++)
					m_indexes[i].ReIndex(m_values);
		}

		public void AddIndex(IIndex<TValue> index)
		{
			if (m_primary == null) m_primary = (UniqueIndexedList<TKey, TValue>) index;
			m_indexes.Add(index);
			index.ReIndex(m_values);
		}

		public IIndex<TValue> this[int number]
		{
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get { return (number < m_indexes.Count && number >= 0) ? m_indexes[number] : null; }
		}

		public IIndex<TValue> this[string name]
		{
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get
			{
				foreach (var idx in m_indexes)
				{
					if (String.CompareOrdinal(name, idx.Name) == 0)
						return idx;
				}
				return null;
			}
		}

		public IEnumerable<TValue> GetValues(IEnumerable<int> indexes)
		{
			foreach (int idx in indexes)
				yield return ((IList<TValue>)m_values)[idx];
		}

		public void SubscribeIndex(IIndex<TValue> index, int ownerHashCode = 0)
		{
			if (index == null) return;
			index.AddReference(AddIndexReference.Add, ownerHashCode);
		}

		public void UnSubscribeIndex(IIndex<TValue> index, int ownerHashCode = 0)
		{
			if (index == null) return;
			int refCnt = index.AddReference(AddIndexReference.Release, ownerHashCode);
			if (refCnt <= 0)
				lock (Locker) m_indexes.Remove(index);
		}

		public IIndexedSingle<TKey, TValue> Primary
		{
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get { return m_primary; }
		}

		public IList<TValue> Values
		{
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get { return m_values; }
		}

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public TValue GetSingle<TKey1>(TKey1 key, int indexNumber)
        {
            return ((IIndexedSingle<TKey1, TValue>)this[indexNumber])[key];
        }

		private UniqueIndexedList<TKey, TValue> m_primary;
		private List<IIndex<TValue>> m_indexes;
		private IObservableCollection<TValue> m_values;

		public Object Locker
		{
			[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
			get { return ((IList) m_values).SyncRoot; }
		}

		public int LockTimeout = 2000;

	}
}
