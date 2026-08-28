using KanbanApp.Models;

namespace KanbanApp.ViewModels
{
    public class QuadroDetalheViewModel
    {
        public Quadro Quadro { get; set; } = null!;
        public string Papel { get; set; } = string.Empty;
        public List<ColunaComCartoes> Colunas { get; set; } = new();

        public List<MembroInfo> Membros { get; set; } = new();
    }

    public class ColunaComCartoes
    {
        public Coluna Coluna { get; set; } = null!;
        public List<Cartao> Cartoes { get; set; } = new();
    }
    public class MembroInfo
    {
        public string Nome { get; set; } = string.Empty;
        public string Papel { get; set; } = string.Empty;
    }
}