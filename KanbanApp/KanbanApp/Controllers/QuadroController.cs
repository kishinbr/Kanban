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
        [HttpPost]
        public async Task<IActionResult> Entrar(string codigo)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var quadro = await _quadroRepositorio.BuscarPorCodigo(codigo);

            if (quadro == null)
            {
                TempData["Erro"] = "Código não encontrado.";
                return RedirectToAction("Index", "Painel");
            }

            bool jaEhMembro = await _quadroRepositorio.UsuarioJaEhMembro(quadro.Id, usuarioId);

            if (jaEhMembro)
            {
                TempData["Erro"] = "Você já participa deste kanban.";
                return RedirectToAction("Index", "Painel");
            }

            await _quadroRepositorio.AdicionarEspectador(quadro.Id, usuarioId);

            return RedirectToAction("Index", "Painel");
        }
    }
}