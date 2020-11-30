using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MovieClient.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MovieClient.Controllers
{
    public class MovieController : Controller
    {
        public IConfiguration Configuration { get; }

        public MovieController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> Index()
        {
            List<Movie> movies = new List<Movie>();

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Configuration["APIBaseUrl"]);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string token = await GetJWTToken();
                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage response = await httpClient.GetAsync("api/movies");
                    if (response.IsSuccessStatusCode)
                    {
                        movies = await response.Content.ReadAsAsync<List<Movie>>();
                    }
                    else
                    {
                        ViewBag.Error = response.ReasonPhrase;
                    }

                }
                else
                {
                    ViewBag.Error = "Authentication failed!";
                }

            }


            return View(movies);
        }

        private async Task<string> GetJWTToken()
        {
            string JWTToken = string.Empty;

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Configuration["APIBaseUrl"]);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                User user = new User();
                user.userName = Configuration["UserName"];
                user.password = Configuration["Password"];
                var content = new StringContent(JsonConvert.SerializeObject(user).ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync("api/Users/Login", content);
                if (response.IsSuccessStatusCode)
                {
                    JWTToken = await response.Content.ReadAsAsync<string>();
                }
            }

            return JWTToken;
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        public async Task<ActionResult> Create(Movie movie)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(Configuration["APIBaseUrl"]);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var content = new StringContent(JsonConvert.SerializeObject(movie).ToString(), Encoding.UTF8, "application/json");

                string token = await GetJWTToken();

                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage response = await httpClient.PostAsync("api/movies", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.Error = response.ReasonPhrase;
                        return View();
                    }

                }
                else
                {
                    ViewBag.Error = "Authentication failed!";
                }

                return View();
            }
        }
    }
}
