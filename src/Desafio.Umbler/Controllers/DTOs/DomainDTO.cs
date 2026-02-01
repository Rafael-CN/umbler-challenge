using Desafio.Umbler.Models;

namespace Desafio.Umbler.Controllers.DTOs
{
    public class DomainDTO
    {
        public DomainDTO(Domain domain)
        {
            Name = domain.Name;
            Ip = domain.Ip;
            HostedAt = domain.HostedAt;
        }

        public string Name { get; set; }
        public string Ip { get; set; }
        public string HostedAt { get; set; }
    }
}
