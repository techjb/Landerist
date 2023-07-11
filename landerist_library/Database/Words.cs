using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Database
{
    public class Words
    {
        private const string TABLE_WORDS = "[WORDS]";

        public static bool Insert(string word)
        {
            string query =
                "IF EXISTS (SELECT 1 FROM " + TABLE_WORDS + " WHERE Word = @word) " +
                "   UPDATE WORDS" +
                "   SET Counter = Counter + 1" +
                "   WHERE Word = @word; " +
                "ELSE " +
                "   INSERT INTO " + TABLE_WORDS + " VALUES(@word, 1)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"word", word },
            });
        }
    }
}
