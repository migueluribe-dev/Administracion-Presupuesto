namespace ManejoPresupuesto.Models
{
    public class PaginacionRespuesta
    {
        public int Pagina { get; set; } = 1;

        public int RecordsPorPagina { get; set; } = 5;

        public int CantidadTotalRecors { get; set; }

        public int CantidadTotalDePaginas => (int)Math.Ceiling((double)CantidadTotalRecors / RecordsPorPagina);

        public string? BaseUrl { get; set; }
    }

    public class PaginacionRespuesta<T> : PaginacionRespuesta
    {
        public IEnumerable<T> Elementos { get; set; }
    }
}
