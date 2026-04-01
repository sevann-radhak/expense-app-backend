using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Data.Entities;

namespace ExpenseTracker.Infrastructure.Taxonomy;

public static class ExpenseTaxonomySeeder
{
    private static readonly (string Id, string Name, int SortOrder)[] Categories =
    [
        ("cat_formation", "Formation", 0),
        ("cat_fixed_expenses", "Fixed expenses", 1),
        ("cat_subscriptions_digital", "Subscriptions & digital", 2),
        ("cat_delivery_convenience", "Delivery & convenience", 3),
        ("cat_pets", "Pets", 4),
        ("cat_taxes", "Taxes", 5),
        ("cat_investments", "Investments", 6),
        ("cat_leisure", "Leisure", 7),
        ("cat_health", "Health", 8),
        ("cat_transport", "Transport", 9),
        ("cat_housing", "Housing", 10),
    ];

    public static void Seed(ExpenseTrackerDbContext db, string userId, DateTime utcNow)
    {
        foreach ((string Id, string Name, int SortOrder) in Categories)
        {
            _ = db.Categories.Add(new CategoryEntity
            {
                UserId = userId,
                Id = Id,
                Name = Name,
                SortOrder = SortOrder,
                IsActive = true,
                UpdatedAtUtc = utcNow,
            });

            foreach (SubDef s in SubsForCategory(Id))
            {
                string subId = s.IsOther ? $"{Id}_other" : $"{Id}_{s.Key}";
                _ = db.Subcategories.Add(new SubcategoryEntity
                {
                    UserId = userId,
                    Id = subId,
                    CategoryId = Id,
                    Name = s.Name,
                    Slug = s.Slug,
                    IsSystemReserved = s.IsOther,
                    SortOrder = s.SortOrder,
                    IsActive = true,
                    UpdatedAtUtc = utcNow,
                });
            }
        }
    }

    private readonly record struct SubDef(string Key, string Name, string Slug, int SortOrder, bool IsOther);

    private static IEnumerable<SubDef> SubsForCategory(string categoryId)
    {
        return categoryId switch
        {
            "cat_formation" =>
            [
                new("courses", "Courses & training", "formation_courses", 0, false),
            new("books", "Books & materials", "formation_books", 1, false),
            new("other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "cat_fixed_expenses" =>
            [
                new("water", "Water", "fixed_water", 0, false),
            new("rent", "Rent", "fixed_rent", 1, false),
            new("cellphone", "Cell phone", "fixed_cellphone", 2, false),
            new("hoa", "HOA & building fees", "fixed_hoa", 3, false),
            new("gas", "Natural gas", "fixed_natural_gas", 4, false),
            new("internet_tv", "Internet & TV", "fixed_internet_tv", 5, false),
            new("electricity", "Electricity", "fixed_electricity", 6, false),
            new("groceries", "Groceries & supermarket", "fixed_groceries", 7, false),
            new("other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "cat_subscriptions_digital" =>
            [
                new("streaming", "Streaming video", "dig_streaming", 0, false),
            new("music", "Music & podcasts", "dig_music", 1, false),
            new("cloud", "Cloud storage & sync", "dig_cloud", 2, false),
            new("software", "Software & productivity", "dig_software", 3, false),
            new("gaming", "Gaming & online passes", "dig_gaming", 4, false),
            new("news", "News & reading", "dig_news", 5, false),
            new("other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "cat_delivery_convenience" =>
            [
                new("meal_delivery", "Meal delivery", "del_meal", 0, false),
            new("grocery_quick", "Grocery delivery & quick commerce", "del_grocery_quick", 1, false),
            new("courier", "Courier & errands", "del_courier", 2, false),
            new("laundry", "Laundry & dry cleaning", "del_laundry", 3, false),
            new("other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "cat_pets" =>
            [
                new("vet", "Veterinary", "pets_vet", 0, false),
            new("food", "Food & supplies", "pets_food", 1, false),
            new("toys", "Toys & accessories", "pets_toys", 2, false),
            new("grooming", "Grooming", "pets_grooming", 3, false),
            new("insurance", "Insurance & registration", "pets_insurance", 4, false),
            new("other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "cat_taxes" =>
            [
                new("national", "National taxes", "taxes_national", 0, false),
            new("provincial", "Provincial & municipal", "taxes_provincial", 1, false),
            new("vehicle", "Vehicle taxes & fees", "taxes_vehicle", 2, false),
            new("property", "Property-related taxes", "taxes_property", 3, false),
            new("other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "cat_investments" =>
            [
                new("brokerage", "Brokerage & securities", "inv_brokerage", 0, false),
            new("funds", "Funds & depositary assets", "inv_funds", 1, false),
            new("crypto", "Crypto assets", "inv_crypto", 2, false),
            new("fixed_income", "Fixed income", "inv_fixed_income", 3, false),
            new("other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "cat_leisure" =>
            [
                new("entertainment", "Entertainment", "leisure_entertainment", 0, false),
            new("dining", "Dining out", "leisure_dining", 1, false),
            new("travel", "Travel", "leisure_travel", 2, false),
            new("hobbies", "Hobbies", "leisure_hobbies", 3, false),
            new("other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "cat_health" =>
            [
                new("personal_care", "Personal care", "health_personal_care", 0, false),
            new("pharmacy", "Pharmacy", "health_pharmacy", 1, false),
            new("gym", "Gym & sports", "health_gym", 2, false),
            new("health_plan", "Health plan & prepaid medicine", "health_plan", 3, false),
            new("other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "cat_transport" =>
            [
                new("public", "Public transport", "transport_public", 0, false),
            new("rideshare", "Rideshare & taxi", "transport_rideshare", 1, false),
            new("private_vehicle", "Private vehicle", "transport_private_vehicle", 2, false),
            new("fuel", "Fuel", "transport_fuel", 3, false),
            new("parking", "Parking & tolls", "transport_parking", 4, false),
            new("maintenance", "Vehicle maintenance", "transport_maintenance", 5, false),
            new("other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "cat_housing" =>
            [
                new("decoration", "Decoration", "housing_decoration", 0, false),
            new("appliances", "Appliances", "housing_appliances", 1, false),
            new("cleaning", "Cleaning supplies", "housing_cleaning", 2, false),
            new("furniture", "Furniture", "housing_furniture", 3, false),
            new("repairs", "Repairs & improvements", "housing_repairs", 4, false),
            new("other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            _ => [],
        };
    }
}
