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

namespace Dynatrace.MetricUtils
{
	public static class DynatraceMetricApiConstants
	{
		private const string OneAgentEndpoint = "http://localhost:14499/metrics/ingest";
		private const int LinesLimit = 1000;
		private const int MaxDims = 50;

		/// <summary>
		/// The default local OneAgent endpoint to which metrics can be exported.
		/// See https://www.dynatrace.com/support/help/how-to-use-dynatrace/metrics/metric-ingestion/ingestion-methods/local-api/ for more information.
		/// </summary>
		public static string DefaultOneAgentEndpoint => OneAgentEndpoint;

		/// <summary>The maximum number of lines per metrics ingest request.</summary>
		public static int PayloadLinesLimit => LinesLimit;

		/// <summary>The maximum number of dimensions that can be attached to one metric key.</summary>
		public static int MaximumDimensions => MaxDims;
	}
}
