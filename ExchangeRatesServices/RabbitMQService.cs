using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace Services;

public class RabbitMQService : IRabbitMQService
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

        InitializeAsync().Wait();
    }

    public async Task InitializeAsync()
    {
        _connection = await _factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        await _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }

    public async Task SendMessageAsync(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        await _channel.BasicPublishAsync(exchange: "", routingKey: _queueName, mandatory: false, body: body);
        Console.WriteLine($"[x] Sent {message}");
    }
}
