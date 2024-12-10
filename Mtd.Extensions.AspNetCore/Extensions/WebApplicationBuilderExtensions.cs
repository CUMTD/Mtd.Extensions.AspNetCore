using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Mtd.Extensions.AspNetCore.Config;

namespace Mtd.Extensions.AspNetCore.Extensions;

/// <summary>
/// Extension methods for <see cref="WebApplicationBuilder"/>.
/// </summary>
public static class WebApplicationBuilderExtensions
{

	/// <summary>
	/// Configure the application to retrieve its configuration from Azure Key Vault.
	/// </summary>
	/// <typeparam name="T">
	/// The program's entry point class, usually <c>Program</c>. This is used by
	/// <see cref="M:Microsoft.Extensions.Configuration.UserSecretsConfigurationExtensions.AddUserSecrets``1(Microsoft.Extensions.Configuration.IConfigurationBuilder)"/>
	/// assembly containing user secrets.
	/// </typeparam>
	/// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
	/// <param name="keyVaultConfig">The configuration object containing Key Vault settings.</param>
	/// <returns>The configured <see cref="WebApplicationBuilder"/>.</returns>
	/// <remarks>
	/// The configuration order is:
	/// 1. Azure Key Vault (overrides appsettings and other initial sources)
	/// 2. Environment Variables (optional, using the specified prefix, overriding Key Vault)
	/// 3. User Secrets (in Development environment only, overriding all above if present)
	/// </remarks>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="keyVaultConfig"/> is null.</exception>
	/// <exception cref="ValidationException">Thrown if <paramref name="keyVaultConfig"/> does not meet validation criteria.</exception>
	public static WebApplicationBuilder ConfigureForKeyVault<T>(this WebApplicationBuilder builder, KeyVaultConfig keyVaultConfig) where T : class
	{
		ArgumentNullException.ThrowIfNull(builder, nameof(builder));
		ArgumentNullException.ThrowIfNull(keyVaultConfig, nameof(keyVaultConfig));

		// Validate the keyVaultConfig using data annotations.
		keyVaultConfig.Validate();

		// Add Environment Variables configuration source (if prefix is set).
		if (!string.IsNullOrWhiteSpace(keyVaultConfig.EnvironmentVariablePrefix))
		{
			builder.Configuration.AddEnvironmentVariables(keyVaultConfig.EnvironmentVariablePrefix);
		}

		// In development, add User Secrets last, allowing them to override all above settings.
		if (builder.Environment.IsDevelopment())
		{
			builder.Configuration.AddUserSecrets<T>();
		}

		return builder;
	}

}
