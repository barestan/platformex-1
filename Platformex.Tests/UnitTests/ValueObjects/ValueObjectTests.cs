using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Platformex.Tests.UnitTests.ValueObjects
{
    public class ValueObjectTests : Test
    {
        public class StringObject : ValueObject
        {
            public string StringValue { get; set; }
        }

        public class ListObject : ValueObject
        {
            public List<StringObject> StringValues { get; set; }

            public ListObject(params string[] strings)
            {
                StringValues = strings.Select(s => new StringObject{StringValue = s}).ToList();
            }

            protected override IEnumerable<object> GetEqualityComponents()
            {
                return StringValues;
            }
        }

        [Fact]
        public void SameStringObjectsAreEqual()
        {
            // Arrange
            var str = A<string>();
            var stringObject1 = new StringObject { StringValue = str };
            var stringObject2 = new StringObject { StringValue = str };

            // Assert
            stringObject1.GetHashCode().Should().Be(stringObject2.GetHashCode());
            stringObject1.Equals(stringObject2).Should().BeTrue();
            (stringObject1 == stringObject2).Should().BeTrue();
        }

        [Fact]
        public void DifferentStringObjectsAreNotEqual()
        {
            // Arrange
            var stringObject1 = new StringObject { StringValue = A<string>() };
            var stringObject2 = new StringObject { StringValue = A<string>() };

            // Assert
            stringObject1.GetHashCode().Should().NotBe(stringObject2.GetHashCode());
            stringObject1.Equals(stringObject2).Should().BeFalse();
            (stringObject1 == stringObject2).Should().BeFalse();
        }

        [Fact]
        public void SameListObjectsAreEqual()
        {
            // Arrange
            var values = Many<string>().ToArray();
            var listObject1 = new ListObject(values);
            var listObject2 = new ListObject(values);

            // Assert
            listObject1.GetHashCode().Should().Be(listObject2.GetHashCode(), "hash code");
            listObject1.Equals(listObject2).Should().BeTrue("Equals");
            (listObject1 == listObject2).Should().BeTrue("==");
        }

        [Fact]
        public void DifferentListObjectsAreNotEqual()
        {
            // Arrange
            var listObject1 = new ListObject(Many<string>().ToArray());
            var listObject2 = new ListObject(Many<string>().ToArray());

            // Assert
            listObject1.GetHashCode().Should().NotBe(listObject2.GetHashCode(), "hash code");
            listObject1.Equals(listObject2).Should().BeFalse("Equals");
            (listObject1 == listObject2).Should().BeFalse("==");
        }
    }
}
