using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

using Mtd.Extensions.AspNetCore.Config;
using Mtd.Extensions.AspNetCore.Extensions;

namespace Mtd.Extensions.AspNetCore.Test.Tests;

[TestClass]
public class WebApplicationBuilderExtensionsTests
{
	[TestMethod]
	public void ConfigureForKeyVault_NullBuilder_Throws()
	{
		WebApplicationBuilder? builder = null;
		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "https://mykeyvault.vault.azure.net/"
		};

		Assert.ThrowsException<ArgumentNullException>(() => builder!.ConfigureForKeyVault<Program>(config));
	}

	[TestMethod]
	public void ConfigureForKeyVault_NullConfig_Throws()
	{
		var builder = WebApplication.CreateBuilder();
		KeyVaultConfig? config = null;

		Assert.ThrowsException<ArgumentNullException>(() => builder.ConfigureForKeyVault<Program>(config!));
	}

	[TestMethod]
	public void ConfigureForKeyVault_InvalidConfig_ThrowsValidationException()
	{
		var builder = WebApplication.CreateBuilder();
		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "not-a-valid-url" // invalid URL
		};

		Assert.ThrowsException<ValidationException>(() => builder.ConfigureForKeyVault<Program>(config));
	}

	[TestMethod]
	public void ConfigureForKeyVault_ValidConfig_NoEnvironmentPrefix_InNonDev_DoesNotAddUserSecrets()
	{
		// Set environment to Production
		var builder = WebApplication.CreateBuilder([]);
		builder.Environment.EnvironmentName = Environments.Production;

		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "https://mykeyvault.vault.azure.net/"
		};

		builder.ConfigureForKeyVault<Program>(config);

		// Validate that no user secrets configuration source is present
		Assert.IsFalse(builder.Configuration.Sources.Any(s => s.GetType().Name.Contains("UserSecrets")),
			"UserSecrets configuration source should not be present in non-dev environments without explicit secrets.");
	}

	[TestMethod]
	public void ConfigureForKeyVault_ValidConfig_WithEnvironmentPrefix_AddsEnvironmentVariables()
	{
		var builder = WebApplication.CreateBuilder([]);
		builder.Environment.EnvironmentName = Environments.Production;

		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "https://mykeyvault.vault.azure.net/",
			EnvironmentVariablePrefix = "MYAPP_"
		};

		builder.ConfigureForKeyVault<Program>(config);

		// Check that environment variables source is present.
		Assert.IsTrue(builder.Configuration.Sources.Any(s => s.GetType().Name.Contains("EnvironmentVariables")),
			"Environment variables source should be present when prefix is provided.");
	}

	[TestMethod]
	public void ConfigureForKeyVault_ValidConfig_Development_AddsUserSecrets()
	{
		// Arrange
		var builder = WebApplication.CreateBuilder([]);
		builder.Environment.EnvironmentName = Environments.Development;

		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "https://mykeyvault.vault.azure.net/"
		};

		// Act
		// TestStartupClass has a user secret id registered.
		builder.ConfigureForKeyVault<TestStartupClass>(config);

		// Filter down to only JSON configuration sources
		var userSecretSources = builder.Configuration.Sources
			.OfType<JsonConfigurationSource>()
			.Where(js => js?.Path?.EndsWith("secrets.json", StringComparison.OrdinalIgnoreCase) ?? false);

		// Assert
		Assert.IsTrue(userSecretSources.Count() == 1, "User secrets configuration file (secrets.json) was not added to the configuration.");
	}

	[TestMethod]
	public void ConfigureForKeyVault_ValidConfig_Production_NoUserSecrets()
	{
		// Arrange
		var builder = WebApplication.CreateBuilder([]);
		builder.Environment.EnvironmentName = Environments.Production;

		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "https://mykeyvault.vault.azure.net/"
		};

		// Act
		// TestStartupClass has a user secret id registered.
		builder.ConfigureForKeyVault<TestStartupClass>(config);

		// Filter down to only JSON configuration sources
		var userSecretSources = builder.Configuration.Sources
			.OfType<JsonConfigurationSource>()
			.Where(js => js?.Path?.EndsWith("secrets.json", StringComparison.OrdinalIgnoreCase) ?? false);

		// Assert
		Assert.IsTrue(userSecretSources.Count() == 0, "User secrets configuration file (secrets.json) was added to the configuration.");
	}
}
