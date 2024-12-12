using System.ComponentModel.DataAnnotations;

using Mtd.Extensions.AspNetCore.Filter;

namespace Mtd.Extensions.AspNetCore.Config;

/// <summary>
/// Configuration options for Swagger.
/// </summary>
public class SwaggerConfig : ValidatableConfig
{
	/// <summary>
	/// The API title to display in Swagger.
	/// </summary>
	[Required(ErrorMessage = "A title is required.")]
	public required string Title { get; set; }

	/// <summary>
	/// The API description to display in Swagger.
	/// </summary>
	[Required(ErrorMessage = "A description is required.")]
	public required string Description { get; set; }

	/// <summary>
	/// The contact name for the API.
	/// </summary>
	public string ContactName { get; set; } = "MTD";

	/// <summary>
	/// The Contact email for the API.
	/// </summary>
	[EmailAddress(ErrorMessage = "ContactEmail must be a valid email address.")]
	public string ContactEmail { get; set; } = "developer@mtd.org";

	/// <summary>
	/// The Major version of the API.
	/// </summary>
	[Range(1, uint.MaxValue, ErrorMessage = "MajorVersion must be greater than 0.")]
	public int MajorVersion { get; set; } = 1;

	/// <summary>
	/// The Minor version of the API.
	/// </summary>
	[Range(0, uint.MaxValue, ErrorMessage = "MinorVersion must be greater than or equal to 0.")]
	public uint MinorVersion { get; set; } = 0;

	/// <summary>
	/// The API version. Calculated from <see cref="MajorVersion"/> and <see cref="MinorVersion"/>.
	/// </summary>
	public string ApiVersion => $"{MajorVersion}.{MinorVersion}";

	/// <summary>
	/// Add configuration for API key security. Use with <see cref="SimpleApiKeyFilter"/>.
	/// </summary>
	public bool IncludeApiKeySecurity { get; set; } = true;

	/// <summary>
	/// Include XML comments in the Swagger documentation.
	/// </summary>
	/// <remarks>
	/// If this is included, the project should have the <code>&lt;GenerateDocumentationFile&gt;true&lt;/GenerateDocumentationFile&gt;</code>.
	/// </remarks>
	public bool IncludeXmlComments { get; set; } = true;

	/// <summary>
	/// If true, will put the Swagger UI at the root of the application rather than at /swagger.
	/// </summary>
	public bool RunSwaggerAtRoot { get; set; } = true;

	/// <summary>
	/// The path to the file to use for Swagger UI. If null, the default CSS will be used.
	/// </summary>
	public string? CustomCssPath { get; set; } = null;

}
