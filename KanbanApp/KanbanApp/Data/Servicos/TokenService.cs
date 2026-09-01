using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KanbanApp.Data.Servicos
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GerarToken(int usuarioId, string nome, string email)
        {
            var chaveSecreta = _configuration["Jwt:ChaveSecreta"]!;
            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                new Claim(ClaimTypes.Name, nome),
                new Claim(ClaimTypes.Email, email)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Emissor"],
                audience: _configuration["Jwt:Audiencia"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credenciais
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}