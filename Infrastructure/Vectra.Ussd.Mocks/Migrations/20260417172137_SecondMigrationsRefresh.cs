using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vectra.Ussd.Mocks.Migrations
{
    /// <inheritdoc />
    public partial class SecondMigrationsRefresh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TierLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    TransferType = table.Column<int>(type: "INTEGER", nullable: false),
                    SingleTransactionLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DailyTransactionLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DailyTransactionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AirtimeRecharges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderPhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    BeneficiaryPhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    RemitterAccountNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Network = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionReference = table.Column<string>(type: "TEXT", nullable: true),
                    AggregatorReference = table.Column<string>(type: "TEXT", nullable: true),
                    IsSelfRecharge = table.Column<bool>(type: "INTEGER", nullable: false),
                    Channel = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirtimeRecharges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataBundles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Telco = table.Column<int>(type: "INTEGER", nullable: false),
                    BundleName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DataSizeMB = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValidityDays = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataBundles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MockBvnRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BvnNumber = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 11, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    gender = table.Column<int>(type: "INTEGER", nullable: false),
                    SignatureUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockBvnRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountNumber = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 10, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    BvnNumber = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 11, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    TierLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    HasPnd = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Balance = table.Column<decimal>(type: "TEXT", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    SignatureUrl = table.Column<string>(type: "TEXT", nullable: true),
                    isUssdRegistered = table.Column<bool>(type: "INTEGER", nullable: false),
                    ussdPin1Hash = table.Column<string>(type: "TEXT", nullable: true),
                    ussdPin2Hash = table.Column<string>(type: "TEXT", nullable: true),
                    PinTrials = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MockFraudFlags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhoneNumber = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 11, nullable: false),
                    BvnNumber = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 15, nullable: false),
                    Reason = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountNumber = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockFraudFlags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MockNibssSimReassignedRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    IsReassigned = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    LastAssignedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsServiceAvailable = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockNibssSimReassignedRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MockNibssSimSwapRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    IsSwapped = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    LastSwapDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsServiceAvailable = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockNibssSimSwapRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nubans",
                columns: table => new
                {
                    AccountNumber = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 10, nullable: false),
                    AccountName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BankCode = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 3, nullable: false),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nubans", x => x.AccountNumber);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SenderAccountNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ReceiverAccountNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransferType = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    FailureReason = table.Column<string>(type: "TEXT", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "TEXT", maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MockATMCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpirationDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CVV = table.Column<int>(type: "INTEGER", nullable: false),
                    CardNumber = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Network = table.Column<int>(type: "INTEGER", nullable: false),
                    isActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActivated = table.Column<bool>(type: "INTEGER", nullable: false),
                    cardPINHash = table.Column<string>(type: "TEXT", nullable: true),
                    PosEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AtmEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    WebEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Contactless = table.Column<bool>(type: "INTEGER", nullable: false),
                    InternationalUsage = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockATMCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockATMCards_ServiceAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "ServiceAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountTiers_TierLevel_TransferType",
                table: "AccountTiers",
                columns: new[] { "TierLevel", "TransferType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataBundles_Telco_BundleName",
                table: "DataBundles",
                columns: new[] { "Telco", "BundleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MockATMCards_AccountId",
                table: "MockATMCards",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DateCreated",
                table: "Transactions",
                column: "DateCreated");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_IdempotencyKey",
                table: "Transactions",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReceiverAccountNumber",
                table: "Transactions",
                column: "ReceiverAccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SenderAccountNumber",
                table: "Transactions",
                column: "SenderAccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionId",
                table: "Transactions",
                column: "TransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountTiers");

            migrationBuilder.DropTable(
                name: "AirtimeRecharges");

            migrationBuilder.DropTable(
                name: "DataBundles");

            migrationBuilder.DropTable(
                name: "MockATMCards");

            migrationBuilder.DropTable(
                name: "MockBvnRecords");

            migrationBuilder.DropTable(
                name: "MockFraudFlags");

            migrationBuilder.DropTable(
                name: "MockNibssSimReassignedRecords");

            migrationBuilder.DropTable(
                name: "MockNibssSimSwapRecords");

            migrationBuilder.DropTable(
                name: "Nubans");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "ServiceAccounts");
        }
    }
}
