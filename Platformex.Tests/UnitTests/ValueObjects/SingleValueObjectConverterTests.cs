using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Platformex.Tests.UnitTests.ValueObjects
{
    public class SingleValueObjectConverterTests : Test
    {
        public enum MagicEnum
        {
            Two = 2,
            Zero = 0,
            Three = 3,
            One = 1
        }
        
        [Theory]
        [InlineData("test  test", "\"test  test\"")]
        [InlineData("42", "\"42\"")]
        [InlineData("", "\"\"")]
        [InlineData(null, "null")]
        public void StringSerilization(string value, string expectedJson)
        {
            // Arrange
            var stringSvo = new StringSvo(value);

            // Act
            var json = JsonConvert.SerializeObject(stringSvo);

            // Assert
            json.Should().Be(expectedJson);
        }

        [Fact]
        public void StringDeserializationEmptyShouldResultInNull()
        {
            // Act
            var stringSvo = JsonConvert.DeserializeObject<StringSvo>(string.Empty);

            // Assert
            stringSvo.Should().BeNull();
        }

        [Theory]
        [InlineData("\"\"", "")]
        [InlineData("\"test\"", "test")]
        public void StringDeserialization(string json, string expectedValue)
        {
            // Act
            var stringSvo = JsonConvert.DeserializeObject<StringSvo>(json);

            // Assert
            stringSvo?.Value.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(0, "0")]
        [InlineData(42, "42")]
        [InlineData(-1, "-1")]
        public void IntSerialization(int value, string expectedJson)
        {
            // Arrange
            var intSvo = new IntSvo(value);

            // Act
            var json = JsonConvert.SerializeObject(intSvo);

            // Assert
            json.Should().Be(expectedJson);
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData("42", 42)]
        [InlineData("-1", -1)]
        public void IntDeserialization(string json, int expectedValue)
        {
            // Act
            var intSvo = JsonConvert.DeserializeObject<IntSvo>(json);

            // Assert
            intSvo?.Value.Should().Be(expectedValue);
        }
        
        [Theory]
        [InlineData("\"One\"", MagicEnum.One)]
        [InlineData("1", MagicEnum.One)]
        [InlineData("2", MagicEnum.Two)]
        public void EnumDeserilization(string json, MagicEnum expectedValue)
        {
            // Act
            var intSvo = JsonConvert.DeserializeObject<EnumSvo>(json);

            // Assert
            intSvo?.Value.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData((int)MagicEnum.Zero, "0")]
        [InlineData((int)MagicEnum.One, "1")]
        [InlineData((int)MagicEnum.Two, "2")]
        [InlineData((int)MagicEnum.Three, "3")]
        public void EnumSerialization(int value, string expectedJson)
        {
            // Arrange
            var intSvo = new IntSvo(value);

            // Act
            var json = JsonConvert.SerializeObject(intSvo);

            // Assert
            json.Should().Be(expectedJson);
        }
       
        [JsonConverter(typeof(SingleValueObjectConverter))]
        public class StringSvo : SingleValueObject<string>
        {
            public StringSvo(string value) : base(value) { }
        }

        [JsonConverter(typeof(SingleValueObjectConverter))]
        public class IntSvo : SingleValueObject<int>
        {
            public IntSvo(int value) : base(value) { }
        }
        
        [JsonConverter(typeof(SingleValueObjectConverter))]
        public class EnumSvo : SingleValueObject<MagicEnum>
        {
            public EnumSvo(MagicEnum value) : base(value) { }
        }
    }
}
