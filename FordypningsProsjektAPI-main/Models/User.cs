namespace Emne9_Fordypningsprosjekt_API.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public int HighScore { get; set; }
}