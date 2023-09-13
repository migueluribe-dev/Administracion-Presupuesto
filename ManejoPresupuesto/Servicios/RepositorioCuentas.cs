using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }

    public class RepositorioCuentas : IRepositorioCuentas
    {
        private readonly string connectionString;


        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO ManejoPresupuesto.dbo.Cuentas(nombre, tipo_cuenta, descripcion, balance) 
                                                               VALUES(@Nombre, @TipoCuentaId, @Descripcion, @Balance);

                                                               SELECT SCOPE_IDENTITY();", cuenta);

            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Cuenta>(@"select c.id as Id, c.nombre as Nombre, balance as Balance, tc.nombre as TipoCuenta 
                                                            from Cuentas c
                                                            inner join  Tipos_Cuentas tc
                                                            on tc.id = c.tipo_cuenta
                                                            where tc.usuario_id = @UsuarioId
                                                            order by tc.orden", new { usuarioId });
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.ExecuteAsync(@"update Cuentas
                                                                set nombre = @Nombre, balance = @Balance, descripcion = @Descripcion, 
                                                                tipo_cuenta = @TipoCuentaId
                                                                where id = @Id", cuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("delete Cuentas where id = @Id", new { id });
        }

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(@"select c.id as Id, c.nombre as Nombre, balance as Balance, descripcion as Descripcion, tipo_cuenta as TipoCuentaId
                                                            from Cuentas c
                                                            inner join  Tipos_Cuentas tc
                                                            on tc.id = c.tipo_cuenta
                                                            where tc.usuario_id = @UsuarioId and c.id = @id", new {id, usuarioId});
        }
    }
}
