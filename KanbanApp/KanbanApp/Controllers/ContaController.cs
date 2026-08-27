using KanbanApp.Data.Repositorios;
using KanbanApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanApp.Controllers
{
    public class ContaController : Controller
    {
        private readonly UsuarioRepositorio _usuarioRepositorio;

        public ContaController(UsuarioRepositorio usuarioRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
        }

        [HttpGet]
        public IActionResult Cadastro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Cadastro(string nome, string email, string senha)
        {
            var usuarioExistente = await _usuarioRepositorio.BuscarPorEmail(email);
            if (usuarioExistente != null)
            {
                ModelState.AddModelError("", "Este email já está cadastrado.");
                return View();
            }

            var novoUsuario = new Usuario
            {
                Nome = nome,
                Email = email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha)
            };

            await _usuarioRepositorio.Cadastrar(novoUsuario);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string email, string senha)
        {
            var usuario = await _usuarioRepositorio.BuscarPorEmail(email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash))
            {
                ModelState.AddModelError("", "Email ou senha inválidos.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieKanban");

            await HttpContext.SignInAsync("CookieKanban", new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }
    }
}