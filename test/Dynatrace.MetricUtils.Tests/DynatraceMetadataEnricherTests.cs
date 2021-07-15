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
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Dynatrace.MetricUtils.Tests
{
	public class DynatraceMetadataEnricherTests
	{
		private static readonly ILogger Logger = NullLogger<DynatraceMetadataEnricher>.Instance;

		[Fact]
		public void ValidMultiline()
		{
			var enricher = new DynatraceMetadataEnricher(Logger);
			var metadata = enricher.ProcessMetadata(new[] {"a=123", "b=456"});
			metadata.Should().Equal(new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("a", "123"), new KeyValuePair<string, string>("b", "456")
			});
		}

		[Fact]
		public void WrongSyntax()
		{
			var enricher = new DynatraceMetadataEnricher(Logger);
			enricher.ProcessMetadata(new[] {"=0x5c14d9a68d569861"}).Should().BeEmpty();
			enricher.ProcessMetadata(new[] {"otherKey="}).Should().BeEmpty();
			enricher.ProcessMetadata(new[] {""}).Should().BeEmpty();
			enricher.ProcessMetadata(new[] {"="}).Should().BeEmpty();
			enricher.ProcessMetadata(new[] {"==="}).Should().BeEmpty();
			enricher.ProcessMetadata(new string[] { }).Should().BeEmpty();
		}

		[Fact]
		public void IndirectionFileMissing()
		{
			var fileReader = Mock.Of<IFileReader>();
			Mock.Get(fileReader).Setup(f => f.ReadAllText(It.IsAny<string>())).Throws<FileNotFoundException>();

			var targetList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue")
			};
			var unitUnderTest = new DynatraceMetadataEnricher(Logger, fileReader);

			unitUnderTest.EnrichWithDynatraceMetadata(targetList);
			// contains only the element that was in the list before.
			targetList.Should()
				.Equal(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue")
				});
			Mock.Get(fileReader)
				.Verify(mock => mock.ReadAllText("dt_metadata_e617c525669e072eebe3d0f08212e8f2.properties"),
					Times.Once());
		}

		[Fact]
		public void IndirectionFileNotAccessible()
		{
			var fileReader = Mock.Of<IFileReader>();
			Mock.Get(fileReader).Setup(f => f.ReadAllText(It.IsAny<string>())).Throws<UnauthorizedAccessException>();
			var kv = new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue");
			var targetList = new List<KeyValuePair<string, string>> {kv};

			var unitUnderTest = new DynatraceMetadataEnricher(Logger, fileReader);
			unitUnderTest.EnrichWithDynatraceMetadata(targetList);

			targetList.Should()
				.Equal(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue")
				});
			Mock.Get(fileReader).Verify(f => f.ReadAllText("dt_metadata_e617c525669e072eebe3d0f08212e8f2.properties"),
				Times.Once());
		}

		[Fact]
		public void IndirectionFileAnyOtherException()
		{
			var fileReader = Mock.Of<IFileReader>();
			// there is a whole host of exceptions that can be thrown by ReadAllText: https://docs.microsoft.com/en-us/dotnet/api/system.io.file.readalltext?view=net-5.0
			Mock.Get(fileReader).Setup(f => f.ReadAllText(It.IsAny<string>())).Throws<Exception>();
			var kv = new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue");
			var targetList = new List<KeyValuePair<string, string>> {kv};

			var unitUnderTest = new DynatraceMetadataEnricher(Logger, fileReader);
			unitUnderTest.EnrichWithDynatraceMetadata(targetList);

			targetList.Should()
				.Equal(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue")
				});
			Mock.Get(fileReader).Verify(f => f.ReadAllText("dt_metadata_e617c525669e072eebe3d0f08212e8f2.properties"),
				Times.Once());
		}

		[Fact]
		public void IndirectionFileEmpty()
		{
			var fileReader = Mock.Of<IFileReader>();
			Mock.Get(fileReader).Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("");
			var kv = new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue");
			var targetList = new List<KeyValuePair<string, string>> {kv};

			var unitUnderTest = new DynatraceMetadataEnricher(Logger, fileReader);
			unitUnderTest.EnrichWithDynatraceMetadata(targetList);

			targetList.Should()
				.Equal(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue")
				});
			Mock.Get(fileReader).Verify(f => f.ReadAllText("dt_metadata_e617c525669e072eebe3d0f08212e8f2.properties"),
				Times.Once());
			// if the metadata file is empty, there should be no attempt at reading the contents.
			Mock.Get(fileReader).Verify(f => f.ReadAllLines(""), Times.Never());
		}

		[Fact]
		public void IndirectionFileContainsAdditionalText()
		{
			var fileReader = Mock.Of<IFileReader>();
			var indirectionFileContent =
				@"indirection_file_name.properties
            some other text
            and some more text";

			Mock.Get(fileReader).Setup(f => f.ReadAllText(It.IsAny<string>())).Returns(indirectionFileContent);
			var kv = new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue");
			var targetList = new List<KeyValuePair<string, string>> {kv};

			var unitUnderTest = new DynatraceMetadataEnricher(Logger, fileReader);
			unitUnderTest.EnrichWithDynatraceMetadata(targetList);

			targetList.Should()
				.Equal(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue")
				});
			Mock.Get(fileReader).Verify(f => f.ReadAllText("dt_metadata_e617c525669e072eebe3d0f08212e8f2.properties"),
				Times.Once());
			// if the metadata file is empty, there should be no attempt at reading the contents.
			Mock.Get(fileReader).Verify(f => f.ReadAllLines(indirectionFileContent), Times.Once());
		}

		[Fact]
		public void IndirectionTargetMissing()
		{
			var fileReader = Mock.Of<IFileReader>();
			Mock.Get(fileReader).Setup(f => f.ReadAllText(It.IsAny<string>()))
				.Returns("indirection_file_name.properties");
			Mock.Get(fileReader).Setup(f => f.ReadAllLines(It.IsAny<string>())).Throws<FileNotFoundException>();
			var kv = new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue");
			var targetList = new List<KeyValuePair<string, string>> {kv};

			var unitUnderTest = new DynatraceMetadataEnricher(Logger, fileReader);
			unitUnderTest.EnrichWithDynatraceMetadata(targetList);

			targetList.Should()
				.Equal(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue")
				});
			Mock.Get(fileReader).Verify(f => f.ReadAllText("dt_metadata_e617c525669e072eebe3d0f08212e8f2.properties"),
				Times.Once());
			Mock.Get(fileReader).Verify(f => f.ReadAllLines("indirection_file_name.properties"), Times.Once());
		}

		[Fact]
		public void IndirectionTargetInvalidAccess()
		{
			var fileReader = Mock.Of<IFileReader>();
			Mock.Get(fileReader).Setup(f => f.ReadAllText(It.IsAny<string>()))
				.Returns("indirection_file_name.properties");
			Mock.Get(fileReader).Setup(f => f.ReadAllLines(It.IsAny<string>())).Throws<AccessViolationException>();
			var kv = new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue");
			var targetList = new List<KeyValuePair<string, string>> {kv};

			var unitUnderTest = new DynatraceMetadataEnricher(Logger, fileReader);
			unitUnderTest.EnrichWithDynatraceMetadata(targetList);

			targetList.Should()
				.Equal(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue")
				});
			Mock.Get(fileReader).Verify(f => f.ReadAllText("dt_metadata_e617c525669e072eebe3d0f08212e8f2.properties"),
				Times.Once());
			Mock.Get(fileReader).Verify(f => f.ReadAllLines("indirection_file_name.properties"), Times.Once());
		}

		[Fact]
		public void IndirectionTargetThrowsAnyOtherException()
		{
			var fileReader = Mock.Of<IFileReader>();
			Mock.Get(fileReader).Setup(f => f.ReadAllText(It.IsAny<string>()))
				.Returns("indirection_file_name.properties");
			Mock.Get(fileReader).Setup(f => f.ReadAllLines(It.IsAny<string>())).Throws<Exception>();
			var kv = new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue");
			var targetList = new List<KeyValuePair<string, string>> {kv};

			var unitUnderTest = new DynatraceMetadataEnricher(Logger, fileReader);
			unitUnderTest.EnrichWithDynatraceMetadata(targetList);

			targetList.Should()
				.Equal(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue")
				});
			Mock.Get(fileReader).Verify(f => f.ReadAllText("dt_metadata_e617c525669e072eebe3d0f08212e8f2.properties"),
				Times.Once());
			Mock.Get(fileReader).Verify(f => f.ReadAllLines("indirection_file_name.properties"), Times.Once());
		}

		[Fact]
		public void IndirectionTargetEmpty()
		{
			var fileReader = Mock.Of<IFileReader>();
			Mock.Get(fileReader).Setup(f => f.ReadAllText(It.IsAny<string>()))
				.Returns("indirection_file_name.properties");
			Mock.Get(fileReader).Setup(f => f.ReadAllLines(It.IsAny<string>())).Returns(Array.Empty<string>());
			var kv = new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue");
			var targetList = new List<KeyValuePair<string, string>> {kv};

			var unitUnderTest = new DynatraceMetadataEnricher(Logger, fileReader);
			unitUnderTest.EnrichWithDynatraceMetadata(targetList);

			targetList.Should()
				.Equal(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue")
				});
			Mock.Get(fileReader).Verify(f => f.ReadAllText("dt_metadata_e617c525669e072eebe3d0f08212e8f2.properties"),
				Times.Once());
			Mock.Get(fileReader).Verify(f => f.ReadAllLines("indirection_file_name.properties"), Times.Once());
		}

		[Fact]
		public void IndirectionTargetValid()
		{
			var fileReader = Mock.Of<IFileReader>();
			Mock.Get(fileReader).Setup(f => f.ReadAllText(It.IsAny<string>()))
				.Returns("indirection_file_name.properties");
			Mock.Get(fileReader).Setup(f => f.ReadAllLines(It.IsAny<string>()))
				.Returns(new[] {"key1=value1", "key2=value2"});
			var kv = new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue");
			var targetList = new List<KeyValuePair<string, string>> {kv};

			var unitUnderTest = new DynatraceMetadataEnricher(Logger, fileReader);
			unitUnderTest.EnrichWithDynatraceMetadata(targetList);

			targetList.Should().Equal(
				new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue"),
					new KeyValuePair<string, string>("key1", "value1"),
					new KeyValuePair<string, string>("key2", "value2")
				}
			);
			Mock.Get(fileReader).Verify(f => f.ReadAllText("dt_metadata_e617c525669e072eebe3d0f08212e8f2.properties"),
				Times.Once());
			Mock.Get(fileReader).Verify(f => f.ReadAllLines("indirection_file_name.properties"), Times.Once());
		}

		[Fact]
		public void IndirectionTargetValidWithInvalidLines()
		{
			var fileReader = Mock.Of<IFileReader>();
			Mock.Get(fileReader).Setup(f => f.ReadAllText(It.IsAny<string>()))
				.Returns("indirection_file_name.properties");
			Mock.Get(fileReader).Setup(f => f.ReadAllLines(It.IsAny<string>()))
				.Returns(new[] {"key1=value1", "key2=", "=value2", "==="});
			var kv = new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue");
			var targetList = new List<KeyValuePair<string, string>> {kv};

			var unitUnderTest = new DynatraceMetadataEnricher(Logger, fileReader);
			unitUnderTest.EnrichWithDynatraceMetadata(targetList);

			targetList.Should().Equal(
				new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("initialDimensionKey", "initialDimensionValue"),
					new KeyValuePair<string, string>("key1", "value1")
				}
			);
			Mock.Get(fileReader).Verify(f => f.ReadAllText("dt_metadata_e617c525669e072eebe3d0f08212e8f2.properties"),
				Times.Once());
			Mock.Get(fileReader).Verify(f => f.ReadAllLines("indirection_file_name.properties"), Times.Once());
		}
	}
}
