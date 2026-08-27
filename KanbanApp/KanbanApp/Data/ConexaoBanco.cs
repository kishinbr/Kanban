using Npgsql;
using System.Data;

namespace KanbanApp.Data
{
    public class ConexaoBanco
    {
        private readonly string _stringConexao;

        public ConexaoBanco(IConfiguration configuration)
        {
            _stringConexao = configuration.GetConnectionString("KanbanDb")
                ?? throw new Exception("String de conexão 'KanbanDb' não encontrada no appsettings.json");
        }

        public IDbConnection CriarConexao()
        {
            return new NpgsqlConnection(_stringConexao);
        }
    }
}