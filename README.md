# Chirp (CLI)

## Prereqs
- .NET 8 SDK
- (Optional) Azure CLI if you deploy

## Run the database service (local)
```bash
cd ~/Documents/GitHub/Chirp
dotnet run --project src/Chirp.CSVDB.Service/Chirp.CSVDB.Service.csproj
```
It prints the URL, e.g. `http://localhost:5263`.

## Use the CLI (local service)
```bash
export CHIRP_DB_URL="http://localhost:5263"
dotnet run --project src/Chirp.CLI.Client/Chirp.CLI.Client.csproj -- cheep "hello"
dotnet run --project src/Chirp.CLI.Client/Chirp.CLI.Client.csproj -- read 10
```

## Use the CLI (Azure service)
```bash
export CHIRP_DB_URL="https://bdsagroup23chirpremotedb-ewbqerfhfrekapa6.swedencentral-01.azurewebsites.net"
dotnet run --project src/Chirp.CLI.Client/Chirp.CLI.Client.csproj -- cheep "hello from azure"
dotnet run --project src/Chirp.CLI.Client/Chirp.CLI.Client.csproj -- read 10
```

## Service endpoints
- `POST /cheep` (JSON body: `{ "Author": "...", "Message": "...", "Timestamp": 1684229348 }`)
- `GET /cheeps` (returns `[]` or a list of cheeps)

Quick test:
```bash
BASE="$CHIRP_DB_URL"
curl -i "$BASE/cheeps"
curl -i -X POST "$BASE/cheep" -H "Content-Type: application/json" \
  -d '{"Author":"me","Message":"hi","Timestamp":1684229348}'
```
