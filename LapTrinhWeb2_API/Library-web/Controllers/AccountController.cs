using library_web.Models.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Library_web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;
        public AccountController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(loginDTO model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            try
            {
                // gọi API đăng nhập để lấy JWT
                var client = httpClientFactory.CreateClient();
                var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                var httpResponseMess = await client.PostAsync("https://localhost:7233/api/User/Login", content);
                if (!httpResponseMess.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
                    return View(model);
                }
                var response = await httpResponseMess.Content.ReadFromJsonAsync<loginResponseDTO>();
                if (response == null || string.IsNullOrEmpty(response.JwtToken))
                {
                    ViewBag.Error = "API không trả về token";
                    return View(model);
                }

                // đọc claim + hạn dùng từ payload JWT
                var payload = ReadJwtPayload(response.JwtToken);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.Email, model.Username)
                };
                // API tạo token bằng ClaimTypes.Role nên khóa trong payload là URI dài; vẫn hỗ trợ khóa "role"
                if (payload.TryGetProperty(ClaimTypes.Role, out var roleElement) || payload.TryGetProperty("role", out roleElement))
                {
                    if (roleElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var r in roleElement.EnumerateArray())
                            claims.Add(new Claim(ClaimTypes.Role, r.GetString()!));
                    }
                    else claims.Add(new Claim(ClaimTypes.Role, roleElement.GetString()!));
                }

                var expiresUtc = DateTimeOffset.FromUnixTimeSeconds(payload.GetProperty("exp").GetInt64());
                var props = new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = expiresUtc // cookie hết hạn cùng lúc với token
                };
                // lưu token vào cookie để các request sau lấy ra gửi lên API
                props.StoreTokens(new[] { new AuthenticationToken { Name = "access_token", Value = response.JwtToken } });

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), props);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(registerDTO model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                // tài khoản đăng ký từ web chỉ được cấp role Read (không cho tự chọn Write)
                var body = new { Username = model.Username, Password = model.Password, Roles = new[] { "Read" } };
                var client = httpClientFactory.CreateClient();
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var httpResponseMess = await client.PostAsync("https://localhost:7233/api/User/Register", content);
                if (!httpResponseMess.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Đăng ký thất bại (email đã tồn tại hoặc mật khẩu không hợp lệ)";
                    return View(model);
                }
                TempData["Success"] = "Đăng ký thành công, hãy đăng nhập";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private static JsonElement ReadJwtPayload(string jwt)
        {
            var base64 = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            return JsonDocument.Parse(Convert.FromBase64String(base64)).RootElement.Clone();
        }
    }
}
