# ionic-feedback-backend

![Build status](https://markus.visualstudio.com/_apis/public/build/definitions/3f10c4e1-78c9-45de-ab42-209d7b1c10eb/17/badge)

Backend for ionic-feedback-sample:

tbd

## Usage

in solution directory:

```bash
dotnet restore
dotnet run --project Markus.Feedback.Backend/Markus.Feedback.Backend.csproj --contentRoot Markus.Feedback.Backend/
dotnet run --project Markus.Feedback.Backend.ConsoleTest/Markus.Feedback.Backend.ConsoleTest.csproj
```

in project directory:

```bash
dotnet restore
dotnet run
dotnet watch run
```

## Open topics

- add app info, device info and log messages as attachment
- add screenshot as attachment
- encrypt mail credentials
- publish to azure
- configure origins per app

## Change history

### 1.0.0
