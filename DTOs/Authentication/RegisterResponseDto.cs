namespace EcommerceAPI.DTOs.Authentication;

public class RegisterResponseDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}