using KanbanApp.Data.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanApp.Controllers.Api
{
    [ApiController]
    [Route("api/cartoes")]
    [Authorize]
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

        public class MoverCartaoRequest
        {
            public int NovaColunaId { get; set; }
            public int NovaOrdem { get; set; }
        }

        [HttpPut("{id}/mover")]
        public async Task<IActionResult> Mover(int id, [FromBody] MoverCartaoRequest request)
        {
            int usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var cartao = await _cartaoRepositorio.BuscarPorId(id);
            if (cartao == null)
            {
                return NotFound();
            }

            var colunaAtual = await _colunaRepositorio.BuscarPorId(cartao.ColunaId);
            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(colunaAtual!.QuadroId, usuarioId);
            if (papel != "dono")
            {
                return Forbid();
            }

            await _cartaoRepositorio.MoverParaColuna(id, request.NovaColunaId, request.NovaOrdem);

            return Ok();
        }
    }
}