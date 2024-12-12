using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

using Mtd.Extensions.AspNetCore.Config;

namespace Mtd.Extensions.AspNetCore.Filter;

/// <summary>
/// A simple API key filter that validates requests based on the API key provided in the X-ApiKey header.
/// </summary>
/// <remarks>
/// Example usage:
/// <code>
/// builder.Services.AddControllers(options =&gt; options.Filters.Add&lt;SimpleApiKeyFilter&gt;());
/// </code>
/// </remarks>
public class SimpleApiKeyFilter : IAuthorizationFilter
{
	private const string API_KEY_HEADER = "X-ApiKey";

	private readonly string[] _keys;
	private readonly ILogger<SimpleApiKeyFilter> _logger;

	/// <summary>
	/// Creates a new instance of <see cref="SimpleApiKeyFilter"/>.
	/// </summary>
	/// <param name="apiKeyConfig">The configuration containing the list of valid API keys for request validation.</param>
	/// <param name="logger">The logger used to log details during the authorization process.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="apiKeyConfig"/> or <paramref name="logger"/> is <c>null</c>.</exception>
	/// <exception cref="ValidationException">Thrown if <paramref name="apiKeyConfig"/> is invalid.</exception>
	public SimpleApiKeyFilter(ApiKeyConfig apiKeyConfig, ILogger<SimpleApiKeyFilter> logger)
	{
		ArgumentNullException.ThrowIfNull(apiKeyConfig, nameof(apiKeyConfig));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		// Validate the apiKeyConfig using data annotations.
		apiKeyConfig.Validate();

		_keys = apiKeyConfig.Keys;
		_logger = logger;
	}

	/// <summary>
	/// Authorizes the incoming request by validating the API key present in the X-ApiKey header.
	/// </summary>
	/// <param name="context">The current HTTP request context, which contains request headers and the result of the authorization filter.</param>
	public void OnAuthorization(AuthorizationFilterContext context)
	{
		_logger.LogTrace("Executing {filterName}", nameof(SimpleApiKeyFilter));

		if (context.HttpContext.Request.Headers.TryGetValue(API_KEY_HEADER, out var key))
		{
			if (_keys.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogTrace("Valid API key detected.");
				return;
			}
			else
			{
				_logger.LogWarning("API Key provided but did not match any valid keys.");
			}
		}
		else
		{
			_logger.LogInformation("No API key was provided in the request header.");
		}

		context.Result = new UnauthorizedResult();
	}
}
