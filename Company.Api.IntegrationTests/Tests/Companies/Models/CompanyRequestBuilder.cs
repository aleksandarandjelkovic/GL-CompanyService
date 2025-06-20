namespace Company.Api.IntegrationTests.Tests.Companies.Models
{
    public class CompanyRequestBuilder
    {
        private readonly CreateCompanyRequest _request;

        public CompanyRequestBuilder()
        {
            _request = new CreateCompanyRequest();
        }

        public CompanyRequestBuilder WithName(string name)
        {
            _request.Name = name;
            return this;
        }

        public CompanyRequestBuilder WithTicker(string ticker)
        {
            _request.Ticker = ticker;
            return this;
        }

        public CompanyRequestBuilder WithExchange(string exchange)
        {
            _request.Exchange = exchange;
            return this;
        }

        public CompanyRequestBuilder WithISIN(string isin)
        {
            _request.ISIN = isin;
            return this;
        }

        public CompanyRequestBuilder WithWebsite(string website)
        {
            _request.Website = website;
            return this;
        }

        public CreateCompanyRequest Build()
        {
            return _request;
        }
    }
}