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
    public class FCMToken : ControllerBase
    {
        // POST: api/fcmtoken/
        // Add a new token for the device
        [HttpPost]
        [Route("")]
        public async Task<ActionResult> AddNewToken([FromBody] OtherModels.FcmTokens tokens)
        {
            IsItMyTurnContext context = new IsItMyTurnContext();

            try
            {
                if (tokens.OldToken == "")
                {
                    FcmTokens fcmToken = new FcmTokens()
                    {
                        Token = tokens.NewToken
                    };

                    context.FcmTokens.Add(fcmToken);
                    context.SaveChanges();

                    return Ok("Token has added successfully!");
                }
                else
                {
                    bool success = await DeleteOldToken(tokens.OldToken);
                    if (success)
                    {
                        FcmTokens fcmToken = new FcmTokens()
                        {
                            Token = tokens.NewToken
                        };

                        context.FcmTokens.Add(fcmToken);
                        context.SaveChanges();

                        return Ok("Token has added successfully!");
                    }
                    else
                    {
                        return BadRequest("Problem detected while deleting an old token.");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Problem detected while adding a new token. Error message: " + ex.InnerException);
            }
            finally
            {
                context.Dispose();
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
