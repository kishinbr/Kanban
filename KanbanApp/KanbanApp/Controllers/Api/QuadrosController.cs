using KanbanApp.Data.Repositorios;
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

        public QuadrosController(QuadroRepositorio quadroRepositorio)
        {
            _quadroRepositorio = quadroRepositorio;
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
    }
}