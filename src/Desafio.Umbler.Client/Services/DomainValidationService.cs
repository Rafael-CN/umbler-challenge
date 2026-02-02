using System.Text.RegularExpressions;

namespace Desafio.Umbler.Client.Services
{
    public static class DomainValidationService
    {
        private const int MinLength = 4;
        private const int MaxLength = 253;

        private static readonly Regex DomainRegex = new(
            @"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled
        );

        public static bool IsValid(string? domainName, out string? errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(domainName))
            {
                errorMessage = "Por favor, digite um domínio.";
                return false;
            }

            var domain = domainName.Trim().ToLowerInvariant();

            if (domain.Length < MinLength)
            {
                errorMessage = $"O domínio deve ter pelo menos {MinLength} caracteres.";
                return false;
            }

            if (domain.Length > MaxLength)
            {
                errorMessage = $"O domínio não pode exceder {MaxLength} caracteres.";
                return false;
            }

            if (!domain.Contains('.'))
            {
                errorMessage = "Digite um domínio válido com extensão (ex: exemplo.com).";
                return false;
            }

            if (!DomainRegex.IsMatch(domain))
            {
                errorMessage = "O formato do domínio é inválido.";
                return false;
            }

            return true;
        }

        public static string Normalize(string domainName)
        {
            return domainName.Trim().ToLowerInvariant();
        }
    }
}
