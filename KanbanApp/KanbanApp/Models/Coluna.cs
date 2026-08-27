namespace KanbanApp.Models
{
    public class Coluna
    {
        public int Id { get; set; }
        public int QuadroId { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}