using System.Text.RegularExpressions;

namespace landerist_library.Tools
{
    public partial class Validate
    {
        public static bool Email(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var trimmedEmail = email.Trim();
            if (trimmedEmail.EndsWith(".", StringComparison.Ordinal))
            {
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(trimmedEmail);
                return string.Equals(addr.Address, trimmedEmail, StringComparison.Ordinal);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool CadastralReference(string? cadastralReference)
        {
            if (string.IsNullOrWhiteSpace(cadastralReference))
            {
                return false;
            }

            Regex regex = RegexCadastralReference();
            return regex.IsMatch(cadastralReference.Trim());
        }

        public static bool Phone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            int digitCount = 0;

            foreach (char c in phone)
            {
                if (char.IsDigit(c))
                {
                    digitCount++;
                }
            }

            return digitCount >= 6;
        }

        [GeneratedRegex(@"^[0-9]{7}[A-Z]{2}[0-9]{4}[A-Z]([0-9]{4}[A-Z]{2})?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex RegexCadastralReference();
    }
}
