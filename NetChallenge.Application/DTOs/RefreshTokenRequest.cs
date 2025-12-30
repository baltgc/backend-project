using System.ComponentModel.DataAnnotations;

namespace NetChallenge.Application.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}


