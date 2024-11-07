namespace Services;

using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;

public class RabbitMQService : IAsyncDisposable, IRabbitMQService
{
    private IConnection _connection;
    private IChannel _channel;
    private readonly string _queueName;
    private readonly ConnectionFactory _factory;

    public RabbitMQService(IConfiguration configuration)
    {
        _factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"],
            Port = Convert.ToInt32(configuration["RabbitMQ:Port"]),
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"]
        };

        _queueName = configuration["RabbitMQ:QueueName"];
    }

    public async Task InitializeAsync()
    {
        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        await _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public async Task SendMessageAsync(string message)
    {
        await InitializeAsync();
        var body = Encoding.UTF8.GetBytes(message);

        await _channel.BasicPublishAsync(exchange: "", routingKey: _queueName, mandatory: false, body: body);
        Console.WriteLine($"[x] Sent {message}");
        DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();
        if (_connection != null)
            await _connection.CloseAsync();
    }
}
