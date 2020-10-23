// Copyright © 2016 phil.o@codeproject.com
//
// This source code is released under the Code Project Open License 1.02.
// Please see http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Collections.Generic;
using System.Linq;

namespace RedSpiceBot.ArtifactGenerator
{
	/// <summary>
	/// Implements a mixed functionality between a <see cref="List{T}"/> and a <see cref="HashSet{T}"/>. Useful when
	/// there is the need to maintain a list of distinct values (like a HashSet does), but also to be able to return a
	/// value at a specific index (like a List does).
	/// </summary>
	/// <typeparam name="T"><see cref="Type"/> of list's elements.</typeparam>
	[Serializable]
	public class HashList<T> : List<T>
	{
		#region Fields
		private readonly HashSet<T> _hashSet;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the <see cref="HashSet{T}"/> contained in this <see cref="HashList{T}"/>.
		/// </summary>
		public HashSet<T> HashSet
		{
			get { return _hashSet; }
		}

		/// <summary>
		/// Gets or sets the item at specified index in the collection.
		/// </summary>
		/// <param name="index">Index in the collection of the item to get or set.</param>
		/// <returns><typeparamref name="T"/> item at <paramref name="index"/> in the collection.</returns>
		/// <exception cref="InvalidOperationException">An existing value elsewhere in the collection is provided to the
		/// setter.</exception>
		public new T this[int index]
		{
			get { return base[index]; }
			set
			{
				T current = base[index];
				if (!value.Equals(current))
				{
					if (_hashSet.Contains(value))
					{
						throw new InvalidOperationException($"Element {value} is already in the collection");
					}
					_hashSet.Remove(current);
					_hashSet.Add(value);
					base[index] = value;
				}
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new <see cref="HashList{T}"/> instance with default capacity.
		/// </summary>
		public HashList()
			: base()
		{
			_hashSet = new HashSet<T>();
		}

		/// <summary>
		/// Initializes a new <see cref="HashList{T}"/> instance with specified capacity.
		/// </summary>
		/// <param name="capacity"></param>
		public HashList(int capacity)
			: base(capacity)
		{
			_hashSet = new HashSet<T>();
		}

		/// <summary>
		/// Initializes a new <see cref="HashList{T}"/> instance with specified collection. Duplicates will not be present
		/// in the resulting collection.
		/// </summary>
		/// <param name="collection">Collection of <typeparamref name="T"/> values used to initialize this
		/// <see cref="HashList{T}"/> instance.</param>
		public HashList(IEnumerable<T> collection)
			: base(collection.Count())
		{
			_hashSet = new HashSet<T>();
			foreach (T item in collection)
			{
				Add(item);
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Tries to add a specified value to the collection, and returns whether addition has succeeded.
		/// </summary>
		/// <param name="item"><typeparamref name="T"/> item to add to the collection.</param>
		/// <returns>True if <paramref name="item"/> was added to the collection, otherwise false.</returns>
		public new bool Add(T item)
		{
			bool added = _hashSet.Add(item);
			if (added)
			{
				base.Add(item);
			}
			return added;
		}

		/// <summary>
		/// Adds specified items to the collection. Duplicates will not be added.
		/// </summary>
		/// <param name="collection">Collection of <typeparamref name="T"/> items to add to the collection.</param>
		public new void AddRange(IEnumerable<T> collection)
		{
			foreach (T item in collection)
			{
				Add(item);
			}
		}

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		public new void Clear()
		{
			_hashSet.Clear();
			base.Clear();
		}

		/// <summary>
		/// Returns whether specified item is present in the collection.
		/// </summary>
		/// <param name="item"><typeparamref name="T"/> item to search for.</param>
		/// <returns>True if <paramref name="item"/> is present in the collection, otherwise false.</returns>
		public new bool Contains(T item)
		{
			return _hashSet.Contains(item);
		}

		/// <summary>
		/// Tries to insert an item in the collection, and returns whether insertion has succeeded.
		/// </summary>
		/// <param name="index">Index in the collection at which inserting <paramref name="item"/>.</param>
		/// <param name="item"><typeparamref name="T"/> item to insert in the collection at <paramref name="index"/>.
		/// </param>
		/// <returns>True if <paramref name="item"/> could be inserted in the collection at <paramref name="index"/>,
		/// otherwise false.</returns>
		public new bool Insert(int index, T item)
		{
			bool added = _hashSet.Add(item);
			if (added)
			{
				base.Insert(index, item);
			}
			return added;
		}

		/// <summary>
		/// Inserts specified items in the collection at specified index. Duplicates will not be inserted.
		/// </summary>
		/// <param name="index">Index in the collection at which inserting <paramref name="collection"/>'s items.</param>
		/// <param name="collection">Collection of <typeparamref name="T"/> items to insert in the collection at
		/// <paramref name="index"/>.</param>
		public new void InsertRange(int index, IEnumerable<T> collection)
		{
			foreach (T item in collection)
			{
				if (Insert(index, item))
				{
					index++;
				}
			}
		}

		/// <summary>
		/// Tries to remove an item from the collection, and returns whether removal has succeeded.
		/// </summary>
		/// <param name="item"><typeparamref name="T"/> item to remove from the collection.</param>
		/// <returns>True if <paramref name="item"/> could be removed from the collection, otherwise false.</returns>
		public new bool Remove(T item)
		{
			bool removed = false;
			if (_hashSet.Remove(item))
			{
				removed = base.Remove(item);
			}
			return removed;
		}

		/// <summary>
		/// Tries to remove items from the collection based on a specified <see cref="Predicate{T}"/>, and returns the
		/// number of items that were removed.
		/// </summary>
		/// <param name="predicate"><see cref="Predicate{T}"/> to use for determining whether or not a given item should
		/// be removed.</param>
		/// <returns>Number of items which were removed from the collection.</returns>
		public new int RemoveAll(Predicate<T> predicate)
		{
			_hashSet.RemoveWhere(predicate);
			return base.RemoveAll(predicate);
		}

		/// <summary>
		/// Removes the item at specified index in the collection.
		/// </summary>
		/// <param name="index">Index in the collection of the item to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is lower than zero, or
		/// <paramref name="index"/> is greater than or equals actual number of elements in the collection.</exception>
		public new void RemoveAt(int index)
		{
			_hashSet.Remove(this[index]);
			base.RemoveAt(index);
		}

		/// <summary>
		/// Removes items within specified range in the collection.
		/// </summary>
		/// <param name="index">Index in the collection at which starting to remove items.</param>
		/// <param name="count">Number of items to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is lower than zero, or the sum of
		/// <paramref name="index"/> and <paramref name="count"/> is greater than or equals actual number of elements in
		/// the collection.</exception>
		public new void RemoveRange(int index, int count)
		{
			int end = index + count;
			while (index < end)
			{
				RemoveAt(index++);
			}
		}
		#endregion
	}
}
