# OpenIMSFrontend

OpenIMSFrontend is the web-frontend of OpenIMS.

See the [reference](./Other/Reference/ReferenceContent/index.md) and the [hints](./Other/Reference/ReferenceContent/Hints.md) for details.

## Development server

Run `task RunFrontend` (or shorter: `task rf`) in the repository-folder, or `npm run start` here. Navigate to `http://localhost:4200/`.

To reach a backend without running into CORS-issues, start the local reverse-proxy as well (`task lru`) and use the address it serves.

## Build

Run `npm run build` to build the project. The build-artifacts are stored in `Other/Artifacts/BuildResult_WebApplication`.

## Running unit tests

Run `npm run test` to execute the unit-tests via [Vitest](https://vitest.dev).

## Running the visual-regression-tests

Run `python RunTestcases.py` in `Other/QualityCheck`. The tests always run in a container, see the hints.
