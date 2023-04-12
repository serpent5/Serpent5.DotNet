namespace Serpent5.Core.Tests;

public class ArgumentExceptionExtensionsTests
{
    [Fact]
    public void ThrowIfNullOrWhiteSpace_Throws_ArgumentNullException_When_Input_Is_Null()
    {
        string inputValue = null!;
        Assert.Throws<ArgumentNullException>(nameof(inputValue), () => ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(inputValue));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowIfNullOrWhiteSpace_Throws_ArgumentException_When_Input_Is_Empty_Or_WhiteSpace_(string inputValue)
        => Assert.Throws<ArgumentException>(nameof(inputValue), () => ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(inputValue));

    [Fact]
    public void ThrowIfNullOrWhiteSpace_Does_Not_Throw_When_Input_Is_Not_Null_Or_Empty_Or_WhiteSpace()
        => Assert.Null(Record.Exception(() => ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(TestFakes.String)));
}
