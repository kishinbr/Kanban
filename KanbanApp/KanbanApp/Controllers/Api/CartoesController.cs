using KanbanApp.Data.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanApp.Controllers.Api
{
    [ApiController]
    [Route("api/cartoes")]
    [Authorize(AuthenticationSchemes = "CookieKanban,Bearer")]
    public class CartoesController : ControllerBase
    {
        private readonly CartaoRepositorio _cartaoRepositorio;
        private readonly ColunaRepositorio _colunaRepositorio;
        private readonly QuadroRepositorio _quadroRepositorio;

        public CartoesController(
            CartaoRepositorio cartaoRepositorio,
            ColunaRepositorio colunaRepositorio,
            QuadroRepositorio quadroRepositorio)
        {
            _cartaoRepositorio = cartaoRepositorio;
            _colunaRepositorio = colunaRepositorio;
            _quadroRepositorio = quadroRepositorio;
        }

        private int UsuarioIdLogado => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public class MoverCartaoRequest
        {
            public int NovaColunaId { get; set; }
            public int NovaOrdem { get; set; }
        }

        [HttpPut("{id}/mover")]
        public async Task<IActionResult> Mover(int id, [FromBody] MoverCartaoRequest request)
        {
            var cartao = await _cartaoRepositorio.BuscarPorId(id);
            if (cartao == null)
            {
                return NotFound();
            }

            var colunaAtual = await _colunaRepositorio.BuscarPorId(cartao.ColunaId);
            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(colunaAtual!.QuadroId, UsuarioIdLogado);
            if (papel != "dono")
            {
                return Forbid();
            }

            await _cartaoRepositorio.MoverParaColuna(id, request.NovaColunaId, request.NovaOrdem);

            return Ok();
        }

        public class CriarCartaoRequest
        {
            public int ColunaId { get; set; }
            public string Titulo { get; set; } = string.Empty;
            public string? Descricao { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarCartaoRequest request)
        {
            var coluna = await _colunaRepositorio.BuscarPorId(request.ColunaId);
            if (coluna == null)
            {
                return NotFound();
            }

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(coluna.QuadroId, UsuarioIdLogado);
            if (papel != "dono")
            {
                return Forbid();
            }

            var cartoesExistentes = await _cartaoRepositorio.ListarPorColuna(request.ColunaId);
            int novaOrdem = cartoesExistentes.Any() ? cartoesExistentes.Max(c => c.Ordem) + 1 : 0;

            var cartaoId = await _cartaoRepositorio.Criar(request.ColunaId, request.Titulo, request.Descricao, novaOrdem);

            return Ok(new { cartaoId });
        }

        public class EditarCartaoRequest
        {
            public string Titulo { get; set; } = string.Empty;
            public string? Descricao { get; set; }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarCartaoRequest request)
        {
            var cartao = await _cartaoRepositorio.BuscarPorId(id);
            if (cartao == null)
            {
                return NotFound();
            }

            var coluna = await _colunaRepositorio.BuscarPorId(cartao.ColunaId);
            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(coluna!.QuadroId, UsuarioIdLogado);
            if (papel != "dono")
            {
                return Forbid();
            }

            await _cartaoRepositorio.Editar(id, request.Titulo, request.Descricao);

            return Ok(new { mensagem = "Cartão atualizado." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var cartao = await _cartaoRepositorio.BuscarPorId(id);
            if (cartao == null)
            {
                return NotFound();
            }

            var coluna = await _colunaRepositorio.BuscarPorId(cartao.ColunaId);
            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(coluna!.QuadroId, UsuarioIdLogado);
            if (papel != "dono")
            {
                return Forbid();
            }

            await _cartaoRepositorio.Excluir(id);

            return Ok(new { mensagem = "Cartão excluído." });
        }
    }
}