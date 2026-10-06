using ThaiX.Application.Common.Extensions;

namespace ThaiX.Application.UnitTests.Common.Extensions;

public sealed class PredicateExtensionsTests
{
    [Fact]
    public void True_ShouldReturnPredicateThatAlwaysReturnsTrue()
    {
        // Arrange
        var predicate = PredicateExtensions.True<int>();
        var compiled = predicate.Compile();

        // Assert
        compiled(0).Should().BeTrue();
        compiled(42).Should().BeTrue();
        compiled(-1).Should().BeTrue();
    }

    [Fact]
    public void False_ShouldReturnPredicateThatAlwaysReturnsFalse()
    {
        // Arrange
        var predicate = PredicateExtensions.False<int>();
        var compiled = predicate.Compile();

        // Assert
        compiled(0).Should().BeFalse();
        compiled(42).Should().BeFalse();
        compiled(-1).Should().BeFalse();
    }

    [Fact]
    public void And_ShouldCombineWithLogicalAnd()
    {
        // Arrange
        var greaterThan5 = PredicateExtensions.True<int>().And(x => x > 5);
        var lessThan10 = greaterThan5.And(x => x < 10);
        var compiled = lessThan10.Compile();

        // Assert
        compiled(7).Should().BeTrue();
        compiled(3).Should().BeFalse();
        compiled(12).Should().BeFalse();
    }

    [Fact]
    public void Or_ShouldCombineWithLogicalOr()
    {
        // Arrange
        var lessThan3 = PredicateExtensions.False<int>().Or(x => x < 3);
        var greaterThan7 = lessThan3.Or(x => x > 7);
        var compiled = greaterThan7.Compile();

        // Assert
        compiled(1).Should().BeTrue();
        compiled(9).Should().BeTrue();
        compiled(5).Should().BeFalse();
    }

    [Fact]
    public void Not_ShouldNegateExpression()
    {
        // Arrange
        var isEven = PredicateExtensions.True<int>().And(x => x % 2 == 0);
        var isOdd = isEven.Not();
        var compiled = isOdd.Compile();

        // Assert
        compiled(1).Should().BeTrue();
        compiled(3).Should().BeTrue();
        compiled(2).Should().BeFalse();
        compiled(4).Should().BeFalse();
    }

    [Fact]
    public void And_WithStringPredicates_ShouldWorkCorrectly()
    {
        // Arrange
        var startsWithA = PredicateExtensions.True<string>().And(s => s.StartsWith("A"));
        var longerThan3 = startsWithA.And(s => s.Length > 3);
        var compiled = longerThan3.Compile();

        // Assert
        compiled("Apple").Should().BeTrue();
        compiled("Ant").Should().BeFalse();    // too short
        compiled("Banana").Should().BeFalse(); // doesn't start with A
    }

    [Fact]
    public void Or_WithStringPredicates_ShouldWorkCorrectly()
    {
        // Arrange
        var startsWithA = PredicateExtensions.False<string>().Or(s => s.StartsWith("A"));
        var startsWithB = startsWithA.Or(s => s.StartsWith("B"));
        var compiled = startsWithB.Compile();

        // Assert
        compiled("Apple").Should().BeTrue();
        compiled("Banana").Should().BeTrue();
        compiled("Cherry").Should().BeFalse();
    }

    [Fact]
    public void ComplexCombination_ShouldWorkCorrectly()
    {
        // Build: (x > 0 AND x < 100) OR x == -1
        var inRange = PredicateExtensions.True<int>()
            .And(x => x > 0)
            .And(x => x < 100);
        var isMinusOne = PredicateExtensions.True<int>().And(x => x == -1);
        var combined = inRange.Or(isMinusOne);
        var compiled = combined.Compile();

        // Assert
        compiled(50).Should().BeTrue();
        compiled(-1).Should().BeTrue();
        compiled(0).Should().BeFalse();
        compiled(100).Should().BeFalse();
        compiled(-2).Should().BeFalse();
    }
}
