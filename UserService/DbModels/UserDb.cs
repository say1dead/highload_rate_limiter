namespace UserService.DbModels;

public class UserDb
{
    public int id { get; set; }
    public string login { get; set; } = null!;
    public string password { get; set; } = null!;
    public string name { get; set; }
    public string surname { get; set; }
    public int age { get; set; }
}