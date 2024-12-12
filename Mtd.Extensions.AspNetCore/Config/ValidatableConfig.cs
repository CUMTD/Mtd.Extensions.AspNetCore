using System.ComponentModel.DataAnnotations;

namespace Mtd.Extensions.AspNetCore.Config;

/// <summary>
/// Base class for configuration classes that can be validated using data annotations.
/// </summary>
public abstract class ValidatableConfig
{
	/// <summary>
	/// Validates this ApiConfig instance using data annotations.
	/// Throws <see cref="ValidationException"/> if validation fails.
	/// </summary>
	/// <exception cref="ValidationException">Thrown if validaiton fails.</exception>
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
