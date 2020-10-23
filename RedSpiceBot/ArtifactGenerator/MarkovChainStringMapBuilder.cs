// Copyright © 2016 phil.o@codeproject.com
//
// This source code is released under the Code Project Open License 1.02.
// Please see http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RedSpiceBot.ArtifactGenerator
{
	/// <summary>
	/// Implements a <see cref="MarkovChainMap{T}"/> which will operate on strings.
	/// </summary>
	public class MarkovChainStringMapBuilder : MarkovChainMapBuilder<string>
	{
		#region Fields
		private static readonly Lazy<StringBuilder> _sb = new Lazy<StringBuilder>(() => new StringBuilder(16));
		private bool _skipWhitespace;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets whether white spaces should be considered while building maps. When set to true, individual
		/// samples will be split when containing white space(s). When set to false, white spaces will not be ignored, and
		/// thus will appear as valid states in the resulting map.
		/// </summary>
		public bool SkipWhitespace
		{
			get { return _skipWhitespace; }
			set { _skipWhitespace = value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new <see cref="MarkovChainStringMapBuilder"/> with specified depth and whitespace-skipping mode,
		/// and default <see cref="PartitionFunc{T}"/> and <see cref="AggregationFunc{T}"/> functions for string type.
		/// </summary>
		/// <param name="depth">Depth of the <see cref="MarkovChainMap{T}"/> to build.</param>
		/// <param name="skipWhitespace">Whether or not white spaces should be considered while building maps. When set to
		/// true, individual samples will be split when containing white space(s). When set to false, white spaces will
		/// not be ignored, and thus will appear as valid states in the resulting map.</param>
		/// <exception cref="ArgumentException"><paramref name="depth"/> is less than one.</exception>
		public MarkovChainStringMapBuilder(int depth, bool skipWhitespace = true)
			: this(depth, (v) => ToStringArray(v), (v, s, c) => Join(v, s, c), skipWhitespace) { }

		/// <summary>
		/// Initializes a new <see cref="MarkovChainStringMapBuilder"/> with specified depth and whitespace-skipping mode,
		/// as well as <see cref="PartitionFunc{T}"/> and <see cref="AggregationFunc{T}"/> functions for string type.
		/// </summary>
		/// <param name="depth">Depth of the <see cref="MarkovChainMap{T}"/> to build.</param>
		/// <param name="partition"><see cref="PartitionFunc{T}"/> which will be used to build the
		/// <see cref="MarkovChainMap{T}"/>.</param>
		/// <param name="aggregate"><see cref="AggregationFunc{T}"/> which will be used to build the
		/// <see cref="MarkovChainMap{T}"/>.</param>
		/// <param name="skipWhitespace">Whether or not white spaces should be considered while building maps. When set to
		/// true, individual samples will be split when containing white space(s). When set to false, white spaces will
		/// not be ignored, and thus will appear as valid states in the resulting map.</param>
		/// <exception cref="ArgumentException"><paramref name="depth"/> is less than one.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="partition"/> is null, or <paramref name="aggregate"/>
		/// is null.</exception>
		public MarkovChainStringMapBuilder(int depth, PartitionFunc<string> partition, AggregationFunc<string> aggregate,
			bool skipWhitespace = true)
			: base(depth, partition, aggregate)
		{
			_skipWhitespace = skipWhitespace;
		}
		#endregion

		#region Methods
		#region Public Methods
		/// <summary>
		/// Returns a new <see cref="MarkovChainMap{T}"/> of strings from a specified samples file.
		/// </summary>
		/// <param name="path">Path to the samples file.</param>
		/// <param name="commentSpecifier">A string which is used to denote comments in the samples file (i.e., the lines
		/// that will not be used to build the <see cref="MarkovChainMap{T}"/>).</param>
		/// <returns>A new <see cref="MarkovChainMap{T}"/> of strings from file specified by <paramref name="path"/>.
		/// </returns>
		/// <exception cref="ArgumentException"><paramref name="path"/> is an empty string.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> is null, or
		/// <paramref name="commentSpecifier"/> is null.</exception>
		/// <exception cref="DirectoryNotFoundException">Specified <pramref name="path"/> is invalid, such as being on an
		/// unmapped drive.</exception>
		/// <exception cref="FileNotFoundException">File specified by <paramref name="path"/> cannot be found.</exception>
		/// <exception cref="IOException"><paramref name="path"/> includes an incorrect or invalid syntax for file name,
		/// directory name, or volume label.</exception>
		/// <remarks>
		/// <para>A <see cref="StreamReader"/> will be used, which will read the text file line-by-line. Lines beginning
		/// with <paramref name="commentSpecifier"/> will not be used, as well as empty and whitespace lines.</para>
		/// </remarks>
		public MarkovChainMap<string> TrainFromFile(string path, string commentSpecifier = "%")
		{
			commentSpecifier = commentSpecifier ?? string.Empty;
			List<string> values = new List<string>();
			string value;
			using (StreamReader reader = new StreamReader(path))
			{
				while (!reader.EndOfStream)
				{
					value = (_skipWhitespace) ? ReadWord(reader) : reader.ReadLine();
					if (!string.IsNullOrWhiteSpace(value))
					{
						if (string.IsNullOrEmpty(commentSpecifier) || !value.StartsWith(commentSpecifier))
						{
							values.Add(value);
						}
					}
				}
			}
			return Train(values);
		}
		#endregion

		#region Static Methods
		private static string Join(IEnumerable<string> slices, int start, int count)
		{
			string result = string.Empty;
			switch (slices.Count())
			{
				case 0:
					break;
				case 1:
					result = slices.First();
					break;
				default:
					string[] values = slices.Skip(start).Take(count).ToArray();
					result = string.Join(string.Empty, values);
					//int length = values.Length;
					//StringBuilder sb = _sb.Value;
					//sb.Clear();
					//for (int i = 0; i < length; i++) {
					//	sb.Append(values[i]);
					//}
					//result = sb.ToString();
					break;
			}
			return result;
		}

		private static string ReadWord(StreamReader reader)
		{
			StringBuilder sb = _sb.Value;
			sb.Clear();
			char c;
			int read;
			while ((read = reader.Read()) != -1)
			{
				c = (char)read;
				if (char.IsWhiteSpace(c))
				{
					break;
				}
				sb.Append(c);
			}
			return sb.ToString();
		}

		private static string[] ToStringArray(string input)
		{
			List<string> characters = new List<string>(input.Length);
			foreach (char c in input)
			{
				characters.Add(c.ToString());
			}
			return characters.ToArray();
		}
		#endregion
		#endregion
	}
}