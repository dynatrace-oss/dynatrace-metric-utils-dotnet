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

using System;
using System.Runtime.Serialization;

namespace Dynatrace.MetricUtils
{
	/// <summary>Exception thrown if errors appear in this library.</summary>
	public class MetricException : ArgumentException
	{
		/// <summary>Default constructor.</summary>
		public MetricException()
		{
		}

		/// <summary>
		/// Create a <see cref="MetricException" /> with the given message. Passed on to the base class (
		/// <see cref="ArgumentException" />).
		/// </summary>
		public MetricException(string message) : base(message)
		{
		}

		/// <summary>
		/// Create a <see cref="MetricException" /> with the given message and inner exception. Passed on to the base
		/// class ( <see cref="ArgumentException" />).
		/// </summary>
		public MetricException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Create a <see cref="MetricException" />  Serialization info and Streaming context. Passed on to the base class
		/// ( <see cref="ArgumentException" />).
		/// </summary>
		protected MetricException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
