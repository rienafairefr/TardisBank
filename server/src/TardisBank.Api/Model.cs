using System;
using System.Net;
using System.Web;
using E247.Fun;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace TardisBank.Api
{
    public class Login
    {
        public int LoginId { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public Result<Account, TardisFault> AssertAccount(Account account)
            => account.LoginId == LoginId
            ? (Result<Account, TardisFault>) account
            : new TardisFault(HttpStatusCode.NotFound, "Not Found");
    }

    public static class LoginExtensions
    {
        public static Result<Login, TardisFault> AssertLogin(this Maybe<Login> maybe)
            => maybe.ToTardisResult(HttpStatusCode.InternalServerError, "Expected auth token not present");

        public static Result<Account, TardisFault> AssertAccount(this Maybe<Login> maybe, Account account)
            => maybe.ToTardisResult(HttpStatusCode.InternalServerError, "Expected auth token not present")
                .Bind(login => login.AssertAccount(account));

        public static EmailMessage CreateVerificationEmail(this Login login, string token, HttpRequest request)
        {
            var uri = UriHelper.BuildAbsolute(
                request.Scheme,
                request.Host,
                request.PathBase,
                new PathString("/verify").Add(new PathString($"/{token}"))
            );
            return new EmailMessage
            {
                ToAddress = login.Email,
                Subject = "Please verify your Tardis Bank account",
                Body = $"Please click on this link or copy into browser: {uri}"
            };
        }
    }

    public class Account
    {
        public int AccountId { get; set; }
        public int LoginId { get; set; }
        public string AccountName { get; set; }
    }

    public class Transaction
    {
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public DateTimeOffset TransactionDate { get; set;}
        public decimal Amount { get; set;}
        public decimal Balance { get; set; }

        public Transaction CalculateBalance(Transaction lastTransaction)
        {
            Balance = lastTransaction.Balance + Amount;
            return this;
        }
    }

    public class Schedule
    {
        public int ScheduleId { get; set; }
        public int AccountId { get; set; }
        public TimePeriod TimePeriod { get; set; }
        public DateTimeOffset NextRun { get; set; }
        public decimal Amount { get; set;}
    }

    public enum TimePeriod
    {
        day,
        week,
        month
    }

    public class Token
    {
        public Login Login { get; set; }
        public DateTimeOffset Expires { get; set; }
        public TokenType TokenType { get; set; }
    }

    public enum TokenType
    {
        Login,
        Verification,
        PasswordReset
    }

    public class PasswordChange
    {
        public string OldPassword { get; set; }
        public string NewPasswordHash { get; set; }
    }

    public static class PasswordChangeExtensions
    {
        public static Result<PasswordChange, TardisFault> AssertOldPasswordMatches(
            this PasswordChange passwordChange, 
            Login login)
            => Password.HashMatches(passwordChange.OldPassword, login.PasswordHash)
            ? (Result<PasswordChange, TardisFault>) passwordChange
            : new TardisFault("Old password is incorrect.");
    }

    public class EmailMessage
    {
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}