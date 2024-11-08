
namespace Services;

public interface IRabbitMQService
{
    Task InitializeAsync();
    Task SendMessageAsync(string message);
}