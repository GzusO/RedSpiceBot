// Copyright © 2016 phil.o@codeproject.com
//
// This source code is released under the Code Project Open License 1.02.
// Please see http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Collections.Generic;

namespace RedSpiceBot.NameGenerator
{
	/// <summary>
	/// Implements an object which is able to build a <see cref="MarkovChainMap{T}"/> when provided with a list of
	/// possible chain-sequence samples.
	/// </summary>
	/// <typeparam name="T"><see cref="Type"/> of <see cref="MarkovChainMap{T}"/> states.</typeparam>
	public class MarkovChainMapBuilder<T>
	{
		#region Fields
		private AggregationFunc<T> _aggregate;
		private int _depth;
		private PartitionFunc<T> _partition;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the <see cref="AggregationFunc{T}"/> which will be used to build <see cref="MarkovChainMap{T}"/>
		/// instances.
		/// </summary>
		/// <exception cref="ArgumentNullException">A null <paramref name="value"/> is provided to the setter.</exception>
		protected AggregationFunc<T> Aggregate
		{
			get { return _aggregate; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}
				_aggregate = value;
			}
		}

		/// <summary>
		/// Gets or sets the depth of the <see cref="MarkovChainMap{T}"/> which this
		/// <see cref="MarkovChainMapBuilder{T}"/> will build.
		/// </summary>
		/// <exception cref="ArgumentException">A <paramref name="value"/> less than one is provided to the setter.
		/// </exception>
		public virtual int Depth
		{
			get { return _depth; }
			set
			{
				if (value < 1)
				{
					throw new ArgumentException(nameof(value));
				}
				_depth = value;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="PartitionFunc{T}"/> which will be used to build <see cref="MarkovChainMap{T}"/>
		/// instances.
		/// </summary>
		/// <exception cref="ArgumentNullException">A null <paramref name="value"/> is provided to the setter.</exception>
		protected PartitionFunc<T> Partition
		{
			get { return _partition; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}
				_partition = value;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new <see cref="MarkovChainMapBuilder{T}"/> instance with specified depth, and aggregation and
		/// partition functions.
		/// </summary>
		/// <param name="depth">Depth of the <see cref="MarkovChainMap{T}"/> to build.</param>
		/// <param name="partition"><see cref="PartitionFunc{T}"/> which will be used to build the
		/// <see cref="MarkovChainMap{T}"/>.</param>
		/// <param name="aggregate"><see cref="AggregationFunc{T}"/> which will be used to build the
		/// <see cref="MarkovChainMap{T}"/>.</param>
		/// <exception cref="ArgumentException"><paramref name="depth"/> is less than one.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="partition"/> is null, or <paramref name="aggregate"/>
		/// is null.</exception>
		public MarkovChainMapBuilder(int depth, PartitionFunc<T> partition, AggregationFunc<T> aggregate)
		{
			if (depth < 1)
			{
				throw new ArgumentException(nameof(depth));
			}
			if (partition == null)
			{
				throw new ArgumentNullException(nameof(partition));
			}
			if (aggregate == null)
			{
				throw new ArgumentNullException(nameof(aggregate));
			}
			_depth = depth;
			_partition = partition;
			_aggregate = aggregate;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Builds a new <see cref="MarkovChainMap{T}"/> from specified list of samples, and returns the result.
		/// </summary>
		/// <param name="samples">A collection of <typeparamref name="T"/> samples used to build the
		/// <see cref="MarkovChainMap{T}"/>.</param>
		/// <returns>A new <see cref="MarkovChainMap{T}"/> built from <paramref name="samples"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="samples"/> is null.</exception>
		public MarkovChainMap<T> Train(IEnumerable<T> samples)
		{
			MarkovChainMap<T> map = new MarkovChainMap<T>(_depth);
			foreach (T sample in samples)
			{
				if (sample != null)
				{
					Train(map, sample);
				}
			}
			return map;
		}

		/// <summary>
		/// Qualifies specified map with new data from specified sample.
		/// </summary>
		/// <param name="map"><see cref="MarkovChainMap{T}"/> to qualify.</param>
		/// <param name="sample"><typeparamref name="T"/> value to train <paramref name="map"/> with.</param>
		protected virtual void Train(MarkovChainMap<T> map, T sample)
		{
			T[] array = _partition(sample);
			int length = array.Length;
			int depth = 1;
			FrequencyMap<T> frequencyMap;
			StateFrequency stateFrequency;
			T key, value;
			int position, current, limit;
			while (depth <= _depth)
			{
				if (depth < length)
				{
					limit = length - depth;
					for (position = 0; (current = position + depth) < length; position++)
					{
						key = _aggregate(array, position, depth);
						value = array[position + depth];
						map.EventuallyAdd(key, new FrequencyMap<T>());
						frequencyMap = map[key];
						frequencyMap.EventuallyAdd(value, new StateFrequency());
						stateFrequency = frequencyMap[value];
						if (position == 0)
						{
							stateFrequency.IncrementFirst();
						}
						else if (current < limit)
						{
							stateFrequency.IncrementInner();
						}
						else
						{
							stateFrequency.IncrementLast();
						}
					}
				}
				depth++;
			}
		}
		#endregion
	}
}