# Developing

How to run the app and the API locally and check your changes by hand.

## Prerequisites

- [Node.js][node] `^22.22.3` or `^24.15.0` (required by Angular 22)
- [Yarn][yarn]: the repository ships its own Yarn release in `.yarn/releases`, so any `yarn` on your `PATH` will use it
- [.NET 6 SDK][dotnet]
- [Azure Functions Core Tools v4][func]

You don't need Azurite: the API only uses HTTP triggers.

## Install

```bash
yarn install
```

## Run

Use two terminals.

### 1. API (Azure Function)

```bash
cd api
func start
```

The API listens on `http://localhost:7071`, which matches `apiBaseUrl` in `src/environment.json`.

### 2. Front-end (Angular)

From the repository root:

```bash
yarn start
```

Open <http://localhost:4200>.

:rotating_light: keep the default port `4200`. The API only allows the `http://localhost:4200` origin, so on any other port every preflight fails.

## Before pushing

```bash
yarn lint
yarn build-prod
```

[node]: https://nodejs.org/
[yarn]: https://yarnpkg.com/
[dotnet]: https://dotnet.microsoft.com/download/dotnet/6.0
[func]: https://learn.microsoft.com/azure/azure-functions/functions-run-local
