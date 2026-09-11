# StoreFrontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 21.2.7.

## Project structure

The app is layered: **core** (cross-cutting technical concerns), **shared** (reusable,
feature-agnostic UI) and **modules** (one vertical slice per business capability).
A page never talks to `HttpClient` directly — it reads a store, the store calls an
API service, and the API service owns the URL.

```
src/
├── environments/                  # apiBaseUrl per build configuration
├── styles/common/                 # global SCSS partials
└── app/
    ├── app.config.ts app.routes.ts    # bootstrap and composition
    ├── core/                          # interceptors, layout, models, services, utils
    ├── shared/components/             # modal, product card
    └── modules/{auth,home,products}/  # api, data-access (stores), models,
                                       #   pages, components, utils
```

Path aliases: `@app/*` → `src/app/*`, `@env/*` → `src/environments/*`.
Feature stores are provided by the page that uses them, so their state dies with
the page; only genuinely shared services are `providedIn: 'root'`.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Docker

### Build the image

```bash
docker build -t store-frontend .
```

### Run the container

```bash
docker run -p 8080:80 store-frontend
```

Then open your browser and navigate to `http://localhost:8080/`.

### Build and run with a custom port

```bash
docker run -p 3000:80 store-frontend
```

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
