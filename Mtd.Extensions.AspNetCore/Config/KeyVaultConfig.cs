using System.ComponentModel.DataAnnotations;

namespace Mtd.Extensions.AspNetCore.Config;

/// <summary>
/// Configuration for Azure Key Vault.
/// </summary>
public class KeyVaultConfig : ValidatableConfig
{
	/// <summary>
	/// The prefix to use for environment variables. Must end with '_'
	/// </summary>
	[RegularExpression("^[a-zA-Z][a-zA-Z_]*[a-zA-Z]_$", ErrorMessage = "EnvironmentVariablePrefix must start with a letter, may contain letters or underscores in between, and must end with '_'.")]
	public string? EnvironmentVariablePrefix { get; set; }

	/// <summary>
	/// The URL of the KeyVault.
	/// </summary>
	[Required(ErrorMessage = "KeyVaultUrl is required.")]
	[Url(ErrorMessage = "KeyVaultUrl must be a valid URL.")]
	public required string KeyVaultUrl { get; set; }

	/// <summary>
	/// The URI of the KeyVault, derived from KeyVaultUrl.
	/// </summary>
	public Uri KeyVaultUri => new(KeyVaultUrl);
}
