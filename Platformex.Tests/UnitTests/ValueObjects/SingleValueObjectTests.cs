using System;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Platformex.Tests.UnitTests.ValueObjects
{
public class SingleValueObjectTests : Test
    {
        public class StringSingleValue : SingleValueObject<string>
        {
            public StringSingleValue(string value) : base(value) { }
        }

        public enum MagicEnum
        {
            Two = 2,
            Zero = 0,
            Three = 3,
            One = 1
        }

        public class MagicEnumSingleValue : SingleValueObject<MagicEnum>
        {
            public MagicEnumSingleValue(MagicEnum value) : base(value) { }
        }

        [Fact]
        public void Ordering()
        {
            // Arrange
            var values = Many<string>(10);
            var orderedValues = values.OrderBy(s => s).ToList();
            values.Should().NotEqual(orderedValues); // Data test
            var singleValueObjects = values.Select(s => new StringSingleValue(s)).ToList();

            // Act
            var orderedSingleValueObjects = singleValueObjects.OrderBy(v => v).ToList();

            // Assert
            orderedSingleValueObjects.Select(v => v.Value).Should().BeEquivalentTo(
                orderedValues,
                o => o.WithStrictOrdering());
        }

        [Fact]
        public void EnumOrdering()
        {
            // Arrange
            var values = Many<MagicEnum>(10);
            var orderedValues = values.OrderBy(s => s).ToList();
            values.Should().NotEqual(orderedValues); // Data test
            var singleValueObjects = values.Select(s => new MagicEnumSingleValue(s)).ToList();

            // Act
            var orderedSingleValueObjects = singleValueObjects.OrderBy(v => v).ToList();

            // Assert
            orderedSingleValueObjects.Select(v => v.Value).Should().BeEquivalentTo(
                orderedValues,
                o => o.WithStrictOrdering());
        }

        [Fact]
        public void ProtectAgainsInvalidEnumValues()
        {
            // Act + Assert
            // ReSharper disable once ObjectCreationAsStatement
            var exception = Assert.Throws<ArgumentException>(() => new MagicEnumSingleValue((MagicEnum)42));
            exception.Message.Should().Be("The value '42' isn't defined in enum 'MagicEnum'");
        }

        [Fact]
        public void EnumOrderingManual()
        {
            // Arrange
            var values = new[]
                {
                    new MagicEnumSingleValue(MagicEnum.Zero), 
                    new MagicEnumSingleValue(MagicEnum.Three), 
                    new MagicEnumSingleValue(MagicEnum.One), 
                    new MagicEnumSingleValue(MagicEnum.Two), 
                };
            
            // Act
            var orderedValues = values
                .OrderBy(v => v)
                .Select(v => v.Value)
                .ToList();
            
            // Assert
            orderedValues.Should().BeEquivalentTo(
                new []
                {
                    MagicEnum.Zero,
                    MagicEnum.One,
                    MagicEnum.Two,
                    MagicEnum.Three,
                },
                o => o.WithStrictOrdering());
        }

        [Fact]
        public void NullEquals()
        {
            // Arrange
            var obj = new StringSingleValue(A<string>());
            var @null = null as StringSingleValue;

            // Assert
            // ReSharper disable once ExpressionIsAlwaysNull
            obj.Equals(@null).Should().BeFalse();
        }

        [Fact]
        public void EqualsForSameValues()
        {
            // Arrange
            var value = A<string>();
            var obj1 = new StringSingleValue(value);
            var obj2 = new StringSingleValue(value);

            // Assert
            (obj1 == obj2).Should().BeTrue();
            obj1.Equals(obj2).Should().BeTrue();
        }

        [Fact]
        public void EqualsForDifferentValues()
        {
            // Arrange
            var value1 = A<string>();
            var value2 = A<string>();
            var obj1 = new StringSingleValue(value1);
            var obj2 = new StringSingleValue(value2);

            // Assert
            (obj1 == obj2).Should().BeFalse();
            obj1.Equals(obj2).Should().BeFalse();
        }

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

        [JsonConverter(typeof(SingleValueObjectConverter))]
        public class IntSingleValue : SingleValueObject<int>
        {
            public IntSingleValue(int value) : base(value) { }
        }

        public class WithNullableIntSingleValue
        {
            public IntSingleValue I { get; }

            public WithNullableIntSingleValue(
                IntSingleValue i)
            {
                I = i;
            }
        }

        [Fact]
        public void DeserializeNullableIntWithoutValue()
        {
            // Arrange
            var json = JsonConvert.SerializeObject(new { });

            // Act
            var with = JsonConvert.DeserializeObject<WithNullableIntSingleValue>(json);

            // Assert
            with.Should().NotBeNull();
            with?.I.Should().BeNull();
        }

        [Fact]
        public void DeserializeNullableIntWithNullValue()
        {
            // Arrange
            var json = "{\"i\":null}";

            // Act
            var with = JsonConvert.DeserializeObject<WithNullableIntSingleValue>(json);

            // Assert
            with.Should().NotBeNull();
            with?.I.Should().BeNull();
        }

        [Fact]
        public void DeserializeNullableIntWithValue()
        {
            // Arrange
            var i = A<int>();
            var json = JsonConvert.SerializeObject(new { i });

            // Act
            var with = JsonConvert.DeserializeObject<WithNullableIntSingleValue>(json);

            // Assert
            with.Should().NotBeNull();
            with?.I.Value.Should().Be(i);
        }

        [Fact]
        public void SerializeNullableIntWithoutValue()
        {
            // Arrange
            var with = new WithNullableIntSingleValue(null);

            // Act
            var json = JsonConvert.SerializeObject(with, Settings);

            // Assert
            json.Should().Be("{}");
        }

        [Fact]
        public void SerializeNullableIntWithValue()
        {
            // Arrange
            var with = new WithNullableIntSingleValue(new IntSingleValue(42));

            // Act
            var json = JsonConvert.SerializeObject(with, Settings);

            // Assert
            json.Should().Be("{\"I\":42}");
        }
    }
}
