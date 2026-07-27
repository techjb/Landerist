# Landerist architecture

Landerist is being migrated incrementally from a single legacy library to a layered modular monolith. New code follows this dependency direction:

```text
Domain model (Pages, Websites) <- Application <- Infrastructure <- Console / hosts
```

## Physical projects

`landerist_domain` physically owns the `Pages` and `Websites` domain areas.
It depends only on the .NET base class library and `landerist_orels`. Parsing, downloaders, persistence, logging and other integration concerns remain outside the domain project.

`landerist_application` physically owns use cases, policies and ports. It depends only on `landerist_domain`, `landerist_orels` and the .NET base class library.

`landerist_infrastructure` is the incremental destination for adapters. It currently owns HTTP transport, browser maintenance and the decoupled website network/refresh/robots adapters, and depends inward on `landerist_application` and `landerist_domain`; remaining adapters stay in `landerist_library/Infrastructure` until their legacy dependencies are removed.
## Boundaries

`landerist_application/Application` contains use cases, policies and ports. It may depend only on `Application`, `Pages`, `Websites` and the .NET base class library. It must not depend on configuration, SQL, browser implementations, external providers or logging implementations.

`landerist_library/Infrastructure` implements Application ports. SQL Server, browser automation, cloud SDKs and legacy adapters belong on this side of the boundary.

Code outside `Application` and `Infrastructure` must not introduce new references to either boundary. Existing reverse references are migration debt recorded in `landerist_architecture_tests/ArchitectureBaseline.txt`.

## Enforcement

```powershell
dotnet test .\landerist_architecture_tests\landerist_architecture_tests.csproj
```

The tests verify that Application does not acquire outer-layer dependencies, folder namespaces match, `Pages` and `Websites` do not reach into `Infrastructure`, `Database` or `LegacyDatabase`, the legacy dependency baseline cannot grow and resolved dependencies are removed from that baseline.

The baseline is a ratchet, not an allow-list for new work. When a dependency is removed from source, remove its line from the baseline in the same change.

## Migration sequence

1. Put new orchestration in Application behind explicit ports.
2. Implement ports in Infrastructure.
3. Change a legacy facade to call the Application use case.
4. Move callers away from the facade.
5. Remove the reverse dependency and its baseline entry.
6. Extract physical projects after the dependency graph is acyclic.
