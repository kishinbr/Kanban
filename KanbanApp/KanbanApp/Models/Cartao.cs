namespace KanbanApp.Models
{
    public class Cartao
    {
        public int Id { get; set; }
        public int ColunaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int Ordem { get; set; }
    }
}