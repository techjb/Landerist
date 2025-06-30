using System.Text.RegularExpressions;

namespace landerist_library.Tools
{
    public partial class Validate
    {
        public static bool Email(string email)
        {
            var trimmedEmail = email.Trim();
            if (trimmedEmail.EndsWith('.'))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        public static bool CadastralReference(string cadastralReference)
        {
            Regex regex = RegexCadastralReference();
            return regex.IsMatch(cadastralReference);
        }

        public static bool Phone(string phone)
        {
            int digitCount = 0;

            foreach (char c in phone)
            {
                if (char.IsDigit(c))
                {
                    digitCount++;
                }

                if (digitCount >= 6)
                {
                    return true;
                }
            }

            return false;
        }

        [GeneratedRegex(@"^[0-9]{7}[A-Z]{2}[0-9]{4}[A-Z]{1}([0-9]{4}[A-Z]{2})?$")]
        private static partial Regex RegexCadastralReference();


        //public static void RemoveInvalidCatastralReferences()
        //{
        //    var listings = ES_Listings.GetListingWithCatastralReference();
        //    int total = listings.Count;
        //    int validCount = 0;
        //    int invalidCount = 0;
        //    foreach (var listing in listings)
        //    {
        //        RemoveInvalidCatastralReference(listing);
        //    }
        //    Console.WriteLine($"Total: {total}, Valid: {validCount}, Invalid: {invalidCount}");
        //}

        //private static void RemoveInvalidCatastralReference(Listing listing)
        //{
        //    if (string.IsNullOrEmpty(listing.cadastralReference))
        //    {
        //        return;
        //    }
        //    var isValid = CadastralReference(listing.cadastralReference);
        //    if (!isValid)
        //    {
        //        string query =
        //            "UPDATE " + ES_Listings.TABLE_ES_LISTINGS + " " +
        //            "SET [CadastralReference] = NULL " +
        //            "WHERE [Guid] = @Guid";
        //        new DataBase().Query(query, new Dictionary<string, object?> {
        //            { "Guid" , listing.guid }
        //        });
        //    }
        //}
    }
}
