using System.Threading.Tasks;
using LWS_Gateway.Model;
using LWS_Gateway.Model.Request;
using LWS_Gateway.Repository;

namespace LWS_Gateway.Service;

public class AuthenticationService
{
    private readonly IAccountRepository _accountRepository;

    public AuthenticationService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }
    
    public async Task<Account> AuthenticateUserRequest(AuthenticationRequest request)
    {
        return await _accountRepository.AuthenticateUserAsync(request.UserToken);
    }
}