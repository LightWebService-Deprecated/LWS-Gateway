namespace LWS_Gateway.Model
{
    public class AccessToken
    {
        public long CreatedAt { get; set; }
        public long ExpiresAt { get; set; }
        public string Token { get; set; }
    }
}