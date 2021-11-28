using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LWS_Gateway.CustomException;
using LWS_Gateway.Model;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace LWS_Gateway.Service
{
    public class UserService
    {
        private readonly ILogger _logger;
        private readonly IAccountRepository _accountRepository;

        public UserService(ILogger<AuthenticationService> logger, IAccountRepository repository)
        {
            _logger = logger;
            _accountRepository = repository;
        }

        public async Task RegisterRequest(RegisterRequest request)
        {
            try
            {
                await _accountRepository.CreateAccountAsync(request);
            }
            catch (Exception e)
            {
                HandleRegisterError(e, request);
            }
        }

        public async Task<AccessToken> LoginRequest(LoginRequest request)
        {
            var loginResult = await _accountRepository.LoginAccountAsync(request)
                ?? throw new ApiServerException(StatusCodes.Status401Unauthorized,
                    "Login failed! Please enter credential again.");

            var accessToken = CreateAccessToken(loginResult);

            await _accountRepository.SaveAccessTokenAsync(loginResult.UserEmail, accessToken);

            return accessToken;
        }

        public async Task<Account> AuthenticateUserRequest(AuthenticationRequest request)
        {
            return await _accountRepository.AuthenticateUserAsync(request.UserToken);
        }

        public async Task DropoutUserRequest(string userEmail)
        {
            await _accountRepository.DropoutUserAsync(userEmail);
        }

        private AccessToken CreateAccessToken(Account account)
        {
            using var shaManaged = new SHA512Managed();
            var targetString = $"{DateTime.Now.Ticks}/{account.UserEmail}/{Guid.NewGuid().ToString()}";
            var targetByte = Encoding.UTF8.GetBytes(targetString);
            var result = shaManaged.ComputeHash(targetByte);

            return new AccessToken
            {
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(20).ToUnixTimeSeconds(),
                Token = BitConverter.ToString(result).Replace("-", string.Empty)
            };
        }
        
        /// <summary>
        /// Handle Mongo - Write Exception(Global Handler)
        /// </summary>
        /// <param name="superException">Master Exception[Supertype Exception]</param>
        /// <param name="toRegister">User entity tried to register.</param>
        /// <returns>Result Object.</returns>
        private void HandleRegisterError(Exception superException, RegisterRequest toRegister)
        {
            _logger.LogCritical($"Exception Occurred! Message: {superException.Message}");
            // When Error type is MongoWriteException
            if (superException is MongoWriteException mongoWriteException)
            {
                // When Error Type is 'Duplicate Key'
                if (mongoWriteException.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    throw new ApiServerException(StatusCodes.Status409Conflict,
                        $"Cannot register user {toRegister.UserEmail}! User already exists.");
                }
            }

            throw new ApiServerException(StatusCodes.Status500InternalServerError,
                $"Unknown Error Occurred! {superException.Message}", superException);
        }
    }
}