using Company.Application.DTOs;
using Company.Application.Validators;
using FluentValidation.TestHelper;

namespace Company.Application.UnitTests.Validators
{
    public class CreateCompanyRequestValidatorTests
    {
        private readonly CreateCompanyRequestValidator _validator;

        public CreateCompanyRequestValidatorTests()
        {
            _validator = new CreateCompanyRequestValidator();
        }

        [Fact]
        public void Should_Pass_When_AllPropertiesAreValid()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US1234567890",
                Website = "https://test-company.com"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_When_NameIsEmpty()
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
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Fail_When_TickerIsEmpty()
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
            result.ShouldHaveValidationErrorFor(x => x.Ticker);
        }

        [Fact]
        public void Should_Fail_When_ExchangeIsEmpty()
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
            result.ShouldHaveValidationErrorFor(x => x.Exchange);
        }

        [Fact]
        public void Should_Fail_When_IsinIsEmpty()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = ""
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.ISIN);
        }

        [Fact]
        public void Should_Fail_When_IsinIsNotCorrectLength()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US12345" // Too short
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.ISIN)
                .WithErrorMessage("ISIN must be exactly 12 characters");
        }

        [Fact]
        public void Should_Fail_When_IsinDoesNotStartWithTwoLetters()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "123456789012" // Doesn't start with 2 letters
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.ISIN)
                .WithErrorMessage("ISIN must start with 2 letters");
        }

        [Fact]
        public void Should_Fail_When_WebsiteIsInvalid()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US1234567890",
                Website = "invalid-url"
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Website);
        }

        [Fact]
        public void Should_Pass_When_WebsiteIsEmpty()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US1234567890",
                Website = ""
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Website);
        }

        [Fact]
        public void Should_Pass_When_WebsiteIsNull()
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US1234567890",
                Website = null
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Website);
        }

        [Theory]
        [InlineData("http://example.com")]
        [InlineData("https://example.com")]
        [InlineData("https://www.example.co.uk")]
        [InlineData("https://subdomain.example.com/path?query=value")]
        public void Should_Pass_When_WebsiteIsValid(string website)
        {
            // Arrange
            var request = new CreateCompanyRequest
            {
                Name = "Test Company",
                Ticker = "TEST",
                Exchange = "NYSE",
                ISIN = "US1234567890",
                Website = website
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Website);
        }
    }
}