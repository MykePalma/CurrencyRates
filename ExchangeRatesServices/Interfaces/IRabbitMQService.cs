
namespace Services
{
    public interface IRabbitMQService
    {
        ValueTask DisposeAsync();
        Task InitializeAsync();
        Task SendMessageAsync(string message);
    }
}