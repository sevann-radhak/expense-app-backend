using ExpenseTracker.Infrastructure.Data.Entities;

namespace ExpenseTracker.Infrastructure.Services;

internal static class DevDemoScenarioGenerator
{
    private const int DemoYear = 2025;
    private const string Currency = "USD";
    private const string PiId = "pi_demo_main";

    private static Random CreateRng(string userId)
    {
        unchecked
        {
            uint h = 2166136261u;
            foreach (char c in userId)
            {
                h ^= c;
                h *= 16777619u;
            }

            int seed = (int)(h & 0x7FFFFFFF);
            if (seed == 0)
            {
                seed = 1;
            }

            return new Random(seed);
        }
    }

    public static void AppendDemoRows(
        string userId,
        DateTime utcNow,
        ICollection<PaymentInstrumentEntity> paymentInstruments,
        ICollection<ExpenseEntity> expenses,
        ICollection<IncomeEntryEntity> incomeEntries)
    {
        Random rng = CreateRng(userId);
        string[] cardLabels = ["Visa everyday", "Debit principal", "Visa rewards", "Mastercard daily"];
        string[] banks = ["Regional CU", "Metro Bank", "Union Savings", "First National"];
        paymentInstruments.Add(
            new PaymentInstrumentEntity
            {
                UserId = userId,
                Id = PiId,
                Label = $"{Pick(rng, cardLabels)} · {Pick(rng, banks)}",
                BankName = Pick(rng, banks),
                IsActive = true,
                IsDefault = true,
                FeeDescription = "",
                UpdatedAtUtc = utcNow,
            });

        int expenseSeq = 0;
        int incomeSeq = 0;
        decimal baseRent = R(rng, 980m, 1450m);
        decimal baseSalary = R(rng, 3200m, 5200m);

        for (int month = 1; month <= 12; month++)
        {
            bool peak = month is 6 or 12;
            int peakExtra = peak ? rng.Next(4, 10) : 0;

            AddExpense(
                expenses,
                userId,
                ref expenseSeq,
                utcNow,
                month,
                DayInMonth(rng, month, 1, 5),
                "cat_fixed_expenses",
                "cat_fixed_expenses_rent",
                R(rng, baseRent * 0.98m, baseRent * 1.02m),
                false,
                null,
                "Rent");
            AddExpense(
                expenses,
                userId,
                ref expenseSeq,
                utcNow,
                month,
                DayInMonth(rng, month, 6, 12),
                "cat_fixed_expenses",
                "cat_fixed_expenses_electricity",
                R(rng, 45m, 120m),
                rng.NextDouble() < 0.35,
                PiId,
                "Electricity");
            AddExpense(
                expenses,
                userId,
                ref expenseSeq,
                utcNow,
                month,
                DayInMonth(rng, month, 8, 14),
                "cat_fixed_expenses",
                "cat_fixed_expenses_internet_tv",
                R(rng, 55m, 95m),
                rng.NextDouble() < 0.5,
                PiId,
                "Internet & TV");
            AddExpense(
                expenses,
                userId,
                ref expenseSeq,
                utcNow,
                month,
                DayInMonth(rng, month, 3, 9),
                "cat_fixed_expenses",
                "cat_fixed_expenses_gas",
                R(rng, 18m, 85m),
                rng.NextDouble() < 0.25,
                null,
                "Natural gas");
            AddExpense(
                expenses,
                userId,
                ref expenseSeq,
                utcNow,
                month,
                DayInMonth(rng, month, 10, 18),
                "cat_fixed_expenses",
                "cat_fixed_expenses_cellphone",
                R(rng, 35m, 85m),
                rng.NextDouble() < 0.6,
                PiId,
                "Cell phone");

            int groceryRuns = rng.Next(4, 8) + (peak ? rng.Next(1, 4) : 0);
            for (int g = 0; g < groceryRuns; g++)
            {
                AddExpense(
                    expenses,
                    userId,
                    ref expenseSeq,
                    utcNow,
                    month,
                    DayInMonth(rng, month, 1, 28),
                    "cat_fixed_expenses",
                    "cat_fixed_expenses_groceries",
                    R(rng, 42m, 165m),
                    rng.NextDouble() < 0.45,
                    PiId,
                    Pick(rng, ["Supermarket", "Grocery run", "Weekly groceries", "Corner store"]));
            }

            int fuelRuns = rng.Next(2, 5) + (peak ? rng.Next(1, 3) : 0);
            for (int f = 0; f < fuelRuns; f++)
            {
                AddExpense(
                    expenses,
                    userId,
                    ref expenseSeq,
                    utcNow,
                    month,
                    DayInMonth(rng, month, 1, 28),
                    "cat_transport",
                    "cat_transport_fuel",
                    R(rng, 35m, 95m),
                    true,
                    PiId,
                    "Gas station");
            }

            int dining = rng.Next(2, 6) + (peak ? rng.Next(2, 6) : 0);
            for (int d = 0; d < dining; d++)
            {
                AddExpense(
                    expenses,
                    userId,
                    ref expenseSeq,
                    utcNow,
                    month,
                    DayInMonth(rng, month, 1, 28),
                    "cat_leisure",
                    "cat_leisure_dining",
                    R(rng, 12m, 85m),
                    true,
                    PiId,
                    Pick(rng, ["Lunch out", "Dinner", "Cafe", "Takeout", "Brunch"]));
            }

            if (rng.NextDouble() < 0.55)
            {
                AddExpense(
                    expenses,
                    userId,
                    ref expenseSeq,
                    utcNow,
                    month,
                    DayInMonth(rng, month, 1, 28),
                    "cat_subscriptions_digital",
                    Pick(
                        rng,
                        [
                            "cat_subscriptions_digital_streaming",
                            "cat_subscriptions_digital_music",
                            "cat_subscriptions_digital_cloud",
                        ]),
                    R(rng, 8m, 22m),
                    true,
                    PiId,
                    "Subscription");
            }

            if (rng.NextDouble() < 0.4)
            {
                AddExpense(
                    expenses,
                    userId,
                    ref expenseSeq,
                    utcNow,
                    month,
                    DayInMonth(rng, month, 1, 28),
                    "cat_health",
                    Pick(
                        rng,
                        [
                            "cat_health_pharmacy",
                            "cat_health_personal_care",
                            "cat_health_gym",
                        ]),
                    R(rng, 15m, 95m),
                    rng.NextDouble() < 0.5,
                    PiId,
                    Pick(rng, ["Pharmacy", "Gym", "Personal care"]));
            }

            if (rng.NextDouble() < 0.35)
            {
                AddExpense(
                    expenses,
                    userId,
                    ref expenseSeq,
                    utcNow,
                    month,
                    DayInMonth(rng, month, 1, 28),
                    "cat_transport",
                    Pick(
                        rng,
                        [
                            "cat_transport_public",
                            "cat_transport_rideshare",
                            "cat_transport_parking",
                        ]),
                    R(rng, 3.5m, 38m),
                    rng.NextDouble() < 0.55,
                    PiId,
                    Pick(rng, ["Transit", "Parking", "Rideshare"]));
            }

            if (month % 3 == 0 && rng.NextDouble() < 0.65)
            {
                AddExpense(
                    expenses,
                    userId,
                    ref expenseSeq,
                    utcNow,
                    month,
                    DayInMonth(rng, month, 20, 28),
                    "cat_taxes",
                    Pick(
                        rng,
                        [
                            "cat_taxes_national",
                            "cat_taxes_provincial",
                            "cat_taxes_vehicle",
                        ]),
                    R(rng, 45m, 420m),
                    rng.NextDouble() < 0.3,
                    null,
                    "Taxes / fees");
            }

            for (int p = 0; p < peakExtra; p++)
            {
                AddExpense(
                    expenses,
                    userId,
                    ref expenseSeq,
                    utcNow,
                    month,
                    DayInMonth(rng, month, 1, 28),
                    "cat_leisure",
                    Pick(
                        rng,
                        [
                            "cat_leisure_travel",
                            "cat_leisure_entertainment",
                            "cat_leisure_dining",
                            "cat_leisure_hobbies",
                        ]),
                    R(rng, 35m, 680m),
                    true,
                    PiId,
                    month == 6
                        ? Pick(rng, ["Summer trip", "Hotels", "Flights", "Weekend away"])
                        : Pick(rng, ["Holiday travel", "Gifts", "Year-end dining", "Hotels"]));
            }

            if (rng.NextDouble() < 0.25)
            {
                AddExpense(
                    expenses,
                    userId,
                    ref expenseSeq,
                    utcNow,
                    month,
                    DayInMonth(rng, month, 1, 28),
                    "cat_housing",
                    Pick(
                        rng,
                        [
                            "cat_housing_cleaning",
                            "cat_housing_repairs",
                            "cat_housing_appliances",
                        ]),
                    R(rng, 18m, 220m),
                    rng.NextDouble() < 0.4,
                    PiId,
                    "Home");
            }

            if (rng.NextDouble() < 0.2)
            {
                AddExpense(
                    expenses,
                    userId,
                    ref expenseSeq,
                    utcNow,
                    month,
                    DayInMonth(rng, month, 1, 28),
                    "cat_pets",
                    Pick(
                        rng,
                        [
                            "cat_pets_food",
                            "cat_pets_vet",
                            "cat_pets_grooming",
                        ]),
                    R(rng, 22m, 180m),
                    rng.NextDouble() < 0.35,
                    PiId,
                    "Pet");
            }

            int pay1 = Math.Clamp(rng.Next(12, 17), 1, DaysInMonth(month));
            int pay2 = Math.Clamp(rng.Next(26, 31), 1, DaysInMonth(month));
            AddIncome(
                incomeEntries,
                userId,
                ref incomeSeq,
                utcNow,
                DemoYear,
                month,
                pay1,
                "inc_cat_employment",
                "inc_sub_emp_salary",
                R(rng, baseSalary * 0.97m, baseSalary * 1.03m),
                "Salary deposit");
            AddIncome(
                incomeEntries,
                userId,
                ref incomeSeq,
                utcNow,
                DemoYear,
                month,
                pay2,
                "inc_cat_employment",
                "inc_sub_emp_salary",
                R(rng, baseSalary * 0.97m, baseSalary * 1.03m),
                "Salary deposit");
        }

        int bonusCount = rng.Next(1, 4);
        for (int b = 0; b < bonusCount; b++)
        {
            int month = rng.Next(1, 13);
            AddIncome(
                incomeEntries,
                userId,
                ref incomeSeq,
                utcNow,
                DemoYear,
                month,
                DayInMonth(rng, month, 10, 22),
                "inc_cat_employment",
                "inc_sub_emp_bonus",
                R(rng, 400m, 2200m),
                Pick(rng, ["Performance bonus", "Quarterly bonus", "Year-end bonus"]));
        }

        if (rng.NextDouble() < 0.75)
        {
            int month = rng.Next(1, 13);
            AddIncome(
                incomeEntries,
                userId,
                ref incomeSeq,
                utcNow,
                DemoYear,
                month,
                DayInMonth(rng, month, 5, 20),
                "inc_cat_self_employment",
                "inc_sub_self_project",
                R(rng, 250m, 1800m),
                Pick(rng, ["Freelance invoice", "Side project", "Consulting"]));
        }

        if (rng.NextDouble() < 0.5)
        {
            int month = rng.Next(1, 13);
            AddIncome(
                incomeEntries,
                userId,
                ref incomeSeq,
                utcNow,
                DemoYear,
                month,
                DayInMonth(rng, month, 8, 25),
                "inc_cat_employment",
                "inc_sub_emp_reimbursement",
                R(rng, 45m, 320m),
                "Employer reimbursement");
        }

        if (rng.NextDouble() < 0.45)
        {
            int month = rng.Next(1, 13);
            AddIncome(
                incomeEntries,
                userId,
                ref incomeSeq,
                utcNow,
                DemoYear,
                month,
                DayInMonth(rng, month, 1, 28),
                "inc_cat_investment_passive",
                Pick(
                    rng,
                    [
                        "inc_sub_inv_dividends",
                        "inc_sub_inv_interest",
                    ]),
                R(rng, 12m, 185m),
                Pick(rng, ["Dividend", "Interest", "Cash distribution"]));
        }

        if (rng.NextDouble() < 0.35)
        {
            int month = rng.Next(1, 13);
            AddIncome(
                incomeEntries,
                userId,
                ref incomeSeq,
                utcNow,
                DemoYear,
                month,
                DayInMonth(rng, month, 1, 28),
                "inc_cat_refunds_reversals",
                Pick(
                    rng,
                    [
                        "inc_sub_ref_purchase",
                        "inc_sub_ref_fee",
                    ]),
                R(rng, 8m, 120m),
                "Refund / reversal");
        }
    }

    private static void AddExpense(
        ICollection<ExpenseEntity> list,
        string userId,
        ref int seq,
        DateTime utc,
        int month,
        int dayOfMonth,
        string categoryId,
        string subcategoryId,
        decimal amount,
        bool card,
        string? piId,
        string description)
    {
        seq++;
        string id = $"d{DemoYear}e{seq:D6}";
        decimal rounded = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        list.Add(
            new ExpenseEntity
            {
                UserId = userId,
                Id = id,
                OccurredOn = $"{DemoYear}-{Month2(month)}-{Day2(Math.Clamp(dayOfMonth, 1, DaysInMonth(month)))}",
                CategoryId = categoryId,
                SubcategoryId = subcategoryId,
                AmountOriginal = rounded,
                CurrencyCode = Currency,
                ManualFxRateToUsd = 1m,
                AmountUsd = rounded,
                PaidWithCreditCard = card,
                PaymentInstrumentId = card ? piId : null,
                Description = description,
                UpdatedAtUtc = utc,
            });
    }

    private static void AddIncome(
        ICollection<IncomeEntryEntity> list,
        string userId,
        ref int seq,
        DateTime utc,
        int year,
        int month,
        int dayOfMonth,
        string categoryId,
        string subcategoryId,
        decimal amount,
        string description)
    {
        seq++;
        string id = $"d{year}i{seq:D6}";
        decimal rounded = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        list.Add(
            new IncomeEntryEntity
            {
                UserId = userId,
                Id = id,
                ReceivedOn = $"{year}-{Month2(month)}-{Day2(dayOfMonth)}",
                IncomeCategoryId = categoryId,
                IncomeSubcategoryId = subcategoryId,
                AmountOriginal = rounded,
                CurrencyCode = Currency,
                ManualFxRateToUsd = 1m,
                AmountUsd = rounded,
                Description = description,
                UpdatedAtUtc = utc,
            });
    }

    private static int DayInMonth(Random rng, int month, int minDay, int maxDay)
    {
        int last = DaysInMonth(month);
        int lo = Math.Clamp(Math.Min(minDay, maxDay), 1, last);
        int hi = Math.Clamp(Math.Max(minDay, maxDay), 1, last);
        return rng.Next(lo, hi + 1);
    }

    private static int DaysInMonth(int month)
    {
        return month switch
        {
            2 => 28,
            4 or 6 or 9 or 11 => 30,
            _ => 31,
        };
    }

    private static string Month2(int m)
    {
        return m < 10 ? $"0{m}" : m.ToString();
    }

    private static string Day2(int d)
    {
        return d < 10 ? $"0{d}" : d.ToString();
    }

    private static decimal R(Random rng, decimal min, decimal max)
    {
        return min + ((max - min) * (decimal)rng.NextDouble());
    }

    private static T Pick<T>(Random rng, T[] items)
    {
        return items[rng.Next(items.Length)];
    }
}
