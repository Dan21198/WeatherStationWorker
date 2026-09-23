# WeatherStationWorker

Background worker that periodically downloads weather-station XML, converts it to JSON, and stores each run in PostgreSQL.

## What it does

- Polls a configured weather-station URL every hour
- Parses the XML response and serializes it to JSON
- Stores download results in PostgreSQL
- Records failures when the station is unavailable or returns invalid data

## Tech stack

- .NET 10 Worker Service
- Entity Framework Core
- PostgreSQL
- Newtonsoft.Json
- HttpClient resilience handlers

## Project structure

- `WeatherStationWorker/` - worker application
- `WeatherStationWorker.Tests/` - unit tests
- `compose.yaml` - PostgreSQL + worker stack
- `docker-compose.yml` - minimal worker image definition

## Configuration

The app gets runtime values from Docker Compose via `.env`.

### `.env`

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=Password123!
POSTGRES_DB=WeatherStationDb
WEATHER_STATION_URL=https://pastebin.com/raw/PMQueqDV
```

### Compose variables

- `POSTGRES_USER` - PostgreSQL user
- `POSTGRES_PASSWORD` - PostgreSQL password
- `POSTGRES_DB` - PostgreSQL database name
- `WEATHER_STATION_URL` - XML endpoint to poll

## Run with Docker

```bash
docker compose -f compose.yaml up --build
```

## Tests

```bash
dotnet test
```

## Data model

Each download creates a `WeatherDataRecord` with:

- `DownloadedAt`
- `IsOnline`
- `JsonData` when the XML request succeeds
- `ErrorMessage` when the request or parsing fails

