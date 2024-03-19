using System.Text.RegularExpressions;

namespace landerist_library.Tools
{
    public partial class Validate
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
            Regex rgx = RegexCadastralReference();
            return rgx.IsMatch(cadastralReference);
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

        [GeneratedRegex(@"^([a-zA-Z0-9]{14}|[a-zA-Z0-9]{20})$")]

        private static partial Regex RegexCadastralReference();
    }
}
