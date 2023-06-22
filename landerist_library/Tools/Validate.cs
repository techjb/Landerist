using System.Text.RegularExpressions;

namespace landerist_library.Tools
{
    public class Validate
    {
        public static bool Email(string email)
        {
            var trimmedEmail = email.Trim();
            if (trimmedEmail.EndsWith("."))
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
            Regex rgx = new(@"^[a-zA-Z0-9]{14}(?:[a-zA-Z0-9]{6})?$");
            return rgx.IsMatch(cadastralReference);
        }
    }
}
