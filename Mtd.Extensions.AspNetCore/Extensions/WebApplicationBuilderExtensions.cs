using System.ComponentModel.DataAnnotations;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Mtd.Extensions.AspNetCore.Config;

using Serilog;

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
	public static WebApplicationBuilder Mtd_ConfigureForKeyVault<T>(this WebApplicationBuilder builder, KeyVaultConfig keyVaultConfig) where T : class
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

	/// <summary>
	/// Binds an option to a configuration section and runs validation on startup.
	/// </summary>
	/// <typeparam name="T">The type to bind the config section to.</typeparam>
	/// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
	/// <param name="sectionName">The config section name that represents the options.</param>
	/// <returns>The configured <see cref="WebApplicationBuilder"/>.</returns>
	public static WebApplicationBuilder Mtd_BindOptionToConfigSection<T>(this WebApplicationBuilder builder, string sectionName) where T : class
	{
		ArgumentNullException.ThrowIfNull(builder, nameof(builder));
		ArgumentException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));

		builder
			.Services
			.AddOptions<T>()
			.BindConfiguration(sectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		return builder;
	}

	/// <summary>
	/// Adds Serilog logging to the application.
	/// </summary>
	/// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
	/// <returns>The configured <see cref="WebApplicationBuilder"/>.</returns>
	/// <remarks>
	/// This method reads the Serilog configuration from the loaded config.
	/// A sample Serilog configuration is shown below in JSON format:
	/// <code>
	/// {
	///    "Serilog":{
	///       "Using":[
	///          "Serilog.Sinks.Seq",
	///          "Serilog.Enrichers.Environment",
	///          "Serilog.Enrichers.Process",
	///          "Serilog.Enrichers.Thread"
	///       ],
	///       "MinimumLevel":{
	///          "Default":"Information",
	///          "Override":{
	///             "Microsoft":"Warning",
	///             "System":"Warning",
	///             "Microsoft.Hosting.Lifetime":"Information"
	///          }
	///       },
	///       "Enrich":[
	///          "FromLogContext",
	///          "WithMachineName",
	///          "WithEnvironmentUserName",
	///          "WithProcessId",
	///          "WithThreadId",
	///          "WithAssemblyName",
	///          "WithAssemblyVersion"
	///       ],
	///       "WriteTo":[
	///          {
	///             "Name":"Seq",
	///             "Args":{
	///                "serverUrl":"&lt;SEQ Server Url&gt;",
	///                "apiKey":"&lt;Uses Azure key Vault&gt;"
	///             }
	///          },
	///          {
	///              "Name": "Console",
	///              "Args": {
	///                  "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
	///                  "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} &lt;s:{SourceContext}&gt;{NewLine}{Exception}",
	///                  "restrictedToMinimumLevel": "Information"
	///              }
	///          }
	///       ]
	///    }
	/// }
	/// </code>
	///
	/// With this configuration, Serilog will log to SEQ with severl enrichers and minimum levels set.
	/// This configuration can be stored in appsettings.json though secrets should be stored in Azure Key Vault.
	/// The following NuGet packages are required for this configuration:
	/// Serilog.Enrichers.AssemblyName
	/// Serilog.Enrichers.Environment
	/// Serilog.Enrichers.Process
	/// Serilog.Sinks.Console
	/// Serilog.Sinks.Seq
	/// </remarks>
	public static WebApplicationBuilder Mtd_AddSerilogLogging(this WebApplicationBuilder builder)
	{
		ArgumentNullException.ThrowIfNull(builder, nameof(builder));

		builder
			.Host
			.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

		return builder;
	}

	/// <summary>
	/// Configure the application to use Swagger for API documentation with some standard configs as options.
	/// </summary>
	/// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
	/// <param name="swaggerConfig">Config object for setting up Swagger.</param>
	/// <param name="additionalConfiguration">This action will run last. Allows you to add addtional config to Swagger.</param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="swaggerConfig"/> is null.</exception>
	/// <exception cref="ValidationException">Thrown if <paramref name="swaggerConfig"/> does not meet validation criteria.</exception>

	public static WebApplicationBuilder Mtd_ConfigureSwagger(this WebApplicationBuilder builder, SwaggerConfig swaggerConfig, Action<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>? additionalConfiguration = null)
	{
		ArgumentNullException.ThrowIfNull(builder, nameof(builder));
		ArgumentNullException.ThrowIfNull(swaggerConfig, nameof(swaggerConfig));

		// Validate the swaggerConfig using data annotations.
		swaggerConfig.Validate();

		builder.Services.AddEndpointsApiExplorer();

		builder.Services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc($"v{swaggerConfig.ApiVersion}", new OpenApiInfo
			{
				Version = swaggerConfig.ApiVersion,
				Title = swaggerConfig.Title,
				Description = swaggerConfig.Description,
				Contact = new OpenApiContact
				{
					Name = swaggerConfig.ContactName,
					Email = swaggerConfig.ContactEmail
				}
			});

			options.EnableAnnotations();

			if (swaggerConfig.IncludeXmlComments)
			{
				// Set the comments path for the Swagger JSON and UI.
				// This allows swagger to use XML comments to enhance documentation.
				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				options.IncludeXmlComments(xmlPath);
			}

			if (swaggerConfig.IncludeApiKeySecurity)
			{
				var authMethodName = "API Key - Header";
				var securityScheme = new OpenApiSecurityScheme
				{
					Reference = new OpenApiReference
					{
						Type = ReferenceType.SecurityScheme,
						Id = authMethodName
					},
					Description = "Provide your API key in the header using X-ApiKey.",
					In = ParameterLocation.Header,
					Name = "X-ApiKey",
					Type = SecuritySchemeType.ApiKey,
					Scheme = "API Key"
				};
				options.AddSecurityDefinition(authMethodName, securityScheme);
				options.AddSecurityRequirement(new OpenApiSecurityRequirement() {
				{ securityScheme, Array.Empty <string>() }
			 });
			}

			additionalConfiguration?.Invoke(options);

		});

		return builder;
	}

}
