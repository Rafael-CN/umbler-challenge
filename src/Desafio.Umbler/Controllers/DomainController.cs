using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Desafio.Umbler.Controllers.DTOs;
using Desafio.Umbler.Services;
using Desafio.Umbler.Validators;
using System;

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
            if (!DomainValidator.IsValid(domainName, out string errorMessage))
            {
                return BadRequest(new { error = errorMessage });
            }

            try
            {
                var domain = await _domainService.GetDomainAsync(domainName);
                if (domain == null || string.IsNullOrEmpty(domain.Ip))
                {
                    return NotFound(new { error = "Não foi possível obter informações do domínio" });
                }


                return Ok(new DomainDTO(domain));
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro interno inesperado ao processar a requisição" });
            }
            
        }
    }
}
