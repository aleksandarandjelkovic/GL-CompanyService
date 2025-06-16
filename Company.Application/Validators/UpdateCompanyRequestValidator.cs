using Company.Application.DTOs;
using FluentValidation;

namespace Company.Application.Validators
{
    /// <summary>
    /// Validator for <see cref="UpdateCompanyRequest"/>.
    /// </summary>
    public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCompanyRequestValidator"/> class.
        /// </summary>
        public UpdateCompanyRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required");

            RuleFor(x => x.Ticker)
                .NotEmpty().WithMessage("Ticker is required");

            RuleFor(x => x.Exchange)
                .NotEmpty().WithMessage("Exchange is required");

            RuleFor(x => x.ISIN)
                .NotEmpty().WithMessage("ISIN is required")
                .Length(12).WithMessage("ISIN must be exactly 12 characters")
                .Matches("^[A-Za-z]{2}").WithMessage("ISIN must start with 2 letters");

            RuleFor(x => x.Website)
                .Must(BeAValidUrl).When(x => !string.IsNullOrWhiteSpace(x.Website))
                .WithMessage("Website must be a valid URL");
        }

        /// <summary>
        /// Validates that the website is a valid URL if provided.
        /// </summary>
        /// <param name="website">The website URL to validate.</param>
        /// <returns><c>true</c> if the website is valid or empty; otherwise, <c>false</c>.</returns>
        private bool BeAValidUrl(string? website)
        {
            if (string.IsNullOrWhiteSpace(website))
                return true;
            return Uri.TryCreate(website, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
} 