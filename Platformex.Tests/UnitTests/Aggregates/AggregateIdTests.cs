using FluentAssertions;
using Platformex.Tests.TestHelpers;
using Xunit;

namespace Platformex.Tests.UnitTests.Agregates
{
    public class AggregateIdTests
    {
        [Fact]
        public void ManuallyCreatedIsOk()
        {
            // Arrange
            const string value = "thingy-d15b1562-11f2-4645-8b1a-f8b946b566d3";

            // Act
            var testId = ThingyId.With(value);

            // Test
            testId.Value.Should().Be(value);
        }

        [Fact]
        public void CreatedIsDifferent()
        {
            // Act
            var id1 = ThingyId.New;
            var id2 = ThingyId.New;

            // Assert
            id1.Value.Should().NotBe(id2.Value);
        }

        [Fact]
        public void SameIdsAreEqual()
        {
            // Arrange
            const string value = "thingy-d15b1562-11f2-4645-8b1a-f8b946b566d3";
            var id1 = ThingyId.With(value);
            var id2 = ThingyId.With(value);

            // Assert
            id1.Equals(id2).Should().BeTrue();
            (id1 == id2).Should().BeTrue();
        }

        [Fact]
        public void DifferentAreNotEqual()
        {
            // Arrange
            var id1 = ThingyId.With("thingy-7ddc487f-02ad-4be3-a6ef-71203d333c61");
            var id2 = ThingyId.With("thingy-d15b1562-11f2-4645-8b1a-f8b946b566d3");

            // Assert
            id1.Equals(id2).Should().BeFalse();
            (id1 == id2).Should().BeFalse();
        }
    }
}
