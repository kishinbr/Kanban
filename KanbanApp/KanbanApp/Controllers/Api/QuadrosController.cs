using KanbanApp.Data.Repositorios;
using KanbanApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanApp.Controllers.Api
{
    [ApiController]
    [Route("api/quadros")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class QuadrosController : ControllerBase
    {
        private readonly QuadroRepositorio _quadroRepositorio;

        private readonly ColunaRepositorio _colunaRepositorio;
        private readonly CartaoRepositorio _cartaoRepositorio;

        public QuadrosController(
            QuadroRepositorio quadroRepositorio,
            ColunaRepositorio colunaRepositorio,
            CartaoRepositorio cartaoRepositorio)
        {
            _quadroRepositorio = quadroRepositorio;
            _colunaRepositorio = colunaRepositorio;
            _cartaoRepositorio = cartaoRepositorio;
        }

        private int UsuarioIdLogado => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var quadros = await _quadroRepositorio.ListarPorUsuario(UsuarioIdLogado);
            return Ok(quadros);
        }

        public class CriarQuadroRequest
        {
            public string Nome { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarQuadroRequest request)
        {
            var codigo = await _quadroRepositorio.GerarCodigoUnico();
            var quadroId = await _quadroRepositorio.Criar(request.Nome, codigo, UsuarioIdLogado);

            return Ok(new { quadroId, codigo });
        }
        public class EntrarRequest
        {
            public string Codigo { get; set; } = string.Empty;
        }



        [HttpPost("entrar")]
        public async Task<IActionResult> Entrar([FromBody] EntrarRequest request)
        {
            var quadro = await _quadroRepositorio.BuscarPorCodigo(request.Codigo);

            if (quadro == null)
            {
                return NotFound(new { mensagem = "Código não encontrado." });
            }

            var jaEhMembro = await _quadroRepositorio.UsuarioJaEhMembro(quadro.Id, UsuarioIdLogado);

            if (jaEhMembro)
            {
                return BadRequest(new { mensagem = "Você já participa deste kanban." });
            }

            await _quadroRepositorio.AdicionarEspectador(quadro.Id, UsuarioIdLogado);

            return Ok(new { quadroId = quadro.Id, nome = quadro.Nome });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Ver(int id)
        {
            var quadro = await _quadroRepositorio.BuscarPorId(id);
            if (quadro == null)
            {
                return NotFound();
            }

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(id, UsuarioIdLogado);
            if (papel == null)
            {
                return Forbid();
            }

            var colunas = await _colunaRepositorio.ListarPorQuadro(id);
            var membros = await _quadroRepositorio.ListarMembros(id);

            var viewModel = new QuadroDetalheViewModel
            {
                Quadro = quadro,
                Papel = papel,
                Membros = membros.ToList(),
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

            return Ok(viewModel);
        }
    }
}