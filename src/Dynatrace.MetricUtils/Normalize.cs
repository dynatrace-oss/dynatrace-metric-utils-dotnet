// <copyright company="Dynatrace LLC">
// Copyright 2021 Dynatrace LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Dynatrace.MetricUtils
{
	/// <summary>
	/// Static class containing helper functions to normalize and escape metric keys, dimension keys, and dimension
	/// values.
	/// </summary>
	internal static class Normalize
	{
		private const int MaxLengthMetricKey = 250;
		private const int MaxLengthDimensionKey = 100;
		private const int MaxLengthDimensionValue = 250;

		//  Metric keys (mk)
		//  characters not valid as leading characters in the first identifier key section
		private static readonly Regex ReMkFirstIdentifierSectionInvalidStartRange =
			new Regex("^[^a-zA-Z_]+", RegexOptions.Compiled);

		// characters not valid as leading characters in subsequent subsections.
		private static readonly Regex ReMkSubsequentIdentifierSectionInvalidStartRange =
			new Regex("^[^a-zA-Z0-9_]+", RegexOptions.Compiled);

		// invalid characters for the rest of the key.
		private static readonly Regex ReMkInvalidCharacters = new Regex("[^a-zA-Z0-9_\\-]+", RegexOptions.Compiled);

		// Dimension keys (dk)
		// Dimension keys start with a lowercase letter or an underscore.
		private static readonly Regex ReDkSectionInvalidStartRange = new Regex("^[^a-z_]+", RegexOptions.Compiled);

		// invalid characters in the rest of the dimension key
		private static readonly Regex ReDkInvalidCharacters = new Regex("[^a-z0-9_\\-:]+", RegexOptions.Compiled);

		// Dimension values (dv)
		// Characters that need to be escaped in dimension values
		private static readonly Regex ReDvCharactersToEscape = new Regex("([= ,\\\\\"])", RegexOptions.Compiled);
		private static readonly Regex ReDvControlCharacters = new Regex("[\\p{C}]+", RegexOptions.Compiled);

		// This regex checks if there is an odd number of trailing backslashes in the string. It can be
		// read as: {not a slash}{any number of 2-slash pairs}{one slash}{end line}.
		private static readonly Regex ReDvHasOddNumberOfTrailingBackslashes =
			new Regex("[^\\\\](?:\\\\\\\\)*\\\\$", RegexOptions.Compiled);

		/// <summary>Transforms OpenTelemetry metric names into Dynatrace-compatible metric keys</summary>
		/// <returns>A valid Dynatrace metric key or null, if the input could not be normalized</returns>
		internal static string MetricKey(string key)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				return null;
			}

			if (key.Length > MaxLengthMetricKey)
			{
				key = key.Substring(0, MaxLengthMetricKey);
			}

			var sections = key.Split('.');
			if (sections.Length == 0)
			{
				return null;
			}

			var firstSection = true;
			var normalizedKeyBuilder = new StringBuilder();

			foreach (var section in sections)
			{
				// check only if it is empty, and ignore a null check.
				if (section.Length == 0)
				{
					if (firstSection)
					{
						return null;
					}

					// skip empty sections
					continue;
				}

				// first key section cannot start with a number while subsequent sections can.
				var normalizedSection = firstSection
					? ReMkFirstIdentifierSectionInvalidStartRange.Replace(section, "_")
					: ReMkSubsequentIdentifierSectionInvalidStartRange.Replace(section, "_");

				// replace invalid chars with an underscore
				normalizedSection = ReMkInvalidCharacters.Replace(normalizedSection, "_");

				// re-concatenate the split sections separated with dots.
				if (!firstSection)
				{
					normalizedKeyBuilder.Append(".");
				}
				else
				{
					firstSection = false;
				}

				normalizedKeyBuilder.Append(normalizedSection);
			}

			return normalizedKeyBuilder.ToString();
		}

		/// <summary>Normalize a dimension key</summary>
		/// <returns>The normalized dimension key.</returns>
		internal static string DimensionKey(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return "";
			}

			if (key.Length > MaxLengthDimensionKey)
			{
				key = key.Substring(0, MaxLengthDimensionKey);
			}

			var sections = key.Split('.');
			var normalizedKeyBuilder = new StringBuilder();
			var firstSection = true;

			foreach (var section in sections)
			{
				if (section.Length > 0)
				{
					// move to lowercase
					var normalizedSection = section.ToLower();
					// replace consecutive leading chars with an underscore.
					normalizedSection = ReDkSectionInvalidStartRange.Replace(normalizedSection, "_");
					// replace consecutive invalid characters within the section with one underscore:
					normalizedSection = ReDkInvalidCharacters.Replace(normalizedSection, "_");

					// re-concatenate the split sections separated with dots.
					if (!firstSection)
					{
						normalizedKeyBuilder.Append(".");
					}
					else
					{
						firstSection = false;
					}

					normalizedKeyBuilder.Append(normalizedSection);
				}
			}

			return normalizedKeyBuilder.ToString();
		}

		/// <summary>Normalize a dimension value</summary>
		/// <returns>The normalized dimension value.</returns>
		internal static string DimensionValue(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return "";
			}

			if (value.Length > MaxLengthDimensionValue)
			{
				value = value.Substring(0, MaxLengthDimensionValue);
			}

			// collapse invalid characters to an underscore.
			value = ReDvControlCharacters.Replace(value, "_");

			return value;
		}

		/// <summary>
		/// Escapes a dimension value. Escaped characters are (separated by a semicolon): = (equal sign); , (comma); \
		/// (backslash); " (double quotes)
		/// </summary>
		/// <returns>The escaped dimension value.</returns>
		internal static string EscapeDimensionValue(string value)
		{
			// escape characters matched by regex with backslash. $1 inserts the matched character.
			var escaped = ReDvCharactersToEscape.Replace(value, "\\$1");
			if (escaped.Length > MaxLengthDimensionValue)
			{
				escaped = escaped.Substring(0, MaxLengthDimensionValue);
				if (ReDvHasOddNumberOfTrailingBackslashes.IsMatch(escaped))
				{
					// string has trailing backslashes. Since every backslash must be escaped, there must be an
					// even number of backslashes, otherwise the substring operation cut an escaped character
					// in half: e.g.: "some_long_string," -> escaped: "some_long_string\," -> cut with substring
					// results in "some_long_string\" since the two slashes were on either side of the char
					// at which the string was cut using substring. If this is the case, trim the last
					// backslash character, resulting in a properly escaped string.
					escaped = escaped.Substring(0, MaxLengthDimensionValue - 1);
				}
			}

			return escaped;
		}

		/// <summary>Escapes all dimension keys and dimension values in the passed list of key-value pairs.</summary>
		internal static List<KeyValuePair<string, string>> DimensionList(
			IEnumerable<KeyValuePair<string, string>> dimensions)
		{
			if (dimensions == null)
			{
				return new List<KeyValuePair<string, string>>();
			}

			var targetList = new List<KeyValuePair<string, string>>();
			foreach (var dimension in dimensions)
			{
				var normalizedKey = DimensionKey(dimension.Key);
				if (!string.IsNullOrEmpty(normalizedKey))
				{
					targetList.Add(new KeyValuePair<string, string>(normalizedKey,
						EscapeDimensionValue(DimensionValue(dimension.Value))));
				}
			}

			return targetList;
		}
	}
}
