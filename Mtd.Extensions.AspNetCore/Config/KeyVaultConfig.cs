using System.ComponentModel.DataAnnotations;

namespace Mtd.Extensions.AspNetCore.Config;

/// <summary>
/// Configuration for Azure Key Vault.
/// </summary>
public class KeyVaultConfig
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

	/// <summary>
	/// Validates this KeyVaultConfig instance using data annotations.
	/// Throws <see cref="ValidationException"/> if validation fails.
	/// </summary>
	public void Validate()
	{
		var validationContext = new ValidationContext(this);
		var validationResults = new List<ValidationResult>();

		if (!Validator.TryValidateObject(this, validationContext, validationResults, validateAllProperties: true))
		{
			// Combine all error messages into a single string for the exception
			var errorMessage = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
			throw new ValidationException($"KeyVaultConfig validation failed: {errorMessage}");
		}
	}
}
