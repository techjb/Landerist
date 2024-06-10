using landerist_library.Configuration;
using landerist_library.Tools;
using System.Data;

namespace landerist_library.Insert.BancoDeDatos
{
    public class InsertBancoDeDatos() : WebsitesInserter(true)
    {
        public static void Start()
        {
            string file = PrivateConfig.INSERT_DIRECTORY + @"bancodedatos.es\Excel\Pedido_completo.csv";
            DataTable dataTable = Csv.ToDataTable(file);
            var uris = ToList(dataTable, "SITIO WEB");
            Insert(uris);
        }
    }
}
