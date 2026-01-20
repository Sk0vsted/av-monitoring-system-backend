# AV Monitoring System – Backend

Backend til **AV Monitoring System**, et cloud-baseret overvågnings- og analyse-system udviklet som bachelorprojekt på Professionsbachelor i Webudvikling (IBA).

Backend-systemet er ansvarligt for overvågning af endpoints, registrering af logs og incidents samt opsamling af analytics, som efterfølgende visualiseres i frontend-dashboardet.

## Projektoversigt

Formålet med backend-løsningen er at:
- Overvåge definerede endpoints via periodiske HTTP-kald
- Registrere responstider, statuskoder og fejl
- Oprette incidents ved fejl og timeouts
- Opsamle overvågningsdata til brug i analyser og dashboards
- Eksponere data via API-endpoints til frontend-applikationen

Systemet er udviklet som en serverless løsning med fokus på skalerbarhed, enkel deployment og tydelig adskillelse af ansvar.

## Teknologier

Projektet er bygget med følgende teknologier:

- **.NET 8** – Backend-platform
- **Azure Functions** – Serverless API og baggrundsprocesser
- **C#** – Applikationslogik
- **Azure Table Storage** – Oprettelse af logs, incidents og analytics
- **Dependency Injection** – Strukturering af services og repositories
- **REST API** – Kommunikation med frontend

## Arkitektur & struktur

Backend-løsningen er opbygget med klar adskillelse mellem ansvar:

- **Functions** – HTTP-triggered endpoints og scheduled time-triggered jobs
- **Services** – Forretningslogik (monitoring, incident-håndtering, analytics)
- **Repositories** – Dataadgang til Azure Table Storage
- **Models / DTOs** – Datamodeller og transportobjekter
- **Options** – Konfiguration til forskellige tables via `StorageOptions`

Arkitekturen understøtter testbarhed, vedligeholdelse og videreudvikling.

## Overvågningsflow

Backend-systemet håndterer bl.a. følgende flow:
1. Endpoints hentes fra eller oprettes på storage
2. HTTP-kald udføres mod hvert endpoint baseret på brugerdefineret tidsinterval
3. Resultater logges (statuskode, responstid, tidspunkt)
4. Fejl og timeouts resulterer i incidents via IncidentEngine
5. Data opsamles til daglige analytics hver nat kl. 02:00 dansk tid
6. Frontend henter data via API-endpoints gennem Azure Functions

## Kom i gang

### Forudsætninger
- .NET 8 SDK
- Visual Studio 2022 (Anbefalet) med Azure Development workload
- Lokale miljøvariabler/konfiguration som kan findes i eksamensmappen

### Lokal Kørsel
Backenden er en Azure Functions (.NET Isolated) løsning og er derfor mest stabil ved at køre lokalt via Visual Studio, da Visual Studio håndterer functions host, debugging og lokale konfigurationer pålideligt i dette setup.

*local.settings.json* er ikke commited til repo'et og skal oprettes lokalt ud fra de angivne miljøvariabler i eksamensmappen

Bemærk: CLI-kørsel kan være mulig, men er **ikke** den primære understøttede lokale workflow for dette projekt!

### Installation
Det anbefales at starte backenden via Visual Studio (Gå til "Opsætning" -> "Opsætning af AV-Monitoring-System-Backend.pdf" i projektmappen). Alternativt kan dependencies hentes via:

```bash
dotnet restore
