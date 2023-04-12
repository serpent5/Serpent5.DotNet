using Microsoft.Extensions.Configuration;

namespace Serpent5.Extensions.Tests;

public class ConfigurationBuilderExtensionsTests
{
    private const string environmentSettingName = "environmentSetting";

    [Fact]
    public void AddHostingDefaults_Reads_From_AppSettings()
    {
        var configurationRoot = CreateConfigurationWithHostingDefaults();

        Assert.Equal("appSettingsValue", configurationRoot["appSettingsSetting"]);
    }

    [Fact]
    public void AddHostingDefaults_Reads_From_AppSettings_Production()
    {
        var configurationRoot = CreateConfigurationWithHostingDefaults();

        Assert.Equal("appSettingsProductionValue", configurationRoot["appSettingsProductionSetting"]);
    }

    [Fact]
    public void AddHostingDefaults_Reads_From_Environment_Variables()
    {
        const string settingName = "environmentVariableSetting";
        const string settingValue = "environmentVariableValue";

        Environment.SetEnvironmentVariable(settingName, settingValue);

        var configurationRoot = CreateConfigurationWithHostingDefaults();

        Assert.Equal(settingValue, configurationRoot[settingName]);
    }

    [Fact]
    public void AddHostingDefaults_Reads_From_Command_Line_Arguments()
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
    public void AddHostingDefaults_AppSettings_Production_Overrides_AppSettings()
    {
        var configurationRoot = CreateConfigurationWithHostingDefaults();

        Assert.Equal("productionOverrideValue", configurationRoot[environmentSettingName]);
    }

    [Fact]
    public void AddHostingDefaults_Environment_Variables_Override_AppSettings_Production()
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
    public void AddHostingDefaults_Command_Line_Arguments_Override_Environment_Variables()
    {
        const string environmentVariableOverrideValue = "environmentVariableOverrideValue";
        Environment.SetEnvironmentVariable(environmentSettingName, environmentVariableOverrideValue);

        try
        {
            const string overrideValue = "commandLineOverrideValue";
            var configurationRoot = CreateConfigurationWithHostingDefaults(new[]
            {
                $"--{environmentSettingName}={overrideValue}"
            });

            Assert.Equal(overrideValue, configurationRoot[environmentSettingName]);
        }
        finally
        {
            Environment.SetEnvironmentVariable(environmentSettingName, null);
        }
    }

    private static IConfiguration CreateConfigurationWithHostingDefaults(string[]? args = null)
        => new ConfigurationBuilder()
            .AddHostingDefaults(args)
            .Build();
}
