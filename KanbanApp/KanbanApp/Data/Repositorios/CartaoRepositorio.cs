using Dapper;
using KanbanApp.Models;

namespace KanbanApp.Data.Repositorios
{
    public class CartaoRepositorio
    {
        private readonly ConexaoBanco _conexaoBanco;

        public CartaoRepositorio(ConexaoBanco conexaoBanco)
        {
            _conexaoBanco = conexaoBanco;
        }

        public async Task<IEnumerable<Cartao>> ListarPorColuna(int colunaId)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = "SELECT * FROM cartoes WHERE coluna_id = @ColunaId ORDER BY ordem";
            return await conexao.QueryAsync<Cartao>(sql, new { ColunaId = colunaId });
        }

        public async Task<int> Criar(int colunaId, string titulo, string? descricao, int ordem)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = @"
                INSERT INTO cartoes (coluna_id, titulo, descricao, ordem)
                VALUES (@ColunaId, @Titulo, @Descricao, @Ordem)
                RETURNING id";

            return await conexao.QuerySingleAsync<int>(sql, new
            {
                ColunaId = colunaId,
                Titulo = titulo,
                Descricao = descricao,
                Ordem = ordem
            });
        }

        public async Task Editar(int id, string titulo, string? descricao)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = "UPDATE cartoes SET titulo = @Titulo, descricao = @Descricao WHERE id = @Id";
            await conexao.ExecuteAsync(sql, new { Id = id, Titulo = titulo, Descricao = descricao });
        }

        public async Task MoverParaColuna(int id, int colunaId, int novaOrdem)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = "UPDATE cartoes SET coluna_id = @ColunaId, ordem = @Ordem WHERE id = @Id";
            await conexao.ExecuteAsync(sql, new { Id = id, ColunaId = colunaId, Ordem = novaOrdem });
        }

        public async Task Excluir(int id)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = "DELETE FROM cartoes WHERE id = @Id";
            await conexao.ExecuteAsync(sql, new { Id = id });
        }
    }
}