using System.ComponentModel.DataAnnotations;

namespace Mtd.Extensions.AspNetCore.Config;

/// <summary>
/// Configuration for API keys.
/// </summary>
public class ApiKeyConfig : ValidatableConfig
{
	/// <summary>
	/// The list of valid API keys.
	/// </summary>
	[Required(ErrorMessage = "Please provide a list of keys.")]
	[MinLength(1, ErrorMessage = "At least one API key is required.")]
	public required string[] Keys { get; set; }

}
