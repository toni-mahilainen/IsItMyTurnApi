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
    public class FCMTokenController : ControllerBase
    {
        // POST: api/fcmtoken/
        // Add a new device if not already exist
        // Add/update a token for device
        [HttpPost]
        [Route("")]
        public async Task<ActionResult> HandleDevicesAndTokens([FromBody] OtherModels.Identifier identifier)
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            var deviceCheck = (from d in context.Devices
                               where d.UniqueIdentifier == identifier.UniqueIdentifier
                               select d).FirstOrDefault();

            // Check if device exists in database
            if (deviceCheck == null)
            {
                Devices device = new Devices()
                {
                    UniqueIdentifier = identifier.UniqueIdentifier
                };

                context.Devices.Add(device);
                int success = await context.SaveChangesAsync();

                // When device has added successfully, a token will be added to device
                if (success > 0)
                {
                    int deviceId = (from d in context.Devices
                                    where d.UniqueIdentifier == identifier.UniqueIdentifier
                                    select d.DeviceId).FirstOrDefault();

                    FcmTokens token = new FcmTokens()
                    {
                        DeviceId = deviceId,
                        Token = identifier.Token
                    };

                    context.FcmTokens.Add(token);
                    context.SaveChanges();

                    return Ok("A new device and a token has added successfully!");
                }
                else
                {
                    return BadRequest("Problem detected while adding a device identifier to database.");
                }
            }
            else
            {
                // If device exists, check if the device has a token
                var tokenCheck = (from ft in context.FcmTokens
                                  where ft.DeviceId == deviceCheck.DeviceId
                                  select ft).FirstOrDefault();

                // If a token is not exist, it will be added to database. Otherwise the old one will be updated
                if (tokenCheck == null)
                {
                    FcmTokens token = new FcmTokens()
                    {
                        DeviceId = deviceCheck.DeviceId,
                        Token = identifier.Token
                    };

                    context.FcmTokens.Add(token);
                    int success = context.SaveChanges();

                    if (success > 0)
                    {
                        return Ok("Token has added successfully!");
                    }
                    else
                    {
                        return BadRequest("Problem detected while adding a token for device to database.");
                    }
                }
                else
                {
                    tokenCheck.Token = identifier.Token;

                    int success = context.SaveChanges();

                    if (success > 0)
                    {
                        return Ok("Token updated successfully!");
                    }
                    else
                    {
                        return BadRequest("Problem detected while updating a token for device to database.");
                    }
                }
            }
        }

        public Task<bool> DeleteOldToken(string oldToken)
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            try
            {
                FcmTokens tokenObj = (from ft in context.FcmTokens
                                      where ft.Token == oldToken
                                      select ft).FirstOrDefault();

                context.FcmTokens.Remove(tokenObj);
                context.SaveChanges();

                return Task.FromResult(true);
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
    }
}
