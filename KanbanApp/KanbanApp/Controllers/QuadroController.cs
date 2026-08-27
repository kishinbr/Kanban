using KanbanApp.Data.Repositorios;
using KanbanApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanApp.Controllers
{
    [Authorize]
    public class QuadroController : Controller
    {
        private readonly QuadroRepositorio _quadroRepositorio;
        private readonly ColunaRepositorio _colunaRepositorio;
        private readonly CartaoRepositorio _cartaoRepositorio;

        public QuadroController(
             QuadroRepositorio quadroRepositorio,
             ColunaRepositorio colunaRepositorio,
             CartaoRepositorio cartaoRepositorio)
        {
            _quadroRepositorio = quadroRepositorio;
            _colunaRepositorio = colunaRepositorio;
            _cartaoRepositorio = cartaoRepositorio;
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
        public async Task<IActionResult> Ver(int id)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var quadro = await _quadroRepositorio.BuscarPorId(id);
            if (quadro == null)
            {
                return NotFound();
            }

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(id, usuarioId);
            if (papel == null)
            {
                return Forbid();
            }

            var colunas = await _colunaRepositorio.ListarPorQuadro(id);

            var viewModel = new QuadroDetalheViewModel
            {
                Quadro = quadro,
                Papel = papel,
                Colunas = new List<ColunaComCartoes>()
            };

            foreach (var coluna in colunas)
            {
                var cartoes = await _cartaoRepositorio.ListarPorColuna(coluna.Id);
                viewModel.Colunas.Add(new ColunaComCartoes
                {
                    Coluna = coluna,
                    Cartoes = cartoes.ToList()
                });
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CriarColuna(int quadroId, string nome)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(quadroId, usuarioId);
            if (papel != "dono")
            {
                return Forbid();
            }

            await _colunaRepositorio.Criar(quadroId, nome);

            return RedirectToAction("Ver", new { id = quadroId });
        }

        [HttpPost]
        public async Task<IActionResult> CriarCartao(int colunaId, string titulo)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var coluna = await _colunaRepositorio.BuscarPorId(colunaId);
            if (coluna == null)
            {
                return NotFound();
            }

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(coluna.QuadroId, usuarioId);
            if (papel != "dono")
            {
                return Forbid();
            }

            var cartoesExistentes = await _cartaoRepositorio.ListarPorColuna(colunaId);
            int novaOrdem = cartoesExistentes.Any() ? cartoesExistentes.Max(c => c.Ordem) + 1 : 0;

            await _cartaoRepositorio.Criar(colunaId, titulo, null, novaOrdem);

            return RedirectToAction("Ver", new { id = coluna.QuadroId });
        }
        [HttpPost]
        public async Task<IActionResult> ExcluirColuna(int colunaId)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var coluna = await _colunaRepositorio.BuscarPorId(colunaId);
            if (coluna == null)
            {
                return NotFound();
            }

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(coluna.QuadroId, usuarioId);
            if (papel != "dono")
            {
                return Forbid();
            }

            await _colunaRepositorio.Excluir(colunaId);

            return RedirectToAction("Ver", new { id = coluna.QuadroId });
        }
        [HttpPost]
        public async Task<IActionResult> ExcluirCartao(int cartaoId, int quadroId)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(quadroId, usuarioId);
            if (papel != "dono")
            {
                return Forbid();
            }

            await _cartaoRepositorio.Excluir(cartaoId);

            return RedirectToAction("Ver", new { id = quadroId });
        }

        [HttpPost]
        public async Task<IActionResult> EditarCartao(int id, string titulo, string? descricao, int quadroId)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(quadroId, usuarioId);
            if (papel != "dono")
            {
                return Forbid();
            }

            await _cartaoRepositorio.Editar(id, titulo, descricao);

            return RedirectToAction("Ver", new { id = quadroId });
        }
        [HttpPost]
        public async Task<IActionResult> EditarColuna(int id, string nome, int quadroId)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(quadroId, usuarioId);
            if (papel != "dono")
            {
                return Forbid();
            }

            await _colunaRepositorio.Editar(id, nome);

            return RedirectToAction("Ver", new { id = quadroId });
        }
        [HttpPost]
        public async Task<IActionResult> Sair(int quadroId)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(quadroId, usuarioId);

            if (papel != "espectador")
            {
                TempData["Erro"] = "Você não pode sair deste kanban.";
                return RedirectToAction("Ver", new { id = quadroId });
            }

            await _quadroRepositorio.RemoverMembro(quadroId, usuarioId);

            return RedirectToAction("Index", "Painel");
        }
    }
}