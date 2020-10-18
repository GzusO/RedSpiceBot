// Copyright © 2016 phil.o@codeproject.com
//
// This source code is released under the Code Project Open License 1.02.
// Please see http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RedSpiceBot.NameGenerator
{
	/// <summary>
	/// Defines a <see cref="Dictionary{TKey, TValue}"/> providing an <see cref="EventuallyAdd(TKey, TValue)"/> method,  
	/// which silently continues when trying to add an existing key, instead of throwing an
	/// <see cref="ArgumentException"/> like <see cref="Dictionary{TKey, TValue}.Add(TKey, TValue)"/> method does.
	/// Moreover, provides the base attribute and constructor to allow .NET binary serialization.
	/// </summary>
	/// <typeparam name="TKey"><see cref="Type"/> of dictionary's keys.</typeparam>
	/// <typeparam name="TValue"><see cref="Type"/> of dictionary's values.</typeparam>
	[Serializable]
	public class Map<TKey, TValue> : Dictionary<TKey, TValue>
	{
		#region Contructors
		/// <summary>
		/// Initializes a new <see cref="Map{TKey, TValue}"/> instance which is empty, has the default initial capacity,
		/// and uses the default <see cref="IEqualityComparer{TKey}"/>.
		/// </summary>
		public Map()
			: base() { }

		/// <summary>
		/// Initializes a new <see cref="Map{TKey, TValue}"/> instance which is empty, has the specified initial capacity,
		/// and uses the default <see cref="IEqualityComparer{TKey}"/>.
		/// </summary>
		public Map(int capacity)
			: base(capacity) { }

		/// <summary>
		/// Initializes a new <see cref="Map{TKey, TValue}"/> instance which is empty, has the default initial capacity,
		/// and uses the specified <see cref="IEqualityComparer{TKey}"/>.
		/// </summary>
		public Map(IEqualityComparer<TKey> comparer)
			: base(comparer) { }

		/// <summary>
		/// Initializes a new <see cref="Map{TKey, TValue}"/> instance which contains elements copied from specified
		/// <see cref="IDictionary{TKey, TValue}"/> and uses the default <see cref="IEqualityComparer{TKey}"/>.
		/// </summary>
		public Map(IDictionary<TKey, TValue> dictionary)
			: base(dictionary) { }

		/// <summary>
		/// Initializes a new <see cref="Map{TKey, TValue}"/> instance which is empty, has the specified initial capacity,
		/// and uses the specified <see cref="IEqualityComparer{TKey}"/>.
		/// </summary>
		public Map(int capacity, IEqualityComparer<TKey> comparer)
			: base(capacity, comparer) { }

		/// <summary>
		/// Initializes a new <see cref="Map{TKey, TValue}"/> instance which contains elements copied from specified
		/// <see cref="IDictionary{TKey, TValue}"/> and uses the specified <see cref="IEqualityComparer{TKey}"/>.
		/// </summary>
		public Map(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
			: base(dictionary, comparer) { }

		/// <summary>
		/// Initializes a new <see cref="Map{TKey, TValue}"/> instance with serialized data.
		/// </summary>
		/// <param name="info">A <see cref="SerializationInfo"/> object containing the information required to serialize
		/// the <see cref="Map{TKey, TValue}"/>.</param>
		/// <param name="context">A <see cref="StreamingContext"/> structure containing the source and destination of the
		/// serialized stream associated with the <see cref="Map{TKey, TValue}"/>.</param>
		protected Map(SerializationInfo info, StreamingContext context)
			: base(info, context) { }
		#endregion

		#region Methods
		/// <summary>
		/// Adds specified key/value pair to the collection, only if key is not already present, otherwise simply
		/// continues without adding. Returns whether item has been added.
		/// </summary>
		/// <param name="key"><typeparamref name="TKey"/> key to add to collection.</param>
		/// <param name="value"><typeparamref name="TValue"/> value to add to collection.</param>
		/// <returns>True if a pair of <paramref name="key"/> and <paramref name="value"/> has been added to the
		/// collection, otherwise false.</returns>
		public bool EventuallyAdd(TKey key, TValue value)
		{
			bool add = !ContainsKey(key);
			if (add)
			{
				Add(key, value);
			}
			return add;
		}
		#endregion
	}
}