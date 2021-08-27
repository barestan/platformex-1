using System;
using FluentAssertions;
using Platformex.Tests.TestHelpers;
using Xunit;

namespace Platformex.Tests.UnitTests.Identity
{
    public class IdentityTests : Test
    {
        [Fact]
        public void NewDeterministic_ReturnsKnownResult()
        {
            // Arrange
            var namespaceId = Guid.Parse("769077C6-F84D-46E3-AD2E-828A576AAAF3");
            const string name = "fantastic 4";

            // Arrange
            var testId = ThingyId.NewDeterministic(namespaceId, name);

            // Assert
            testId.Value.Should().Be("thingy-da7ab6b1-c513-581f-a1a0-7cdf17109deb");
            ThingyId.IsValid(testId.Value).Should().BeTrue();
        }

        [Theory]
        [InlineData("thingy-da7ab6b1-c513-581f-a1a0-7cdf17109deb", "da7ab6b1-c513-581f-a1a0-7cdf17109deb")]
        [InlineData("thingy-00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
        public void WithValidValue(string value, string expectedGuidValue)
        {
            // Arrange
            ThingyId thingyId = null;
            var expectedGuid = Guid.Parse(expectedGuidValue);

            // Act
            var exception = Record.Exception(() => thingyId = ThingyId.With(value));
            Assert.Null(exception);

            // Assert
            thingyId.Should().NotBeNull();
            thingyId.Value.Should().Be(value);
            thingyId.GetGuid().Should().Be(expectedGuid);
        }

        [Fact]
        public void InputOutput()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var thingyId = ThingyId.With(guid);

            // Assert
            thingyId.GetGuid().Should().Be(guid);
        }

        [Fact]
        public void ShouldBeLowerCase()
        {
            // Act
            var testId = ThingyId.New;

            // Assert
            testId.Value.Should().Be(testId.Value.ToLowerInvariant());
        }

        [Fact]
        public void New_IsValid()
        {
            // Arrange
            var testId = ThingyId.New;

            // Assert
            ThingyId.IsValid(testId.Value).Should().BeTrue(testId.Value);
        }

        [Fact]
        public void NewComb_IsValid()
        {
            // Arrange
            var testId = ThingyId.NewComb();

            // Assert
            ThingyId.IsValid(testId.Value).Should().BeTrue(testId.Value);
        }

        [Fact]
        public void NewDeterministic_IsValid()
        {
            // Arrange
            var testId = ThingyId.NewDeterministic(Guid.NewGuid(), Guid.NewGuid().ToString());

            // Assert
            ThingyId.IsValid(testId.Value).Should().BeTrue(testId.Value);
        }

        [Theory]
        [InlineData("da7ab6b1-c513-581f-a1a0-7cdf17109deb")]
        [InlineData("thingyid-da7ab6b1-c513-581f-a1a0-7cdf17109deb")]
        [InlineData("thingy-769077C6-F84D-46E3-AD2E-828A576AAAF3")]
        [InlineData("thingy-pppppppp-pppp-pppp-pppp-pppppppppppp")]
        [InlineData("funny-da7ab6b1-c513-581f-a1a0-7cdf17109deb")]
        [InlineData(null)]
        [InlineData("")]
        public void CannotCreateBadIds(string badIdValue)
        {
            // Act
            Assert.Throws<ArgumentException>(() => ThingyId.With(badIdValue)).Message.Should().Contain("Identity is invalid:");
        }

        public class Id : Identity<Id>
        {
            public Id(string value) : base(value)
            {
            }
        }

        [Fact]
        public void JustId()
        {
            // Arrange
            var guid = A<Guid>();
            var expected = guid.ToString("D");

            // Act
            var id = Id.With(guid);

            // Assert
            id.Value.Should().Be(expected);
        }
    }
}
