using landerist_library.Infrastructure.Sql;

namespace landerist_library.Pages;

public partial class Pages
{
    private static readonly PageMaintenanceRepository MaintenanceRepository =
        new(global::landerist_library.Database.LegacyDatabase.Create());
}