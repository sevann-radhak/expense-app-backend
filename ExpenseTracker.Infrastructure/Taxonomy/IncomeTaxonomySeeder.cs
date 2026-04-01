using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Data.Entities;

namespace ExpenseTracker.Infrastructure.Taxonomy;

public static class IncomeTaxonomySeeder
{
    private static readonly (string Id, string Name, int SortOrder)[] Categories =
    [
        ("inc_cat_employment", "Employment (salaried)", 0),
        ("inc_cat_self_employment", "Self-employment & freelance", 1),
        ("inc_cat_investment_passive", "Investment & passive income", 2),
        ("inc_cat_benefits_transfers", "Benefits, subsidies & transfers", 3),
        ("inc_cat_refunds_reversals", "Refunds & reversals", 4),
        ("inc_cat_miscellaneous", "Miscellaneous income", 5),
    ];

    public static void Seed(ExpenseTrackerDbContext db, string userId, DateTime utcNow)
    {
        foreach ((string Id, string Name, int SortOrder) in Categories)
        {
            _ = db.IncomeCategories.Add(new IncomeCategoryEntity
            {
                UserId = userId,
                Id = Id,
                Name = Name,
                SortOrder = SortOrder,
                IsActive = true,
                UpdatedAtUtc = utcNow,
            });

            foreach (IncSubDef s in SubsForCategory(Id))
            {
                _ = db.IncomeSubcategories.Add(new IncomeSubcategoryEntity
                {
                    UserId = userId,
                    Id = s.Id,
                    CategoryId = Id,
                    Name = s.Name,
                    Slug = s.Slug,
                    IsSystemReserved = s.IsSystemReserved,
                    SortOrder = s.SortOrder,
                    IsActive = true,
                    UpdatedAtUtc = utcNow,
                });
            }
        }
    }

    private readonly record struct IncSubDef(string Id, string Name, string Slug, int SortOrder, bool IsSystemReserved);

    private static IEnumerable<IncSubDef> SubsForCategory(string categoryId)
    {
        return categoryId switch
        {
            "inc_cat_employment" =>
            [
                new("inc_sub_emp_salary", "Scheduled salary / wage", "inc_emp_salary", 0, false),
            new("inc_sub_emp_bonus", "Bonus & commission", "inc_emp_bonus", 1, false),
            new("inc_sub_emp_equity", "Equity compensation (RSU, options)", "inc_emp_equity", 2, false),
            new("inc_sub_emp_allowance", "Allowances & stipends", "inc_emp_allowance", 3, false),
            new("inc_sub_emp_severance", "Severance & settlements", "inc_emp_severance", 4, false),
            new("inc_sub_emp_reimbursement", "Employer reimbursements (income)", "inc_emp_reimbursement", 5, false),
            new("inc_sub_emp_other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "inc_cat_self_employment" =>
            [
                new("inc_sub_self_scheduled", "Recurring client / retainer", "inc_self_scheduled", 0, false),
            new("inc_sub_self_project", "One-off project or invoice", "inc_self_project", 1, false),
            new("inc_sub_self_platform", "Platform payouts (marketplace, gig)", "inc_self_platform", 2, false),
            new("inc_sub_self_licensing", "Royalties & licensing (your work)", "inc_self_licensing", 3, false),
            new("inc_sub_self_consulting", "Consulting & professional fees", "inc_self_consulting", 4, false),
            new("inc_sub_self_other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "inc_cat_investment_passive" =>
            [
                new("inc_sub_inv_dividends", "Dividends", "inc_inv_dividends", 0, false),
            new("inc_sub_inv_interest", "Interest income", "inc_inv_interest", 1, false),
            new("inc_sub_inv_rental", "Rental income", "inc_inv_rental", 2, false),
            new("inc_sub_inv_capital", "Capital gains (cash received)", "inc_inv_capital", 3, false),
            new("inc_sub_inv_return_principal", "Return of principal", "inc_inv_return_principal", 4, false),
            new("inc_sub_inv_other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "inc_cat_benefits_transfers" =>
            [
                new("inc_sub_ben_pension", "Pension / retirement distribution", "inc_ben_pension", 0, false),
            new("inc_sub_ben_government", "Government benefit / subsidy", "inc_ben_government", 1, false),
            new("inc_sub_ben_family", "Family support & gifts received", "inc_ben_family", 2, false),
            new("inc_sub_ben_insurance", "Insurance payout", "inc_ben_insurance", 3, false),
            new("inc_sub_ben_prize", "Prizes & awards", "inc_ben_prize", 4, false),
            new("inc_sub_ben_other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "inc_cat_refunds_reversals" =>
            [
                new("inc_sub_ref_purchase", "Purchase refund", "inc_ref_purchase", 0, false),
            new("inc_sub_ref_chargeback", "Chargeback / dispute credit", "inc_ref_chargeback", 1, false),
            new("inc_sub_ref_tax", "Tax refund", "inc_ref_tax", 2, false),
            new("inc_sub_ref_fee", "Fee reversal", "inc_ref_fee", 3, false),
            new("inc_sub_ref_other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            "inc_cat_miscellaneous" =>
            [
                new("inc_sub_misc_general", "General miscellaneous", "inc_misc_general", 0, false),
            new("inc_sub_misc_other", "Other", TaxonomyConstants.OtherSlug, 999, true),
        ],
            _ => [],
        };
    }
}
