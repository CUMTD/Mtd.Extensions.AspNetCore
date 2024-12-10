using System.ComponentModel.DataAnnotations;

using Mtd.Extensions.AspNetCore.Config;

namespace Mtd.Extensions.AspNetCore.Test.Tests;

[TestClass]
public class KeyVaultConfigTests
{
	[TestMethod]
	public void Validate_ValidConfig_NoException()
	{
		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "https://mykeyvault.vault.azure.net/",
			EnvironmentVariablePrefix = "MYAPP_"
		};

		// Should not throw
		config.Validate();
	}

	[TestMethod]
	public void Validate_MissingKeyVaultUrl_ThrowsValidationException()
	{
		var config = new KeyVaultConfig
		{
			KeyVaultUrl = null!, // required property
			EnvironmentVariablePrefix = "MYAPP_"
		};

		Assert.ThrowsException<ValidationException>(config.Validate);
	}

	[TestMethod]
	public void Validate_InvalidEnvironmentVariablePrefix_ThrowsValidationException()
	{
		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "https://mykeyvault.vault.azure.net/",
			EnvironmentVariablePrefix = "INVALID" // does not end with underscore
		};

		var ex = Assert.ThrowsException<ValidationException>(config.Validate);
		StringAssert.Contains(ex.Message, "EnvironmentVariablePrefix");
	}

	[TestMethod]
	public void Validate_NoPrefix_Valid()
	{
		var config = new KeyVaultConfig
		{
			KeyVaultUrl = "https://mykeyvault.vault.azure.net/",
			EnvironmentVariablePrefix = null
		};

		// No exception expected
		config.Validate();
	}
}
