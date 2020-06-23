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
    public class CompletedShift : ControllerBase
    {
        // GET: api/apartment/
        // Get all completed shifts
        [HttpGet]
        [Route("")]
        public ActionResult GetApartments()
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            try
            {
                var shifts = (from cf in context.CompletedShifts
                                  select new
                                  {
                                      cf.ShiftId,
                                      cf.Apartment,
                                      cf.Date
                                  }).ToList();

                return Ok(shifts);
            }
            catch (Exception ex)
            {
                return BadRequest("Problem detected while getting shifts. Error message: " + ex.Message);
            }
            finally
            {
                context.Dispose();
            }
        }
    }
}
