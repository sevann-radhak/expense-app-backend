using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialBookSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => new { x.UserId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "income_categories",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_income_categories", x => new { x.UserId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "payment_instruments",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    BillingCycleDay = table.Column<int>(type: "int", nullable: true),
                    AnnualFeeAmount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: true),
                    MonthlyFeeAmount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: true),
                    FeeDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    StatementClosingDay = table.Column<int>(type: "int", nullable: true),
                    PaymentDueDay = table.Column<int>(type: "int", nullable: true),
                    NominalAprPercent = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: true),
                    DisplaySuffix = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_instruments", x => new { x.UserId, x.Id });
                });

            migrationBuilder.CreateTable(
                name: "user_book_metadata",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BookRevision = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_book_metadata", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "subcategories",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsSystemReserved = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subcategories", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_subcategories_categories_UserId_CategoryId",
                        columns: x => new { x.UserId, x.CategoryId },
                        principalTable: "categories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "income_subcategories",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsSystemReserved = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_income_subcategories", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_income_subcategories_income_categories_UserId_CategoryId",
                        columns: x => new { x.UserId, x.CategoryId },
                        principalTable: "income_categories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_preferences",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    DefaultCurrencyCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    LastPaymentInstrumentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_preferences", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_preferences_payment_instruments_UserId_LastPaymentInstrumentId",
                        columns: x => new { x.UserId, x.LastPaymentInstrumentId },
                        principalTable: "payment_instruments",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "expense_recurring_series",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AnchorOccurredOn = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RecurrenceJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HorizonMonths = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    SubcategoryId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    AmountOriginal = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ManualFxRateToUsd = table.Column<decimal>(type: "decimal(19,8)", precision: 19, scale: 8, nullable: false),
                    AmountUsd = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    PaidWithCreditCard = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentInstrumentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_recurring_series", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_expense_recurring_series_categories_UserId_CategoryId",
                        columns: x => new { x.UserId, x.CategoryId },
                        principalTable: "categories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expense_recurring_series_payment_instruments_UserId_PaymentInstrumentId",
                        columns: x => new { x.UserId, x.PaymentInstrumentId },
                        principalTable: "payment_instruments",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_expense_recurring_series_subcategories_UserId_SubcategoryId",
                        columns: x => new { x.UserId, x.SubcategoryId },
                        principalTable: "subcategories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "installment_plans",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PaymentCount = table.Column<int>(type: "int", nullable: false),
                    IntervalMonths = table.Column<int>(type: "int", nullable: false),
                    AnchorOccurredOn = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    SubcategoryId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    PaymentInstrumentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PerPaymentAmountOriginal = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ManualFxRateToUsd = table.Column<decimal>(type: "decimal(19,8)", precision: 19, scale: 8, nullable: false),
                    PerPaymentAmountUsd = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_installment_plans", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_installment_plans_categories_UserId_CategoryId",
                        columns: x => new { x.UserId, x.CategoryId },
                        principalTable: "categories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_installment_plans_payment_instruments_UserId_PaymentInstrumentId",
                        columns: x => new { x.UserId, x.PaymentInstrumentId },
                        principalTable: "payment_instruments",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_installment_plans_subcategories_UserId_SubcategoryId",
                        columns: x => new { x.UserId, x.SubcategoryId },
                        principalTable: "subcategories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "income_recurring_series",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AnchorReceivedOn = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RecurrenceJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HorizonMonths = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    IncomeCategoryId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    IncomeSubcategoryId = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    AmountOriginal = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ManualFxRateToUsd = table.Column<decimal>(type: "decimal(19,8)", precision: 19, scale: 8, nullable: false),
                    AmountUsd = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_income_recurring_series", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_income_recurring_series_income_categories_UserId_IncomeCategoryId",
                        columns: x => new { x.UserId, x.IncomeCategoryId },
                        principalTable: "income_categories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_income_recurring_series_income_subcategories_UserId_IncomeSubcategoryId",
                        columns: x => new { x.UserId, x.IncomeSubcategoryId },
                        principalTable: "income_subcategories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OccurredOn = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    SubcategoryId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AmountOriginal = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ManualFxRateToUsd = table.Column<decimal>(type: "decimal(19,8)", precision: 19, scale: 8, nullable: false),
                    AmountUsd = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    PaidWithCreditCard = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentInstrumentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RecurringSeriesId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PaymentExpectationStatus = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PaymentExpectationConfirmedOn = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    InstallmentPlanId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    InstallmentIndex = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_expenses_categories_UserId_CategoryId",
                        columns: x => new { x.UserId, x.CategoryId },
                        principalTable: "categories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expenses_expense_recurring_series_UserId_RecurringSeriesId",
                        columns: x => new { x.UserId, x.RecurringSeriesId },
                        principalTable: "expense_recurring_series",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_expenses_installment_plans_UserId_InstallmentPlanId",
                        columns: x => new { x.UserId, x.InstallmentPlanId },
                        principalTable: "installment_plans",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_expenses_payment_instruments_UserId_PaymentInstrumentId",
                        columns: x => new { x.UserId, x.PaymentInstrumentId },
                        principalTable: "payment_instruments",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_expenses_subcategories_UserId_SubcategoryId",
                        columns: x => new { x.UserId, x.SubcategoryId },
                        principalTable: "subcategories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "income_entries",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ReceivedOn = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IncomeCategoryId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IncomeSubcategoryId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    AmountOriginal = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ManualFxRateToUsd = table.Column<decimal>(type: "decimal(19,8)", precision: 19, scale: 8, nullable: false),
                    AmountUsd = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecurringSeriesId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExpectationStatus = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ExpectationConfirmedOn = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_income_entries", x => new { x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_income_entries_income_categories_UserId_IncomeCategoryId",
                        columns: x => new { x.UserId, x.IncomeCategoryId },
                        principalTable: "income_categories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_income_entries_income_recurring_series_UserId_RecurringSeriesId",
                        columns: x => new { x.UserId, x.RecurringSeriesId },
                        principalTable: "income_recurring_series",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_income_entries_income_subcategories_UserId_IncomeSubcategoryId",
                        columns: x => new { x.UserId, x.IncomeSubcategoryId },
                        principalTable: "income_subcategories",
                        principalColumns: new[] { "UserId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_UserId",
                table: "categories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_expense_recurring_series_UserId",
                table: "expense_recurring_series",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_expense_recurring_series_UserId_CategoryId",
                table: "expense_recurring_series",
                columns: new[] { "UserId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_expense_recurring_series_UserId_PaymentInstrumentId",
                table: "expense_recurring_series",
                columns: new[] { "UserId", "PaymentInstrumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_expense_recurring_series_UserId_SubcategoryId",
                table: "expense_recurring_series",
                columns: new[] { "UserId", "SubcategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_expenses_UserId_CategoryId",
                table: "expenses",
                columns: new[] { "UserId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_expenses_UserId_InstallmentPlanId",
                table: "expenses",
                columns: new[] { "UserId", "InstallmentPlanId" });

            migrationBuilder.CreateIndex(
                name: "IX_expenses_UserId_OccurredOn",
                table: "expenses",
                columns: new[] { "UserId", "OccurredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_expenses_UserId_PaymentInstrumentId",
                table: "expenses",
                columns: new[] { "UserId", "PaymentInstrumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_expenses_UserId_RecurringSeriesId",
                table: "expenses",
                columns: new[] { "UserId", "RecurringSeriesId" });

            migrationBuilder.CreateIndex(
                name: "IX_expenses_UserId_SubcategoryId",
                table: "expenses",
                columns: new[] { "UserId", "SubcategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_income_categories_UserId",
                table: "income_categories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_income_entries_UserId_IncomeCategoryId",
                table: "income_entries",
                columns: new[] { "UserId", "IncomeCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_income_entries_UserId_IncomeSubcategoryId",
                table: "income_entries",
                columns: new[] { "UserId", "IncomeSubcategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_income_entries_UserId_ReceivedOn",
                table: "income_entries",
                columns: new[] { "UserId", "ReceivedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_income_entries_UserId_RecurringSeriesId",
                table: "income_entries",
                columns: new[] { "UserId", "RecurringSeriesId" });

            migrationBuilder.CreateIndex(
                name: "IX_income_recurring_series_UserId",
                table: "income_recurring_series",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_income_recurring_series_UserId_IncomeCategoryId",
                table: "income_recurring_series",
                columns: new[] { "UserId", "IncomeCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_income_recurring_series_UserId_IncomeSubcategoryId",
                table: "income_recurring_series",
                columns: new[] { "UserId", "IncomeSubcategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_income_subcategories_UserId_CategoryId",
                table: "income_subcategories",
                columns: new[] { "UserId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_installment_plans_UserId",
                table: "installment_plans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_installment_plans_UserId_CategoryId",
                table: "installment_plans",
                columns: new[] { "UserId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_installment_plans_UserId_PaymentInstrumentId",
                table: "installment_plans",
                columns: new[] { "UserId", "PaymentInstrumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_installment_plans_UserId_SubcategoryId",
                table: "installment_plans",
                columns: new[] { "UserId", "SubcategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_payment_instruments_UserId",
                table: "payment_instruments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_subcategories_UserId_CategoryId",
                table: "subcategories",
                columns: new[] { "UserId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_user_preferences_UserId_LastPaymentInstrumentId",
                table: "user_preferences",
                columns: new[] { "UserId", "LastPaymentInstrumentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "income_entries");

            migrationBuilder.DropTable(
                name: "user_book_metadata");

            migrationBuilder.DropTable(
                name: "user_preferences");

            migrationBuilder.DropTable(
                name: "expense_recurring_series");

            migrationBuilder.DropTable(
                name: "installment_plans");

            migrationBuilder.DropTable(
                name: "income_recurring_series");

            migrationBuilder.DropTable(
                name: "payment_instruments");

            migrationBuilder.DropTable(
                name: "subcategories");

            migrationBuilder.DropTable(
                name: "income_subcategories");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "income_categories");
        }
    }
}
