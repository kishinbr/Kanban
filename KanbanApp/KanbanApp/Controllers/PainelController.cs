using KanbanApp.Data.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanApp.Controllers
{
    [Authorize]
    public class PainelController : Controller
    {
        private readonly QuadroRepositorio _quadroRepositorio;

        public PainelController(QuadroRepositorio quadroRepositorio)
        {
            _quadroRepositorio = quadroRepositorio;
        }

        public async Task<IActionResult> Index()
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var quadros = await _quadroRepositorio.ListarPorUsuario(usuarioId);

            return View(quadros);
        }
    }
}