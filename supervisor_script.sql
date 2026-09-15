IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Branches] (
    [Id] int NOT NULL IDENTITY,
    [BranchName] nvarchar(max) NOT NULL,
    [BranchCode] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [ManagerName] nvarchar(max) NULL,
    [OpenTime] time NULL,
    [CloseTime] time NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
);

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [CategoryName] nvarchar(max) NOT NULL,
    [CategoryCode] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [IconPath] nvarchar(max) NULL,
    [DisplayOrder] int NOT NULL,
    [ParentCategoryId] int NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Categories_Categories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [CompanyInfos] (
    [Id] int NOT NULL IDENTITY,
    [CompanyName] nvarchar(max) NOT NULL,
    [CompanyNameAr] nvarchar(max) NULL,
    [LogoPath] nvarchar(max) NULL,
    [HeaderImagePath] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Mobile] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Website] nvarchar(max) NULL,
    [TaxNumber] nvarchar(max) NULL,
    [CommercialRegister] nvarchar(max) NULL,
    [FooterNote] nvarchar(max) NULL,
    [InvoiceFooter] nvarchar(max) NULL,
    [CurrencySymbol] nvarchar(max) NULL,
    [CurrencyCode] nvarchar(max) NULL,
    [TimeZone] nvarchar(max) NULL,
    [DateFormat] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_CompanyInfos] PRIMARY KEY ([Id])
);

CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [CustomerName] nvarchar(max) NOT NULL,
    [CustomerCode] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Mobile] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [TaxNumber] nvarchar(max) NULL,
    [OpeningBalance] decimal(18,2) NOT NULL,
    [CurrentBalance] decimal(18,2) NOT NULL,
    [TotalPurchases] decimal(18,2) NOT NULL,
    [LoyaltyPoints] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);

CREATE TABLE [DiscountSettings] (
    [Id] int NOT NULL IDENTITY,
    [DiscountName] nvarchar(max) NOT NULL,
    [DiscountCode] nvarchar(max) NULL,
    [DiscountType] nvarchar(max) NOT NULL,
    [DiscountValue] decimal(18,2) NOT NULL,
    [MaxDiscountAmount] decimal(18,2) NULL,
    [MinPurchaseAmount] decimal(18,2) NULL,
    [UsageLimit] int NULL,
    [UsedCount] int NOT NULL,
    [StartDate] datetime2 NULL,
    [EndDate] datetime2 NULL,
    [ApplicableProducts] nvarchar(max) NULL,
    [ApplicableCategories] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_DiscountSettings] PRIMARY KEY ([Id])
);

CREATE TABLE [PaymentMethods] (
    [Id] int NOT NULL IDENTITY,
    [MethodName] nvarchar(max) NOT NULL,
    [PaymentType] nvarchar(max) NULL,
    [IconPath] nvarchar(max) NULL,
    [IsDefault] bit NOT NULL,
    [ProcessingFee] decimal(18,2) NULL,
    [Notes] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_PaymentMethods] PRIMARY KEY ([Id])
);

CREATE TABLE [Roles] (
    [Id] int NOT NULL IDENTITY,
    [RoleName] nvarchar(450) NOT NULL,
    [Permissions] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);

CREATE TABLE [Settings] (
    [Id] int NOT NULL IDENTITY,
    [SettingKey] nvarchar(450) NOT NULL,
    [SettingValue] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [Group] nvarchar(max) NULL,
    [IsEncrypted] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Settings] PRIMARY KEY ([Id])
);

CREATE TABLE [Suppliers] (
    [Id] int NOT NULL IDENTITY,
    [SupplierName] nvarchar(max) NOT NULL,
    [SupplierCode] nvarchar(max) NULL,
    [ContactPerson] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Mobile] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [TaxNumber] nvarchar(max) NULL,
    [CommercialRegister] nvarchar(max) NULL,
    [OpeningBalance] decimal(18,2) NOT NULL,
    [CurrentBalance] decimal(18,2) NOT NULL,
    [Notes] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Suppliers] PRIMARY KEY ([Id])
);

CREATE TABLE [TaxSettings] (
    [Id] int NOT NULL IDENTITY,
    [TaxName] nvarchar(max) NOT NULL,
    [TaxCode] nvarchar(max) NULL,
    [TaxRate] decimal(18,2) NOT NULL,
    [IsDefault] bit NOT NULL,
    [IsInclusive] bit NOT NULL,
    [EffectiveDate] datetime2 NULL,
    [ExpiryDate] datetime2 NULL,
    [Notes] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_TaxSettings] PRIMARY KEY ([Id])
);

CREATE TABLE [Units] (
    [Id] int NOT NULL IDENTITY,
    [UnitName] nvarchar(max) NOT NULL,
    [UnitSymbol] nvarchar(max) NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Units] PRIMARY KEY ([Id])
);

CREATE TABLE [PrinterSettings] (
    [Id] int NOT NULL IDENTITY,
    [PrinterName] nvarchar(max) NOT NULL,
    [PrinterType] nvarchar(max) NOT NULL,
    [IpAddress] nvarchar(max) NULL,
    [Port] int NULL,
    [PrinterModel] nvarchar(max) NULL,
    [PaperWidth] int NULL,
    [PaperHeight] int NULL,
    [IsDefault] bit NOT NULL,
    [IsThermal] bit NOT NULL,
    [CopyCount] int NULL,
    [BranchId] int NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_PrinterSettings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PrinterSettings_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Warehouses] (
    [Id] int NOT NULL IDENTITY,
    [WarehouseName] nvarchar(max) NOT NULL,
    [WarehouseCode] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [ManagerName] nvarchar(max) NULL,
    [IsMainWarehouse] bit NOT NULL,
    [BranchId] int NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Warehouses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Warehouses_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(max) NOT NULL,
    [Username] nvarchar(450) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [LastLogin] datetime2 NULL,
    [IsLocked] bit NOT NULL,
    [FailedLoginAttempts] int NOT NULL,
    [RoleId] int NOT NULL,
    [BranchId] int NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Users_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [ProductName] nvarchar(max) NOT NULL,
    [ProductCode] nvarchar(450) NOT NULL,
    [Barcode] nvarchar(450) NULL,
    [SKU] nvarchar(max) NULL,
    [PurchasePrice] decimal(18,2) NOT NULL,
    [SellingPrice] decimal(18,2) NOT NULL,
    [WholesalePrice] decimal(18,2) NULL,
    [TaxRate] decimal(18,2) NOT NULL,
    [MinStock] int NOT NULL,
    [MaxStock] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [ImagePath] nvarchar(max) NULL,
    [Color] nvarchar(max) NULL,
    [Size] nvarchar(max) NULL,
    [Weight] decimal(18,2) NULL,
    [IsTaxable] bit NOT NULL,
    [IsTrackStock] bit NOT NULL,
    [IsService] bit NOT NULL,
    [CategoryId] int NULL,
    [UnitId] int NOT NULL,
    [SupplierId] int NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Products_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Products_Units_UnitId] FOREIGN KEY ([UnitId]) REFERENCES [Units] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Username] nvarchar(max) NULL,
    [FullName] nvarchar(max) NULL,
    [Action] nvarchar(max) NOT NULL,
    [EntityName] nvarchar(max) NULL,
    [EntityId] int NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [Browser] nvarchar(max) NULL,
    [Device] nvarchar(max) NULL,
    [ActionType] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ProductWarehouses] (
    [Id] int NOT NULL IDENTITY,
    [Quantity] int NOT NULL,
    [ReservedQuantity] int NOT NULL,
    [ReorderPoint] int NOT NULL,
    [ProductId] int NOT NULL,
    [WarehouseId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_ProductWarehouses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductWarehouses_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProductWarehouses_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);

CREATE INDEX [IX_Categories_ParentCategoryId] ON [Categories] ([ParentCategoryId]);

CREATE INDEX [IX_PrinterSettings_BranchId] ON [PrinterSettings] ([BranchId]);

CREATE UNIQUE INDEX [IX_Products_Barcode] ON [Products] ([Barcode]) WHERE [Barcode] IS NOT NULL;

CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);

CREATE UNIQUE INDEX [IX_Products_ProductCode] ON [Products] ([ProductCode]);

CREATE INDEX [IX_Products_SupplierId] ON [Products] ([SupplierId]);

CREATE INDEX [IX_Products_UnitId] ON [Products] ([UnitId]);

CREATE INDEX [IX_ProductWarehouses_ProductId] ON [ProductWarehouses] ([ProductId]);

CREATE INDEX [IX_ProductWarehouses_WarehouseId] ON [ProductWarehouses] ([WarehouseId]);

CREATE UNIQUE INDEX [IX_Roles_RoleName] ON [Roles] ([RoleName]);

CREATE UNIQUE INDEX [IX_Settings_SettingKey] ON [Settings] ([SettingKey]);

CREATE INDEX [IX_Users_BranchId] ON [Users] ([BranchId]);

CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);

CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);

CREATE INDEX [IX_Warehouses_BranchId] ON [Warehouses] ([BranchId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260703152840_InitialCreate', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Users] ADD [StoreName] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260705154453_AddStoreNameToUser', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [StockAlerts] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [WarehouseId] int NOT NULL,
    [AlertType] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [IsRead] bit NOT NULL,
    [ReadAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_StockAlerts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StockAlerts_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockAlerts_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [StockMovements] (
    [Id] int NOT NULL IDENTITY,
    [MovementNumber] nvarchar(max) NOT NULL,
    [ProductId] int NOT NULL,
    [WarehouseId] int NOT NULL,
    [MovementType] nvarchar(max) NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [TotalPrice] decimal(18,2) NOT NULL,
    [Notes] nvarchar(max) NULL,
    [SupplierId] int NULL,
    [CustomerId] int NULL,
    [UserId] int NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_StockMovements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StockMovements_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockMovements_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockMovements_Suppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [Suppliers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockMovements_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockMovements_Warehouses_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [StockTransfers] (
    [Id] int NOT NULL IDENTITY,
    [TransferNumber] nvarchar(max) NOT NULL,
    [ProductId] int NOT NULL,
    [FromWarehouseId] int NOT NULL,
    [ToWarehouseId] int NOT NULL,
    [Quantity] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [UserId] int NULL,
    [CompletedAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_StockTransfers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StockTransfers_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockTransfers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockTransfers_Warehouses_FromWarehouseId] FOREIGN KEY ([FromWarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_StockTransfers_Warehouses_ToWarehouseId] FOREIGN KEY ([ToWarehouseId]) REFERENCES [Warehouses] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_StockAlerts_ProductId] ON [StockAlerts] ([ProductId]);

CREATE INDEX [IX_StockAlerts_WarehouseId] ON [StockAlerts] ([WarehouseId]);

CREATE INDEX [IX_StockMovements_CustomerId] ON [StockMovements] ([CustomerId]);

CREATE INDEX [IX_StockMovements_ProductId] ON [StockMovements] ([ProductId]);

CREATE INDEX [IX_StockMovements_SupplierId] ON [StockMovements] ([SupplierId]);

CREATE INDEX [IX_StockMovements_UserId] ON [StockMovements] ([UserId]);

CREATE INDEX [IX_StockMovements_WarehouseId] ON [StockMovements] ([WarehouseId]);

CREATE INDEX [IX_StockTransfers_FromWarehouseId] ON [StockTransfers] ([FromWarehouseId]);

CREATE INDEX [IX_StockTransfers_ProductId] ON [StockTransfers] ([ProductId]);

CREATE INDEX [IX_StockTransfers_ToWarehouseId] ON [StockTransfers] ([ToWarehouseId]);

CREATE INDEX [IX_StockTransfers_UserId] ON [StockTransfers] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260710134249_AddStockManagement', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [Invoices] (
    [Id] int NOT NULL IDENTITY,
    [InvoiceNumber] nvarchar(max) NOT NULL,
    [CustomerId] int NOT NULL,
    [BranchId] int NOT NULL,
    [UserId] int NULL,
    [InvoiceDate] datetime2 NOT NULL,
    [SubTotal] decimal(18,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [DiscountAmount] decimal(18,2) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [Notes] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invoices_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Invoices_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Invoices_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [InvoiceItems] (
    [Id] int NOT NULL IDENTITY,
    [InvoiceId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [TotalPrice] decimal(18,2) NOT NULL,
    [TaxRate] decimal(18,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_InvoiceItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InvoiceItems_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InvoiceItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Payments] (
    [Id] int NOT NULL IDENTITY,
    [InvoiceId] int NOT NULL,
    [PaymentMethodId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [ReferenceNumber] nvarchar(max) NULL,
    [PaymentDate] datetime2 NOT NULL,
    [Notes] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payments_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Payments_PaymentMethods_PaymentMethodId] FOREIGN KEY ([PaymentMethodId]) REFERENCES [PaymentMethods] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_InvoiceItems_InvoiceId] ON [InvoiceItems] ([InvoiceId]);

CREATE INDEX [IX_InvoiceItems_ProductId] ON [InvoiceItems] ([ProductId]);

CREATE INDEX [IX_Invoices_BranchId] ON [Invoices] ([BranchId]);

CREATE INDEX [IX_Invoices_CustomerId] ON [Invoices] ([CustomerId]);

CREATE INDEX [IX_Invoices_UserId] ON [Invoices] ([UserId]);

CREATE INDEX [IX_Payments_InvoiceId] ON [Payments] ([InvoiceId]);

CREATE INDEX [IX_Payments_PaymentMethodId] ON [Payments] ([PaymentMethodId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260710135615_AddFinanceModule', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [SystemSettings] (
    [Id] int NOT NULL IDENTITY,
    [TimeZone] nvarchar(max) NULL,
    [FiscalYearStartMonth] int NULL,
    [DateFormat] nvarchar(max) NULL,
    [InvoicePrefix] nvarchar(max) NULL,
    [InvoicePadding] int NULL,
    [InvoiceFooterMessage] nvarchar(max) NULL,
    [DefaultPrintCopies] int NULL,
    [ShowCompanyLogoOnInvoice] bit NOT NULL,
    [DefaultTaxRate] decimal(18,2) NULL,
    [TaxCalculationMethod] nvarchar(max) NULL,
    [DefaultLowStockAlert] int NULL,
    [AutoUpdateStockAfterSale] bit NOT NULL,
    [AutoUpdateStockAfterReturn] bit NOT NULL,
    [EnableLowStockAlerts] bit NOT NULL,
    [AllowNegativeStock] bit NOT NULL,
    [UseBarcodeScannerInPOS] bit NOT NULL,
    [DefaultLanguage] nvarchar(max) NULL,
    [AvailableLanguages] nvarchar(max) NULL,
    [DiscountPrefix] nvarchar(max) NULL,
    [DiscountSuffix] nvarchar(max) NULL,
    [DiscountPosition] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260724173931_AddSystemSettings', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [Notifications] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [Type] nvarchar(max) NULL,
    [RedirectUrl] nvarchar(max) NULL,
    [IsRead] bit NOT NULL,
    [ReadAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260728173149_addmodel', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260728173303_AddNotificationsTable', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [SystemSettings] ADD [AutoPrintOnPayment] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [SystemSettings] ADD [CurrencySymbolPosition] nvarchar(max) NULL;

ALTER TABLE [SystemSettings] ADD [DecimalPlaces] int NULL;

ALTER TABLE [SystemSettings] ADD [DefaultDirection] nvarchar(max) NULL;

ALTER TABLE [SystemSettings] ADD [RoundDecimals] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [SystemSettings] ADD [SystemName] nvarchar(max) NULL;

ALTER TABLE [CompanyInfos] ADD [BranchName] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260817054355_AddSettingsExtraFields', N'10.0.0');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Notifications] ADD [Module] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260817073210_AddModuleToNotifications', N'10.0.0');

COMMIT;
GO

