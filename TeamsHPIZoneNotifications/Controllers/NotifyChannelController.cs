﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AdaptiveCards;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TeamsProactiveMessageApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyChannelController : ControllerBase
    {
        private static IConfiguration configuration = null;
        private static string botAccessToken = null;
        private static DateTime? botAccessTokenExpiration = null;
        private static HttpClient httpClient = null;
        //private readonly ILogger<NotifyChannelController> _logger;

        //public NotifyChannelController(ILogger<NotifyChannelController> logger)
        //{
        //    _logger = logger;
        //}

        // POST api/NotifyChannel
        // BODY: message (string)
        // Creates a new message in the Teams channel configured in appsettings
        [HttpPost]
        public ActionResult Post([FromBody] string message)
        {
            try
            {
                // load configuration from appSettings
                configuration = configuration ??
                    new ConfigurationBuilder()
                        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();

                httpClient = new HttpClient();

                // get bearer token to make Teams API call
                if (botAccessToken == null
                        || botAccessTokenExpiration == null
                        || DateTime.UtcNow > botAccessTokenExpiration)
                {
                    //await this.FetchTokenAsync(configuration, httpClient);
                    this.FetchTokenAsync(configuration, httpClient).Wait();
                }

                // make call to Teams REST API to POST a NEW message to a channel
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, configuration["ConversationsUrl"]))
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", botAccessToken);
                   
                    var payload = "{" +
                        "\"bot\": {" +
                             "\"id\": \"" + configuration["MicrosoftAppId"] + "\"" +
                        "}," +
                        "\"isGroup\": true," +
                        "\"channelData\": {" +
                            "\"channel\": {" +
                                "\"id\": \"" + configuration["ChannelId"] + "\"" +
                            "}," +
                             "\"notification\": {" +
                                "\"alert\": true" +
                             "}" +
                        "}," +
                        "\"activity\": {" +
                            "\"type\": \"message\"," +
                            "\"type\": \"message\"," +
                            "\"text\": \"" + message + "\"," +
                            "\"attachments\": []," +
                            "\"entities\": []" +
                        "}" +
                      "}";


                    //AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
                    //card.Body.Add(new AdaptiveTextBlock()
                    //{
                    //    Text = "Hello",
                    //    Size = AdaptiveTextSize.ExtraLarge
                    //});

                    //card.Body.Add(new AdaptiveImage()
                    //{
                    //    Url = new Uri("http://adaptivecards.io/content/cats/1.png")
                    //});

                    //// serialize the card to JSON
                    //string json = card.ToJson();

                    requestMessage.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                    //requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");


                    using (var sendResponse = httpClient.SendAsync(requestMessage).Result)
                    {
                        if (sendResponse.StatusCode == HttpStatusCode.Created)
                        {
                            var jsonResponseString = sendResponse.Content.ReadAsStringAsync().Result;
                            dynamic jsonResponse = JsonConvert.DeserializeObject(jsonResponseString);

                            return Created(jsonResponse.id.ToString(), jsonResponseString);
                        }
                        else
                        {
                            return StatusCode((int)sendResponse.StatusCode, sendResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        // PUT api/NotifyChannel/guid
        // PARAM: message id [19:blahblahblah@thread.blah;messageid=123456789]
        // BODY: nothing
        // Updates an existing message in a Teams channel
        [HttpPut("{id}")]
        public ActionResult Put(string id, [FromBody] string message)
        {
            try
            {
                // load configuration from appSettings
                configuration = configuration ??
                    new ConfigurationBuilder()
                        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();

                httpClient = new HttpClient();

                // get bearer token to make Teams API call
                if (botAccessToken == null
                        || botAccessTokenExpiration == null
                        || DateTime.UtcNow > botAccessTokenExpiration)
                {
                    this.FetchTokenAsync(configuration, httpClient).Wait();
                }

                //TODO: have them pass id AND activityid instead?
                string messageId = id.Split(";messageid=")[1];

                // make call to Teams REST API to UPDATE an EXISTING message in a channel
                var updateUrl = $"{configuration["ConversationsUrl"]}/{id}/activities/{messageId}";

                using (var requestMessage = new HttpRequestMessage(HttpMethod.Put, updateUrl))
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", botAccessToken);

                    var payloadString = "{" +
                        "\"type\": \"message\"," +
                        "\"text\": \"[UPDATED] :: " + message + "\"," +
                        "\"attachments\": []," +
                        "\"entities\": []" +
                      "}" +
                      "}";

                    requestMessage.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");

                    using (var sendResponse = httpClient.SendAsync(requestMessage).Result)
                    {
                        if (sendResponse.StatusCode == HttpStatusCode.OK)
                        {
                            var jsonResponseString = sendResponse.Content.ReadAsStringAsync().Result;
                            return Ok(jsonResponseString);
                        }
                        else
                        {
                            return StatusCode((int)sendResponse.StatusCode, sendResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }


        // POST api/NotifyChannel/guid
        // PARAM: message id [19:blahblahblah@thread.blah;messageid=123456789]
        // BODY: nothing
        // Updates an existing message in a Teams channel
        [HttpPost("{id}")]
        public ActionResult Post(string id, [FromBody] string message)
        {
            try
            {
                // load configuration from appSettings
                configuration = configuration ??
                    new ConfigurationBuilder()
                        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();

                httpClient = new HttpClient();

                // get bearer token to make Teams API call
                if (botAccessToken == null
                        || botAccessTokenExpiration == null
                        || DateTime.UtcNow > botAccessTokenExpiration)
                {
                    this.FetchTokenAsync(configuration, httpClient).Wait();
                }

                //TODO: have them pass id AND activityid instead?
                string messageId = id.Split(";messageid=")[1];

                // make call to Teams REST API to UPDATE an EXISTING message in a channel
                var updateUrl = $"{configuration["ConversationsUrl"]}/{id}/activities/{messageId}";

                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, updateUrl))
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", botAccessToken);

                    var payloadString = "{" +
                        "\"type\": \"message\"," +
                        "\"text\": \"[MORE INFO] :: " + message + "\"," +
                        "\"attachments\": []," +
                        "\"entities\": []" +
                      "}" +
                      "}";

                    requestMessage.Content = new StringContent(payloadString, Encoding.UTF8, "application/json");

                    using (var sendResponse = httpClient.SendAsync(requestMessage).Result)
                    {
                        if (sendResponse.StatusCode == HttpStatusCode.Created)
                        {
                            var jsonResponseString = sendResponse.Content.ReadAsStringAsync().Result;
                            return Ok(jsonResponseString);
                        }
                        else
                        {
                            return StatusCode((int)sendResponse.StatusCode, sendResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        /*
         *  UTILITY / HELPERS
         */

        private async Task FetchTokenAsync(IConfiguration configuration, HttpClient httpClient)
        {
            var values = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", configuration["MicrosoftAppId"] },
                    { "client_secret", configuration["MicrosoftAppPassword"] },
                    { "scope", "https://api.botframework.com/.default" },
                };

            var content = new FormUrlEncodedContent(values);

            using (var tokenResponse = await httpClient.PostAsync("https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token", content))
            {
                if (tokenResponse.StatusCode == HttpStatusCode.OK)
                {
                    //var accessTokenContent = await tokenResponse.Content.ReadAsAsync<AccessTokenResponse>();
                    var accessTokenContent = JsonConvert.DeserializeObject<AccessTokenResponse>(await tokenResponse.Content.ReadAsStringAsync());

                    botAccessToken = accessTokenContent.AccessToken;

                    var expiresInSeconds = 121;

                    // If parsing fails, out variable is set to 0, so need to set the default
                    if (!int.TryParse(accessTokenContent.ExpiresIn, out expiresInSeconds))
                    {
                        expiresInSeconds = 121;
                    }

                    // Remove two minutes in order to have a buffer amount of time.
                    botAccessTokenExpiration = DateTime.UtcNow + TimeSpan.FromSeconds(expiresInSeconds - 120);
                }
                else
                {
                    throw new Exception("Error fetching bot access token.");
                }
            }
        }
        private class AccessTokenResponse
        {
            [JsonProperty("token_type")]
            public string TokenType { get; set; }

            [JsonProperty("expires_in")]
            public string ExpiresIn { get; set; }

            [JsonProperty("ext_expires_in")]
            public string ExtExpiresIn { get; set; }

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }
        }
    }
}