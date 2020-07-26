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
    public class CompletedShiftController : ControllerBase
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
        // Add a new completed shift and send the push notifications to all registered devices
        [HttpPost]
        [Route("")]
        public async Task<ActionResult> AddNewShift([FromBody] CompletedShifts newShiftdata)
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
                    int successCount = context.SaveChanges();

                    // Notifications
                    bool notificationSuccess = await HandleNotification();
                    
                    if (notificationSuccess == true && successCount > 0)
                    {
                        // Everything is OK
                        return Ok("New shift has added successfully!");
                    }
                    else if (notificationSuccess == false && successCount > 0)
                    {
                        // Problems with notifications. Addition to database is OK
                        return StatusCode(201);
                    }
                    else
                    {
                        // Nothing is OK
                        return BadRequest("Problems with notification and adding the row to database!");
                    }
                }
                else
                {
                    return NotFound("New shift data is missing!");
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
        public async Task<ActionResult> UpdateShiftData(int id, [FromBody] CompletedShifts newShiftData)
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
                        bool hasApartmentChanged = shift.ApartmentId != newShiftData.ApartmentId;

                        shift.ApartmentId = newShiftData.ApartmentId;
                        shift.Date = newShiftData.Date;

                        int successCount = context.SaveChanges();

                        // When the apartment ID has changed, the push notifications will be sent
                        if (hasApartmentChanged)
                        {
                            // Notifications
                            bool notificationSuccess = await HandleNotification();

                            if (notificationSuccess == true && successCount > 0)
                            {
                                // Everything is OK
                                return Ok("A shift has updated successfully!");
                            }
                            else if (notificationSuccess == false && successCount > 0)
                            {
                                // Problems with notifications. Database update is OK
                                return StatusCode(201);
                            }
                            else
                            {
                                // Nothing is OK
                                return BadRequest("Problems with notification and updating the row to database!");
                            }
                        }
                        else
                        {
                            return Ok("A shift has updated successfully!");
                        }
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
        public async Task<ActionResult> DeleteShift(int id)
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            try
            {
                if (id != 0)
                {
                    // Searching right shift with ID
                    var shift = context.CompletedShifts.Find(id);

                    context.Remove(shift);
                    int successCount = context.SaveChanges();

                    // Notifications
                    bool notificationSuccess = await HandleNotification();

                    if (notificationSuccess == true && successCount > 0)
                    {
                        // Everything is OK
                        return Ok("New shift has deleted successfully!");
                    }
                    else if (notificationSuccess == false && successCount > 0)
                    {
                        // Problems with notifications. Deletion from database is OK
                        return StatusCode(201);
                    }
                    else
                    {
                        // Nothing is OK
                        return BadRequest("Problems with notification and deleting the row from database!");
                    }
                }
                else
                {
                    return BadRequest("Shift ID is null!");
                }
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

            // Title and body for notification
            FCMNotification notification = new FCMNotification();
            notification.title = "Leikkuuvuoro vaihtui!";
            notification.body = "Seuraavana vuorossa: " + nextApartmentInShift;
            notification.sound = "default";

            // Tokens to array from database which will get the notification
            string[] fcmTokens = (from ft in context.FcmTokens
                                  select ft.Token).ToArray();

            List<bool> successList = new List<bool>();

            // The notification is sent to device and the result will be added to list of successes
            foreach (var token in fcmTokens)
            {
                FCMBody body = new FCMBody();
                body.registration_ids = new string[] { token };
                body.notification = notification;

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
            HttpClient client = new HttpClient();
            var httpContent = JsonConvert.SerializeObject(fcmBody);

            // Server key for authorization
            var authorization = string.Format("key={0}", "AAAA_V-6Iio:APA91bF5-SIIcue9XdaALtO1is8Vlkk2PUVn9Z21LaeYurCI0y0s-1ZNtDA6ZxsMPpzTqM1Lh0uEf1SH-PBMIhOODp6sOY7v7PUOLBqyt-H3PcvbC4-mPmcaUH2Xk52ermtqvNua7vIH");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorization);

            // Notification content to Firebase API in JSON format
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

                // If the apartment ID is 6, next in shift will be an apartment 1
                if (lastApartmentId < 6)
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
                return $"Error: {ex.Message}";
            }
            finally
            {
                context.Dispose();
            }
        }
    }
}
