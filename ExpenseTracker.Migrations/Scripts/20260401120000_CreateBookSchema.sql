/*
  Book schema v1 — aligned with expense-app Drift tables (snake_case columns) + server tenancy.
  See sibling repo docs/05-sync-spec.md. Composite PK (user_id, id) on all book tables.
  Money stored as DECIMAL (not FLOAT) per backend architecture rules.
*/

CREATE TABLE dbo.payment_instruments (
    user_id NVARCHAR(450) NOT NULL,
    id NVARCHAR(128) NOT NULL,
    label NVARCHAR(512) NOT NULL,
    bank_name NVARCHAR(512) NULL,
    billing_cycle_day INT NULL,
    annual_fee_amount DECIMAL(19, 4) NULL,
    monthly_fee_amount DECIMAL(19, 4) NULL,
    fee_description NVARCHAR(MAX) NOT NULL CONSTRAINT DF_payment_instruments_fee_description DEFAULT (N''),
    is_active BIT NOT NULL CONSTRAINT DF_payment_instruments_is_active DEFAULT (1),
    is_default BIT NOT NULL CONSTRAINT DF_payment_instruments_is_default DEFAULT (0),
    statement_closing_day INT NULL,
    payment_due_day INT NULL,
    nominal_apr_percent DECIMAL(19, 6) NULL,
    credit_limit DECIMAL(19, 4) NULL,
    display_suffix NVARCHAR(64) NULL,
    updated_at_utc DATETIME2(3) NOT NULL CONSTRAINT DF_payment_instruments_updated_at DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_payment_instruments PRIMARY KEY (user_id, id)
);

CREATE TABLE dbo.categories (
    user_id NVARCHAR(450) NOT NULL,
    id NVARCHAR(128) NOT NULL,
    name NVARCHAR(512) NOT NULL,
    description NVARCHAR(MAX) NULL,
    sort_order INT NOT NULL CONSTRAINT DF_categories_sort_order DEFAULT (0),
    is_active BIT NOT NULL CONSTRAINT DF_categories_is_active DEFAULT (1),
    updated_at_utc DATETIME2(3) NOT NULL CONSTRAINT DF_categories_updated_at DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_categories PRIMARY KEY (user_id, id)
);

CREATE TABLE dbo.subcategories (
    user_id NVARCHAR(450) NOT NULL,
    id NVARCHAR(128) NOT NULL,
    category_id NVARCHAR(128) NOT NULL,
    name NVARCHAR(512) NOT NULL,
    description NVARCHAR(MAX) NULL,
    slug NVARCHAR(256) NOT NULL,
    is_system_reserved BIT NOT NULL CONSTRAINT DF_subcategories_is_system_reserved DEFAULT (0),
    sort_order INT NOT NULL CONSTRAINT DF_subcategories_sort_order DEFAULT (0),
    is_active BIT NOT NULL CONSTRAINT DF_subcategories_is_active DEFAULT (1),
    updated_at_utc DATETIME2(3) NOT NULL CONSTRAINT DF_subcategories_updated_at DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_subcategories PRIMARY KEY (user_id, id),
    CONSTRAINT FK_subcategories_categories FOREIGN KEY (user_id, category_id)
        REFERENCES dbo.categories (user_id, id) ON DELETE CASCADE
);

CREATE TABLE dbo.income_categories (
    user_id NVARCHAR(450) NOT NULL,
    id NVARCHAR(128) NOT NULL,
    name NVARCHAR(512) NOT NULL,
    description NVARCHAR(MAX) NULL,
    sort_order INT NOT NULL CONSTRAINT DF_income_categories_sort_order DEFAULT (0),
    is_active BIT NOT NULL CONSTRAINT DF_income_categories_is_active DEFAULT (1),
    updated_at_utc DATETIME2(3) NOT NULL CONSTRAINT DF_income_categories_updated_at DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_income_categories PRIMARY KEY (user_id, id)
);

CREATE TABLE dbo.income_subcategories (
    user_id NVARCHAR(450) NOT NULL,
    id NVARCHAR(128) NOT NULL,
    category_id NVARCHAR(128) NOT NULL,
    name NVARCHAR(512) NOT NULL,
    description NVARCHAR(MAX) NULL,
    slug NVARCHAR(256) NOT NULL,
    is_system_reserved BIT NOT NULL CONSTRAINT DF_income_subcategories_is_system_reserved DEFAULT (0),
    sort_order INT NOT NULL CONSTRAINT DF_income_subcategories_sort_order DEFAULT (0),
    is_active BIT NOT NULL CONSTRAINT DF_income_subcategories_is_active DEFAULT (1),
    updated_at_utc DATETIME2(3) NOT NULL CONSTRAINT DF_income_subcategories_updated_at DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_income_subcategories PRIMARY KEY (user_id, id),
    CONSTRAINT FK_income_subcategories_income_categories FOREIGN KEY (user_id, category_id)
        REFERENCES dbo.income_categories (user_id, id) ON DELETE CASCADE
);

CREATE TABLE dbo.expense_recurring_series (
    user_id NVARCHAR(450) NOT NULL,
    id NVARCHAR(128) NOT NULL,
    anchor_occurred_on NVARCHAR(32) NOT NULL,
    recurrence_json NVARCHAR(MAX) NOT NULL,
    horizon_months INT NOT NULL,
    active BIT NOT NULL CONSTRAINT DF_expense_recurring_series_active DEFAULT (1),
    category_id NVARCHAR(128) NOT NULL,
    subcategory_id NVARCHAR(128) NOT NULL,
    amount_original DECIMAL(19, 4) NOT NULL,
    currency_code NVARCHAR(16) NOT NULL CONSTRAINT DF_expense_recurring_series_currency DEFAULT (N'USD'),
    manual_fx_rate_to_usd DECIMAL(19, 8) NOT NULL CONSTRAINT DF_expense_recurring_series_fx DEFAULT (1),
    amount_usd DECIMAL(19, 4) NOT NULL,
    paid_with_credit_card BIT NOT NULL CONSTRAINT DF_expense_recurring_series_paid_cc DEFAULT (0),
    description NVARCHAR(MAX) NOT NULL CONSTRAINT DF_expense_recurring_series_description DEFAULT (N''),
    payment_instrument_id NVARCHAR(128) NULL,
    updated_at_utc DATETIME2(3) NOT NULL CONSTRAINT DF_expense_recurring_series_updated_at DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_expense_recurring_series PRIMARY KEY (user_id, id),
    CONSTRAINT FK_expense_recurring_series_categories FOREIGN KEY (user_id, category_id)
        REFERENCES dbo.categories (user_id, id),
    CONSTRAINT FK_expense_recurring_series_subcategories FOREIGN KEY (user_id, subcategory_id)
        REFERENCES dbo.subcategories (user_id, id),
    CONSTRAINT FK_expense_recurring_series_payment_instruments FOREIGN KEY (user_id, payment_instrument_id)
        REFERENCES dbo.payment_instruments (user_id, id) ON DELETE SET NULL
);

CREATE TABLE dbo.income_recurring_series (
    user_id NVARCHAR(450) NOT NULL,
    id NVARCHAR(128) NOT NULL,
    anchor_received_on NVARCHAR(32) NOT NULL,
    recurrence_json NVARCHAR(MAX) NOT NULL,
    horizon_months INT NOT NULL,
    active BIT NOT NULL CONSTRAINT DF_income_recurring_series_active DEFAULT (1),
    income_category_id NVARCHAR(128) NOT NULL,
    income_subcategory_id NVARCHAR(128) NOT NULL,
    amount_original DECIMAL(19, 4) NOT NULL,
    currency_code NVARCHAR(16) NOT NULL CONSTRAINT DF_income_recurring_series_currency DEFAULT (N'USD'),
    manual_fx_rate_to_usd DECIMAL(19, 8) NOT NULL CONSTRAINT DF_income_recurring_series_fx DEFAULT (1),
    amount_usd DECIMAL(19, 4) NOT NULL,
    description NVARCHAR(MAX) NOT NULL CONSTRAINT DF_income_recurring_series_description DEFAULT (N''),
    updated_at_utc DATETIME2(3) NOT NULL CONSTRAINT DF_income_recurring_series_updated_at DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_income_recurring_series PRIMARY KEY (user_id, id),
    CONSTRAINT FK_income_recurring_series_income_categories FOREIGN KEY (user_id, income_category_id)
        REFERENCES dbo.income_categories (user_id, id),
    CONSTRAINT FK_income_recurring_series_income_subcategories FOREIGN KEY (user_id, income_subcategory_id)
        REFERENCES dbo.income_subcategories (user_id, id)
);

CREATE TABLE dbo.installment_plans (
    user_id NVARCHAR(450) NOT NULL,
    id NVARCHAR(128) NOT NULL,
    payment_count INT NOT NULL,
    interval_months INT NOT NULL CONSTRAINT DF_installment_plans_interval_months DEFAULT (1),
    anchor_occurred_on NVARCHAR(32) NOT NULL,
    category_id NVARCHAR(128) NOT NULL,
    subcategory_id NVARCHAR(128) NOT NULL,
    payment_instrument_id NVARCHAR(128) NULL,
    per_payment_amount_original DECIMAL(19, 4) NOT NULL,
    currency_code NVARCHAR(16) NOT NULL CONSTRAINT DF_installment_plans_currency DEFAULT (N'USD'),
    manual_fx_rate_to_usd DECIMAL(19, 8) NOT NULL CONSTRAINT DF_installment_plans_fx DEFAULT (1),
    per_payment_amount_usd DECIMAL(19, 4) NOT NULL,
    description NVARCHAR(MAX) NOT NULL CONSTRAINT DF_installment_plans_description DEFAULT (N''),
    updated_at_utc DATETIME2(3) NOT NULL CONSTRAINT DF_installment_plans_updated_at DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_installment_plans PRIMARY KEY (user_id, id),
    CONSTRAINT FK_installment_plans_categories FOREIGN KEY (user_id, category_id)
        REFERENCES dbo.categories (user_id, id),
    CONSTRAINT FK_installment_plans_subcategories FOREIGN KEY (user_id, subcategory_id)
        REFERENCES dbo.subcategories (user_id, id),
    CONSTRAINT FK_installment_plans_payment_instruments FOREIGN KEY (user_id, payment_instrument_id)
        REFERENCES dbo.payment_instruments (user_id, id) ON DELETE SET NULL
);

CREATE TABLE dbo.expenses (
    user_id NVARCHAR(450) NOT NULL,
    id NVARCHAR(128) NOT NULL,
    occurred_on NVARCHAR(32) NOT NULL,
    category_id NVARCHAR(128) NOT NULL,
    subcategory_id NVARCHAR(128) NOT NULL,
    amount_original DECIMAL(19, 4) NOT NULL,
    currency_code NVARCHAR(16) NOT NULL CONSTRAINT DF_expenses_currency DEFAULT (N'USD'),
    manual_fx_rate_to_usd DECIMAL(19, 8) NOT NULL CONSTRAINT DF_expenses_fx DEFAULT (1),
    amount_usd DECIMAL(19, 4) NOT NULL,
    paid_with_credit_card BIT NOT NULL CONSTRAINT DF_expenses_paid_cc DEFAULT (0),
    description NVARCHAR(MAX) NOT NULL CONSTRAINT DF_expenses_description DEFAULT (N''),
    payment_instrument_id NVARCHAR(128) NULL,
    recurring_series_id NVARCHAR(128) NULL,
    payment_expectation_status NVARCHAR(64) NULL,
    payment_expectation_confirmed_on NVARCHAR(32) NULL,
    installment_plan_id NVARCHAR(128) NULL,
    installment_index INT NULL,
    updated_at_utc DATETIME2(3) NOT NULL CONSTRAINT DF_expenses_updated_at DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_expenses PRIMARY KEY (user_id, id),
    CONSTRAINT FK_expenses_categories FOREIGN KEY (user_id, category_id)
        REFERENCES dbo.categories (user_id, id),
    CONSTRAINT FK_expenses_subcategories FOREIGN KEY (user_id, subcategory_id)
        REFERENCES dbo.subcategories (user_id, id),
    CONSTRAINT FK_expenses_payment_instruments FOREIGN KEY (user_id, payment_instrument_id)
        REFERENCES dbo.payment_instruments (user_id, id) ON DELETE SET NULL,
    CONSTRAINT FK_expenses_expense_recurring_series FOREIGN KEY (user_id, recurring_series_id)
        REFERENCES dbo.expense_recurring_series (user_id, id) ON DELETE CASCADE,
    CONSTRAINT FK_expenses_installment_plans FOREIGN KEY (user_id, installment_plan_id)
        REFERENCES dbo.installment_plans (user_id, id) ON DELETE SET NULL
);

CREATE TABLE dbo.income_entries (
    user_id NVARCHAR(450) NOT NULL,
    id NVARCHAR(128) NOT NULL,
    received_on NVARCHAR(32) NOT NULL,
    income_category_id NVARCHAR(128) NOT NULL,
    income_subcategory_id NVARCHAR(128) NOT NULL,
    amount_original DECIMAL(19, 4) NOT NULL,
    currency_code NVARCHAR(16) NOT NULL CONSTRAINT DF_income_entries_currency DEFAULT (N'USD'),
    manual_fx_rate_to_usd DECIMAL(19, 8) NOT NULL CONSTRAINT DF_income_entries_fx DEFAULT (1),
    amount_usd DECIMAL(19, 4) NOT NULL,
    description NVARCHAR(MAX) NOT NULL CONSTRAINT DF_income_entries_description DEFAULT (N''),
    recurring_series_id NVARCHAR(128) NULL,
    expectation_status NVARCHAR(64) NULL,
    expectation_confirmed_on NVARCHAR(32) NULL,
    updated_at_utc DATETIME2(3) NOT NULL CONSTRAINT DF_income_entries_updated_at DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_income_entries PRIMARY KEY (user_id, id),
    CONSTRAINT FK_income_entries_income_categories FOREIGN KEY (user_id, income_category_id)
        REFERENCES dbo.income_categories (user_id, id),
    CONSTRAINT FK_income_entries_income_subcategories FOREIGN KEY (user_id, income_subcategory_id)
        REFERENCES dbo.income_subcategories (user_id, id),
    CONSTRAINT FK_income_entries_income_recurring_series FOREIGN KEY (user_id, recurring_series_id)
        REFERENCES dbo.income_recurring_series (user_id, id) ON DELETE CASCADE
);

CREATE TABLE dbo.user_preferences (
    user_id NVARCHAR(450) NOT NULL,
    locale NVARCHAR(32) NULL,
    default_currency_code NVARCHAR(16) NULL,
    last_payment_instrument_id NVARCHAR(128) NULL,
    updated_at_utc DATETIME2(3) NOT NULL CONSTRAINT DF_user_preferences_updated_at DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_user_preferences PRIMARY KEY (user_id),
    CONSTRAINT FK_user_preferences_last_instrument FOREIGN KEY (user_id, last_payment_instrument_id)
        REFERENCES dbo.payment_instruments (user_id, id) ON DELETE SET NULL
);
