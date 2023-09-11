using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace HackerNewsService.Controllers
{
    [ApiController]
    [Route("api/stories")]
    public class StoriesController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;

        public StoriesController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet("best")]
        public async Task<IActionResult> GetListOfBestStories(int n)
        {
            if (n <= 0)
            {
                return BadRequest("Given value is invalid");
            }

            var storyIds = await GetListofStoryIds();
            var stories = await GetNoOfStoryDetails(storyIds, n);

            return Ok(stories);
        }

        private async Task<List<int>> GetListofStoryIds()
        {
            using var _httpClient = _clientFactory.CreateClient();
            var response = await _httpClient.GetFromJsonAsync<List<int>>("https://hacker-news.firebaseio.com/v0/beststories.json");
            return response ?? new List<int>();
        }

        private async Task<List<Story>> GetNoOfStoryDetails(List<int> storyIds, int n)
        {
            using var client = _clientFactory.CreateClient();
            var stories = new List<Story>();
            try
            {
                foreach (var storyId in storyIds.Take(n))
                {
                    var story = await client.GetFromJsonAsync<Story>($"https://hacker-news.firebaseio.com/v0/item/{storyId}.json");
                    if (story != null)
                    {
                        stories.Add(story);
                    }
                }
            }catch (Exception ex)
            {
                // Exception we have handle if we get no response from Url
            }
            return stories.OrderByDescending(s => s.Score).ToList();
        }

    }
}
