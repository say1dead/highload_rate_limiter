namespace UserRequestsKafkaGenerator.Services;

public sealed class WorkItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public int UserId { get; set; }
    public string Endpoint { get; set; }
    public int Rpm { get; set; }

    public WorkItem(int userId, string endpoint, int rpm)
    {
        UserId = userId;
        Endpoint = endpoint;
        Rpm = rpm;
    }

    public override string ToString() =>
        $"{Id} | user_id={UserId}, endpoint={Endpoint}, rpm={Rpm}";
}
