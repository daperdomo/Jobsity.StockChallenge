# Jobsity.StockChallenge

A small ASP.NET Core chat app with stock quote support. Messages go through SignalR, stock commands go through RabbitMQ, and chat history is stored in SQL Server.

## Requirements

- Docker Desktop
- Linux containers enabled in Docker Desktop

Check Docker before running the app:

```powershell
docker version
docker compose version
```

`docker version` should include a `Server` section. If it does not, Docker Desktop is not ready yet.

## Run

From the repository root:

```powershell
docker compose up --build
```

Compose starts four containers:

- `web`
- `bot`
- `rabbitmq`
- `sqlserver`

The web app runs the EF migrations on startup, so the database is created automatically.

## URLs

- App: `http://localhost:8080`
- RabbitMQ UI: `http://localhost:15672`
- RabbitMQ login: `guest` / `guest`
- SQL Server: `localhost,1433`
- SQL Server login: `sa` / `Jobsity_StockChallenge_2026!`

## Configuration

The default SQL Server password can be changed before starting Compose:

```powershell
$env:SQL_SERVER_PASSWORD="TuPasswordSeguro_123!"
docker compose up --build
```

The password must satisfy SQL Server password complexity rules.

## Chat usage

1. Open `http://localhost:8080`.
2. Register or log in.
3. Send a message or a stock command:

```text
/stock=aapl.us
```

The bot posts the quote back to the same chat room.

If SQL Server does not start, check that the password satisfies complexity rules and that port `1433` is free.

If RabbitMQ does not start, check that ports `5672` and `15672` are free.
