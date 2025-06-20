namespace Company.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with initial test data.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Seeds the Companies table with test data if it is empty.
    /// </summary>
    /// <param name="context">The database context.</param>
    public static void Seed(CompanyDbContext context)
    {
        if (!context.Companies.Any())
        {
            context.Companies.AddRange(new[]
            {
                Domain.Entities.Company.Create("Apple Inc.", "AAPL", "NASDAQ", "US0378331005", "http://www.apple.com").Value!,
                Domain.Entities.Company.Create("British Airways Plc", "BAIRY", "Pink Sheets", "US1104193065", null).Value!,
                Domain.Entities.Company.Create("Heineken NV", "HEIA", "Euronext Amsterdam", "NL0000009165", null).Value!,
                Domain.Entities.Company.Create("Panasonic Corp", "6752", "Tokyo Stock Exchange", "JP3866800000", "http://www.panasonic.co.jp").Value!,
                Domain.Entities.Company.Create("Porsche Automobil", "PAH3", "Deutsche Börse", "DE000PAH0038", "https://www.porsche.com/").Value!
            });
            context.SaveChanges();
        }
    }
}