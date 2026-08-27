namespace KanbanApp.Models
{
    public class Quadro
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int UsuarioDonoId { get; set; }
        public string CodigoCompartilhamento { get; set; } = string.Empty;

        // Não é uma coluna da tabela "quadros" -
        // vem do JOIN com "membros", indicando o papel do usuário logado nesse quadro
        public string Papel { get; set; } = string.Empty;
    }
}