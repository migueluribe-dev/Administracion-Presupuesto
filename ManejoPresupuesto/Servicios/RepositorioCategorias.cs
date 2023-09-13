using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{

    public interface IRepositorioCategorias
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task<int> Contar(int usuarioId);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int ususrioId, PaginacionViewModel paginacion);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
    }
    public class RepositorioCategorias : IRepositorioCategorias
    {
        private readonly string connectionString;

        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"insert into Categorias (nombre, tipo_operacion_id, usuario_id)
                                                              values (@Nombre, @TipoOperacionId, @UsuarioId);

                                                                SELECT SCOPE_IDENTITY();", categoria);

            categoria.Id = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacion)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@$"select id as Id, nombre as Nombre, tipo_operacion_id as TipoOperacionId, usuario_id as UsuarioId 
                                                            from Categorias where usuario_id = @UsuarioId
                                                            ORDER BY Nombre
                                                            OFFSET {paginacion.RecordsASaltar} ROWS FETCH NEXT {paginacion.RecordsPorPagina} ROWS ONLY",
                                                            new { usuarioId });
        }

        public async Task<int> Contar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Categorias WHERE usuario_id = @usuarioId", new { usuarioId });
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@"select id as Id, nombre as Nombre, tipo_operacion_id as TipoOperacionId, usuario_id as UsuarioId 
                                                            from Categorias where usuario_id = @usuarioId and tipo_operacion_id = @tipoOperacionId", new { usuarioId, tipoOperacionId });
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(@"select id as Id, nombre as Nombre, tipo_operacion_id as TipoOperacionId, usuario_id as UsuarioId 
                                                                            from Categorias where id = @id and usuario_id = @UsuarioId", new { id, usuarioId });
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"update Categorias set nombre = @Nombre, tipo_operacion_id = @TipoOperacionId where id = @id", categoria);

        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(@"delete Categorias where id = @Id", new {id});
        }
    }
}
