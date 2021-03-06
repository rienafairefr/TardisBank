using System;
using System.Threading.Tasks;
using Npgsql;
using TardisBank.Api;
using Xunit;
using Xunit.Abstractions;

namespace TardisBank.IntegrationTests
{
    public class DbTests
    {
        private readonly ITestOutputHelper output;
        private readonly string connectionString;

        public DbTests(ITestOutputHelper output)
        {
            this.output = output;
            connectionString = Environment.GetEnvironmentVariable("TARDISBANK_DB_CON");
        }

        [Fact]
        public async Task ShouldBeAbleToInsertLogin()
        {
            var login = new Login
            {
                Email = $"{Guid.NewGuid().ToString()}@mailinator.com",
                PasswordHash = Guid.NewGuid().ToString()
            };

            var returnedLogin = await Db.InsertLogin(connectionString, login);

            Assert.Equal(login.Email, returnedLogin.Email);
            Assert.Equal(login.PasswordHash, returnedLogin.PasswordHash);
            Assert.True(returnedLogin.LoginId > 0);
        }

        [Fact]
        public async Task ShouldBeAbleToGetLoginByEmail()
        {
            var login = new Login
            {
                Email = $"{Guid.NewGuid().ToString()}@mailinator.com",
                PasswordHash = Guid.NewGuid().ToString()
            };

            await Db.InsertLogin(connectionString, login);
            var result = await Db.LoginByEmail(connectionString, login.Email);

            Assert.True(result.HasValue);
            var returnedLogin = result.Value;
            Assert.Equal(login.Email, returnedLogin.Email);
            Assert.Equal(login.PasswordHash, returnedLogin.PasswordHash);
            Assert.True(returnedLogin.LoginId > 0);
        }

        [Fact]
        public async Task ShouldBeAbleToUpdatePassword()
        {
            var login = new Login
            {
                Email = $"{Guid.NewGuid().ToString()}@mailinator.com",
                PasswordHash = Guid.NewGuid().ToString()
            };

            await Db.InsertLogin(connectionString, login);
            var result = await Db.LoginByEmail(connectionString, login.Email);

            var newPasswordHash = Guid.NewGuid().ToString();
            await Db.UpdateLoginPassword(connectionString, result.Value.LoginId, newPasswordHash);

            var loginByIdResult = await Db.LoginById(connectionString, result.Value.LoginId);

            Assert.Equal(newPasswordHash, loginByIdResult.PasswordHash);
        }

        [Fact]
        public async Task ShouldBeAbleToInsertSelectAndDeleteAccount()
        {
            var login = new Login
            {
                Email = $"{Guid.NewGuid().ToString()}@mailinator.com",
                PasswordHash = Guid.NewGuid().ToString()
            };

            var returnedLogin = await Db.InsertLogin(connectionString, login);

            var account = new Account
            {
                LoginId = returnedLogin.LoginId,
                AccountName = Guid.NewGuid().ToString()
            };

            var returnedAccount = await Db.InsertAccount(connectionString, account);

            Assert.Equal(account.LoginId, returnedAccount.LoginId);
            Assert.Equal(account.AccountName, returnedAccount.AccountName);
            Assert.True(returnedAccount.AccountId > 0);

            {
                var accounts = await Db.AccountByLogin(connectionString, returnedLogin);
                Assert.Collection(accounts, 
                    x => Assert.Equal(returnedAccount.AccountId, x.AccountId));
            }

            {
                var singleAccount = await Db.AccountById(connectionString, returnedAccount.AccountId);
                Assert.True(singleAccount.HasValue);
                Assert.Equal(returnedAccount.AccountId, singleAccount.Value.AccountId);
            }

            await Db.DeleteAccount(connectionString, returnedAccount);

            {
                var accounts = await Db.AccountByLogin(connectionString, returnedLogin);
                Assert.Empty(accounts);
            }
        }

        [Fact]
        public async Task ShouldBeAbleToInsertAndListTransactions()
        {
            var login = new Login
            {
                Email = $"{Guid.NewGuid().ToString()}@mailinator.com",
                PasswordHash = Guid.NewGuid().ToString()
            };

            var returnedLogin = await Db.InsertLogin(connectionString, login);

            var account = new Account
            {
                LoginId = returnedLogin.LoginId,
                AccountName = Guid.NewGuid().ToString()
            };

            var returnedAccount = await Db.InsertAccount(connectionString, account);

            var transaction = new Transaction
            {
                AccountId = returnedAccount.AccountId,
                TransactionDate = DateTimeOffset.Now,
                Amount = 2.40M,
                Balance = 11.66M
            };

            var returnTransaction = await Db.InsertTransaction(connectionString, transaction);

            Assert.Equal(transaction.AccountId, returnTransaction.AccountId);
            Assert.Equal(transaction.TransactionDate.Minute, returnTransaction.TransactionDate.Minute);
            Assert.Equal(transaction.Amount, returnTransaction.Amount);
            Assert.Equal(transaction.Balance, returnTransaction.Balance);
            Assert.True(returnTransaction.TransactionId > 0);

            {
                var transactions = await Db.TransactionsByAccount(connectionString, returnedAccount);
                Assert.Collection(transactions, 
                    x => Assert.Equal(returnTransaction.TransactionId, x.TransactionId));
            }
        }

        [Fact]
        public async Task ShouldBeAbleToInsertListAndDeleteSchedules()
        {
            var login = new Login
            {
                Email = $"{Guid.NewGuid().ToString()}@mailinator.com",
                PasswordHash = Guid.NewGuid().ToString()
            };

            var returnedLogin = await Db.InsertLogin(connectionString, login);

            var account = new Account
            {
                LoginId = returnedLogin.LoginId,
                AccountName = Guid.NewGuid().ToString()
            };

            var returnedAccount = await Db.InsertAccount(connectionString, account);

            var schedule = new Schedule
            {
                AccountId = returnedAccount.AccountId,
                TimePeriod = TimePeriod.week,
                NextRun = DateTimeOffset.Now,
                Amount = 3.0M
            };

            var returnedSchedule = await Db.InsertSchedule(connectionString, schedule);

            Assert.Equal(schedule.AccountId, returnedSchedule.AccountId);
            Assert.Equal(schedule.TimePeriod, returnedSchedule.TimePeriod);
            Assert.Equal(schedule.NextRun.Day, returnedSchedule.NextRun.Day);
            Assert.Equal(schedule.Amount, returnedSchedule.Amount);
            Assert.True(returnedSchedule.ScheduleId > 0);

            var schedules = await Db.ScheduleByAccount(connectionString, returnedAccount);
            Assert.Collection(schedules, x => Assert.Equal(returnedSchedule.ScheduleId, x.ScheduleId));

            await Db.DeleteSchedule(connectionString, returnedSchedule);

            var schedulesAfterDeletion = await Db.ScheduleByAccount(connectionString, returnedAccount);
            Assert.Empty(schedulesAfterDeletion);
        }
    }
}