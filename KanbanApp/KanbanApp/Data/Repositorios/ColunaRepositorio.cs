using Dapper;
using KanbanApp.Models;

namespace KanbanApp.Data.Repositorios
{
    public class ColunaRepositorio
    {
        private readonly ConexaoBanco _conexaoBanco;

        public ColunaRepositorio(ConexaoBanco conexaoBanco)
        {
            _conexaoBanco = conexaoBanco;
        }

        public async Task<IEnumerable<Coluna>> ListarPorQuadro(int quadroId)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = "SELECT * FROM colunas WHERE quadro_id = @QuadroId ORDER BY id";
            return await conexao.QueryAsync<Coluna>(sql, new { QuadroId = quadroId });
        }

        public async Task<int> Criar(int quadroId, string nome)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = @"
                INSERT INTO colunas (quadro_id, nome)
                VALUES (@QuadroId, @Nome)
                RETURNING id";

            return await conexao.QuerySingleAsync<int>(sql, new { QuadroId = quadroId, Nome = nome });
        }

        public async Task Excluir(int id)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = "DELETE FROM colunas WHERE id = @Id";
            await conexao.ExecuteAsync(sql, new { Id = id });
        }
        public async Task<Coluna?> BuscarPorId(int id)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = "SELECT * FROM colunas WHERE id = @Id";
            return await conexao.QueryFirstOrDefaultAsync<Coluna>(sql, new { Id = id });
        }

        public async Task Editar(int id, string nome)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = "UPDATE colunas SET nome = @Nome WHERE id = @Id";
            await conexao.ExecuteAsync(sql, new { Id = id, Nome = nome });
        }
    }
}