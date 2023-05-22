using System.Data;

namespace landerist_library.Database
{
    public class CNIG : DBDelimitations
    {
        private const string TABLE_CNIG = "[CNIG]";

        public static bool DeleteAll()
        {
            return DeleteAll(TABLE_CNIG);
        }

        public static bool Insert(string the_geom, string inspireId, string natCode, string nameUnit)
        {
            string geom = "geography::STGeomFromWKB(0x" + the_geom + ", 4326)";
            string query =
                "INSERT INTO " + TABLE_CNIG + " VALUES(" + geom + ",@inspireId, @natCode, @nameUnit)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"inspireId", inspireId },
                {"natCode", natCode },
                {"nameUnit", nameUnit },
            });
        }

        public static bool MakeValidAll()
        {
            return MakeValidTheGeom(TABLE_CNIG);
        }

        public static bool ReorientIfNeccesary()
        {
            return ReorientTheGeomIfNeccesary(TABLE_CNIG);
        }

        public static DataRow? Get(double latitude, double longitude)
        {
            return GetdDataRow(TABLE_CNIG, "natcode, nameunit", latitude, longitude);            
        }
    }
}
