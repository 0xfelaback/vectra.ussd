using Bogus;
using Microsoft.AspNetCore.Identity;
using Vectra.Ussd.Domain.Entities.CoreBanking;
using Vectra.Ussd.Mocks;

public class DateSeeder
{
    private static IPasswordHasher<string> _passwordHasher = new PasswordHasher<string>();
    public DateSeeder(IPasswordHasher<string> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }
    public static Dictionary<string, List<object>> GenerateMockRecords(int count = 1000)
    {
        var bvnRecords = new Faker<MockBvnRecord>()
        .RuleFor(b => b.BvnNumber, f => f.Random.ReplaceNumbers("###########"))
        .RuleFor(b => b.FirstName, f => f.Name.FirstName())
        .RuleFor(b => b.LastName, f => f.Name.LastName())
        .RuleFor(b => b.MiddleName, f => f.Name.FirstName().OrNull(f, 0.2f))
        .RuleFor(b => b.DateOfBirth, (f, _) => f.Date.PastDateOnly(60, DateOnly.FromDateTime(DateTime.Now.AddYears(-18))))
        .RuleFor(b => b.PhoneNumber, f => f.Phone.PhoneNumber("080########"))
        .RuleFor(x => x.ImageUrl, (f, _) => f.Internet.Avatar())
        .RuleFor(x => x.gender, (f, _) => f.PickRandom<MockBvnRecord.Gender>())
        .RuleFor(x => x.SignatureUrl, (f, _) => f.Internet.Url().OrNull(f, 0.2f))
        .RuleFor(x => x.IsActive, (f, _) => f.Random.Bool(0.8f)).Generate(count);

        var simSwapRecords = new Faker<MockSimSwapRecord>()
        .RuleFor(s => s.PhoneNumber, f => f.PickRandom(bvnRecords).PhoneNumber)
        .RuleFor(s => s.LastSwapDate, (f, _) => f.Date.Past(60, DateTime.Now.AddYears(-18)))
        .RuleFor(s => s.IsSwapped, f => f.Random.Bool(0.2f))
        .RuleFor(s => s.IsServiceAvailable, f => f.Random.Bool(0.9f)).Generate(count);

        var simReassignedRecords = new Faker<MockSimReassignedRecord>()
        .RuleFor(s => s.PhoneNumber, f => f.PickRandom(bvnRecords).PhoneNumber)
        .RuleFor(s => s.LastAssignedDate, (f, _) => f.Date.Past(60, DateTime.Now.AddYears(-18)))
        .RuleFor(s => s.IsReassigned, f => f.Random.Bool(0.1f))
        .RuleFor(s => s.IsServiceAvailable, f => f.Random.Bool(0.9f)).Generate(count);

        var customerRecords = new Faker<Customer>()
        .RuleFor(c => c.FirstName, f => f.Name.FirstName())
        .RuleFor(c => c.LastName, f => f.Name.LastName())
        .RuleFor(c => c.MiddleName, f => f.Name.FirstName().OrNull(f, 0.2f))
        .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber("080########"))
        .RuleFor(c => c.BvnNumber, f => f.Random.ReplaceNumbers("###########"))
        .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.FirstName, c.LastName))
        .RuleFor(c => c.IsUssdRegistered, f => f.Random.Bool(0.7f))
        .RuleFor(c => c.IsPinSet, (f, c) => c.IsUssdRegistered)
        .RuleFor(c => c.ussdPin1Hash, (f, c) => c.IsUssdRegistered ? _passwordHasher.HashPassword(c.PhoneNumber!, "1234") : null)
        .RuleFor(c => c.Status, f => f.PickRandom<Customer.CustomerStatus>())
        .RuleFor(c => c.CreatedAt, f => f.Date.Past(2))
        .Generate(count);

        var customerAccounts = new List<CustomerAccount>();
        foreach (var customer in customerRecords)
        {
            var accountFaker = new Faker<CustomerAccount>()
                .RuleFor(a => a.CustomerId, _ => customer.Id)
                .RuleFor(a => a.Customer, _ => customer)
                .RuleFor(a => a.AccountNumber, f => f.Random.ReplaceNumbers("##########"))
                .RuleFor(a => a.PhoneNumber, _ => customer.PhoneNumber!)
                .RuleFor(a => a.BvnNumber, _ => customer.BvnNumber!)
                .RuleFor(a => a.Type, f => f.PickRandom<CustomerAccount.AccountType>())
                .RuleFor(a => a.Status, f => f.PickRandom<CustomerAccount.AccountStatus>())
                .RuleFor(a => a.TierLevel, f => f.Random.Number(1, 3))
                .RuleFor(a => a.Balance, f => f.Finance.Amount(1000, 1000000))
                .RuleFor(a => a.HasPnd, f => f.Random.Bool(0.05f))
                .RuleFor(a => a.IsUssdRegistered, _ => customer.IsUssdRegistered)
                .RuleFor(a => a.IsLinked, f => f.Random.Bool(0.9f))
                .RuleFor(a => a.ImageUrl, f => f.Internet.Avatar())
                .RuleFor(a => a.DateCreated, f => f.Date.Past(3));

            int accountsCount = new Faker().Random.Number(1, 3);
            var accounts = accountFaker.Generate(accountsCount);
            customerAccounts.AddRange(accounts);
            customer.CustomerAccounts = accounts;
        }

        var serviceAccounts = new List<ServiceAccount>();
        foreach (var account in customerAccounts.Where(a => a.IsLinked))
        {
            var serviceAccount = new ServiceAccount
            {
                CustomerId = account.CustomerId,
                Customer = account.Customer,
                CustomerAccountId = account.Id,
                CustomerAccount = account,
                AccountNumber = account.AccountNumber,
                PhoneNumber = account.PhoneNumber,
                BvnNumber = account.BvnNumber,
                IsPrimary = !serviceAccounts.Any(s => s.CustomerId == account.CustomerId),
                DateLinked = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 100))
            };
            serviceAccounts.Add(serviceAccount);
            account.ServiceAccount = serviceAccount;
            account.Customer.ServiceAccounts.Add(serviceAccount);
        }

        foreach (var account in customerAccounts)
        {
            var faker = new Faker();
            if (faker.Random.Bool(0.6f))
            {
                var cardCount = faker.Random.Number(1, 3);
                for (var i = 0; i < cardCount; i++)
                {
                    account.Cards.Add(new MockATMCard
                    {
                        CustomerAccountId = account.Id,
                        CustomerAccount = account,
                        Type = faker.PickRandom<MockATMCard.cardType>(),
                        Network = faker.PickRandom<MockATMCard.cardNetwork>(),
                        ExpirationDate = DateOnly.FromDateTime(faker.Date.Future(5)),
                        CVV = faker.Random.Number(100, 999),
                        CardNumber = faker.Finance.CreditCardNumber().Replace("-", string.Empty).Replace(" ", string.Empty),
                        Contactless = faker.Random.Bool(0.8f),
                        InternationalUsage = faker.Random.Bool(0.7f),
                        IsActive = true,
                        IsActivated = faker.Random.Bool(0.9f)
                    });
                }
            }
        }

        var nubanRecords = new Faker<NubanInterbankAccounts>()
        .RuleFor(n => n.AccountNumber, f => f.Random.ReplaceNumbers("##########"))
        .RuleFor(n => n.AccountName, f => f.Name.FullName())
        .RuleFor(n => n.BankCode, f => f.PickRandom(new[] { "232", "011", "044", "058", "214", "035", "057", "023", "050", "221" }))
        .RuleFor(n => n.BankName, (f, n) => n.BankCode switch
        {
            "232" => "Sterling Bank",
            "011" => "First Bank of Nigeria",
            "044" => "Access Bank",
            "058" => "Guaranty Trust Bank",
            "214" => "First City Monument Bank",
            "035" => "Wema Bank",
            "057" => "Zenith Bank",
            "023" => "Citibank Nigeria",
            "050" => "Ecobank Nigeria",
            "221" => "Stanbic IBTC Bank",
            _ => "Unknown Bank"
        })
        .RuleFor(n => n.IsActive, f => f.Random.Bool(0.95f)).Generate(count);

        var fakeTransactions = new Faker<TransactionHistory>()
            .RuleFor(t => t.TransactionId, f => f.Random.Guid().ToString("N"))
            .RuleFor(t => t.SenderAccountNumber, f => f.PickRandom(customerAccounts).AccountNumber)
            .RuleFor(t => t.ReceiverAccountNumber, f => f.PickRandom(customerAccounts).AccountNumber)
            .RuleFor(t => t.Amount, f => f.Finance.Amount(100, 50000))
            .RuleFor(t => t.TransferType, f => f.PickRandom<TransferType>())
            .RuleFor(t => t.Status, f => f.Random.WeightedRandom(new[] { TransactionHistory.TransactionStatus.Success, TransactionHistory.TransactionStatus.Failed }, new[] { 0.9f, 0.1f }))
            .RuleFor(t => t.FailureReason, (f, t) => t.Status == TransactionHistory.TransactionStatus.Failed ? f.PickRandom("Insufficient balance", "Wrong PIN", "Limit exceeded") : null)
            .RuleFor(t => t.DateCreated, f => f.Date.Past(1))
            .RuleFor(t => t.BankName, (f, t) => t.TransferType == TransferType.Interbank ? f.PickRandom(nubanRecords).BankName : "Sterling Bank")
            .RuleFor(t => t.IdempotencyKey, (f, t) => t.Status == TransactionHistory.TransactionStatus.Success ? f.Random.Guid().ToString("N") : null).Generate(customerAccounts.Count * 2);

        var airtimeRecharges = new Faker<AirtimeRecharge>()
            .RuleFor(r => r.SenderPhoneNumber, f => f.PickRandom(customerAccounts).PhoneNumber)
            .RuleFor(r => r.BeneficiaryPhoneNumber, f => f.PickRandom(customerAccounts).PhoneNumber)
            .RuleFor(r => r.RemitterAccountNumber, f => f.PickRandom(customerAccounts).AccountNumber)
            .RuleFor(r => r.Amount, f => f.Finance.Amount(100, 10000))
            .RuleFor(r => r.Network, f => f.PickRandom<AirtimeRecharge.AirtimeNetwork>())
            .RuleFor(r => r.Status, f => f.PickRandom<AirtimeRecharge.TransactionStatus>())
            .RuleFor(r => r.TransactionReference, (f, r) => r.Status != AirtimeRecharge.TransactionStatus.Pending ? f.Random.Guid().ToString("N") : null)
            .RuleFor(r => r.IsSelfRecharge, f => f.Random.Bool(0.8f))
            .RuleFor(r => r.Channel, f => f.PickRandom(new[] { "USSD", "API", "MOBILE" }))
            .RuleFor(r => r.CreatedAt, f => f.Date.Past(1))
            .Generate(1000);

        var result = new Dictionary<string, List<object>>(){
            { "bvn", bvnRecords.Cast<object>().ToList() },
            { "simswap", simSwapRecords.Cast<object>().ToList() },
            { "reassigned", simReassignedRecords.Cast<object>().ToList() },
            { "customers", customerRecords.Cast<object>().ToList() },
            { "accounts", customerAccounts.Cast<object>().ToList() },
            { "serviceAccounts", serviceAccounts.Cast<object>().ToList() },
            { "nuban", nubanRecords.Cast<object>().ToList() },
            { "accountTiers", GetDefaultAccountTiers().Cast<object>().ToList() },
            { "transactions", fakeTransactions.Cast<object>().ToList() },
            { "airtimeRecharges", airtimeRecharges.Cast<object>().ToList() },
            { "dataBundles", GetDefaultDataBundles().Cast<object>().ToList() },
            };

        return result;
    }

    private static List<AccountTier> GetDefaultAccountTiers()
    {
        return new List<AccountTier>
        {
            new() { TierLevel = 1, TransferType = TransferType.Intrabank, SingleTransactionLimit = 50000m, DailyTransactionLimit = 100000m, DailyTransactionCount = 10, IsActive = true },
            new() { TierLevel = 1, TransferType = TransferType.Interbank, SingleTransactionLimit = 20000m, DailyTransactionLimit = 50000m, DailyTransactionCount = 5, IsActive = true },
            new() { TierLevel = 2, TransferType = TransferType.Intrabank, SingleTransactionLimit = 200000m, DailyTransactionLimit = 500000m, DailyTransactionCount = 20, IsActive = true },
            new() { TierLevel = 2, TransferType = TransferType.Interbank, SingleTransactionLimit = 100000m, DailyTransactionLimit = 200000m, DailyTransactionCount = 10, IsActive = true },
            new() { TierLevel = 3, TransferType = TransferType.Intrabank, SingleTransactionLimit = 1000000m, DailyTransactionLimit = 5000000m, DailyTransactionCount = 50, IsActive = true },
            new() { TierLevel = 3, TransferType = TransferType.Interbank, SingleTransactionLimit = 500000m, DailyTransactionLimit = 1000000m, DailyTransactionCount = 25, IsActive = true }
        };
    }

    private static List<DataBundle> GetDefaultDataBundles()
    {
        return new List<DataBundle>
    {
        // ======================= MTN =======================
        new() { Telco = AirtimeRecharge.AirtimeNetwork.MTN, BundleName = "75MB Daily", DataSizeMB = 75, Price = 75m, ValidityDays = 1, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.MTN, BundleName = "350MB Daily", DataSizeMB = 350, Price = 200m, ValidityDays = 1, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.MTN, BundleName = "1GB Daily", DataSizeMB = 1024, Price = 350m, ValidityDays = 1, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.MTN, BundleName = "1GB Weekly", DataSizeMB = 1024, Price = 500m, ValidityDays = 7, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.MTN, BundleName = "5GB Weekly", DataSizeMB = 5120, Price = 1500m, ValidityDays = 7, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.MTN, BundleName = "2GB Monthly", DataSizeMB = 2048, Price = 1200m, ValidityDays = 30, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.MTN, BundleName = "10GB Monthly", DataSizeMB = 10240, Price = 3500m, ValidityDays = 30, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.MTN, BundleName = "25GB Monthly", DataSizeMB = 25600, Price = 7500m, ValidityDays = 30, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.MTN, BundleName = "Night Plan 500MB", DataSizeMB = 500, Price = 50m, ValidityDays = 1, IsActive = true },

        // ======================= AIRTEL =======================
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Airtel, BundleName = "100MB Daily", DataSizeMB = 100, Price = 100m, ValidityDays = 1, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Airtel, BundleName = "300MB Daily", DataSizeMB = 300, Price = 200m, ValidityDays = 1, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Airtel, BundleName = "1GB Daily", DataSizeMB = 1024, Price = 300m, ValidityDays = 1, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.Airtel, BundleName = "1.5GB Weekly", DataSizeMB = 1536, Price = 500m, ValidityDays = 7, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Airtel, BundleName = "6GB Weekly", DataSizeMB = 6144, Price = 1500m, ValidityDays = 7, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.Airtel, BundleName = "3GB Monthly", DataSizeMB = 3072, Price = 1500m, ValidityDays = 30, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Airtel, BundleName = "15GB Monthly", DataSizeMB = 15360, Price = 4000m, ValidityDays = 30, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Airtel, BundleName = "40GB Monthly", DataSizeMB = 40960, Price = 10000m, ValidityDays = 30, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.Airtel, BundleName = "YouTube Night 1GB", DataSizeMB = 1024, Price = 100m, ValidityDays = 1, IsActive = true },

        // ======================= GLO =======================
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Globacom, BundleName = "200MB Daily", DataSizeMB = 200, Price = 100m, ValidityDays = 1, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Globacom, BundleName = "500MB Daily", DataSizeMB = 500, Price = 200m, ValidityDays = 1, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Globacom, BundleName = "1GB Daily", DataSizeMB = 1024, Price = 300m, ValidityDays = 1, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.Globacom, BundleName = "2GB Weekly", DataSizeMB = 2048, Price = 500m, ValidityDays = 7, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Globacom, BundleName = "7GB Weekly", DataSizeMB = 7168, Price = 1500m, ValidityDays = 7, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.Globacom, BundleName = "4GB Monthly", DataSizeMB = 4096, Price = 1500m, ValidityDays = 30, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Globacom, BundleName = "20GB Monthly", DataSizeMB = 20480, Price = 5000m, ValidityDays = 30, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.Globacom, BundleName = "50GB Monthly", DataSizeMB = 51200, Price = 12000m, ValidityDays = 30, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.Globacom, BundleName = "Night Plan 1GB", DataSizeMB = 1024, Price = 100m, ValidityDays = 1, IsActive = true },

        // ======================= 9MOBILE =======================
        new() { Telco = AirtimeRecharge.AirtimeNetwork.NineMobile, BundleName = "100MB Daily", DataSizeMB = 100, Price = 100m, ValidityDays = 1, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.NineMobile, BundleName = "500MB Daily", DataSizeMB = 500, Price = 200m, ValidityDays = 1, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.NineMobile, BundleName = "1GB Daily", DataSizeMB = 1024, Price = 300m, ValidityDays = 1, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.NineMobile, BundleName = "1.5GB Weekly", DataSizeMB = 1536, Price = 500m, ValidityDays = 7, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.NineMobile, BundleName = "7GB Weekly", DataSizeMB = 7168, Price = 1500m, ValidityDays = 7, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.NineMobile, BundleName = "3GB Monthly", DataSizeMB = 3072, Price = 1500m, ValidityDays = 30, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.NineMobile, BundleName = "15GB Monthly", DataSizeMB = 15360, Price = 4000m, ValidityDays = 30, IsActive = true },
        new() { Telco = AirtimeRecharge.AirtimeNetwork.NineMobile, BundleName = "50GB Monthly", DataSizeMB = 51200, Price = 12000m, ValidityDays = 30, IsActive = true },

        new() { Telco = AirtimeRecharge.AirtimeNetwork.NineMobile, BundleName = "Social Pack (WhatsApp)", DataSizeMB = 500, Price = 200m, ValidityDays = 7, IsActive = true },
    };
    }

}