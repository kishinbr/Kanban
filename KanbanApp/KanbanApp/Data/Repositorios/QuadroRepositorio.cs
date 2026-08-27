using Dapper;
using KanbanApp.Models;

namespace KanbanApp.Data.Repositorios
{
    public class QuadroRepositorio
    {
        private readonly ConexaoBanco _conexaoBanco;

        public QuadroRepositorio(ConexaoBanco conexaoBanco)
        {
            _conexaoBanco = conexaoBanco;
        }

        public async Task<IEnumerable<Quadro>> ListarPorUsuario(int usuarioId)
        {
            using var conexao = _conexaoBanco.CriarConexao();

            string sql = @"
                SELECT q.id, q.nome, q.usuario_dono_id, q.codigo_compartilhamento, m.papel
                FROM quadros q
                INNER JOIN membros m ON m.quadro_id = q.id
                WHERE m.usuario_id = @UsuarioId";

            return await conexao.QueryAsync<Quadro>(sql, new { UsuarioId = usuarioId });
        }
        public async Task<int> Criar(string nome, string codigoCompartilhamento, int usuarioDonoId)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            conexao.Open();
            using var transacao = conexao.BeginTransaction();

            try
            {
                string sqlQuadro = @"
            INSERT INTO quadros (nome, usuario_dono_id, codigo_compartilhamento)
            VALUES (@Nome, @UsuarioDonoId, @CodigoCompartilhamento)
            RETURNING id";

                int quadroId = await conexao.QuerySingleAsync<int>(sqlQuadro, new
                {
                    Nome = nome,
                    UsuarioDonoId = usuarioDonoId,
                    CodigoCompartilhamento = codigoCompartilhamento
                }, transacao);

                string sqlMembro = @"
            INSERT INTO membros (quadro_id, usuario_id, papel)
            VALUES (@QuadroId, @UsuarioId, 'dono')";

                await conexao.ExecuteAsync(sqlMembro, new
                {
                    QuadroId = quadroId,
                    UsuarioId = usuarioDonoId
                }, transacao);

                transacao.Commit();
                return quadroId;
            }
            catch
            {
                transacao.Rollback();
                throw;
            }
        }

        private string GerarCodigo()
        {
            const string caracteresPermitidos = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            return new string(Enumerable.Repeat(caracteresPermitidos, 6)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        }
        public async Task<bool> CodigoExiste(string codigo)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = "SELECT COUNT(1) FROM quadros WHERE codigo_compartilhamento = @Codigo";
            int count = await conexao.QuerySingleAsync<int>(sql, new { Codigo = codigo });
            return count > 0;
        }

        public async Task<string> GerarCodigoUnico()
        {
            string codigo;
            do
            {
                codigo = GerarCodigo();
            } while (await CodigoExiste(codigo));

            return codigo;
        }
        public async Task<Quadro?> BuscarPorCodigo(string codigo)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = "SELECT * FROM quadros WHERE codigo_compartilhamento = @Codigo";
            return await conexao.QueryFirstOrDefaultAsync<Quadro>(sql, new { Codigo = codigo });
        }

        public async Task<bool> UsuarioJaEhMembro(int quadroId, int usuarioId)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = "SELECT COUNT(1) FROM membros WHERE quadro_id = @QuadroId AND usuario_id = @UsuarioId";
            int count = await conexao.QuerySingleAsync<int>(sql, new { QuadroId = quadroId, UsuarioId = usuarioId });
            return count > 0;
        }

        public async Task AdicionarEspectador(int quadroId, int usuarioId)
        {
            using var conexao = _conexaoBanco.CriarConexao();
            string sql = @"
        INSERT INTO membros (quadro_id, usuario_id, papel)
        VALUES (@QuadroId, @UsuarioId, 'espectador')";

            await conexao.ExecuteAsync(sql, new { QuadroId = quadroId, UsuarioId = usuarioId });
        }
    }
}