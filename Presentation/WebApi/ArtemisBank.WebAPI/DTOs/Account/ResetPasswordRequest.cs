namespace ArtemisBank.WebAPI.DTOs.Account
{
    public record ResetPasswordRequest( string UserId, string Token, string Password, string ConfirmPassword);
}
