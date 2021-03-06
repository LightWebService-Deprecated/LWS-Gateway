using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LWS_Gateway.CustomException;
using LWS_Gateway.Model;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace LWS_Gateway.Service
{
    public class UserService
    {
        private readonly ILogger _logger;
        private readonly IAccountRepository _accountRepository;
        private readonly IKubernetesService _kubernetesService;

        public UserService(ILogger<UserService> logger, IAccountRepository repository, IKubernetesService kubernetesService)
        {
            _logger = logger;
            _accountRepository = repository;
            _kubernetesService = kubernetesService;
        }

        public async Task<AccountProjection> GetAccountProjection(string userId)
        {
            var accountInfo = await _accountRepository.GetAccountOrDefaultAsync(userId)
                              ?? throw new ApiServerException(StatusCodes.Status404NotFound,
                                  $"Cannot find User with user id: {userId}");

            return accountInfo.ToProjection();
        }

        [ExcludeFromCodeCoverage]
        public async Task RegisterRequest(RegisterRequest request)
        {
            try
            {
                request.UserPassword = BCrypt.Net.BCrypt.HashPassword(request.UserPassword);
                var user = await _accountRepository.CreateAccountAsync(request);
                await _kubernetesService.CreateNameSpace(user.Id);
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
                    
            if (!CheckPasswordCorrect(request.UserPassword, loginResult.UserPassword))
            {
                throw new ApiServerException(StatusCodes.Status401Unauthorized,
                    "Login failed! Please enter credential again.");
            }

            var accessToken = CreateAccessToken(loginResult);

            await _accountRepository.SaveAccessTokenAsync(loginResult.UserEmail, accessToken);

            return accessToken;
        }

        [ExcludeFromCodeCoverage]
        private bool CheckPasswordCorrect(string plainPassword, string hashedPassword)
        {
            bool correct = false;
            try
            {
                correct = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
            } catch {}
            

            return correct;
        }

        public async Task DropoutUserRequest(string userId)
        {
            await _accountRepository.DropoutUserAsync(userId);
            await _kubernetesService.DeleteNameSpace(userId);
        }

        private AccessToken CreateAccessToken(Account account)
        {
            var previousToken =
                account.UserAccessTokens.FirstOrDefault(a =>
                    a.ExpiresAt > DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            if (previousToken != null)
            {
                return previousToken;
            }
            
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