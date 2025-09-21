# Hacker News Middle-End (Angular + .NET)

### Monorepo with:

server/ – ASP.NET Core (.NET 9) API, mounted under /api

client/ – Angular app (Material) consuming the API

### First Time Setup

1) Install root dev tools for the monorepo
```bash
npm i -g concurrently
```
2) Install Angular deps
```bash
cd client
npm i
cd ..
```
3) Restore .NET deps
```bash
dotnet restore server/Hn.Api
dotnet restore server/Hn.Tests
```

4) `package.json` at root level
```json
{
  "name": "hn-app",
  "private": true,
  "devDependencies": { "concurrently": "^9.0.0" },
  "scripts": {
    "dev": "concurrently \"dotnet watch run --project server/Hn.Api\" \"npm --prefix client run start:proxy\""
  }
}
```

### Run everything
```bash
npm run dev
```

### Run Separately
1) Server (From Root Level)
```bash
dotnet run --launch-profile Hn.Api --project server/Hn.Api
```

2) Client (Angular)
```bash
cd client
npm run start:proxy
```
If you do not have "start:proxy" script then
```bash
ng serve --proxy-config proxy.conf.json
```

### Note For Proxy File
#### Place on client level folder `proxy.conf.json`
```json
{
    "/api": {
        "target": "http://localhost:5181",
        "secure": false,
        "changeOrigin": true,
        "logLevel": "debug"
    }
}
```