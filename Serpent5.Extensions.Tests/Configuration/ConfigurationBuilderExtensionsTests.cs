using Microsoft.Extensions.Configuration;

namespace Serpent5.Extensions.Configuration.Tests;

public class ConfigurationBuilderExtensionsTests
{
    private const string environmentSettingName = "environmentSetting";

    [Fact]
    public void UsesAppSettingsValue()
    {
        var configurationRoot = CreateConfigurationWithHostingDefaults();

        Assert.Equal("appSettingsValue", configurationRoot["appSettingsSetting"]);
    }

    [Fact]
    public void UsesAppSettingsEnvironmentValue()
    {
        var configurationRoot = CreateConfigurationWithHostingDefaults();

        Assert.Equal("appSettingsProductionValue", configurationRoot["appSettingsProductionSetting"]);
    }

    [Fact]
    public void UsesEnvironmentVariablesValue()
    {
        const string settingName = "environmentVariableSetting";
        const string settingValue = "environmentVariableValue";

        Environment.SetEnvironmentVariable(settingName, settingValue);

        var configurationRoot = CreateConfigurationWithHostingDefaults();

        Assert.Equal(settingValue, configurationRoot[settingName]);
    }

    [Fact]
    public void UsesCommandLineValue()
    {
        const string settingName = "commandLineSetting";
        const string settingValue = "commandLineValue";

        var configurationRoot = CreateConfigurationWithHostingDefaults(new[]
        {
            $"--{settingName}={settingValue}"
        });

        Assert.Equal(settingValue, configurationRoot[settingName]);
    }

    [Fact]
    public void UsesAppSettingsEnvironmentOverrideValue()
    {
        var configurationRoot = CreateConfigurationWithHostingDefaults();

        Assert.Equal("productionOverrideValue", configurationRoot[environmentSettingName]);
    }

    [Fact]
    public void UsesEnvironmentVariableOverrideValue()
    {
        const string overrideValue = "environmentVariableOverrideValue";
        Environment.SetEnvironmentVariable(environmentSettingName, overrideValue);

        try
        {
            var configurationRoot = CreateConfigurationWithHostingDefaults();

            Assert.Equal(overrideValue, configurationRoot[environmentSettingName]);
        }
        finally
        {
            Environment.SetEnvironmentVariable(environmentSettingName, null);
        }
    }

    [Fact]
    public void UsesCommandLineVariableOverrideValue()
    {
        const string overrideValue = "commandLineOverrideValue";
        var configurationRoot = CreateConfigurationWithHostingDefaults(new[]
        {
            $"--{environmentSettingName}={overrideValue}"
        });

        Assert.Equal(overrideValue, configurationRoot[environmentSettingName]);
    }

    private static IConfiguration CreateConfigurationWithHostingDefaults(string[]? args = null)
        => new ConfigurationBuilder()
            .AddHostingDefaults(args)
            .Build();
}
