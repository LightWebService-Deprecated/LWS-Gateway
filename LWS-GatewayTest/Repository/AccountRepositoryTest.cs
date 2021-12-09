using System;
using System.Collections.Generic;
using LWS_Gateway.Configuration;
using LWS_Gateway.Model;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Repository;
using LWS_GatewayTest.Docker;
using MongoDB.Driver;
using Xunit;

namespace LWS_GatewayTest.Repository;

[Collection("DockerIntegration")]
public class AccountRepositoryTest
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMongoCollection<Account> _accountCollection;

    public AccountRepositoryTest()
    {
        var mongoContext = new MongoContext(new MongoConfiguration
        {
            MongoConnection = "mongodb://root:testPassword@localhost:27017",
            MongoDbName = $"{Guid.NewGuid().ToString()}"
        });
        
        _accountCollection = mongoContext.MongoDatabase.GetCollection<Account>(nameof(Account));
        _accountRepository = new AccountRepository(mongoContext);
    }

    [Fact(DisplayName = "CreateAccountAsync: CreateAccountAsync should insert document well.")]
    public async void Is_CreateAccountAsync_Inserts_Document_Well()
    {
        // Let
        var registerRequest = new RegisterRequest
        {
            UserEmail = "test",
            UserPassword = "test"
        };
        
        // Do
        await _accountRepository.CreateAccountAsync(registerRequest);
        
        // Check
        var list = await (await _accountCollection.FindAsync(Builders<Account>.Filter.Empty))
            .ToListAsync();
        Assert.Single(list);
        Assert.Equal(registerRequest.UserEmail, list[0].UserEmail);
        Assert.Equal(registerRequest.UserPassword, list[0].UserPassword);
    }

    [Fact(DisplayName =
        "CreateAccountAsync: CreateAccountAsync should throw MongoException when duplicated key found.")]
    public async void Is_CreateAccountAsync_Throws_MongoException_When_DuplicatedKey_Found()
    {
        // Let
        var registerRequest = new RegisterRequest
        {
            UserEmail = "test",
            UserPassword = "test"
        };
        
        // Do
        await _accountRepository.CreateAccountAsync(registerRequest);
        await Assert.ThrowsAnyAsync<MongoWriteException>(() => _accountRepository.CreateAccountAsync(registerRequest));
    }

    [Fact(DisplayName = "LoginAccountAsync: LoginAccountAsync should return null when credential is not found.")]
    public async void Is_LoginAccountAsync_Returns_Null_When_Credential_Is_Not_Found()
    {
        // Let
        var loginRequest = new LoginRequest
        {
            UserEmail = "test",
            UserPassword = "test"
        };
        
        // Do
        var result = await _accountRepository.LoginAccountAsync(loginRequest);
        
        // Check
        Assert.Null(result);
    }

    [Fact(DisplayName =
        "LoginAccountAsync: LoginAccountAsync should return account information when credential is correct.")]
    public async void Is_LoginAccountAsync_Returns_Account_Information_When_Credential_Correct()
    {
        // Let
        var account = new Account()
        {
            UserEmail = "test",
            UserPassword = "test"
        };
        var loginRequest = new LoginRequest
        {
            UserEmail = account.UserEmail,
            UserPassword = account.UserPassword
        };
        await _accountCollection.InsertOneAsync(account);
        
        // Do
        var result = await _accountRepository.LoginAccountAsync(loginRequest);
        
        // Check
        Assert.NotNull(result);
        Assert.Equal(account.UserEmail, result.UserEmail);
    }

    [Fact(DisplayName = "SaveAccessTokenAsync: SaveAccessTokenAsync should add access token to database well.")]
    public async void Is_SaveAccessTokenAsync_Saves_Token_To_Database_Well()
    {
        // Let
        var account = new Account()
        {
            UserEmail = "test",
            UserPassword = "test",
            UserAccessTokens = new List<AccessToken>()
        };
        var accessToken = new AccessToken
        {
            CreatedAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.Now.AddDays(2).ToUnixTimeSeconds(),
            Token = "test"
        };
        await _accountCollection.InsertOneAsync(account);
        
        // Do
        var result = await _accountRepository.SaveAccessTokenAsync(account.UserEmail, accessToken);
        
        // Check
        Assert.NotNull(result);
        
        var userList = await (await _accountCollection.FindAsync(Builders<Account>.Filter.Empty))
            .ToListAsync();
        Assert.Single(userList);
        Assert.Single(userList[0].UserAccessTokens);
        Assert.Equal(accessToken.CreatedAt, userList[0].UserAccessTokens[0].CreatedAt);
        Assert.Equal(accessToken.ExpiresAt, userList[0].UserAccessTokens[0].ExpiresAt);
        Assert.Equal(accessToken.Token, userList[0].UserAccessTokens[0].Token);
    }

    [Fact(DisplayName =
        "AuthenticateUserAsync: AuthenticateUserAsync should return null when corresponding user is not found.")]
    public async void Is_AuthenticateUserAsync_Returns_Null_When_Not_Found()
    {
        // Let
        // Do
        var result = await _accountRepository.AuthenticateUserAsync("test");
        
        // Check
        Assert.Null(result);
    }

    [Fact(DisplayName =
        "AuthenticateUserAsync: AuthenticateUserAsync should return account information when token is actually found.")]
    public async void Is_AuthenticateUserAsync_Works_Well()
    {
        // Let
        var account = new Account()
        {
            UserEmail = "test",
            UserPassword = "test",
            UserAccessTokens = new List<AccessToken>
            {
                new()
                {
                    CreatedAt = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    ExpiresAt = DateTimeOffset.Now.AddDays(2).ToUnixTimeSeconds(),
                    Token = "test"
                }
            }
        };
        await _accountCollection.InsertOneAsync(account);
        
        // Do
        var result = await _accountRepository.AuthenticateUserAsync(account.UserAccessTokens[0].Token);
        
        // Check
        Assert.NotNull(result);
        Assert.Equal(account.UserEmail, result.UserEmail);
    }

    [Fact(DisplayName = "DropoutUserAsync: DropoutUserAsync should remove user well.")]
    public async void Is_DropoutUserAsync_Removes_User_Well()
    {
        // Let
        var account = new Account
        {
            UserEmail = "test",
            UserPassword = "testPassword@"
        };
        await _accountCollection.InsertOneAsync(account);
        
        // Do
        await _accountRepository.DropoutUserAsync(account.Id);
        
        // Check
        var dbList = await (await _accountCollection.FindAsync(Builders<Account>.Filter.Empty))
            .ToListAsync();
        Assert.Empty(dbList);
    }
    
}