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
        // GET: api/completedshift/
        // Get all completed shifts
        [HttpGet]
        [Route("")]
        public ActionResult GetCompletedShifts()
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            try
            {
                var shifts = (from cf in context.CompletedShifts
                              select new
                              {
                                  cf.ShiftId,
                                  cf.Apartment.ApartmentName,
                                  cf.Date.Date
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

        // POST: api/completedshift/
        // Add a new completed shift
        [HttpPost]
        [Route("")]
        public ActionResult AddNewShift([FromBody] CompletedShifts newShift)
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            try
            {
                context.CompletedShifts.Add(newShift);
                context.SaveChanges();

                return Ok("New shift has added successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest("Problem detected while adding a new shift. Error message: " + ex.Message);
            }
            finally
            {
                context.Dispose();
            }
        }

        // DELETE: api/completedshift/{shiftId}
        // Delete a shift
        [HttpDelete]
        [Route("{id}")]
        public ActionResult DeleteShift(int id)
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            try
            {
                if (id != 0)
                {
                    // Searching right user with ID
                    var shift = context.CompletedShifts.Find(id);

                    context.Remove(shift);
                    context.SaveChanges();
                }

                return Ok("Shift has deleted succesfully!");
            }
            catch (Exception ex)
            {
                return BadRequest("Problem detected while deleting a shift. Error message: " + ex.InnerException);
            }
            finally
            {
                context.Dispose();
            }
        }
    }
}
