# Landerist architecture

Landerist is being migrated incrementally from a single legacy library to a layered modular monolith. New code follows this dependency direction:

```text
Domain model (Pages, Websites) <- Application <- Infrastructure <- Console / hosts
```

## Physical projects

`landerist_domain` physically owns the `Pages` and `Websites` domain areas.
It depends only on the .NET base class library and `landerist_orels`. Parsing, downloaders, persistence, logging and other integration concerns remain outside the domain project.

`landerist_application` physically owns use cases, policies and ports. It depends only on `landerist_domain`, `landerist_orels` and the .NET base class library.

`landerist_infrastructure` is the incremental destination for adapters. It currently owns HTTP transport, browser maintenance, decoupled website adapters, the SQL connection abstraction, Page/Website/Listing persistence, page services, statistics repositories, and the complete scraping adapter set, including persistence, policy, classification, page indexing, downloading, and browser lifecycle, plus sitemap fetching and indexing. The superseded HTML indexer hierarchy has been removed; page indexing now flows through application ports and focused HTML navigation and content-inspection adapters owned by Infrastructure. Listing-input preparation remains behind its own Application port while the legacy parser is migrated; batch JSONL writing also consumes that port instead of parser extensions. Generic batch contracts, provider selection, and JSONL writing are owned by Infrastructure through `BatchProvider`; legacy `LLMProvider` is mapped only at migration boundaries. Batch upload orchestration and options are also owned by Infrastructure, with SQL registration hidden behind `IBatchRegistrationStore`. `SqlBatchRegistrationStore` persists `BatchProvider` directly, so batch creation no longer converts back to the legacy parser enum. Batch cleanup also uses `IBatchStore`, explicit filesystem options, and an artifact-cleaner port instead of configuration globals or Vertex AI statics. Batch download orchestration likewise uses `IBatchStore`, a provider catalog, explicit parallelism, application logging, and a response-parser port; legacy AI clients are wired only at composition boundaries. The former `BatchRepository` and `Database.Batch` model have been removed; SQL batch access is split between registration and store adapters using `BatchProvider` and `BatchRecord`. Decoupled recurring jobs and the system scheduler are also owned by Infrastructure; daily maintenance is exposed through `IAddressDataMaintenance`, allowing the daily job to live in Infrastructure as well; the local-AI job and parsing task are owned by Infrastructure. Parser execution, token budgeting, and listing-input preparation cross explicit ports, leaving only migration adapters in the compatibility project. Those adapters are grouped by capability under Administration or Parsing; the legacy `Infrastructure/Tasks` folder no longer exists. It depends inward on `landerist_application` and `landerist_domain`; remaining adapters stay in `landerist_library/Infrastructure` until their legacy dependencies are removed.
## Composition and runtime configuration

`landerist_console/Program.cs` is only the process bootstrapper. It creates the Generic Host, calls `AddLanderist`, registers `LanderistWorker`, and runs the host. It must not construct application or infrastructure services directly.

`AddLanderist` is the public composition entry point. It validates `LanderistRuntimeOptions` and delegates registration in dependency order:

```text
Runtime options
    -> Persistence
        -> Scraping
            -> Tasks
```

Each area has a small coordinator that delegates to cohesive registration modules:

- Persistence: database and legacy bootstrap, repositories, then persistence adapters and application-facing services.
- Scraping: HTTP/browser infrastructure, website acquisition, then listing lifecycle and scraping pipeline.
- Tasks: parsing, scrape/batch jobs, local-AI jobs, then recurring jobs.

Large object graphs are split by responsibility instead of being assembled in `Program` or in a single service-registration file:

- `LanderistListingParserProviderComposition` configures OpenAI, Vertex and LocalAI parser clients; `LanderistAiComposition` assembles parser orchestration and materialization.
- `LanderistBatchProviderComposition` configures remote batch providers; `LanderistBatchComposition` assembles batch jobs.
- `LanderistPageScrapingComposition` assembles per-page acquisition, classification and indexing; `LanderistScrapeExecutionComposition` assembles page selection, throttling, locks and batch execution; `LanderistScrapingPipelineFactory` joins those graphs.
- `LanderistDistributionComposition` depends on the explicit `IListingAdministrationService` application port and must not use `IServiceProvider` as a service locator.

`LanderistDatabaseAdapterFactory` is the deliberate compatibility boundary for database-backed adapters that still require multiple independent executors. Legacy adapters may be constructed at this boundary, but they must not leak into `Program` or the small area coordinators.

Database, proxy, browser, Chrome maintenance, integration, AI, batch and execution-role settings cross the composition boundary as validated typed options under `LanderistRuntimeOptions`. `LanderistRuntimeOptionsAdapter` is the sole compatibility edge that translates legacy static configuration. New composition code must consume typed runtime options instead of reading `Config` or `LanderistSettings` directly.

The composition rules are enforced by architecture tests: bootstrap and area coordinators remain small, specialized graphs remain in their owning modules, repository creation goes through `IDatabaseFactory`, and service-locator dependencies are rejected.
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
