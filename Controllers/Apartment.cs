using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IsItMyTurnApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IsItMyTurnApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Apartment : ControllerBase
    {
        // GET: api/apartment/
        // Get all apartments
        [HttpGet]
        [Route("")]
        public ActionResult GetApartments()
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            try
            {
                var apartments = (from a in context.Apartments
                                  select new 
                                  {
                                      a.ApartmentId,
                                      a.Apartment
                                  }).ToList();

                return Ok(apartments);
            }
            catch (Exception ex)
            {
                return BadRequest("Problem detected while getting apartments. Error message: " + ex.Message);
            }
            finally
            {
                context.Dispose();
            }
        }
    }
}
