using K8SConfigMap.Api.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace K8SConfigMap.Api.Controllers
{
    [ApiController]
    public class OperationsController : ControllerBase
    {
        private readonly IOptionsMonitor<ServiceRouting> _configuration;

        public OperationsController(IOptionsMonitor<ServiceRouting> configuration)
        {
            _configuration = configuration;
            _configuration.OnChange((sr) =>
            {
                Console.WriteLine("configuration change detected at " + DateTime.Now);
            });

        }

        [HttpGet]
        [Route("/operations/map")]
        public IActionResult GetMap()
        {
            return Ok(_configuration.CurrentValue);
        }
    }
}
