namespace Serpent5.Core.Tests;

public class ArgumentExceptionExtensionsTests
{
    [Fact]
    public void ThrowsArgumentNullExceptionWhenInputIsNull()
    {
        string inputValue = null!;
        Assert.Throws<ArgumentNullException>(nameof(inputValue), () => ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(inputValue));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowsArgumentExceptionWhenInputIsEmptyOrWhiteSpace(string inputValue)
        => Assert.Throws<ArgumentException>(nameof(inputValue), () => ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(inputValue));

    [Fact]
    public void DoesNotThrowExceptionWhenInputIsNotNullEmptyOrWhiteSpace()
        => Assert.Null(Record.Exception(() => ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace("AnyNotNullEmptyOrWhiteSpaceString")));
}
