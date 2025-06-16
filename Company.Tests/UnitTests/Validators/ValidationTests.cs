using System;
using Company.Application.DTOs;
using Company.Application.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Company.Tests.UnitTests.Validators
{
    public class ValidationTests
    {
        #region CreateCompanyRequestValidator Tests
        
        public class CreateCompanyRequestValidatorTests
        {
            private readonly CreateCompanyRequestValidator _validator;

            public CreateCompanyRequestValidatorTests()
            {
                _validator = new CreateCompanyRequestValidator();
            }

            [Fact]
            public void ShouldHaveError_When_ISIN_StartsWithNumbers()
            {
                // Arrange
                var request = new CreateCompanyRequest
                {
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "123456789012" // Invalid - starts with numbers
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldHaveValidationErrorFor(x => x.ISIN)
                    .WithErrorMessage("ISIN must start with 2 letters");
            }

            [Fact]
            public void ShouldHaveError_When_ISIN_HasIncorrectLength()
            {
                // Arrange
                var request = new CreateCompanyRequest
                {
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US123" // Invalid - too short
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldHaveValidationErrorFor(x => x.ISIN)
                    .WithErrorMessage("ISIN must be exactly 12 characters");
            }

            [Fact]
            public void ShouldHaveError_When_Name_IsEmpty()
            {
                // Arrange
                var request = new CreateCompanyRequest
                {
                    Name = "",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US1234567890"
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldHaveValidationErrorFor(x => x.Name)
                    .WithErrorMessage("Name is required");
            }

            [Fact]
            public void ShouldHaveError_When_Ticker_IsEmpty()
            {
                // Arrange
                var request = new CreateCompanyRequest
                {
                    Name = "Test Company",
                    Ticker = "",
                    Exchange = "NYSE",
                    ISIN = "US1234567890"
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldHaveValidationErrorFor(x => x.Ticker)
                    .WithErrorMessage("Ticker is required");
            }

            [Fact]
            public void ShouldHaveError_When_Exchange_IsEmpty()
            {
                // Arrange
                var request = new CreateCompanyRequest
                {
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "",
                    ISIN = "US1234567890"
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldHaveValidationErrorFor(x => x.Exchange)
                    .WithErrorMessage("Exchange is required");
            }

            [Fact]
            public void ShouldHaveError_When_Website_IsInvalidUrl()
            {
                // Arrange
                var request = new CreateCompanyRequest
                {
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US1234567890",
                    Website = "invalid-url" // Invalid URL
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldHaveValidationErrorFor(x => x.Website)
                    .WithErrorMessage("Website must be a valid URL");
            }

            [Fact]
            public void ShouldNotHaveError_When_Website_IsValidUrl()
            {
                // Arrange
                var request = new CreateCompanyRequest
                {
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US1234567890",
                    Website = "https://testcompany.com" // Valid URL
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldNotHaveValidationErrorFor(x => x.Website);
            }

            [Fact]
            public void ShouldNotHaveError_When_Website_IsNull()
            {
                // Arrange
                var request = new CreateCompanyRequest
                {
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US1234567890",
                    Website = null // Website is optional
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldNotHaveValidationErrorFor(x => x.Website);
            }

            [Fact]
            public void ShouldNotHaveErrors_When_AllFieldsAreValid()
            {
                // Arrange
                var request = new CreateCompanyRequest
                {
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US1234567890",
                    Website = "https://testcompany.com"
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldNotHaveAnyValidationErrors();
            }
        }

        #endregion

        #region UpdateCompanyRequestValidator Tests

        public class UpdateCompanyRequestValidatorTests
        {
            private readonly UpdateCompanyRequestValidator _validator;

            public UpdateCompanyRequestValidatorTests()
            {
                _validator = new UpdateCompanyRequestValidator();
            }

            [Fact]
            public void ShouldHaveError_When_Id_IsEmpty()
            {
                // Arrange
                var request = new UpdateCompanyRequest
                {
                    Id = Guid.Empty,
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US1234567890"
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldHaveValidationErrorFor(x => x.Id)
                    .WithErrorMessage("Id is required");
            }

            [Fact]
            public void ShouldHaveError_When_ISIN_StartsWithNumbers()
            {
                // Arrange
                var request = new UpdateCompanyRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "123456789012" // Invalid - starts with numbers
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldHaveValidationErrorFor(x => x.ISIN)
                    .WithErrorMessage("ISIN must start with 2 letters");
            }

            [Fact]
            public void ShouldHaveError_When_ISIN_HasIncorrectLength()
            {
                // Arrange
                var request = new UpdateCompanyRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US123" // Invalid - too short
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldHaveValidationErrorFor(x => x.ISIN)
                    .WithErrorMessage("ISIN must be exactly 12 characters");
            }

            [Fact]
            public void ShouldHaveError_When_Name_IsEmpty()
            {
                // Arrange
                var request = new UpdateCompanyRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US1234567890"
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldHaveValidationErrorFor(x => x.Name)
                    .WithErrorMessage("Name is required");
            }

            [Fact]
            public void ShouldHaveError_When_Website_IsInvalidUrl()
            {
                // Arrange
                var request = new UpdateCompanyRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US1234567890",
                    Website = "invalid-url" // Invalid URL
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldHaveValidationErrorFor(x => x.Website)
                    .WithErrorMessage("Website must be a valid URL");
            }

            [Fact]
            public void ShouldNotHaveErrors_When_AllFieldsAreValid()
            {
                // Arrange
                var request = new UpdateCompanyRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Company",
                    Ticker = "TEST",
                    Exchange = "NYSE",
                    ISIN = "US1234567890",
                    Website = "https://testcompany.com"
                };

                // Act & Assert
                var result = _validator.TestValidate(request);
                result.ShouldNotHaveAnyValidationErrors();
            }
        }

        #endregion
    }
} 