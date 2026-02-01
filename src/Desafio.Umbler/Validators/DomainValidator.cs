using System.Text.RegularExpressions;

namespace Desafio.Umbler.Validators
{
    public class DomainValidator
    {
        private const int MinLength = 4;
        private const int MaxLength = 253;

        private static readonly Regex DomainRegex = new(
            @"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled
        );

        public static bool IsValid(string domainName, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(domainName))
            {
                errorMessage = "O nome do domínio não pode ser vazio";
                return false;
            }

            domainName = domainName.Trim().ToLowerInvariant();

            if (domainName.Length < MinLength)
            {
                errorMessage = $"O nome do domínio deve ter pelo menos {MinLength} caracteres";
                return false;
            }

            if (domainName.Length > MaxLength)
            {
                errorMessage = $"O nome do domínio não pode exceder {MaxLength} caracteres";
                return false;
            }

            if (!domainName.Contains('.'))
            {
                errorMessage = "O nome do domínio deve conter uma extensão válida (ex.: .com, .net)";
                return false;
            }

            if (!DomainRegex.IsMatch(domainName))
            {
                errorMessage = "O nome do domínio contém caracteres inválidos ou está mal formatado";
                return false;
            }

            return true;
        }
    }
}
