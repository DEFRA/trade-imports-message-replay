# Trade Imports Message Replay

The Trade Imports Message Replay is a .NET application which replays data from DMP Blob storage and feeds it into BTMS.


The Message replay is an on demand service, and uses Hangfire to run jobs.  Hangfire is using in memory storage, this was to avoid the amount of Mongo calls when persisted storage was used.

The Message Replay will by pass the Gateway and go direct to the BTMS service.  The Message Replay handles the following message types:

|Msssage Type|Service |
|----------------|-------------------------------|
|ImportPreNotification|`Imports Processor`            |
|ClearnanceRequest          |`Imports Processor`            |
|Finalisation          |`Imports Processor`|
|ALVS Decision          |`Decision Comparer`|


* [Prerequisites](#prerequisites) 
* [Setup Process](#setup-process)
* [How to run in development](#how-to-run-in-development)
* [How to run Tests](#how-to-run-tests)
* [Running](#running)
* [Deploying](#deploying)
* [SonarCloud](#sonarCloud)
* [Dependabot](#dependabot)
* [Licence Information](#licence-information)
* [About the Licence](#about-the-licence)

### Prerequisites

- .NET 9 SDK
- Docker
  - wiremock - used for mocking out http requests 
  

### Setup Process

- Install the .NET 9 SDK
- Install Docker
  - Run the following Docker Compose to set up locally running queues for testing
  ```bash
  docker compose -f compose.yml up -d
  ```

### How to run in development

Run the application with the command:

```bash
dotnet run --project src/MessageReplay/MessageReplay.csproj
```

### How to run Tests

Run the unit tests with:

```bash
dotnet test --filter "Category!=IntegrationTest"
```
Run the integration tests with:
```bash
dotnet test --filter "Category=IntegrationTest"
```
Run all tests with:
```bash
dotnet test 
```

#### Unit Tests
Some unit tests may run an in memory instance service. 

Unit tests that need to edit the value of an application setting can be done via the`appsettings.IntegrationTests.json`

#### Integration Tests
Integration tests run against the built docker image.

Because these use the built docker image, the `appsettings.json` will be used, should any values need to be overridden, then they can be injected as an environment variable via the`compose.yml`

### Deploying

Before deploying via CDP set the correct config for the environment as per the `appsettings.Development.json`.

### SonarCloud

Example SonarCloud configuration are available in the GitHub Action workflows.

### Dependabot

We are using dependabot

### Licence Information

THIS INFORMATION IS LICENSED UNDER THE CONDITIONS OF THE OPEN GOVERNMENT LICENCE found at:

<http://www.nationalarchives.gov.uk/doc/open-government-licence/version/3>

### About the licence

The Open Government Licence (OGL) was developed by the Controller of Her Majesty's Stationery Office (HMSO) to enable information providers in the public sector to license the use and re-use of their information under a common open licence.

It is designed to encourage use and re-use of information freely and flexibly, with only a few conditions.


