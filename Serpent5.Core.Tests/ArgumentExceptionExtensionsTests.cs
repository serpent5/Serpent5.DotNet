namespace Serpent5.Core.Tests;

public class ArgumentExceptionExtensionsTests
{
    [Fact]
    public void ThrowIfNullOrWhiteSpace_InputIsNull_ThrowsArgumentNullException()
    {
        string inputValue = null!;
        Assert.Throws<ArgumentNullException>(nameof(inputValue), () => ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(inputValue));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowIfNullOrWhiteSpace_InputIsEmptyOrWhiteSpace_ThrowsArgumentException(string inputValue)
        => Assert.Throws<ArgumentException>(nameof(inputValue), () => ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(inputValue));

    [Fact]
    public void ThrowIfNullOrWhiteSpace_InputIsNotNullEmptyOrWhiteSpace_DoesNotThrowException()
        => Assert.Null(Record.Exception(() => ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace("anyNotNullEmptyOrWhiteSpaceString")));
}
