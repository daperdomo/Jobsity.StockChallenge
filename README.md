# Jobsity.StockChallenge

Chat application for stock quote lookups. The web app publishes stock commands to RabbitMQ, the bot consumes those commands, fetches the quote, and publishes the response back to the chat.

## Requirements

- Docker Desktop
- Docker Compose included with Docker Desktop
- Linux containers enabled in Docker Desktop

To verify Docker is ready:

```powershell
docker version
docker compose version
```

`docker version` should show both `Client` and `Server` information.

## Run With Docker

From the repository root:

```powershell
docker compose up --build
```

This starts:

- `web`: ASP.NET Core site at `http://localhost:8080`
- `bot`: worker that processes stock commands
- `rabbitmq`: message broker
- `sqlserver`: SQL Server database

The database is created automatically when the site starts by running the Entity Framework migrations.

## URLs

- Site: `http://localhost:8080`
- RabbitMQ Management: `http://localhost:15672`
- RabbitMQ username: `guest`
- RabbitMQ password: `guest`
- SQL Server: `localhost,1433`
- SQL Server username: `sa`
- Default SQL Server password: `Jobsity_StockChallenge_2026!`

## Configuration

The [docker-compose.yml](docker-compose.yml) file configures all services using environment variables.

To change the SQL Server password:

```powershell
$env:SQL_SERVER_PASSWORD="TuPasswordSeguro_123!"
docker compose up --build
```

## Chat Usage

1. Open `http://localhost:8080`.
2. Register or log in.
3. Send a regular message or a stock command:

```text
/stock=aapl.us
```

The bot replies in the chat with the fetched quote.

If SQL Server does not start, check that the password satisfies complexity rules and that port `1433` is not already in use.

If RabbitMQ does not start, check that ports `5672` and `15672` are not already in use.
