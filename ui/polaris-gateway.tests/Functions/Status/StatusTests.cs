﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NSubstitute;
using RumpoleGateway.Domain.Validators;
using Xunit;

namespace RumpoleGateway.Tests.Functions.Status
{
	public class CoreDataApiCaseInformationByUrnFunctionTests : SharedMethods.SharedMethods
	{
		private readonly ILogger<RumpoleGateway.Functions.Status.Status> _mockLogger = Substitute.For<ILogger<RumpoleGateway.Functions.Status.Status>>();
		private readonly IAuthorizationValidator _mockTokenValidator = Substitute.For<IAuthorizationValidator>();

		public CoreDataApiCaseInformationByUrnFunctionTests()
		{
			_mockTokenValidator.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>()).ReturnsForAnyArgs(true);
		}
		
		[Fact]
		public async Task StatusFunction_Should_Return_Response_400_When_No_CorrelationId_Supplied()
		{
			//Arrange
			var statusFunction = GetStatusFunction();

			//Act
			var results = await statusFunction.Run(CreateHttpRequestWithoutCorrelationId()) as Microsoft.AspNetCore.Mvc.ObjectResult;

			//Assert
			Assert.Equal(400, results?.StatusCode);
		}
		
		[Fact]
		public async Task StatusFunction_Should_Return_Response_400_When_No_Authorization_Supplied()
		{
			//Arrange
			var statusFunction = GetStatusFunction();

			//Act
			var results = await statusFunction.Run(CreateHttpRequestWithoutToken()) as Microsoft.AspNetCore.Mvc.ObjectResult;

			//Assert
			Assert.Equal(400, results?.StatusCode);
		}
		
		[Fact]
		public async Task StatusFunction_Should_Return_Response_401_When_Invalid_Authorization_Supplied()
		{
			//Arrange
			var statusFunction = GetStatusFunction();
			_mockTokenValidator.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>()).ReturnsForAnyArgs(false);

			//Act
			var results = await statusFunction.Run(CreateHttpRequest()) as Microsoft.AspNetCore.Mvc.ObjectResult;

			//Assert
			Assert.Equal(401, results?.StatusCode);
		}

		[Fact]
		public async Task StatusFunction_Should_Return_Response_200()
		{
			//Arrange
			var statusFunction = GetStatusFunction();

			//Act
			var results = await statusFunction.Run(CreateHttpRequest()) as Microsoft.AspNetCore.Mvc.ObjectResult;

			//Assert
			Assert.Equal(200, results?.StatusCode);
		}

		private RumpoleGateway.Functions.Status.Status GetStatusFunction()
		{
			return new RumpoleGateway.Functions.Status.Status(_mockLogger, _mockTokenValidator);
		}

	}
}
