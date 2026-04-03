using ExpenseTracker.Infrastructure.Sync;
using System.Text.Json;

namespace ExpenseTracker.UnitTests.Sync;

[Trait("Category", "Unit")]
public sealed class BookSyncValidatorTests
{
    [Fact]
    public void ValidatePut_MinimalValid_ReturnsNull()
    {
        string? err = BookSyncValidator.ValidatePut(MinimalValidRequest());
        _ = err.Should().BeNull();
    }

    [Fact]
    public void ValidatePut_WrongSchemaVersion_ReturnsError()
    {
        PutBookSyncRequest req = MinimalValidRequest();
        req.SchemaVersion = 8;
        string? err = BookSyncValidator.ValidatePut(req);
        _ = err.Should().NotBeNull();
    }

    [Fact]
    public void ValidatePut_SubcategoryUnknownCategory_ReturnsError()
    {
        PutBookSyncRequest req = MinimalValidRequest();
        req.Subcategories =
        [
            new SubcategorySyncDto
            {
                Id = "sub1",
                CategoryId = "missing",
                Name = "S",
                Slug = "s",
                IsSystemReserved = false,
                SortOrder = 0,
            },
        ];
        string? err = BookSyncValidator.ValidatePut(req);
        _ = err.Should().Contain("unknown categoryId");
    }

    [Fact]
    public void ValidatePut_NonEmptyPartialPayments_ReturnsError()
    {
        PutBookSyncRequest req = MinimalValidRequest();
        req.PartialPayments = [JsonDocument.Parse("{\"id\":\"x\"}").RootElement.Clone()];
        string? err = BookSyncValidator.ValidatePut(req);
        _ = err.Should().Contain("partialPayments");
    }

    private static PutBookSyncRequest MinimalValidRequest()
    {
        return new()
        {
            ExpectedBookRevision = 0,
            SchemaVersion = BookSyncConstants.CurrentSchemaVersion,
            ExportedAt = DateTime.UtcNow,
            Categories = [new CategorySyncDto { Id = "cat1", Name = "C", SortOrder = 0 }],
            Subcategories =
            [
                new SubcategorySyncDto
                {
                    Id = "sub1",
                    CategoryId = "cat1",
                    Name = "S",
                    Slug = "s",
                    IsSystemReserved = false,
                    SortOrder = 0,
                },
            ],
            IncomeCategories = [new IncomeCategorySyncDto { Id = "ic1", Name = "I", SortOrder = 0 }],
            IncomeSubcategories =
            [
                new IncomeSubcategorySyncDto
                {
                    Id = "is1",
                    CategoryId = "ic1",
                    Name = "S",
                    Slug = "s",
                    IsSystemReserved = false,
                    SortOrder = 0,
                },
            ],
            PaymentInstruments = [],
            ExpenseRecurringSeries = [],
            Expenses = [],
            IncomeEntries = [],
            IncomeRecurringSeries = [],
            InstallmentPlans = [],
            PartialPayments = [],
        };
    }
}
