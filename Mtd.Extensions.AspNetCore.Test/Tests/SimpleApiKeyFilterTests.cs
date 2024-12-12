using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mtd.Extensions.AspNetCore.Config;
using Mtd.Extensions.AspNetCore.Filter;

namespace Mtd.Extensions.AspNetCore.Test.Tests;

[TestClass]
internal class SimpleApiKeyFilterTests
{
	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Constructor_NullApiKeyConfig_Throws()
	{
		var logger = new NullLogger<SimpleApiKeyFilter>();
		var filter = new SimpleApiKeyFilter(null!, logger);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public void Constructor_NullLogger_Throws()
	{
		var config = new ApiKeyConfig
		{
			Keys = ["ValidKey"]
		};

		var options = new OptionsWrapper<ApiKeyConfig>(config);

		var filter = new SimpleApiKeyFilter(options, null!);
	}

	[TestMethod]
	[ExpectedException(typeof(ValidationException))]
	public void Constructor_InvalidApiKeyConfig_Throws()
	{
		var logger = new NullLogger<SimpleApiKeyFilter>();
		var config = new ApiKeyConfig
		{
			Keys = [] // No keys provided
		};

		var options = new OptionsWrapper<ApiKeyConfig>(config);

		var filter = new SimpleApiKeyFilter(options, logger);
	}

	[TestMethod]
	public void OnAuthorization_ValidKey_AllowsRequest()
	{
		var logger = new NullLogger<SimpleApiKeyFilter>();
		var config = new ApiKeyConfig
		{
			Keys = ["ValidKey"]
		};

		var options = new OptionsWrapper<ApiKeyConfig>(config);

		var filter = new SimpleApiKeyFilter(options, logger);

		var httpContext = new DefaultHttpContext();
		httpContext.Request.Headers["X-ApiKey"] = "ValidKey";
		var context = new AuthorizationFilterContext(
			new ActionContext(
				httpContext,
				new Microsoft.AspNetCore.Routing.RouteData(),
				new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
			[]);

		filter.OnAuthorization(context);
		Assert.IsNull(context.Result, "Result should be null because the key is valid and request is allowed.");
	}

	[TestMethod]
	public void OnAuthorization_InvalidKey_ReturnsUnauthorized()
	{
		var logger = new NullLogger<SimpleApiKeyFilter>();
		var config = new ApiKeyConfig
		{
			Keys = ["ValidKey"]
		};

		var options = new OptionsWrapper<ApiKeyConfig>(config);

		var filter = new SimpleApiKeyFilter(options, logger);

		var httpContext = new DefaultHttpContext();
		httpContext.Request.Headers["X-ApiKey"] = "InvalidKey";
		var context = new AuthorizationFilterContext(
			new ActionContext(
				httpContext,
				new Microsoft.AspNetCore.Routing.RouteData(),
				new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
			[]);

		filter.OnAuthorization(context);
		Assert.IsInstanceOfType<UnauthorizedResult>(context.Result);
	}

	[TestMethod]
	public void OnAuthorization_NoKey_ReturnsUnauthorized()
	{
		var logger = new NullLogger<SimpleApiKeyFilter>();
		var config = new ApiKeyConfig
		{
			Keys = ["ValidKey"]
		};

		var options = new OptionsWrapper<ApiKeyConfig>(config);

		var filter = new SimpleApiKeyFilter(options, logger);

		var httpContext = new DefaultHttpContext();
		// No X-ApiKey header
		var context = new AuthorizationFilterContext(
			new ActionContext(
				httpContext,
				new Microsoft.AspNetCore.Routing.RouteData(),
				new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
			[]);

		filter.OnAuthorization(context);

		Assert.IsInstanceOfType<UnauthorizedResult>(context.Result);
	}
}
