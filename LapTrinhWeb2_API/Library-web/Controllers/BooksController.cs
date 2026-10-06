using library_web.Models.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Library_web.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        public BooksController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        // tạo HttpClient có gắn JWT lấy từ cookie đăng nhập
        private async Task<HttpClient> CreateAuthClient()
        {
            var client = httpClientFactory.CreateClient();
            var token = await HttpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // 401: token sai/hết hạn -> đăng xuất, về Login; 403: thiếu quyền -> AccessDenied
        private IActionResult? AuthFail(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
                return RedirectToAction("Login", "Account");
            }
            if (response.StatusCode == HttpStatusCode.Forbidden)
                return RedirectToAction("AccessDenied", "Account");
            return null;
        }

        public async Task<IActionResult> Index([FromQuery] string filterOn = null, string filterQuery=null, string sortBy= null, bool isAscending = true)
        {
            List<BookDTO> response = new List<BookDTO>();
            try
            {
                // lấy dữ liệu books from API
                var client = await CreateAuthClient();
                var httpResponseMess = await client.GetAsync("https://localhost:7233/api/Book/get-all-books?filterOn="+filterOn+"&filterQuery="+filterQuery+"&sortBy="+sortBy+"&isAscending="+isAscending);
                if (AuthFail(httpResponseMess) is IActionResult authResult1) return authResult1;
                httpResponseMess.EnsureSuccessStatusCode();
                response.AddRange(await httpResponseMess.Content.ReadFromJsonAsync<IEnumerable<BookDTO>>());

            }
            catch (Exception ex)
            {
                return View("Error");
            }
            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> addBook()
        {

            var client = await CreateAuthClient();
            List<authorDTO> responseAu = new List<authorDTO>();
            var httpResponseAu = await client.GetAsync("https://localhost:7233/api/Authors/get-all-author");
            if (AuthFail(httpResponseAu) is IActionResult authResult2) return authResult2;
            httpResponseAu.EnsureSuccessStatusCode();
            responseAu.AddRange(await httpResponseAu.Content.ReadFromJsonAsync<IEnumerable<authorDTO>>());
            ViewBag.listAuthor = responseAu;

            List<publisherDTO> responsePu = new List<publisherDTO>();
            var httpResponsePu = await client.GetAsync("https://localhost:7233/api/Publishers/get-all-publisher");
            if (AuthFail(httpResponsePu) is IActionResult authResult3) return authResult3;
            httpResponsePu.EnsureSuccessStatusCode();
            responsePu.AddRange(await httpResponsePu.Content.ReadFromJsonAsync<IEnumerable<publisherDTO>>());
            ViewBag.listPublisher = responsePu;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> addBook(addBookDTO addBookDTO)
        {
            try
            {
                var client = await CreateAuthClient();
                var httpRequestMess = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://localhost:7233/api/Book/add-book"),
                    Content = new StringContent(JsonSerializer.Serialize(addBookDTO), Encoding.UTF8,
                      MediaTypeNames.Application.Json)
                };
                //Console.WriteLine(JsonSerializer.Serialize(addBookDTO));
                var httpResponseMess = await client.SendAsync(httpRequestMess);
                if (AuthFail(httpResponseMess) is IActionResult authResult4) return authResult4;
                httpResponseMess.EnsureSuccessStatusCode();
                var response = await httpResponseMess.Content.ReadFromJsonAsync<addBookDTO>();
                if (response != null)
                {
                    return RedirectToAction("Index", "Books");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View();
        }

        public async Task<IActionResult> listBook(int id)
        {
            BookDTO response = new BookDTO();
            try
            {
                // lấy dữ liệu books from API
                var client = await CreateAuthClient();
                var httpResponseMess = await client.GetAsync("https://localhost:7233/api/Book/get-book-by-id/"+id);
                if (AuthFail(httpResponseMess) is IActionResult authResult5) return authResult5;
                httpResponseMess.EnsureSuccessStatusCode();
                var stringResponseBody = await httpResponseMess.Content.ReadAsStringAsync();
                response = await httpResponseMess.Content.ReadFromJsonAsync<BookDTO>();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> editBook(int id)
        {
            BookDTO responseBook = new BookDTO();
            var client = await CreateAuthClient();
            var httpResponseMess = await client.GetAsync("https://localhost:7233/api/Book/get-book-by-id/" + id);
            if (AuthFail(httpResponseMess) is IActionResult authResult6) return authResult6;
            httpResponseMess.EnsureSuccessStatusCode();
            responseBook = await httpResponseMess.Content.ReadFromJsonAsync<BookDTO>();
            ViewBag.Book = responseBook;

            List<authorDTO> responseAu = new List<authorDTO>();
            var httpResponseAu = await client.GetAsync("https://localhost:7233/api/Authors/get-all-author");
            if (AuthFail(httpResponseAu) is IActionResult authResult7) return authResult7;
            httpResponseAu.EnsureSuccessStatusCode();
            responseAu.AddRange(await httpResponseAu.Content.ReadFromJsonAsync<IEnumerable<authorDTO>>());
            ViewBag.listAuthor = responseAu;

            List<publisherDTO> responsePu = new List<publisherDTO>();
            var httpResponsePu = await client.GetAsync("https://localhost:7233/api/Publishers/get-all-publisher");
            if (AuthFail(httpResponsePu) is IActionResult authResult8) return authResult8;
            httpResponsePu.EnsureSuccessStatusCode();
            responsePu.AddRange(await httpResponsePu.Content.ReadFromJsonAsync<IEnumerable<publisherDTO>>());
            ViewBag.listPublisher = responsePu;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> editBook([FromRoute]int id, editBookDTO bookDTO)
        {
            try
            {
                var client = await CreateAuthClient();
                var httpRequestMess = new HttpRequestMessage()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri("https://localhost:7233/api/Book/update-book-by-id/"+id),
                    Content = new StringContent(JsonSerializer.Serialize(bookDTO), Encoding.UTF8,
                      MediaTypeNames.Application.Json)
                };

                var httpResponseMess = await client.SendAsync(httpRequestMess);
                if (AuthFail(httpResponseMess) is IActionResult authResult9) return authResult9;
                httpResponseMess.EnsureSuccessStatusCode();
                var response = await httpResponseMess.Content.ReadFromJsonAsync<addBookDTO>();
                if (response != null)
                {
                    return RedirectToAction("Index", "Books");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> delBook([FromRoute] int id)
        {
            try
            {
                // lấy dữ liệu books from API
                var client = await CreateAuthClient();
                var httpResponseMess = await client.DeleteAsync("https://localhost:7233/api/Book/delete-book-by-id/" + id);
                if (AuthFail(httpResponseMess) is IActionResult authResult10) return authResult10;
                httpResponseMess.EnsureSuccessStatusCode();
                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View("Index");
        }
    }
}
