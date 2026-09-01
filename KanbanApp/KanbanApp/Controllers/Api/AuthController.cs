using KanbanApp.Data.Repositorios;
using KanbanApp.Data.Servicos;
using KanbanApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace KanbanApp.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UsuarioRepositorio _usuarioRepositorio;
        private readonly TokenService _tokenService;

        public AuthController(UsuarioRepositorio usuarioRepositorio, TokenService tokenService)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _tokenService = tokenService;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Senha { get; set; } = string.Empty;
        }

        public class CadastroRequest
        {
            public string Nome { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Senha { get; set; } = string.Empty;
        }

        public class AuthResponse
        {
            public string Token { get; set; } = string.Empty;
            public int UsuarioId { get; set; }
            public string Nome { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _usuarioRepositorio.BuscarPorEmail(request.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            {
                return Unauthorized(new { mensagem = "Email ou senha inválidos." });
            }

            var token = _tokenService.GerarToken(usuario.Id, usuario.Nome, usuario.Email);

            return Ok(new AuthResponse
            {
                Token = token,
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
            });
        }

        [HttpPost("cadastro")]
        public async Task<IActionResult> Cadastro([FromBody] CadastroRequest request)
        {
            var usuarioExistente = await _usuarioRepositorio.BuscarPorEmail(request.Email);
            if (usuarioExistente != null)
            {
                return BadRequest(new { mensagem = "Este email já está cadastrado." });
            }

            var novoUsuario = new Usuario
            {
                Nome = request.Nome,
                Email = request.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha)
            };

            var novoId = await _usuarioRepositorio.Cadastrar(novoUsuario);

            var token = _tokenService.GerarToken(novoId, novoUsuario.Nome, novoUsuario.Email);

            return Ok(new AuthResponse
            {
                Token = token,
                UsuarioId = novoId,
                Nome = novoUsuario.Nome,
                Email = novoUsuario.Email
            });
        }


        [HttpGet("eu")]
            [Authorize(AuthenticationSchemes = "Bearer")]
            public IActionResult Eu()
            {
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var nome = User.FindFirstValue(ClaimTypes.Name);
                var email = User.FindFirstValue(ClaimTypes.Email);

                return Ok(new
                {
                    usuarioId,
                    nome,
                    email
                });
    }
}
}