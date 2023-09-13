using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace ManejoPresupuesto.Servicios
{

    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }
    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;
        public RepositorioTransacciones(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("Transacciones_Insertar", 
                new {
                    
                    transaccion.UsuarioId, 
                    transaccion.FechaTransaccion, 
                    transaccion.Monto,
                    transaccion.Nota,                    
                    transaccion.CuentaId,
                    transaccion.CategoriaId
                },
                    commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"select tr.id as Id, tr.monto as Monto, tr.fecha_transaccion as FechaTransaccion, ca.nombre as Categoria, cu.nombre as Cuenta, ca.tipo_operacion_id as TipoOperacionId
                                                                from Transacciones tr
                                                                inner join categorias ca
                                                                on ca.id = tr.categoria_id
                                                                inner join cuentas cu
                                                                on cu.id = tr.cuenta_id
                                                                where tr.cuenta_id = @CuentaId and tr.usuario_id = @UsuarioId
                                                                and fecha_transaccion between @FechaInicio and @FechaFin", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>(@"select t.id as Id, t.monto as Monto, t.fecha_transaccion as FechaTransaccion, c.nombre as Categoria, cu.nombre as Cuenta, c.tipo_operacion_id as TipoOperacionId, nota as Nota
                                                                from Transacciones t
                                                                inner join Categorias c
                                                                on c.id = t.categoria_id
                                                                inner join Cuentas cu
                                                                on cu.id = t.cuenta_id
                                                                where t.usuario_id = @UsuarioId
                                                                and fecha_transaccion between @FechaInicio and @FechaFin
                                                                order by t.fecha_transaccion DESC", modelo);

        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Transacciones_Actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Borrar", new { id }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"select datediff(d, @FechaInicio, fecha_transaccion) / 7 + 1 as Semana,
                                                                            sum(monto) as Monto,
                                                                            c.tipo_operacion_id as TipoOperacion
                                                                            from Transacciones t
                                                                            inner join Categorias c
                                                                            on t.categoria_id = c.id
                                                                            where t.usuario_id = @UsuarioId
                                                                            group by datediff(d, @FechaInicio, fecha_transaccion) / 7, c.tipo_operacion_id", modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int año)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"select month(fecha_transaccion) as Mes,
                                                                            sum(monto) as Monto, c.tipo_operacion_id as TipoOperacionId
                                                                            from Transacciones t
                                                                            inner join categorias c
                                                                            on t.categoria_id = c.id
                                                                            where t.usuario_id = @usuarioId and year(fecha_transaccion) = @año
                                                                            group by month(fecha_transaccion), c.tipo_operacion_id", new {usuarioId, año});
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"select tra.*, cat.tipo_operacion_id as TipoOperacionId
                                                                            from Transacciones tra
                                                                            inner join Categorias cat
                                                                            on cat.id = tra.categoria_id
                                                                            where tra.id = @id and tra.usuario_id = @usuarioId", new {id, usuarioId});
        }
    }
}
