using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IsItMyTurnApi.Models;
using IsItMyTurnApi.OtherModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
                                  cf.ApartmentId,
                                  cf.Apartment.ApartmentName,
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

        // POST: api/completedshift/
        // Add a new completed shift
        [HttpPost]
        [Route("")]
        public async Task<ActionResult> AddNewShift([FromBody] NewShift newShiftdata)
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            try
            {
                if (newShiftdata != null)
                {
                    // Object for database
                    CompletedShifts shift = new CompletedShifts()
                    {
                        ApartmentId = newShiftdata.ApartmentId,
                        Date = newShiftdata.Date
                    };

                    context.CompletedShifts.Add(shift);

                    // If the addition to the database succeeded
                    if (context.SaveChanges() > 0)
                    {
                        bool notificationSuccess = await HandleNotification();
                        //await HandleNotification();

                        // If the notification was sent successfully
                        if (notificationSuccess)
                        {
                            return Ok("New shift has added successfully!");
                        }
                        else
                        {
                            return BadRequest("Problems with notification!");
                        }
                    }
                    else
                    {
                        return BadRequest("Problems with adding content to database!");
                    }
                }
                else
                {
                    return NotFound("New shift data missing!");
                }
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

        // PUT: api/completedshift/
        // Update data for completed shift
        [HttpPut]
        [Route("{id}")]
        public ActionResult UpdateShiftData(int id, [FromBody] CompletedShifts newShiftData)
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            if (id != 0)
            {
                CompletedShifts shift = (from cf in context.CompletedShifts
                                         where cf.ShiftId == id
                                         select cf).FirstOrDefault();

                try
                {
                    if (shift != null)
                    {
                        shift.ApartmentId = newShiftData.ApartmentId;
                        shift.Date = newShiftData.Date;

                        return Ok("Shift data has updated successfully!");
                    }
                    else
                    {
                        return NotFound("Shift with ID: " + id + " not found");
                    }

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
            else
            {
                return BadRequest("Problems");
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

        // Build notification
        public async Task<bool> HandleNotification()
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            string nextApartmentInShift = GetApartmentForNextShift();

            FCMNotification notification = new FCMNotification();
            notification.title = "Leikkuuvuoro vaihtui!";
            notification.body = "Seuraavana vuorossa asunto(t) " + nextApartmentInShift + ".";

            FCMData data = new FCMData();
            data.key1 = "";
            data.key2 = "";
            data.key3 = "";
            data.key4 = "";

            string[] fcmTokens = (from ft in context.FcmTokens
                                  select ft.Token).ToArray();

            List<bool> successList = new List<bool>();

            foreach (var token in fcmTokens)
            {
                FCMBody body = new FCMBody();
                body.registration_ids = new string[] { token };
                body.notification = notification;
                body.data = data;

                bool result = await SendNotification(body);
                successList.Add(result);
            }

            // If all the notification send responses are OK, return true
            bool allTrue = successList.All(s => Equals(true, s));

            if (allTrue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Notification request to Firebase
        public async Task<bool> SendNotification(FCMBody fcmBody)
        {
            var httpContent = JsonConvert.SerializeObject(fcmBody);
            HttpClient client = new HttpClient();
            var authorization = string.Format("key={0}", "AAAA_V-6Iio:APA91bF5-SIIcue9XdaALtO1is8Vlkk2PUVn9Z21LaeYurCI0y0s-1ZNtDA6ZxsMPpzTqM1Lh0uEf1SH-PBMIhOODp6sOY7v7PUOLBqyt-H3PcvbC4-mPmcaUH2Xk52ermtqvNua7vIH");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorization);
            StringContent stringContent = new StringContent(httpContent);
            stringContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.PostAsync("https://fcm.googleapis.com/fcm/send", stringContent);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Apartment number for notification
        public string GetApartmentForNextShift()
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

                return currentApartment;
            }
            catch (Exception ex)
            {
                return "Tuntematon";
            }
            finally
            {
                context.Dispose();
            }
        }
    }
}
