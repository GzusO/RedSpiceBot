// Copyright © 2016 phil.o@codeproject.com
//
// This source code is released under the Code Project Open License 1.02.
// Please see http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;

namespace RedSpiceBot.NameGenerator
{
	/// <summary>
	/// Implements a Markov Chain Map, which stores informations about sequences occurences in a samples collection.
	/// </summary>
	/// <typeparam name="T"><see cref="Type"/> of sequential values in the chain.</typeparam>
	[Serializable]
	public sealed class MarkovChainMap<T> : Map<T, FrequencyMap<T>>, ISerializable
	{
		#region Fields
		/// <summary>
		/// Default depth for a <see cref="MarkovChainMap{T}"/>.
		/// </summary>
		public const int DefaultDepth = 1;

		private int _depth;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the depth of this <see cref="MarkovChainMap{T}"/>, i.e. the maximum number of preceding states in the
		/// sequence which will be considered while building the map.
		/// </summary>
		public int Depth
		{
			get { return _depth; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new <see cref="MarkovChainMap{T}"/> instance with specified depth.
		/// </summary>
		/// <param name="depth">Depth of the <see cref="MarkovChainMap{T}"/>.</param>
		public MarkovChainMap(int depth = DefaultDepth)
			: base()
		{
			_depth = depth;
		}

		private MarkovChainMap(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_depth = info.GetInt32(nameof(Depth));
		}
		#endregion

		#region Methods
		/// <summary>
		/// Dumps the contents of this <see cref="MarkovChainMap{T}"/> to a string and returns the result.
		/// </summary>
		/// <returns>String dump of this <see cref="MarkovChainMap{T}"/>.</returns>
		public string Dump()
		{
			FrequencyMap<T> map;
			StateFrequency freq;
			StringBuilder sb = new StringBuilder(1048576);
			string header = $"- Markov Chain Map on type {typeof(T).Name} - Depth: {_depth} -";
			string line = new string('-', header.Length);
			sb.AppendLine(line);
			sb.AppendLine(header);
			sb.AppendLine(line);
			sb.AppendLine($" {Count,7:N0} entries");
			sb.AppendLine($" {FrequencyCount(),7:N0} frequencies");
			sb.AppendLine($" {FrequencyPerEntry(),7:N3} frequency/entry");
			sb.AppendLine(line);
			sb.AppendLine();
			sb.AppendLine($"Key      \tNext\t Start \t Inner \t  End");
			sb.AppendLine($"---------\t----\t-------\t-------\t-------");
			foreach (var x in this.OrderBy(p => p.Key))
			{
				map = x.Value;
				foreach (var y in map.OrderBy(q => q.Key))
				{
					freq = y.Value;
					sb.AppendLine($"{x.Key,9}\t{y.Key,4}\t{freq.FirstCount,7}\t{freq.InnerCount,7}\t{freq.LastCount,7}");
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Dumps the contents of this <see cref="MarkovChainMap{T}"/> to a file.
		/// </summary>
		/// <param name="path">Path to the file in which dumping the contents of this <see cref="MarkovChainMap{T}"/>.
		/// </param>
		/// <exception cref="ArgumentException"><paramref name="path"/> is empty, or it contains the name of a system
		/// device (com1, com2, etc.).</exception>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
		/// <exception cref="DirectoryNotFoundException"><paramref name="path"/> is invalid.</exception>
		/// <exception cref="IOException"><paramref name="path"/> includes an incorrect or invalid syntax for file name,
		/// directory name, or volume label syntax.</exception>
		/// <exception cref="PathTooLongException"><paramref name="path"/> exceeds system-defined maximum length.
		/// </exception>
		/// <exception cref="SecurityException">Caller does not have required permissions.</exception>
		/// <exception cref="UnauthorizedAccessException">Access to <paramref name="path"/> is unauthorized.</exception>
		public void DumpAndSave(string path)
		{
			using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
			{
				writer.Write(Dump());
			}
		}

		/// <summary>
		/// Returns the total number of <see cref="StateFrequency"/> instances contained in this
		/// <see cref="MarkovChainMap{T}"/>'s values.
		/// </summary>
		/// <returns>Total number of <see cref="StateFrequency"/> instances in this <see cref="MarkovChainMap{T}"/>'s
		/// values.</returns>
		public int FrequencyCount()
		{
			return Values.Sum(fm => fm.Count);
		}

		/// <summary>
		/// Returns the average number of <see cref="StateFrequency"/> per <typeparamref name="T"/> key in this
		/// <see cref="MarkovChainMap{T}"/>.
		/// </summary>
		/// <returns>Average number of <see cref="StateFrequency"/> per <typeparamref name="T"/> key in this
		/// <see cref="MarkovChainMap{T}"/>.</returns>
		public double FrequencyPerEntry()
		{
			return (double)FrequencyCount() / Count;
		}

		/// <summary>
		/// Returns the collection of all distinct following states in this <see cref="MarkovChainMap{T}"/>.
		/// </summary>
		/// <returns>The collection of all distinct following states in this <see cref="MarkovChainMap{T}"/>.</returns>
		public List<T> GetNextStates()
		{
			HashSet<T> states = new HashSet<T>();
			T key;
			foreach (var pair in this)
			{
				key = pair.Key;
				foreach (var pair2 in pair.Value)
				{
					states.Add(pair2.Key);
				}
			}
			return states.ToList();
		}

		/// <summary>
		/// Implements the <see cref="ISerializable"/> interface and returns the data needed to serialize this
		/// <see cref="MarkovChainMap{T}"/> instance.
		/// </summary>
		/// <param name="info">A <see cref="SerializationInfo"/> object that contains the information required to
		/// serialize this <see cref="MarkovChainMap{T}"/> instance.</param>
		/// <param name="context">A <see cref="StreamingContext"/> structure that contains the source and destination of
		/// the serialized stream associated with this <see cref="MarkovChainMap{T}"/> instance.</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue(nameof(Depth), _depth);
		}

		/// <summary>
		/// Returns the collection of all starting states in this <see cref="MarkovChainMap{T}"/>.
		/// </summary>
		/// <returns>Collection of all starting states in this <see cref="MarkovChainMap{T}"/>.</returns>
		public List<T> GetStartStates()
		{
			return Keys.ToList();
		}

		/// <summary>
		/// Loads a <see cref="MarkovChainMap{T}"/>, which has previously been serialized with <see cref="Save(string)"/>
		/// method, from file specified by <paramref name="path"/>.
		/// </summary>
		/// <param name="path">Path to the file to which the <see cref="MarkovChainMap{T}"/> has been serialized.</param>
		/// <returns>A new <see cref="MarkovChainMap{T}"/> from serialized data saved to <paramref name="path"/>.
		/// </returns>
		/// <exception cref="ArgumentException"><paramref name="path"/> is an empty string, contains only white space, or
		/// contains one or more invalid characters; or it refers to a non-file device, such as con:, com1:, lpt1:, etc.
		/// in an NTFS environment.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
		/// <exception cref="DirectoryNotFoundException">Directory specified by <paramref name="path"/> cannot be found.
		/// </exception>
		/// <exception cref="FileNotFoundException">File specified by <paramref name="path"/> cannot be found.</exception>
		/// <exception cref="IOException">An I/O error occured.</exception>
		/// <exception cref="NotSupportedException"><paramref name="path"/> refers to a non-file device, such as con:,
		/// com1:, lpt1:, etc. in an non-NTFS environment.</exception>
		/// <exception cref="PathTooLongException"><paramref name="path"/> exceeds system-defined maximum length.
		/// </exception>
		/// <exception cref="SecurityException">Caller does not have required permissions.</exception>
		public static MarkovChainMap<T> Load(string path)
		{
			byte[] bytes = null;
			using (FileStream stream = new FileStream(path, FileMode.Open))
			{
				int length = (int)stream.Length;
				bytes = new byte[length];
				stream.Read(bytes, 0, length);
			}
			return Load(bytes);
		}

		/// <summary>
		/// Loads a <see cref="MarkovChainMap{T}"/> from serialized data.
		/// </summary>
		/// <param name="bytes">Serialized data.</param>
		/// <returns>A new <see cref="MarkovChainMap{T}"/> from <paramref name="bytes"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null.</exception>
		/// <exception cref="SerializationException">Length of <paramref name="bytes"/> is zero, or one of serialized
		/// values does not fit target type.</exception>
		/// <exception cref="SecurityException">Caller does not have required permissions.</exception>
		public static MarkovChainMap<T> Load(byte[] bytes)
		{
			MarkovChainMap<T> map;
			using (MemoryStream stream = new MemoryStream(bytes))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				map = (MarkovChainMap<T>)formatter.Deserialize(stream);
			}
			return map;
		}

		/// <summary>
		/// Merges the contents of a specified <see cref="MarkovChainMap{T}"/> with those of this one.
		/// </summary>
		/// <param name="other"><see cref="MarkovChainMap{T}"/> whose contents to merge to those of this map.</param>
		/// <exception cref="NullReferenceException"><paramref name="other"/> is null.</exception>
		public void Merge(MarkovChainMap<T> other)
		{
			T key;
			foreach (var pair in other)
			{
				key = pair.Key;
				EventuallyAdd(key, new FrequencyMap<T>());
				this[key].Merge(pair.Value);
			}
		}

		/// <summary>
		/// Saves this <see cref="MarkovChainMap{T}"/> to a specified file.
		/// </summary>
		/// <param name="path">Path to the file to which saving this <see cref="MarkovChainMap{T}"/>.</param>
		/// <exception cref="ArgumentException"><paramref name="path"/> is an empty string, contains only white space, or
		/// contains one or more invalid characters; or it refers to a non-file device, such as con:, com1:, lpt1:, etc.
		/// in an NTFS environment.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
		/// <exception cref="DirectoryNotFoundException">Directory specified by <paramref name="path"/> cannot be found.
		/// </exception>
		/// <exception cref="IOException">An I/O error occured.</exception>
		/// <exception cref="NotSupportedException"><paramref name="path"/> refers to a non-file device, such as con:,
		/// com1:, lpt1:, etc. in an non-NTFS environment.</exception>
		/// <exception cref="PathTooLongException"><paramref name="path"/> exceeds system-defined maximum length.
		/// </exception>
		/// <exception cref="SecurityException">Caller does not have required permissions.</exception>
		public void Save(string path)
		{
			using (FileStream stream = new FileStream(path, FileMode.Create))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, this);
			}
		}
		#endregion
	}
}
