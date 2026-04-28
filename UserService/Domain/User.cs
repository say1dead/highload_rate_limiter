namespace UserService.Domain;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Age { get; set; }
}