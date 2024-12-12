using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Builder;

using Mtd.Extensions.AspNetCore.Config;

namespace Mtd.Extensions.AspNetCore.Extensions;

/// <summary>
/// Extension methods for <see cref="WebApplication"/>.
/// </summary>
public static class WebApplicationExtensions
{
	/// <summary>
	/// Add Swagger and Swagger UI to the application.
	/// </summary>
	/// <param name="app">The applicaiton to add.</param>
	/// <param name="swaggerConfig">The Swagger configuraiton to use.</param>
	/// <returns>The app.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="app"/> or <paramref name="swaggerConfig"/> is null.</exception>
	/// <exception cref="ValidationException">Thrown if <paramref name="swaggerConfig"/> does not meet validation criteria.</exception>

	public static WebApplication Mtd_UseSwagger(this WebApplication app, SwaggerConfig swaggerConfig)
	{
		ArgumentNullException.ThrowIfNull(app, nameof(app));
		ArgumentNullException.ThrowIfNull(swaggerConfig, nameof(swaggerConfig));

		// Validate the swaggerConfig using data annotations.
		swaggerConfig.Validate();

		app.UseSwagger();

		app.UseSwaggerUI(options =>
		{
			if (swaggerConfig.RunSwaggerAtRoot)
			{
				options.RoutePrefix = string.Empty; // Serve Swagger UI at the root
			}

			options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
			options.DocumentTitle = swaggerConfig.Title;

			options.DisplayRequestDuration();

			if (!string.IsNullOrWhiteSpace(swaggerConfig.CustomCssPath))
			{
				options.InjectStylesheet(swaggerConfig.CustomCssPath);
			}

			options
				.SwaggerEndpoint(
				$"/swagger/v{swaggerConfig.ApiVersion}/swagger.json",
				$"{options.DocumentTitle} - {swaggerConfig.ApiVersion}".Trim());
		});

		return app;
	}

}
