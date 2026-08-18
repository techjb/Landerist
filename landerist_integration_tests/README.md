# Integration tests

The integration suite requires an isolated SQL Server instance. Configuration is
provided only through these environment variables:

- `LANDERIST_TEST_SQL_DATASOURCE`
- `LANDERIST_TEST_SQL_USER`
- `LANDERIST_TEST_SQL_PASSWORD`
- `LANDERIST_TEST_SQL_DATABASE`
- `LANDERIST_TEST_SQL_ENCRYPT` (optional; defaults to `false`)
- `LANDERIST_TEST_SQL_TRUST_SERVER_CERTIFICATE` (optional; defaults to `true`)

The tests use temporary SQL tables and do not modify permanent schema. CI starts a
dedicated SQL Server 2022 container and supplies these variables automatically.
