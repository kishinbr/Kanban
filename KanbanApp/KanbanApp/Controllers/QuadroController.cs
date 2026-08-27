using KanbanApp.Data.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanApp.Controllers
{
    [Authorize]
    public class QuadroController : Controller
    {
        private readonly QuadroRepositorio _quadroRepositorio;

        public QuadroController(QuadroRepositorio quadroRepositorio)
        {
            _quadroRepositorio = quadroRepositorio;
        }

        [HttpGet]
        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Criar(string nome)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            string codigo = await _quadroRepositorio.GerarCodigoUnico();

            int quadroId = await _quadroRepositorio.Criar(nome, codigo, usuarioId);

            return RedirectToAction("Index", "Painel");
        }
    }
}