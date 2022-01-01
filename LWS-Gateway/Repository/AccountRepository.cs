using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LWS_Gateway.Model;
using LWS_Gateway.Model.Request;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace LWS_Gateway.Repository
{
    public interface IAccountRepository
    {
        /// <summary>
        /// Create Account Asynchronously. If duplicated accounts are found, it will throw exceptions.
        /// </summary>
        /// <param name="message">Request Message from client.</param>
        /// <returns>None</returns>
        public Task<Account> CreateAccountAsync(RegisterRequest message);

        /// <summary>
        /// Try authenticating user with received email/password.
        /// </summary>
        /// <param name="message">Login Request</param>
        /// <returns>Account Information when succeeds to authenticate, or null if failed.</returns>
        public Task<Account> LoginAccountAsync(LoginRequest message);
        
        /// <summary>
        /// Save Pre-Created Access Token to Account.
        /// </summary>
        /// <param name="userEmail">User Email(Account Identifier)</param>
        /// <param name="accessToken">Access Token to Save.</param>
        /// <returns>Saved Access Token(which is identical to input accessToken)</returns>
        public Task<AccessToken> SaveAccessTokenAsync(string userEmail, AccessToken accessToken);
        
        /// <summary>
        /// Authenticate ACCESS Token
        /// </summary>
        /// <param name="userToken">Token to find.</param>
        /// <returns></returns>
        public Task<Account> AuthenticateUserAsync(string userToken);

        /// <summary>
        /// Remove account from user repository.
        /// </summary>
        /// <param name="userEmail">User Identifier</param>
        /// <returns>None.</returns>
        public Task DropoutUserAsync(string userId);

        /// <summary>
        /// Get Account Information by userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<Account> GetAccountOrDefaultAsync(string userId);
    }
    
    public class AccountRepository: IAccountRepository
    {
        private readonly IMongoCollection<Account> _accountCollection;

        public AccountRepository(MongoContext mongoContext)
        {
            _accountCollection = mongoContext.MongoDatabase.GetCollection<Account>(nameof(Account));
            _accountCollection.Indexes.CreateOne(
                new CreateIndexModel<Account>(
                    new BsonDocument {{"userEmail", 1}},
                    new CreateIndexOptions {Unique = true}));
        }

        public async Task<Account> CreateAccountAsync(RegisterRequest message)
        {
            var account = new Account
            {
                UserEmail = message.UserEmail,
                UserPassword = message.UserPassword,
                UserAccessTokens = new List<AccessToken>()
            };
            await _accountCollection.InsertOneAsync(account);

            return account;
        }

        public async Task<Account> LoginAccountAsync(LoginRequest message)
        {
            var findOptions = Builders<Account>.Filter.And(
                Builders<Account>.Filter.Eq(a => a.UserEmail, message.UserEmail),
                Builders<Account>.Filter.Eq(a => a.UserPassword, message.UserPassword)
            );

            return await (await _accountCollection.FindAsync(findOptions)).FirstOrDefaultAsync();
        }

        public async Task<AccessToken> SaveAccessTokenAsync(string userEmail, AccessToken accessToken)
        {
            var findOption = Builders<Account>.Filter.Eq(a => a.UserEmail, userEmail);
            var updateOption = Builders<Account>.Update.Push(a => a.UserAccessTokens, accessToken);

            await _accountCollection.UpdateOneAsync(findOption, updateOption);

            return accessToken;
        }

        public async Task<Account> AuthenticateUserAsync(string userToken)
        {
            var accountQueryable = _accountCollection.AsQueryable();

            var targetAccount = await accountQueryable.Where(a =>
                    a.UserAccessTokens.Any(a =>
                        a.Token == userToken && a.ExpiresAt >= DateTimeOffset.UtcNow.ToUnixTimeSeconds() &&
                        a.IsExpiredExternally != true))
                .FirstOrDefaultAsync();

            return targetAccount;
        }

        public async Task DropoutUserAsync(string userId)
        {
            var filter = Builders<Account>.Filter.Eq(a => a.Id, userId);

            await _accountCollection.DeleteOneAsync(filter);
        }

        public async Task<Account> GetAccountOrDefaultAsync(string userId)
        {
            var targetAccount = await _accountCollection.AsQueryable()
                .Where(a => a.Id == userId)
                .FirstOrDefaultAsync();

            return targetAccount;
        }
    }
}