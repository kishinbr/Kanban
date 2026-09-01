using KanbanApp.Data.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KanbanApp.Controllers.Api
{
    [ApiController]
    [Route("api/colunas")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ColunasController : ControllerBase
    {
        private readonly ColunaRepositorio _colunaRepositorio;
        private readonly QuadroRepositorio _quadroRepositorio;

        public ColunasController(ColunaRepositorio colunaRepositorio, QuadroRepositorio quadroRepositorio)
        {
            _colunaRepositorio = colunaRepositorio;
            _quadroRepositorio = quadroRepositorio;
        }

        private int UsuarioIdLogado => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public class CriarColunaRequest
        {
            public int QuadroId { get; set; }
            public string Nome { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarColunaRequest request)
        {
            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(request.QuadroId, UsuarioIdLogado);
            if (papel != "dono")
            {
                return Forbid();
            }

            var colunaId = await _colunaRepositorio.Criar(request.QuadroId, request.Nome);

            return Ok(new { colunaId });
        }

        public class EditarColunaRequest
        {
            public string Nome { get; set; } = string.Empty;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarColunaRequest request)
        {
            var coluna = await _colunaRepositorio.BuscarPorId(id);
            if (coluna == null)
            {
                return NotFound();
            }

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(coluna.QuadroId, UsuarioIdLogado);
            if (papel != "dono")
            {
                return Forbid();
            }

            await _colunaRepositorio.Editar(id, request.Nome);

            return Ok(new { mensagem = "Coluna atualizada." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var coluna = await _colunaRepositorio.BuscarPorId(id);
            if (coluna == null)
            {
                return NotFound();
            }

            var papel = await _quadroRepositorio.BuscarPapelDoUsuario(coluna.QuadroId, UsuarioIdLogado);
            if (papel != "dono")
            {
                return Forbid();
            }

            await _colunaRepositorio.Excluir(id);

            return Ok(new { mensagem = "Coluna excluída." });
        }
    }
}