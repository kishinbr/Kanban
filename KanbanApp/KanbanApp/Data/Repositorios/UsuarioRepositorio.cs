using Dapper;
using KanbanApp.Models;

namespace KanbanApp.Data.Repositorios
{
    public class UsuarioRepositorio
    {
        private readonly ConexaoBanco _conexaoBanco;

        public UsuarioRepositorio(ConexaoBanco conexaoBanco)
        {
            _conexaoBanco = conexaoBanco;
        }

        public async Task<Usuario?> BuscarPorEmail(string email)
        {
            using var conexao = _conexaoBanco.CriarConexao();

            string sql = "SELECT * FROM usuarios WHERE email = @Email";

            return await conexao.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });
        }

        public async Task<int> Cadastrar(Usuario usuario)
        {
            using var conexao = _conexaoBanco.CriarConexao();

            string sql = @"
                INSERT INTO usuarios (nome, email, senha_hash)
                VALUES (@Nome, @Email, @SenhaHash)
                RETURNING id";

            return await conexao.QuerySingleAsync<int>(sql, usuario);
        }
    }
}