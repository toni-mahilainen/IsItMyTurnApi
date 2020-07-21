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
    public class ApartmentController : ControllerBase
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
                // All apartments to list
                var apartments = (from a in context.Apartments
                                  select new
                                  {
                                      a.ApartmentId,
                                      a.ApartmentName
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

        // GET: api/apartment/currentshift
        // Get an apartment number for current shift
        [HttpGet]
        [Route("currentshift")]
        public ActionResult GetApartmentForCurrentShift()
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            try
            {
                // Id of the apartment which made the last shift
                int lastApartmentId = (from cf in context.CompletedShifts
                                       orderby cf.ShiftId descending
                                       select cf.ApartmentId).FirstOrDefault();

                int currentApartmentId;

                // If the apartment ID is 5, next in shift will be an apartment 1
                if (lastApartmentId < 5)
                {
                    currentApartmentId = lastApartmentId + 1;
                }
                else
                {
                    currentApartmentId = 1;
                }

                // Get the apartment number based on currentApartmentId
                string currentApartment = (from a in context.Apartments
                                           where a.ApartmentId == currentApartmentId
                                           select a.ApartmentName).FirstOrDefault();

                return Ok(currentApartment);
            }
            catch (Exception ex)
            {
                return BadRequest("Problem detected while getting an apartment for current shift. Error message: " + ex.Message);
            }
            finally
            {
                context.Dispose();
            }
        }
    }
}
