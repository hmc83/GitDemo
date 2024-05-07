using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitiesManager.Infrastructure.DatabaseAccess;
using CitiesManager.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.FileProviders;

namespace CitiesManager.Infrastructure.Controllers.v2
{
    //[Route("api/[controller]")]
    [Route("api/[controller]/{version:ApiVersion?}")]
    [ApiVersion("2.0")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<string?>>> GetCities()
        {
            var x = HttpContext;
            if (_context.Cities == null)
            {
                return NotFound(); //404
            }
            return await _context.Cities.Select(temp => temp.CityName).ToListAsync();  //200
        }
    }
}


