using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Platformex.Tests.UnitTests.Aggregates
{
    public class MetadataTests : Test
    {
        [Fact]
        public void TimestampIsSerializedCorrectly()
        {
            // Arrange
            var timestamp = A<DateTimeOffset>();

            // Act
            var sut = new Metadata
                {
                    Timestamp = timestamp
                };

            // Assert
            sut.Timestamp.Should().Be(timestamp);
        }

        [Fact]
        public void EventNameIsSerializedCorrectly()
        {
            // Arrange
            var eventName = A<string>();

            // Act
            var sut = new Metadata
                {
                    EventName = eventName
                };

            // Assert
            sut.EventName.Should().Be(eventName);
        }

        [Fact]
        public void EventVersionIsSerializedCorrectly()
        {
            // Arrange
            var eventVersion = A<int>();

            // Act
            var sut = new Metadata
                {
                    EventVersion = eventVersion
                };

            // Assert
            sut.EventVersion.Should().Be(eventVersion);
        }

        [Fact]
        public void AggregateSequenceNumberIsSerializedCorrectly()
        {
            // Arrange
            var aggregateSequenceNumber = A<int>();

            // Act
            var sut = new Metadata
                {
                    AggregateSequenceNumber = aggregateSequenceNumber
                };

            // Assert
            sut.AggregateSequenceNumber.Should().Be(aggregateSequenceNumber);
        }

        [Fact]
        public void CloneWithCanMerge()
        {
            // Arrange
            var key1 = A<string>();
            var key2 = A<string>();
            var value1 = A<string>();
            var value2 = A<string>();

            // Act
            var metadata1 = new Metadata { [key1] = value1 };
            var metadata2 = metadata1.CloneWith(new KeyValuePair<string, string>(key2, value2));

            // Assert
            metadata1.ContainsKey(key2).Should().BeFalse();

            metadata2.ContainsKey(key1).Should().BeTrue();
            metadata2.ContainsKey(key2).Should().BeTrue();
            metadata2[key1].Should().Be(value1);
            metadata2[key2].Should().Be(value2);
        }

        [Fact]
        public void SerializeDeserializeWithValues()
        {
            // Arrange
            var aggregateName = A<string>();
            var aggregateSequenceNumber = A<int>();
            var timestamp = A<DateTimeOffset>();
            var sut = new Metadata
                {
                    { MetadataKeys.AggregateName, aggregateName },
                    { MetadataKeys.AggregateSequenceNumber, aggregateSequenceNumber.ToString() },
                    { MetadataKeys.Timestamp, timestamp.ToString("O") }
                };

            // Act
            var json = JsonConvert.SerializeObject(sut);
            var metadata = JsonConvert.DeserializeObject<Metadata>(json);

            // Assert
            metadata?.Count.Should().Be(3);
            metadata?.AggregateName.Should().Be(aggregateName);
            metadata?.AggregateSequenceNumber.Should().Be(aggregateSequenceNumber);
            metadata?.Timestamp.Should().Be(timestamp);
        }

        [Fact]
        public void SerializeDeserializeEmpty()
        {
            // Arrange
            var sut = new Metadata();

            // Act
            var json = JsonConvert.SerializeObject(sut);
            var metadata = JsonConvert.DeserializeObject<Metadata>(json);

            // Assert
            json.Should().Be("{}");
            metadata?.Count.Should().Be(0);
        }
    }
}
