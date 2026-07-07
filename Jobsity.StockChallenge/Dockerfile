FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Jobsity.StockChallenge/Jobsity.StockChallenge.csproj", "Jobsity.StockChallenge/"]
COPY ["Jobsity.StockChallenge.Bot/Jobsity.StockChallenge.Bot.csproj", "Jobsity.StockChallenge.Bot/"]
COPY ["Jobsity.StockChallenge.Application/Jobsity.StockChallenge.Application.csproj", "Jobsity.StockChallenge.Application/"]
COPY ["Jobsity.StockChallenge.Domain/Jobsity.StockChallenge.Domain.csproj", "Jobsity.StockChallenge.Domain/"]
COPY ["Jobsity.StockChallenge.Infrastructure/Jobsity.StockChallenge.Infrastructure.csproj", "Jobsity.StockChallenge.Infrastructure/"]

RUN dotnet restore "Jobsity.StockChallenge/Jobsity.StockChallenge.csproj"
RUN dotnet restore "Jobsity.StockChallenge.Bot/Jobsity.StockChallenge.Bot.csproj"

COPY . .

FROM build AS publish-web
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Jobsity.StockChallenge/Jobsity.StockChallenge.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish/web \
    /p:UseAppHost=false

FROM build AS publish-bot
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Jobsity.StockChallenge.Bot/Jobsity.StockChallenge.Bot.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish/bot \
    /p:UseAppHost=false

FROM runtime AS web
WORKDIR /app
COPY --from=publish-web /app/publish/web .
ENTRYPOINT ["dotnet", "Jobsity.StockChallenge.dll"]

FROM runtime AS bot
WORKDIR /app
COPY --from=publish-bot /app/publish/bot .
ENTRYPOINT ["dotnet", "Jobsity.StockChallenge.Bot.dll"]
