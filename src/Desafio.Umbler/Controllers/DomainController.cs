using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Desafio.Umbler.Controllers.DTOs;
using Desafio.Umbler.Services;

namespace Desafio.Umbler.Controllers
{
    [Route("api")]
    public class DomainController : Controller
    {
        private readonly DomainService _domainService;

        public DomainController(DomainService domainService)
        {
            _domainService = domainService;
        }

        [HttpGet, Route("domain/{domainName}")]
        public async Task<IActionResult> Get(string domainName) 
        {
            var domain = await _domainService.GetDomainAsync(domainName);

            DomainDTO dto = new()
            {
                Name = domain.Name,
                Ip = domain.Ip,
                HostedAt = domain.HostedAt
            };

            return Ok(dto);
        }
    }
}
