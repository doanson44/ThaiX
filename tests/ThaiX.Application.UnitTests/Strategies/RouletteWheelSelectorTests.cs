using ThaiX.Application.Strategies;

namespace ThaiX.Application.UnitTests.Strategies;

public sealed class RouletteWheelSelectorTests
{
    private static Dictionary<int, int> FullWeights() =>
        Enumerable.Range(1, 55).ToDictionary(n => n, n => 1);

    [Fact]
    public void SelectWithoutReplacement_WithEnoughWeights_ReturnsExactCountUniqueInRange()
    {
        // Arrange
        var weights = FullWeights();

        // Act
        var result = RouletteWheelSelector.SelectWithoutReplacement(weights, 6, new Random(1234));

        // Assert
        result.Should().HaveCount(6);
        result.Should().OnlyHaveUniqueItems();
        result.Should().OnlyContain(n => n >= 1 && n <= 55);
    }

    [Fact]
    public void SelectWithoutReplacement_WithFixedSeed_IsDeterministic()
    {
        // Arrange
        var weights = FullWeights();

        // Act
        var first = RouletteWheelSelector.SelectWithoutReplacement(weights, 6, new Random(42));
        var second = RouletteWheelSelector.SelectWithoutReplacement(weights, 6, new Random(42));

        // Assert
        first.Should().Equal(second);
    }

    [Fact]
    public void SelectWithoutReplacement_ZeroWeightNumbers_AreNeverSelected()
    {
        // Arrange: only numbers 1-10 carry weight
        var weights = Enumerable.Range(1, 55)
            .ToDictionary(n => n, n => n <= 10 ? 5 : 0);

        // Act + Assert: run many fixed seeds
        for (var seed = 0; seed < 200; seed++)
        {
            var result = RouletteWheelSelector.SelectWithoutReplacement(weights, 6, new Random(seed));
            result.Should().OnlyContain(n => n >= 1 && n <= 10);
        }
    }

    [Fact]
    public void SelectWithoutReplacement_HigherWeight_SelectedMoreOften_AcrossFixedSeeds()
    {
        // Arrange
        var weights = new Dictionary<int, int> { [1] = 990, [2] = 10 };
        var ones = 0;
        var twos = 0;

        // Act
        for (var seed = 0; seed < 2000; seed++)
        {
            var result = RouletteWheelSelector.SelectWithoutReplacement(weights, 1, new Random(seed));
            if (result[0] == 1)
            {
                ones++;
            }
            else
            {
                twos++;
            }
        }

        // Assert: number 1 (990/1000 weight) dominates number 2 (10/1000)
        ones.Should().BeGreaterThan(1900);
        twos.Should().BeLessThan(100);
    }

    [Fact]
    public void SelectWithoutReplacement_WhenPositiveWeightsFewerThanCount_FillsFromRemainingAscending()
    {
        // Arrange
        var weights = new Dictionary<int, int> { [10] = 5, [20] = 5, [30] = 5 };

        // Act
        var result = RouletteWheelSelector.SelectWithoutReplacement(weights, 6, new Random(7));

        // Assert: the 3 positive-weight numbers plus the lowest remaining (1, 2, 3)
        result.Should().HaveCount(6);
        result.Should().OnlyHaveUniqueItems();
        result.Should().Contain(new[] { 10, 20, 30 });
        result.Should().Contain(new[] { 1, 2, 3 });
    }

    [Fact]
    public void SelectWithoutReplacement_WhenAllWeightsZero_FallsBackToAscendingFill()
    {
        // Arrange
        var weights = Enumerable.Range(1, 55).ToDictionary(n => n, _ => 0);

        // Act
        var result = RouletteWheelSelector.SelectWithoutReplacement(weights, 6, new Random(7));

        // Assert
        result.Should().Equal(1, 2, 3, 4, 5, 6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(56)]
    public void SelectWithoutReplacement_InvalidCount_Throws(int count)
    {
        // Arrange
        var act = () => RouletteWheelSelector.SelectWithoutReplacement(FullWeights(), count, new Random(1));

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
