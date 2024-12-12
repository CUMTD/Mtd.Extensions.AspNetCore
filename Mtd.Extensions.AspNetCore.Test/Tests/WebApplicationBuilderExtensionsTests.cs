using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mtd.Extensions.AspNetCore.Config;
using Mtd.Extensions.AspNetCore.Extensions;

namespace Mtd.Extensions.AspNetCore.Test.Tests;

[TestClass]
public class WebApplicationBuilderExtensionsTests
{

	#region KeyVault

	[TestMethod]
	public void Mtd_ConfigureForKeyVault_NullBuilder_Throws()
	{
		WebApplicationBuilder? builder = null;
		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "https://mykeyvault.vault.azure.net/"
		};

		Assert.ThrowsException<ArgumentNullException>(() => builder!.Mtd_ConfigureForKeyVault<Program>(config));
	}

	[TestMethod]
	public void Mtd_ConfigureForKeyVault_NullConfig_Throws()
	{
		var builder = WebApplication.CreateBuilder();
		KeyVaultConfig? config = null;

		Assert.ThrowsException<ArgumentNullException>(() => builder.Mtd_ConfigureForKeyVault<Program>(config!));
	}

	[TestMethod]
	public void Mtd_ConfigureForKeyVault_InvalidConfig_ThrowsValidationException()
	{
		var builder = WebApplication.CreateBuilder();
		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "not-a-valid-url" // invalid URL
		};

		Assert.ThrowsException<ValidationException>(() => builder.Mtd_ConfigureForKeyVault<Program>(config));
	}

	[TestMethod]
	public void Mtd_ConfigureForKeyVault_ValidConfig_NoEnvironmentPrefix_InNonDev_DoesNotAddUserSecrets()
	{
		// Set environment to Production
		var builder = WebApplication.CreateBuilder([]);
		builder.Environment.EnvironmentName = Environments.Production;

		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "https://mykeyvault.vault.azure.net/"
		};

		builder.Mtd_ConfigureForKeyVault<Program>(config);

		// Validate that no user secrets configuration source is present
		Assert.IsFalse(builder.Configuration.Sources.Any(s => s.GetType().Name.Contains("UserSecrets")),
			"UserSecrets configuration source should not be present in non-dev environments without explicit secrets.");
	}

	[TestMethod]
	public void Mtd_ConfigureForKeyVault_ValidConfig_WithEnvironmentPrefix_AddsEnvironmentVariables()
	{
		var builder = WebApplication.CreateBuilder([]);
		builder.Environment.EnvironmentName = Environments.Production;

		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "https://mykeyvault.vault.azure.net/",
			EnvironmentVariablePrefix = "MYAPP_"
		};

		builder.Mtd_ConfigureForKeyVault<Program>(config);

		// Check that environment variables source is present.
		Assert.IsTrue(builder.Configuration.Sources.Any(s => s.GetType().Name.Contains("EnvironmentVariables")),
			"Environment variables source should be present when prefix is provided.");
	}

	[TestMethod]
	public void Mtd_ConfigureForKeyVault_ValidConfig_Development_AddsUserSecrets()
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
		builder.Mtd_ConfigureForKeyVault<TestStartupClass>(config);

		// Filter down to only JSON configuration sources
		var userSecretSources = builder.Configuration.Sources
			.OfType<JsonConfigurationSource>()
			.Where(js => js?.Path?.EndsWith("secrets.json", StringComparison.OrdinalIgnoreCase) ?? false);

		// Assert
		Assert.IsTrue(userSecretSources.Count() == 1, "User secrets configuration file (secrets.json) was not added to the configuration.");
	}

	[TestMethod]
	public void Mtd_ConfigureForKeyVault_ValidConfig_Production_NoUserSecrets()
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
		builder.Mtd_ConfigureForKeyVault<TestStartupClass>(config);

		// Filter down to only JSON configuration sources
		var userSecretSources = builder.Configuration.Sources
			.OfType<JsonConfigurationSource>()
			.Where(js => js?.Path?.EndsWith("secrets.json", StringComparison.OrdinalIgnoreCase) ?? false);

		// Assert
		Assert.IsTrue(!userSecretSources.Any(), "User secrets configuration file (secrets.json) was added to the configuration.");
	}

	#endregion KeyVault

	#region Configure Swagger

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Mtd_ConfigureSwagger_NullBuilder_Throws()
	{
		WebApplicationBuilder? builder = null;
		var swaggerConfig = new SwaggerConfig
		{
			Title = "Test API",
			Description = "Test Desc"
		};
		builder!.Mtd_ConfigureSwagger(swaggerConfig);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Mtd_ConfigureSwagger_NullConfig_Throws()
	{
		var builder = WebApplication.CreateBuilder();
		SwaggerConfig? config = null;
		builder.Mtd_ConfigureSwagger(config!);
	}

	[TestMethod]
	[ExpectedException(typeof(ValidationException))]
	public void Mtd_ConfigureSwagger_InvalidConfig_Throws()
	{
		var builder = WebApplication.CreateBuilder();
		var config = new SwaggerConfig
		{
			Title = "",  // Empty title causes validation failure
			Description = "Test Desc"
		};
		builder.Mtd_ConfigureSwagger(config);
	}

	[TestMethod]
	public void Mtd_ConfigureSwagger_ValidConfig_RegistersSwaggerServices()
	{
		var builder = WebApplication.CreateBuilder();
		var config = new SwaggerConfig
		{
			Title = "Test API",
			Description = "A test API"
		};

		builder.Mtd_ConfigureSwagger(config);

		// Build to finalize the DI container
		using var app = builder.Build();


		Assert.IsNotNull(app);
	}

	#endregion Configure Swagger

	#region Serilog

	[TestClass]
	public class AddSerilogLoggingTests
	{
		[TestMethod]
		public void Mtd_AddSerilogLogging_ValidBuilder_NoException()
		{
			var builder = WebApplication.CreateBuilder();
			// Just ensure it doesn't throw
			builder.Mtd_AddSerilogLogging();
			Assert.IsNotNull(builder.Host);
		}
	}

	#endregion Serilog

	#region Bind Options

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Mtd_BindOptionToConfigSection_NullBuilder_Throws()
	{
		WebApplicationBuilder? builder = null;
		builder!.Mtd_BindOptionToConfigSection<TestOptions>("TestSection");
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void Mtd_BindOptionToConfigSection_EmptySectionName_Throws()
	{
		var builder = WebApplication.CreateBuilder();
		builder.Mtd_BindOptionToConfigSection<TestOptions>("");
	}

	[TestMethod]
	public void Mtd_BindOptionToConfigSection_ValidSection_AddsAndValidatesOptions()
	{
		var builder = WebApplication.CreateBuilder();
		builder.Configuration["TestSection:RequiredValue"] = "Hello";

		builder.Mtd_BindOptionToConfigSection<TestOptions>("TestSection");

		using var app = builder.Build();
		var serviceProvider = app.Services;
		var opts = serviceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();
		Assert.AreEqual("Hello", opts.CurrentValue.RequiredValue);
	}
	internal class TestOptions
	{
		// Add a DataAnnotations attribute to require a field
		[Required]
		public string RequiredValue { get; set; } = "Default";
	}

	#endregion Bind Options
}


