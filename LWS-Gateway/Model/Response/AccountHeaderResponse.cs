namespace LWS_Gateway.Model.Response;

public class AccountHeaderResponse
{
    public string Id { get; set; }
    public string NickName { get; set; }
    public char FirstLetter { get; set; }
    public string Email { get; set; }
    public AccountRole Role { get; set; }
}