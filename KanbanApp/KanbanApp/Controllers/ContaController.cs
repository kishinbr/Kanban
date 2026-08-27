using KanbanApp.Data.Repositorios;
using KanbanApp.Models;
using Microsoft.AspNetCore.Mvc;

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
    }
}