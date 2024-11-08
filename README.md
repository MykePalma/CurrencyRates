# Exchange Rates API
Project for a job interview challenge.

To run the App you will have to create a appsettings like:
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AlphaVantage": {
    "ApiKey": "example"
  },
  "ConnectionStrings": {
    "sqlServerConnection": "example"
  },
  "RabbitMQ": {
    "HostName": "example",
    "Port": "5672",
    "QueueName": "CurrencyRatesMessagingQueue",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

You will also have to create a database and a database table You can use the following script to create the table:
```
CREATE TABLE [dbo].[CurrencyRates](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[CurrencyPair] [nvarchar](10) NOT NULL,
	[Bid] [decimal](18, 4) NOT NULL,
	[Ask] [decimal](18, 4) NOT NULL,
	[LastUpdated] [datetime] NOT NULL
)
```
And get and API key for AlphaVantage and run rabbitMQ and set the respective hostname and settings.

# RabbitMQ
You can run a docker image for rabbitMQ using the following cmd line:
```docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management```

ILogger isn't logging to anywhere, but should be considered to be updated in the future to save to a database (SQL server to reuse our service or preferably to a better logging database  like elasticsearch)

The project was build with a test driven development architecture.
