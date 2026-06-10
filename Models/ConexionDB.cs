using System;
using Microsoft.EntityFrameworkCore; // Necesario para DbContext
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using GestionVentas.Models;
using GestionVentas.Helpers;

namespace GestionVentas.Models
{
    // Agregamos ": DbContext" para que el controlador pueda usar Cajas y SaveChanges
    public class ConexionDB : DbContext
    {
        private readonly string _connectionString;

        // CONSTRUCTOR COMPATIBLE:
        // Mantenemos el IConfiguration para que tus métodos manuales sigan funcionando
        public ConexionDB(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // Este constructor es para que Entity Framework (la Caja) no de errores
        public ConexionDB(DbContextOptions<ConexionDB> options, IConfiguration config)
            : base(options)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // --- SECCIÓN DE TABLAS (Solo esto es lo "nuevo") ---
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }
        public DbSet<Cliente> Clientes { get; set; }

        // --- TU MÉTODO DE SIEMPRE (No se toca) ---
        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(_connectionString);
        }

        // Configuramos EF para que use la misma conexión que tus otros controladores
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }





        //-------------------------- 
        public List<Venta> ObtenerVentasPorUsuario(string nombreUsuario)
        {
            List<Venta> lista = new List<Venta>();

            using (SqlConnection conexion = ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT idFactura, vendedor, montoVenta, diaVenta, idCliente FROM facturas WHERE vendedor = @vendedor";

                using (SqlCommand cmd = new SqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@vendedor", nombreUsuario);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Venta v = new Venta
                            {
                                // --- CAMBIOS APLICADOS AQUÍ ---
                                // Reemplazar reader.GetTipo("Nombre") por reader.GetTipo(reader.GetOrdinal("Nombre"))

                                IdFactura = reader.GetInt32(reader.GetOrdinal("idFactura")),
                                DiaVenta = reader.GetDateTime(reader.GetOrdinal("diaVenta")),
                                MontoVenta = reader.GetDecimal(reader.GetOrdinal("montoVenta")),
                                Vendedor = reader.GetString(reader.GetOrdinal("vendedor")),
                                IdCliente = reader.GetInt32(reader.GetOrdinal("idCliente"))
                            };
                            lista.Add(v);
                        }
                    }
                }
            }

            return lista;
        }



        //--------------------------PROVEEDORES --------------------------------
        public List<Proveedor> ObtenerProveedores()
        {
            var lista = new List<Proveedor>();

            using (var conexion = new SqlConnection(_connectionString))
            {
                conexion.Open();
                var comando = new SqlCommand("SELECT * FROM Proveedor", conexion);
                var reader = comando.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Proveedor
                    {
                        // Mapear la columna 'idProv' a la propiedad 'IdProveedor'
                        IdProveedor = Convert.ToInt32(reader["idProv"]),

                        // Mapear la columna 'nombre' a la propiedad 'NombreProveedor'
                        NombreProveedor = reader["nombre"].ToString(),

                        Telefono = reader["telefono"].ToString(),
                        Domicilio = reader["domicilio"].ToString(),
                        Localidad = reader["localidad"].ToString()
                    });
                }
            }

            return lista;
        }

        // Agregar proveedor
        public void AgregarProveedor(Proveedor prov)
        {
            using (var conexion = new SqlConnection(_connectionString))
            {
                conexion.Open();
                var query = "INSERT INTO Proveedor (nombre, telefono, domicilio, localidad) VALUES (@nombre, @telefono, @domicilio, @localidad)";
                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@nombre", prov.NombreProveedor);
                comando.Parameters.AddWithValue("@telefono", prov.Telefono);
                comando.Parameters.AddWithValue("@domicilio", prov.Domicilio);
                comando.Parameters.AddWithValue("@localidad", prov.Localidad);
                comando.ExecuteNonQuery();
            }
        }

        // Actualizar proveedor
        public void ActualizarProveedor(Proveedor prov)
        {
            using (var conexion = new SqlConnection(_connectionString))
            {
                conexion.Open();
                var query = "UPDATE Proveedor SET nombre = @nombre, telefono = @telefono, domicilio = @domicilio, localidad = @localidad WHERE idProv = @id";
                var comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@id", prov.IdProveedor);
                comando.Parameters.AddWithValue("@nombre", prov.NombreProveedor);
                comando.Parameters.AddWithValue("@telefono", prov.Telefono);
                comando.Parameters.AddWithValue("@domicilio", prov.Domicilio);
                comando.Parameters.AddWithValue("@localidad", prov.Localidad);
                comando.ExecuteNonQuery();
            }
        }

        // Eliminar proveedor
        public void EliminarProveedor(int id)
        {
            using (var conexion = new SqlConnection(_connectionString))
            {
                conexion.Open();
                var comando = new SqlCommand("DELETE FROM Proveedor WHERE idProv = @id", conexion);
                comando.Parameters.AddWithValue("@id", id);
                comando.ExecuteNonQuery();
            }
        }

        //--------------------- CLIENTES -------------------------------------

        public List<Cliente> ObtenerClientes()
        {
            var lista = new List<Cliente>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM Clientes", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Cliente
                    {
                        IdCliente = Convert.ToInt32(reader["idCliente"]),
                        DniCliente = reader["dniCliente"].ToString(),
                        NombreCliente = reader["nombreCliente"].ToString(),
                        Domicilio = reader["domicilio"].ToString(),
                        Localidad = reader["localidad"].ToString(),
                        TelefonoCliente = reader["telefonoCliente"].ToString()
                    });
                }
            }
            return lista;
        }

        public List<Cliente> ObtenerTodosLosClientes()
        {
            List<Cliente> lista = new List<Cliente>();

            using (SqlConnection conexion = new SqlConnection(_connectionString))
            {
                conexion.Open();
                string query = "SELECT * FROM Clientes";

                using (SqlCommand comando = new SqlCommand(query, conexion))
                using (SqlDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Cliente c = new Cliente()
                        {
                            IdCliente = Convert.ToInt32(reader["idCliente"]),
                            DniCliente = reader["dniCliente"].ToString(),
                            NombreCliente = reader["nombreCliente"].ToString(),
                            Domicilio = reader["domicilio"].ToString(),
                            Localidad = reader["localidad"].ToString(),
                            TelefonoCliente = reader["telefonoCliente"].ToString()
                        };

                        lista.Add(c);
                    }
                }
            }

            return lista;
        }


public List<Factura> ObtenerFacturasPendientesPorCliente(int idCliente)
{
    // Si el error persiste aquí, verifica si tu clase es 'Factura' o 'Facturas'
    List<Factura> lista = new List<Factura>();

    using (var conn = ObtenerConexion())
    {
        string sql = @"SELECT idFactura, diaVenta, montoVenta, vendedor, medioPago 
                       FROM facturas 
                       WHERE idCliente = @id AND estado = 'Pendiente' 
                       ORDER BY diaVenta DESC";

        conn.Open();
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@id", idCliente);
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    lista.Add(new Factura // <--- Asegúrate que coincida con tu Model
                    {
                        IdFactura = Convert.ToInt32(dr["idFactura"]),
                        DiaVenta = Convert.ToDateTime(dr["diaVenta"]),
                        MontoVenta = Convert.ToDecimal(dr["montoVenta"]),
                        Vendedor = dr["vendedor"].ToString(),
                        MedioPago = dr["medioPago"].ToString()
                    });
                }
            }
        }
    }
    return lista;
}


public void InsertarGastoCtaCte(int idCliente, decimal total, string vendedor, string detalle, DateTime fecha)
{
    using (var conn = ObtenerConexion())
    {
        // 1. Usamos @DiaVenta en el SQL para que coincida con el nombre de la columna
        string sql = @"INSERT INTO Facturas (DiaVenta, MontoVenta, Vendedor, IdCliente, MedioPago, TipoVenta) 
                       VALUES (@DiaVenta, @total, @vendedor, @idCliente, 'CtaCte', 'Cuenta Corriente')";

        conn.Open();
        using (var cmd = new SqlCommand(sql, conn))
        {
            // 2. Aquí usamos @DiaVenta para que coincida con el string de arriba
            cmd.Parameters.AddWithValue("@DiaVenta", fecha);
            
            cmd.Parameters.AddWithValue("@idCliente", idCliente);
            cmd.Parameters.AddWithValue("@total", total);
            cmd.Parameters.AddWithValue("@vendedor", (object)vendedor ?? DBNull.Value);
            
            cmd.ExecuteNonQuery();
        }

    }
}






// 1. Inserta la factura y devuelve el ID generado para poder meterle los productos después
public int InsertarFacturaCtaCte(int idCliente, decimal total, string vendedor)
{
    using (var conn = ObtenerConexion())
    {
        // El SELECT SCOPE_IDENTITY() es CLAVE para que devuelva el nro de factura
        string sql = @"INSERT INTO Facturas (DiaVenta, MontoVenta, Vendedor, IdCliente, MedioPago, TipoVenta) 
                       VALUES (GETDATE(), @monto, @vendedor, @idCli, 'CtaCte', 'Cuenta Corriente');
                       SELECT SCOPE_IDENTITY();"; 
        
        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@monto", total);
        cmd.Parameters.AddWithValue("@vendedor", vendedor ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@idCli", idCliente);
        
        conn.Open();
        // Usamos ExecuteScalar para capturar el ID
        object result = cmd.ExecuteScalar();
        return (result != null) ? Convert.ToInt32(result) : 0;
    }
}

public void InsertarFacturaItem(int idFactura, int idProducto, string nombre, int cantidad, decimal precio)
{
    using (var conn = ObtenerConexion())
    {
        // Ahora usamos exactamente los nombres de tu SELECT: 
        // idFactura, idItem, nombreProd, cantidad, precio
        string sql = @"INSERT INTO facturaitem (idFactura, idItem, nombreProd, cantidad, precio) 
                       VALUES (@idF, @idI, @nom, @cant, @pre)";
        
        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@idF", idFactura);
        cmd.Parameters.AddWithValue("@idI", idProducto); // Este es tu idItem
        cmd.Parameters.AddWithValue("@nom", nombre);
        cmd.Parameters.AddWithValue("@cant", cantidad);
        cmd.Parameters.AddWithValue("@pre", precio);
        
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}




// 3. Descuenta del stock para que no vendas cosas que no tenés
public void ActualizarStock(int idProducto, int cantidad)
{
    using (var conn = ObtenerConexion())
    {
        // Cantidad viene como negativa (ej: -5), por eso sumamos (Stock = Stock + (-5))
        string sql = "UPDATE Productos SET Stock = Stock + @cant WHERE IdProducto = @id";
        
        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cant", cantidad);
        cmd.Parameters.AddWithValue("@id", idProducto);
        
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}public void RegistrarEnHistorialCtaCte(int idCliente, string concepto, decimal monto, int idProducto, int cantidad)
{
    using (var conn = ObtenerConexion())
    {
        // Corregimos el orden: IdCliente, Fecha, Concepto, Monto, IdProducto, Cantidad
        string sql = @"INSERT INTO HistorialCtaCte (IdCliente, Fecha, Concepto, Monto, IdProducto, Cantidad) 
                       VALUES (@id, @fecha, @conc, @monto, @idProd, @cant)";
        
        var cmd = new SqlCommand(sql, conn);
        
        cmd.Parameters.AddWithValue("@id", idCliente);
        cmd.Parameters.AddWithValue("@fecha", DateTime.Now); // Ahora coincide con la 2da columna
        cmd.Parameters.AddWithValue("@conc", concepto);     // Ahora coincide con la 3ra columna
        cmd.Parameters.Add("@monto", System.Data.SqlDbType.Decimal).Value = monto; // Coincide con la 4ta
        cmd.Parameters.AddWithValue("@idProd", idProducto);
        cmd.Parameters.AddWithValue("@cant", cantidad);
        
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}

public void ActualizarSaldoPorPago(int id, decimal monto) {
    using (var conn = ObtenerConexion()) {
        string sql = "UPDATE ClientesCtaCte SET SaldoActual = SaldoActual - @monto WHERE IdCliente = @id";
        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@monto", monto);
        cmd.Parameters.AddWithValue("@id", id);
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}

public List<Cliente> ObtenerClientesCtaCte()
{
    List<Cliente> lista = new List<Cliente>();
    using (var conn = ObtenerConexion())
    {
        // Traemos a todos los que están en la tabla de cuenta corriente
        // Si quieres que solo aparezcan los que deben plata, agrega: WHERE SaldoActual > 0
        string sql = "SELECT IdCliente, Nombre, Telefono, SaldoActual FROM ClientesCtaCte ORDER BY Nombre ASC";
        
        SqlCommand cmd = new SqlCommand(sql, conn);
        conn.Open();
        
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                lista.Add(new Cliente
                {
                    IdCliente = Convert.ToInt32(reader["IdCliente"]),
                    NombreCliente = reader["Nombre"].ToString(),
                    TelefonoCliente = reader["Telefono"]?.ToString(),
                    SaldoActual = Convert.ToDecimal(reader["SaldoActual"])
                });
            }
        }
    }
    return lista;
}

public List<Cliente> ObtenerClientesGenerales()
{
    List<Cliente> lista = new List<Cliente>();
    using (var conn = ObtenerConexion())
    {
        // CAMBIO AQUÍ: Nombre -> NombreCliente, Telefono -> TelefonoCliente
        string sql = "SELECT IdCliente, NombreCliente, TelefonoCliente, DniCliente FROM Clientes ORDER BY NombreCliente ASC";
        
        SqlCommand cmd = new SqlCommand(sql, conn);
        conn.Open();
        
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                lista.Add(new Cliente
                {
                    IdCliente = Convert.ToInt32(reader["IdCliente"]),
                    NombreCliente = reader["NombreCliente"].ToString(),
                    TelefonoCliente = reader["TelefonoCliente"]?.ToString(),
                    DniCliente = reader["DniCliente"]?.ToString() // Agregamos DNI por si lo usás en el modal
                });
            }
        }
    }
    return lista;
}

public bool VerificarYCerrarCajaVencida()
{
    var cajaAbierta = Cajas
        .Where(c => c.EstaAbierta)
        .OrderByDescending(c => c.FechaApertura)
        .FirstOrDefault();

    if (cajaAbierta != null)
    {
        if (cajaAbierta.FechaApertura.Date < DateTime.Today)
        {
            cajaAbierta.EstaAbierta = false;
            cajaAbierta.FechaCierre =FechaHelper.AhoraArgentina();

            Cajas.Update(cajaAbierta);
            SaveChanges();

            return true;
        }
    }

    return false;
}

public void RegistrarMovimientoCaja(int idCaja, decimal monto, string concepto, string tipo)
{
    using (var conn = ObtenerConexion())
    {
        conn.Open();
        // Iniciamos una transacción para que se hagan ambas cosas o ninguna
        using (var trans = conn.BeginTransaction())
        {
            try
            {
                // 1. Insertar el movimiento
                string sqlMov = @"INSERT INTO MovimientosCaja (CajaId, Fecha, Monto, Concepto, Tipo) 
                                 VALUES (@idCaja, GETDATE(), @monto, @concepto, @tipo)";
                
                SqlCommand cmdMov = new SqlCommand(sqlMov, conn, trans);
                cmdMov.Parameters.AddWithValue("@idCaja", idCaja);
                cmdMov.Parameters.AddWithValue("@monto", monto);
                cmdMov.Parameters.AddWithValue("@concepto", concepto);
                cmdMov.Parameters.AddWithValue("@tipo", tipo); // "Ingreso" o "Egreso"
                cmdMov.ExecuteNonQuery();

                // 2. Actualizar el MontoEsperado en la tabla Cajas
                // Si es Ingreso suma, si es Egreso resta
                string operacion = (tipo.ToLower() == "ingreso") ? "+" : "-";
                string sqlCaja = $"UPDATE Cajas SET MontoEsperado = MontoEsperado {operacion} @monto WHERE Id = @idCaja";
                
                SqlCommand cmdCaja = new SqlCommand(sqlCaja, conn, trans);
                cmdCaja.Parameters.AddWithValue("@monto", monto);
                cmdCaja.Parameters.AddWithValue("@idCaja", idCaja);
                cmdCaja.ExecuteNonQuery();

                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
                throw; // Re-lanzamos el error para que el controlador lo atrape
            }
        }
    }
}
public Cliente ObtenerClienteGeneralPorId(int id)
{
    using (var conn = ObtenerConexion())
    {
        // Nombres corregidos: NombreCliente, TelefonoCliente
        string sql = "SELECT IdCliente, NombreCliente, TelefonoCliente FROM Clientes WHERE IdCliente = @id";
        SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        conn.Open();
        
        using (var reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                return new Cliente
                {
                    IdCliente = Convert.ToInt32(reader["IdCliente"]),
                    NombreCliente = reader["NombreCliente"].ToString(),
                    TelefonoCliente = reader["TelefonoCliente"]?.ToString()
                };
            }
        }
    }
    return null;
}
public void RegistrarPagoEnHistorial(int idCliente, decimal monto, string medio) 
{
    using (var conn = ObtenerConexion()) 
    {
        string sql = "INSERT INTO HistorialCtaCte (IdCliente, Fecha, Concepto, Monto, IdProducto, Cantidad) " +
                     "VALUES (@id, @fecha, @conc, @monto, 0, 0)";
        
        var cmd = new SqlCommand(sql, conn);
        
        // Definimos tipos de datos para evitar errores de conversión
        cmd.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = idCliente;
        cmd.Parameters.Add("@fecha", System.Data.SqlDbType.DateTime).Value =FechaHelper.AhoraArgentina();
        cmd.Parameters.Add("@conc", System.Data.SqlDbType.NVarChar).Value = "PAGO (" + medio.ToUpper() + ")";
        
        // Forzamos el tipo Decimal para que no lo confunda con texto
        cmd.Parameters.Add("@monto", System.Data.SqlDbType.Decimal).Value = monto * -1;

        conn.Open();
        cmd.ExecuteNonQuery();
    }
}
public void InsertarClienteCtaCte(string nombre, string telefono, int idOriginal)
{
    using (var conn = ObtenerConexion())
    {
        // Agregamos la columna IdClienteOriginal al INSERT
        string sql = "INSERT INTO ClientesCtaCte (Nombre, Telefono, SaldoActual, IdClienteOriginal) " +
                     "VALUES (@nom, @tel, 0, @idOrig)";
        
        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nom", nombre);
        cmd.Parameters.AddWithValue("@tel", (object)telefono ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@idOrig", idOriginal); // El ID de la tabla dbo.clientes
        
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}

public Cliente ObtenerClienteCtaCtePorId(int id)
{
    using (var conn = ObtenerConexion())
    {
        // Agregamos IdClienteOriginal al SELECT
        string sql = "SELECT IdCliente, Nombre, IdClienteOriginal, SaldoActual FROM ClientesCtaCte WHERE IdCliente = @id";
        SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        conn.Open();
        using (var reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                return new Cliente {
                    IdCliente = Convert.ToInt32(reader["IdCliente"]),
                    NombreCliente = reader["Nombre"].ToString(),
                    SaldoActual = Convert.ToDecimal(reader["SaldoActual"]),
                    // LEEMOS EL NUEVO CAMPO:
                    IdClienteOriginal = reader["IdClienteOriginal"] != DBNull.Value 
                                        ? Convert.ToInt32(reader["IdClienteOriginal"]) 
                                        : 0 
                };
            }
        }
    }
    return null;
}public void ActualizarSaldoClienteCtaCte(int idCliente, decimal monto)
{
    using (var conn = ObtenerConexion())
    {
        string sql = "UPDATE ClientesCtaCte SET SaldoActual = SaldoActual + @monto WHERE IdCliente = @id";
        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@monto", monto); // SQL maneja la conversión de la coma/punto solo
        cmd.Parameters.AddWithValue("@id", idCliente);
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}

public void SobreescribirSaldoCliente(int idCliente, decimal nuevoTotal)
{
    using (var conn = ObtenerConexion())
    {
        string sql = "UPDATE ClientesCtaCte SET SaldoActual = @nuevoTotal WHERE IdCliente = @id";
        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nuevoTotal", nuevoTotal);
        cmd.Parameters.AddWithValue("@id", idCliente);
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}


public List<MovimientoCtaCte> ObtenerHistorialCtaCte(int idCliente)
{
    List<MovimientoCtaCte> lista = new List<MovimientoCtaCte>();
    using (var conn = ObtenerConexion())
    {
        // Agregamos IdProducto y Cantidad a la consulta SQL
        string sql = @"SELECT Id, Fecha, Concepto, Monto, SaldoResultante, IdProducto, Cantidad 
                       FROM HistorialCtaCte 
                       WHERE IdCliente = @id 
                       ORDER BY Fecha DESC";
        
        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", idCliente);
        conn.Open();
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                lista.Add(new MovimientoCtaCte {
                    Id = Convert.ToInt32(reader["Id"]),
                    Fecha = Convert.ToDateTime(reader["Fecha"]),
                    Concepto = reader["Concepto"]?.ToString() ?? "",
                    
                    // Manejo seguro de decimales (si es NULL pone 0)
                    Monto = reader["Monto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Monto"]),
                    SaldoResultante = reader["SaldoResultante"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["SaldoResultante"]),
                    
                    // Columnas necesarias para el recálculo de precios
                    IdProducto = reader["IdProducto"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IdProducto"]),
                    Cantidad = reader["Cantidad"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Cantidad"])
                });
            }
        }
    }
    return lista;
}

// Método para la Libreta
public void RegistrarMovimientoCtaCte(int idCliente, decimal monto, string concepto, decimal saldoRes)
{
    using (var conn = ObtenerConexion())
    {
        string sql = "INSERT INTO HistorialCtaCte (IdCliente, Fecha, Concepto, Monto, SaldoResultante) VALUES (@id, GETDATE(), @con, @mon, @sal)";
        SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", idCliente);
        cmd.Parameters.AddWithValue("@con", concepto);
        cmd.Parameters.AddWithValue("@mon", monto);
        cmd.Parameters.AddWithValue("@sal", saldoRes);
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}
// MÉTODO PARA LA CAJA (Corregido con hora local y parámetros seguros)
public void RegistrarMovimientoEnCajaReal(int idCaja, string concepto, decimal monto, int tipo, string usuario, string medioPago)
{
    using (var conn = ObtenerConexion())
    {
        // 1. Reemplazamos GETDATE() por @fecha
        string sql = @"INSERT INTO MovimientosCaja (CajaId, Fecha, Concepto, Monto, Tipo, Usuario, MedioPago) 
                       VALUES (@cajaId, @fecha, @concepto, @monto, @tipo, @usuario, @medioPago)";
        
        SqlCommand cmd = new SqlCommand(sql, conn);
        
        // 2. Mapeo de parámetros
        cmd.Parameters.AddWithValue("@cajaId", idCaja);
        
        // Enviamos la hora de C# (tu PC) en lugar de la del servidor SQL
        cmd.Parameters.AddWithValue("@fecha", DateTime.Now); 
        
        cmd.Parameters.AddWithValue("@concepto", concepto);
        
        // Forzamos el tipo decimal para el monto por seguridad
        cmd.Parameters.Add("@monto", System.Data.SqlDbType.Decimal).Value = monto;
        
        cmd.Parameters.AddWithValue("@tipo", tipo); // 0 para Ingreso, 1 para Egreso
        cmd.Parameters.AddWithValue("@usuario", usuario);
        cmd.Parameters.AddWithValue("@medioPago", medioPago);
        
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}
public void RestarStockProducto(int idProducto, int cantidad)
{
    using (var conn = ObtenerConexion())
    {
        // Cambié 'Stock' por 'StockActual'. 
        // Si en tu SQL ves que se llama distinto, poné ese nombre aquí:
        string sql = "UPDATE Productos SET StockActual = StockActual - @cant WHERE IdProducto = @id";
        
        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cant", cantidad);
        cmd.Parameters.AddWithValue("@id", idProducto);
        
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}

public dynamic ObtenerProductoPorCodigo(string codigo)
{
    using (var conn = ObtenerConexion())
    {
        // Ajustá 'Productos' y 'Codigo' a los nombres reales de tu tabla
        string sql = "SELECT IdProducto, Nombre, PrecioVenta FROM Productos WHERE Codigo = @cod";
        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cod", codigo);
        conn.Open();

        using (var reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                return new
                {
                    IdProducto = (int)reader["IdProducto"],
                    Nombre = reader["Nombre"].ToString(),
                    PrecioVenta = (decimal)reader["PrecioVenta"]
                };
            }
        }
    }
    return null;
}

// Método para que el saldo del cliente suba en la tabla principal
public void ActualizarSaldoCliente(int idCliente, decimal monto)
{
    using (var conn = ObtenerConexion())
    {
        string sql = "UPDATE Clientes SET SaldoActual = SaldoActual + @monto WHERE IdCliente = @id";
        var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@monto", monto);
        cmd.Parameters.AddWithValue("@id", idCliente);
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}        public Cliente ObtenerClientePorId(int id)
        {
            Cliente cliente = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT idCliente, dniCliente, nombreCliente,domicilio, localidad,telefonoCliente, SaldoActual FROM Clientes WHERE idCliente = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    cliente = new Cliente
                    {
                        IdCliente = Convert.ToInt32(reader["idCliente"]),
                        DniCliente = reader["dniCliente"].ToString(),
                        NombreCliente = reader["nombreCliente"].ToString(),
                        Domicilio = reader["domicilio"].ToString(),
                        Localidad = reader["localidad"].ToString(),
                        TelefonoCliente = reader["telefonoCliente"].ToString(),
                        SaldoActual = reader["SaldoActual"] != DBNull.Value ? Convert.ToDecimal(reader["SaldoActual"]) : 0m
                    };
                }
            }
            return cliente;
        }

        public void AgregarCliente(Cliente cliente)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Clientes (dniCliente, nombreCliente, domicilio, localidad, telefonoCliente) VALUES (@dni, @nombre, @domicilio, @localidad, @telefonoCliente)", conn);
                cmd.Parameters.AddWithValue("@dni", cliente.DniCliente);
                cmd.Parameters.AddWithValue("@nombre", cliente.NombreCliente);
                cmd.Parameters.AddWithValue("@domicilio", cliente.Domicilio);
                cmd.Parameters.AddWithValue("@localidad", cliente.Localidad);
                cmd.Parameters.AddWithValue("@telefonoCliente", cliente.TelefonoCliente);
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarCliente(Cliente cliente)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Clientes SET dniCliente=@dni, nombreCliente=@nombre, domicilio=@domicilio, localidad=@localidad, telefonoCliente=@telefonoCliente WHERE idCliente=@id", conn);
                cmd.Parameters.AddWithValue("@dni", cliente.DniCliente);
                cmd.Parameters.AddWithValue("@nombre", cliente.NombreCliente);
                cmd.Parameters.AddWithValue("@domicilio", cliente.Domicilio);
                cmd.Parameters.AddWithValue("@localidad", cliente.Localidad);
                cmd.Parameters.AddWithValue("@telefonoCliente", cliente.TelefonoCliente);
                cmd.Parameters.AddWithValue("@id", cliente.IdCliente);
                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarCliente(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM Clientes WHERE idCliente=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }


        //--------------------- PRESUPUESTO -------------------------------------

        public List<Presupuesto> ObtenerPresupuestos()
        {
            var lista = new List<Presupuesto>();

            using (var conn = ObtenerConexion())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Presupuesto ORDER BY Fecha DESC";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Presupuesto
                        {
                            IdPresupuesto = Convert.ToInt32(reader["IdPresupuesto"]),
                            NombreCliente = reader["NombreCliente"].ToString(),
                            TelefonoCliente = reader["TelefonoCliente"].ToString(),
                            Fecha = Convert.ToDateTime(reader["Fecha"])
                        });
                    }
                }
            }

            return lista;
        }

        public void EliminarPresupuesto(int id)
        {
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Presupuesto WHERE IdPresupuesto = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public Presupuesto ObtenerPresupuestoPorId(int id)
        {
            Presupuesto presupuesto = null;

            using (var conn = ObtenerConexion())
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Presupuesto WHERE IdPresupuesto = @id";
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        presupuesto = new Presupuesto
                        {
                            IdPresupuesto = Convert.ToInt32(reader["IdPresupuesto"]),
                            NombreCliente = reader["NombreCliente"].ToString(),
                            TelefonoCliente = reader["TelefonoCliente"].ToString(),
                            Fecha = Convert.ToDateTime(reader["Fecha"])
                        };
                    }
                }

                if (presupuesto != null)
                {
                    presupuesto.Items = new List<PresupuestoItem>();
                    cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM PresupuestoItem WHERE IdPresupuesto = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            presupuesto.Items.Add(new PresupuestoItem
                            {
                                IdItem = Convert.ToInt32(reader["IdItem"]),
                                IdPresupuesto = id,
                                Nombre = reader["Nombre"].ToString(),
                                Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"])
                            });
                        }
                    }
                }
            }

            return presupuesto;
        }






        public void AgregarPresupuesto(Presupuesto presupuesto)
        {
            using (var conexion = ObtenerConexion())
            {
                conexion.Open();
                using (var transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // Insertar presupuesto
                        //var queryPresupuesto = "INSERT INTO Presupuesto (NombreCliente, TelefonoCliente, Fecha) VALUES (@nombre, @telefono, @fecha); SELECT LAST_INSERT_ID();";
                        var queryPresupuesto = "INSERT INTO Presupuesto (NombreCliente, TelefonoCliente, Fecha) VALUES (@nombre, @telefono, @fecha); SELECT SCOPE_IDENTITY();";
                        using (var comando = new SqlCommand(queryPresupuesto, conexion, transaccion))
                        {
                            comando.Parameters.AddWithValue("@nombre", presupuesto.NombreCliente);
                            //comando.Parameters.AddWithValue("@telefono", presupuesto.TelefonoCliente);
                            comando.Parameters.AddWithValue("@telefono", (object)presupuesto.TelefonoCliente ?? DBNull.Value);
                            comando.Parameters.AddWithValue("@fecha", presupuesto.Fecha);

                            presupuesto.IdPresupuesto = Convert.ToInt32(comando.ExecuteScalar());
                        }

                        // Insertar items
                        foreach (var item in presupuesto.Items)
                        {
                            var queryItem = "INSERT INTO PresupuestoItem (IdPresupuesto, Nombre, Cantidad, PrecioUnitario) VALUES (@idPresupuesto, @nombre, @cantidad, @precio);";
                            using (var cmdItem = new SqlCommand(queryItem, conexion, transaccion))
                            {
                                cmdItem.Parameters.AddWithValue("@idPresupuesto", presupuesto.IdPresupuesto);
                                cmdItem.Parameters.AddWithValue("@nombre", item.Nombre);
                                cmdItem.Parameters.AddWithValue("@cantidad", item.Cantidad);

                                // --- SOLUCIÓN DEFINITIVA ---
                                // Ya no usamos strings ni replaces. 
                                // Como el Controlador ya arregló el 'item.PrecioUnitario', lo mandamos como Decimal puro.
                                // Usamos .Add para especificar SqlDbType.Decimal y evitar que SQL se confunda.

                                cmdItem.Parameters.Add("@precio", System.Data.SqlDbType.Decimal).Value = item.PrecioUnitario;

                                // ---------------------------

                                cmdItem.ExecuteNonQuery();
                            }
                        }

                        transaccion.Commit();
                    }
                    catch (Exception)
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

// Asegurate de que el nombre aquí sea "Presupuesto" (singular)
public void ActualizarTelefonoPresupuesto(int id, string nuevoTelefono)
{
    // Ejecutamos una sentencia SQL directa para no pasar por el validador de llaves de EF
    string sql = "UPDATE presupuesto SET telefonoCliente = @p0 WHERE idPresupuesto = @p1";
    this.Database.ExecuteSqlRaw(sql, nuevoTelefono, id);
}
        // 1. VENTAS POR FECHA (DIARIO)
        public List<object> ObtenerTotalVentasPorFecha(DateTime desde, DateTime hasta)
        {
            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                // Usamos CAST en el WHERE para que SQL no se confunda con las horas del servidor
                var cmd = new SqlCommand(@"
            SELECT CAST(diaVenta AS DATE) as fecha, SUM(montoVenta) as totalVendido
            FROM facturas
            WHERE CAST(diaVenta AS DATE) >= CAST(@desde AS DATE) 
              AND CAST(diaVenta AS DATE) <= CAST(@hasta AS DATE)
            GROUP BY CAST(diaVenta AS DATE)
            ORDER BY fecha", conn);

                // Enviamos las fechas tal cual vienen de la vista
                cmd.Parameters.AddWithValue("@desde", desde);
                cmd.Parameters.AddWithValue("@hasta", hasta);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new
                    {
                        fecha = Convert.ToDateTime(reader["fecha"]),
                        totalVendido = Convert.ToDecimal(reader["totalVendido"])
                    });
                }
            }
            return lista;
        }

        // 2. VENTAS POR MES
        public List<object> ObtenerTotalVentasPorMes(DateTime desde, DateTime hasta)
        {
            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                var cmd = new SqlCommand(@"
            SELECT 
                DATEFROMPARTS(YEAR(diaVenta), MONTH(diaVenta), 1) as fecha, 
                SUM(montoVenta) as totalVendido
            FROM facturas
            WHERE diaVenta BETWEEN @desde AND @hasta
            GROUP BY YEAR(diaVenta), MONTH(diaVenta)
            ORDER BY fecha", conn);

                cmd.Parameters.AddWithValue("@desde", desde);
                cmd.Parameters.AddWithValue("@hasta", hasta.Date.AddDays(1).AddTicks(-1));

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new
                    {
                        fecha = Convert.ToDateTime(reader["fecha"]),
                        totalVendido = Convert.ToDecimal(reader["totalVendido"])
                    });
                }
            }
            return lista;
        }

        // 3. VENTAS POR AÑO
        public List<object> ObtenerTotalVentasPorAnio(DateTime desde, DateTime hasta)
        {
            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                var cmd = new SqlCommand(@"
            SELECT 
                DATEFROMPARTS(YEAR(diaVenta), 1, 1) as fecha, 
                SUM(montoVenta) as totalVendido
            FROM facturas
            WHERE diaVenta BETWEEN @desde AND @hasta
            GROUP BY YEAR(diaVenta)
            ORDER BY fecha", conn);

                cmd.Parameters.AddWithValue("@desde", desde);
                cmd.Parameters.AddWithValue("@hasta", hasta.Date.AddDays(1).AddTicks(-1));

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new
                    {
                        fecha = Convert.ToDateTime(reader["fecha"]),
                        totalVendido = Convert.ToDecimal(reader["totalVendido"])
                    });
                }
            }
            return lista;
        }


        //---- USUARIOS ------

        public Usuario ObtenerUsuarioPorNombre(string nombreUsuario)
        {
            Usuario u = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT IdUsuario, Usuario, NombreYApellido, Rol, Contraseña, FotoPerfil FROM Usuarios WHERE Usuario = @nombre";

                using (var command = new SqlCommand(query, connection))
                {
                    // Nota: Se recomienda usar Add para tipado más estricto, pero AddWithValue funciona
                    command.Parameters.AddWithValue("@nombre", nombreUsuario);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            u = new Usuario
                            {
                                // ❌ ANTES (Error): IdUsuario = reader.GetInt32("IdUsuario"),
                                // ✅ AHORA: Usamos GetOrdinal para obtener el índice y pasarlo a GetInt32
                                IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario")),

                                // ✅ AHORA: Usamos GetOrdinal para los strings también, aunque hay otra forma
                                UsuarioNombre = reader.GetString(reader.GetOrdinal("Usuario")),
                                NombreyApellido = reader.GetString(reader.GetOrdinal("NombreYApellido")),
                                Rol = reader.GetString(reader.GetOrdinal("Rol")),
                                Contraseña = reader.GetString(reader.GetOrdinal("Contraseña")),

                                // Mantenemos la lógica de verificación de nulos (que es correcta aquí)
                                FotoPerfil = reader["FotoPerfil"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("FotoPerfil")) : null
                            };
                            Console.WriteLine("Ruta de la maldita foto: " + u.FotoPerfil);
                        }
                    }
                }
            }
            return u;
        }

        public void ActualizarRutaFoto(Usuario u)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Usuarios SET FotoPerfil = @fotoperfil WHERE idUsuario = @idUsuario";
                using (var command = new SqlCommand(query, connection))
                {
                    Console.WriteLine("ID del usuario: " + u.IdUsuario);

                    command.Parameters.AddWithValue("@fotoPerfil", (object?)u.FotoPerfil ?? DBNull.Value);
                    command.Parameters.AddWithValue("@idUsuario", u.IdUsuario);
                    command.ExecuteNonQuery();
                }
            }
        }


        public Usuario ObtenerUsuarioPorId(int idUsuario)
        {
            Usuario usuario = null;
            using (var con = ObtenerConexion())
            {
                con.Open();
                var cmd = new SqlCommand("SELECT * FROM usuarios WHERE idUsuario = @idUsuario", con);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    usuario = new Usuario
                    {
                        IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                        UsuarioNombre = reader["Usuario"].ToString(),
                        Contraseña = reader["contraseña"].ToString(),
                        Rol = reader["rol"].ToString(),
                        NombreyApellido = reader["nombreyApellido"].ToString(),
                        FotoPerfil = reader["FotoPerfil"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("FotoPerfil")) : null
                    };
                }
            }
            return usuario;
        }

        public void AgregarUsuario(Usuario u)
        {
            using (var con = ObtenerConexion())
            {
                con.Open();
                var cmd = new SqlCommand("INSERT INTO usuarios (Usuario, contraseña, rol, nombreyApellido, FotoPerfil) VALUES (@nombre,  @contraseña, @rol, @nombreyApellido, @FotoPerfil)", con);
                cmd.Parameters.AddWithValue("@nombre", u.UsuarioNombre);
                cmd.Parameters.AddWithValue("@contraseña", u.Contraseña);
                cmd.Parameters.AddWithValue("@rol", u.Rol);
                cmd.Parameters.AddWithValue("@nombreyApellido", u.NombreyApellido);
                cmd.Parameters.AddWithValue("@FotoPerfil", u.FotoPerfil);
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarUsuario(Usuario u)
        {
            using (var con = ObtenerConexion())
            {
                con.Open();
                var cmd = new SqlCommand("UPDATE usuarios SET Usuario=@nombre, contraseña=@contraseña, rol=@rol, nombreyApellido=@nombreyApellido, FotoPerfil=@FotoPerfil WHERE idUsuario=@idUsuario", con);

                cmd.Parameters.AddWithValue("@idUsuario", u.IdUsuario);
                cmd.Parameters.AddWithValue("@nombre", u.UsuarioNombre);
                cmd.Parameters.AddWithValue("@contraseña", u.Contraseña);
                cmd.Parameters.AddWithValue("@rol", u.Rol);
                cmd.Parameters.AddWithValue("@nombreyApellido", u.NombreyApellido);
                cmd.Parameters.AddWithValue("@FotoPerfil", u.FotoPerfil);
                cmd.ExecuteNonQuery();
            }
        }

        public void EliminarUsuario(int idUsuario)
        {
            using (var con = ObtenerConexion())
            {
                con.Open();
                var cmd = new SqlCommand("DELETE FROM usuarios WHERE idUsuario=@id", con);
                cmd.Parameters.AddWithValue("@id", idUsuario);
                cmd.ExecuteNonQuery();
            }
        }



        public void ActualizarClave(Usuario u)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Usuarios SET contraseña = @contraseña WHERE IdUsuario = @id";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@contraseña", u.Contraseña);
                    command.Parameters.AddWithValue("@id", u.IdUsuario);
                    command.ExecuteNonQuery();
                }
            }
        }



        public List<Usuario> ObtenerUsuarios()
        {
            List<Usuario> lista = new List<Usuario>();
            using (var con = ObtenerConexion())
            {
                con.Open();
                var cmd = new SqlCommand("SELECT idUsuario, Usuario, contraseña, rol, nombreyApellido, FotoPerfil FROM Usuarios", con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Usuario
                    {
                        IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                        UsuarioNombre = reader["Usuario"].ToString(),
                        Contraseña = reader["contraseña"].ToString(),
                        Rol = reader["rol"].ToString(),
                        NombreyApellido = reader["nombreyApellido"].ToString(),
                        FotoPerfil = !reader.IsDBNull(reader.GetOrdinal("FotoPerfil")) ? reader["FotoPerfil"].ToString() : null
                    });
                }
            }

            //  Asegurar la foto por defecto si está vacía
            foreach (var u in lista)
            {
                if (string.IsNullOrEmpty(u.FotoPerfil))
                {
                    u.FotoPerfil = "/imagenes/usuarios/default.png";
                }
            }

            return lista;
        }




        public Usuario BuscarUsuario(string usuario, string contraseña)
        {
            Usuario encontrado = null;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                string sql = "SELECT * FROM usuarios WHERE usuario = @usuario AND contraseña = @contraseña";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    cmd.Parameters.AddWithValue("@contraseña", contraseña);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            encontrado = new Usuario
                            {
                                IdUsuario = reader.GetInt32(reader.GetOrdinal("idUsuario")),
                                UsuarioNombre = reader.GetString(reader.GetOrdinal("Usuario")),
                                Rol = reader.GetString(reader.GetOrdinal("rol")),
                                NombreyApellido = reader.GetString(reader.GetOrdinal("nombreyApellido")),
                                FotoPerfil = reader.GetString(reader.GetOrdinal("fotoPerfil")),
                            };
                        }
                    }
                }
            }
            return encontrado;
        }

        public void EditarUsuario(Usuario usuario)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var sql = @"UPDATE Usuario SET 
                        UsuarioNombre = @usuario, 
                        NombreyApellido = @nombre, 
                        Contraseña = @pass, 
                        Rol = @rol, 
                        FotoPerfil = @foto
                    WHERE IdUsuario = @id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", usuario.UsuarioNombre);
                    cmd.Parameters.AddWithValue("@nombre", usuario.NombreyApellido);
                    cmd.Parameters.AddWithValue("@pass", usuario.Contraseña);
                    cmd.Parameters.AddWithValue("@rol", usuario.Rol);
                    cmd.Parameters.AddWithValue("@foto", usuario.FotoPerfil ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", usuario.IdUsuario);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        //-------------------------------PRODUCTOS-----------------------------
        public List<Productos> ObtenerProductos()
        {
            List<Productos> productos = new List<Productos>();

            using (SqlConnection conn = ObtenerConexion())
            {
                conn.Open();

                // La consulta SQL explícita es crucial (sin PrecioVenta, ya que es calculado en C#).
                string sql = @"
            SELECT 
                IdProducto, Codigo, Nombre, Descripcion, Categoria, 
                PrecioCosto, RecargoPorcentaje, 
                StockActual, StockMinimo, NombreProveedor, Imagen 
            FROM Productos";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    // --- 1. Obtener los índices de las columnas (Ordinales) ---
                    int ordIdProducto = reader.GetOrdinal("IdProducto");
                    int ordCodigo = reader.GetOrdinal("Codigo");
                    int ordNombre = reader.GetOrdinal("Nombre");
                    int ordDescripcion = reader.GetOrdinal("Descripcion");
                    int ordCategoria = reader.GetOrdinal("Categoria");
                    int ordPrecioCosto = reader.GetOrdinal("PrecioCosto");
                    int ordRecargoPorcentaje = reader.GetOrdinal("RecargoPorcentaje");
                    int ordStockActual = reader.GetOrdinal("StockActual");
                    int ordStockMinimo = reader.GetOrdinal("StockMinimo");
                    int ordNombreProveedor = reader.GetOrdinal("NombreProveedor");
                    int ordImagen = reader.GetOrdinal("Imagen");

                    while (reader.Read())
                    {
                        productos.Add(new Productos
                        {
                            IdProducto = reader.GetInt32(ordIdProducto),
                            Codigo = reader.GetString(ordCodigo),
                            Nombre = reader.GetString(ordNombre),

                            // Manejo de nulos para strings
                            Descripcion = reader.IsDBNull(ordDescripcion) ? null : reader.GetString(ordDescripcion),
                            Categoria = reader.IsDBNull(ordCategoria) ? null : reader.GetString(ordCategoria),

                            // CORRECCIÓN CRÍTICA: Uso de GetValue y Convert.ToDecimal para evitar InvalidCastException (INT a DECIMAL)
                            PrecioCosto = reader.IsDBNull(ordPrecioCosto) ? 0.00M : Convert.ToDecimal(reader.GetValue(ordPrecioCosto)),
                            RecargoPorcentaje = reader.IsDBNull(ordRecargoPorcentaje) ? 0.00M : Convert.ToDecimal(reader.GetValue(ordRecargoPorcentaje)),

                            // Manejo de nulos para enteros
                            StockActual = reader.IsDBNull(ordStockActual) ? 0 : reader.GetInt32(ordStockActual),
                            StockMinimo = reader.IsDBNull(ordStockMinimo) ? 0 : reader.GetInt32(ordStockMinimo),

                            NombreProveedor = reader.IsDBNull(ordNombreProveedor) ? null : reader.GetString(ordNombreProveedor),
                            Imagen = reader.IsDBNull(ordImagen) ? null : reader.GetString(ordImagen)
                        });
                    }
                }
            }

            return productos;
        }

        //-------------------CREAR PRODUCTOS-------------------
        public void AgregarProducto(Productos p)
        {
            using (var con = ObtenerConexion())
            {
                con.Open();
                string query = @"INSERT INTO Productos 
        (codigo, nombre, descripcion, categoria, precioCosto, recargoPorcentaje, precioVenta, stockActual, stockMinimo, nombreProveedor, imagen) 
        VALUES 
        (@codigo, @nombre, @descripcion, @categoria, @precioCosto, @recargoPorcentaje, @precioVenta, @stockActual, @stockMinimo, @nombreProveedor, @imagen)";

                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@codigo", p.Codigo);
                    cmd.Parameters.AddWithValue("@nombre", p.Nombre);
                    cmd.Parameters.AddWithValue("@descripcion", p.Descripcion);
                    cmd.Parameters.AddWithValue("@categoria", p.Categoria);
                    cmd.Parameters.AddWithValue("@precioCosto", p.PrecioCosto);
                    cmd.Parameters.AddWithValue("@recargoPorcentaje", p.RecargoPorcentaje);
                    cmd.Parameters.AddWithValue("@precioVenta", p.PrecioVenta);
                    cmd.Parameters.AddWithValue("@stockActual", p.StockActual);
                    cmd.Parameters.AddWithValue("@stockMinimo", p.StockMinimo);
                    cmd.Parameters.AddWithValue("@nombreProveedor", p.NombreProveedor);
                    cmd.Parameters.AddWithValue("@imagen", p.Imagen ?? "");

                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void EliminarProducto(int idProducto)
        {
            using (var conexion = ObtenerConexion())
            {
                conexion.Open();

                string consulta = "DELETE FROM Productos WHERE IdProducto = @IdProducto";

                using (var comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@IdProducto", idProducto);
                    comando.ExecuteNonQuery();
                }
            }
        }


        public Productos ObtenerProductoPorId(int id)
        {
            Productos prod = null;

            using (var conexion = ObtenerConexion())
            {
                conexion.Open();

                // La consulta SQL es explícita y excluye PrecioVenta (porque es calculada)
                string sql = @"
            SELECT 
                IdProducto, Codigo, Nombre, Descripcion, Categoria, 
                PrecioCosto, RecargoPorcentaje, 
                StockActual, StockMinimo, NombreProveedor, Imagen 
            FROM Productos p 
            WHERE p.IdProducto = @id";

                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", id);
                    using (var lector = comando.ExecuteReader())
                    {
                        if (lector.Read())
                        {
                            // --- 1. Obtener Ordinales ---
                            int ordIdProducto = lector.GetOrdinal("IdProducto");
                            int ordCodigo = lector.GetOrdinal("Codigo");
                            int ordNombre = lector.GetOrdinal("Nombre");
                            int ordCategoria = lector.GetOrdinal("Categoria");
                            int ordDescripcion = lector.GetOrdinal("Descripcion");
                            int ordPrecioCosto = lector.GetOrdinal("PrecioCosto");
                            int ordRecargoPorcentaje = lector.GetOrdinal("RecargoPorcentaje");
                            int ordStockActual = lector.GetOrdinal("StockActual");
                            int ordStockMinimo = lector.GetOrdinal("StockMinimo");
                            int ordNombreProveedor = lector.GetOrdinal("NombreProveedor");
                            int ordImagen = lector.GetOrdinal("Imagen");

                            prod = new Productos
                            {
                                // Lectura de campos básicos
                                IdProducto = lector.GetInt32(ordIdProducto),
                                Codigo = lector.GetString(ordCodigo),
                                Nombre = lector.GetString(ordNombre),

                                // Manejo de nulos (Strings)
                                Categoria = lector.IsDBNull(ordCategoria) ? null : lector.GetString(ordCategoria),
                                Descripcion = lector.IsDBNull(ordDescripcion) ? null : lector.GetString(ordDescripcion),

                                // CORRECCIÓN CRÍTICA (INT a DECIMAL): Uso de GetValue y Convert.ToDecimal
                                PrecioCosto = lector.IsDBNull(ordPrecioCosto) ? 0.00M : Convert.ToDecimal(lector.GetValue(ordPrecioCosto)),
                                RecargoPorcentaje = lector.IsDBNull(ordRecargoPorcentaje) ? 0.00M : Convert.ToDecimal(lector.GetValue(ordRecargoPorcentaje)),
                                // NOTA: PrecioVenta no se asigna, ya que es calculado en el modelo.

                                // Manejo de nulos (Enteros)
                                StockActual = lector.IsDBNull(ordStockActual) ? 0 : lector.GetInt32(ordStockActual),
                                StockMinimo = lector.IsDBNull(ordStockMinimo) ? 0 : lector.GetInt32(ordStockMinimo),

                                NombreProveedor = lector.IsDBNull(ordNombreProveedor) ? null : lector.GetString(ordNombreProveedor),
                                Imagen = lector.IsDBNull(ordImagen) ? null : lector.GetString(ordImagen)
                            };
                        }
                    }
                }
            }

            return prod;
        }


        public void ActualizarProducto(Productos producto)
        {
            using (var conexion = new SqlConnection(_connectionString))
            {
                conexion.Open();
                string sql = @"UPDATE Productos SET 
                        Codigo = @Codigo,
                        Nombre = @Nombre,
                        Categoria = @Categoria,
                        Descripcion = @Descripcion,
                        PrecioCosto = @PrecioCosto,
                        RecargoPorcentaje = @RecargoPorcentaje,
                        StockActual = @StockActual,
                        StockMinimo = @StockMinimo,
                        nombreProveedor = @nombreProveedor,
                        Imagen = @Imagen
                      WHERE IdProducto = @IdProducto";

                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@Codigo", producto.Codigo);
                    comando.Parameters.AddWithValue("@Nombre", producto.Nombre);
                    comando.Parameters.AddWithValue("@Categoria", producto.Categoria);
                    comando.Parameters.AddWithValue("@Descripcion", producto.Descripcion);
                    comando.Parameters.AddWithValue("@PrecioCosto", producto.PrecioCosto);
                    comando.Parameters.AddWithValue("@RecargoPorcentaje", producto.RecargoPorcentaje);
                    // comando.Parameters.AddWithValue("@PrecioVenta", producto.PrecioVenta);
                    comando.Parameters.AddWithValue("@StockActual", producto.StockActual);
                    comando.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                    comando.Parameters.AddWithValue("@NombreProveedor", producto.NombreProveedor);
                    comando.Parameters.AddWithValue("@Imagen", producto.Imagen ?? (object)DBNull.Value);
                    comando.Parameters.AddWithValue("@IdProducto", producto.IdProducto);

                    comando.ExecuteNonQuery();
                }
            }
        }



        //----------------------------OBTIENE EL ULTIMO NRO DE PRESUPUESTO---------------------------

        public int ObtenerMaximoNroPresupuesto()
        {
            int max = 0;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT MAX(idPresupuesto) FROM Presupuesto";
                using (var cmd = new SqlCommand(query, connection))
                {
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        max = Convert.ToInt32(result);
                    }
                }
            }
            return max;
        }



        //------------------------------ STOCK ---------------
        public void RestarStock(int idProducto, int cantidadVendida)
        {
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                string query = "UPDATE productos SET stockActual = stockActual - @cantidad WHERE idProducto = @id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@cantidad", cantidadVendida);
                    cmd.Parameters.AddWithValue("@id", idProducto);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //------------------- VENTAS --------------------
    public (bool success, int idFactura, string error) RegistrarVenta(VentaCompleta venta)
{
    Console.WriteLine("🚀 RegistrarVenta iniciado.");
    using (var conn = ObtenerConexion())
    {
        conn.Open();

        using (var transaction = conn.BeginTransaction())
        {
            try
            {
                int idFactura = 0;

                // 1. DETERMINAR EL ESTADO DE LA FACTURA
                bool esCtaCte = venta.MedioPago.Equals("CtaCte", StringComparison.OrdinalIgnoreCase) || 
                               venta.MedioPago.Equals("Cuenta Corriente", StringComparison.OrdinalIgnoreCase);
                
                string estadoInicial = esCtaCte ? "Pendiente" : "Cobrada";

                // 2. INSERTAR FACTURA (Agregamos la columna 'descuento')
                string insertFactura = @"
                    INSERT INTO facturas (diaVenta, montoVenta, vendedor, idCliente, medioPago, tipoVenta, estado, descuento)
                    VALUES (@diaVenta, @montoVenta, @vendedor, @idCliente, @medioPago, @tipoVenta, @estado, @descuento);
                    SELECT SCOPE_IDENTITY();";

                using (var cmdInsert = new SqlCommand(insertFactura, conn, transaction))
                {
                    cmdInsert.Parameters.AddWithValue("@diaVenta", DateTime.Now);
                    cmdInsert.Parameters.AddWithValue("@montoVenta", venta.MontoVenta);
                    cmdInsert.Parameters.AddWithValue("@vendedor", venta.Vendedor ?? (object)DBNull.Value);
                    cmdInsert.Parameters.AddWithValue("@idCliente", venta.IdCliente);
                    cmdInsert.Parameters.AddWithValue("@medioPago", venta.MedioPago);
                    cmdInsert.Parameters.AddWithValue("@tipoVenta", venta.TipoVenta);
                    cmdInsert.Parameters.AddWithValue("@estado", estadoInicial);
                    
                    // Manejo del descuento: si es 0 o nulo, grabamos DBNull
                    cmdInsert.Parameters.AddWithValue("@descuento", (venta.Descuento.HasValue && venta.Descuento > 0) 
                        ? (object)venta.Descuento.Value 
                        : DBNull.Value);

                    idFactura = Convert.ToInt32(cmdInsert.ExecuteScalar());
                }

                // 3. INSERTAR ITEMS Y ACTUALIZAR STOCK
                foreach (var item in venta.Items)
                {
                    string insertItem = @"
                        INSERT INTO facturaitem (idFactura, idItem, nombreProd, cantidad, precio)
                        VALUES (@idFactura, @idItem, @nombreProd, @cantidad, @precio);";

                    using (var cmdItem = new SqlCommand(insertItem, conn, transaction))
                    {
                        cmdItem.Parameters.AddWithValue("@idFactura", idFactura);
                        cmdItem.Parameters.AddWithValue("@idItem", item.IdProducto);
                        cmdItem.Parameters.AddWithValue("@nombreProd", item.NombreProd ?? "Producto");
                        cmdItem.Parameters.AddWithValue("@cantidad", item.Cantidad);
                        cmdItem.Parameters.AddWithValue("@precio", item.Precio);
                        cmdItem.ExecuteNonQuery();
                    }

                    string restarStock = "UPDATE productos SET stockActual = stockActual - @cantidad WHERE idProducto = @id";
                    using (var cmdStock = new SqlCommand(restarStock, conn, transaction))
                    {
                        cmdStock.Parameters.AddWithValue("@cantidad", item.Cantidad);
                        cmdStock.Parameters.AddWithValue("@id", item.IdProducto);
                        cmdStock.ExecuteNonQuery();
                    }
                }

                // 4. ACTUALIZAR SALDO CLIENTE (Solo si es CtaCte)
                if (esCtaCte)
                {
                    string sqlSaldo = "UPDATE Clientes SET SaldoActual = ISNULL(SaldoActual, 0) + @monto WHERE idCliente = @idCli";
                    using (var cmdSaldo = new SqlCommand(sqlSaldo, conn, transaction))
                    {
                        cmdSaldo.Parameters.AddWithValue("@monto", venta.MontoVenta);
                        cmdSaldo.Parameters.AddWithValue("@idCli", venta.IdCliente);
                        cmdSaldo.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                return (true, idFactura, "");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine("❌ Error en DB: " + ex.Message);
                return (false, 0, ex.Message);
            }
        }
    }
}


public int ObtenerIdCajaAbierta()
{
    using (var conn = ObtenerConexion())
    {
        string sql = "SELECT TOP 1 Id FROM Cajas WHERE EstaAbierta = 1 ORDER BY FechaApertura DESC";
        conn.Open();
        using (var cmd = new SqlCommand(sql, conn))
        {
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}

public (bool success, string error) LiquidarCuentaCorriente(int idCliente, decimal montoTotal, int idCajaAbierta)
{
    using (var conn = ObtenerConexion())
    {
        conn.Open();
        using (var transaction = conn.BeginTransaction())
        {
            try
            {
                // 1. Cambiamos todas las facturas 'Pendiente' a 'Cobrada' para este cliente
                string sqlUpdateFacturas = @"UPDATE facturas 
                                           SET estado = 'Cobrada' 
                                           WHERE idCliente = @idCli AND estado = 'Pendiente'";
                
                using (var cmd = new SqlCommand(sqlUpdateFacturas, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@idCli", idCliente);
                    cmd.ExecuteNonQuery();
                }

                // 2. Ponemos el Saldo del Cliente en 0
                string sqlUpdateSaldo = "UPDATE Clientes SET SaldoActual = 0 WHERE idCliente = @idCli";
                using (var cmdSaldo = new SqlCommand(sqlUpdateSaldo, conn, transaction))
                {
                    cmdSaldo.Parameters.AddWithValue("@idCli", idCliente);
                    cmdSaldo.ExecuteNonQuery();
                }

                // 3. Registramos el ingreso total en los Movimientos de Caja
                string sqlMovCaja = @"INSERT INTO MovimientosCaja (CajaId, Fecha, Monto, Concepto, Tipo) 
                                     VALUES (@cajaId, @fecha, @monto, @concepto, @tipo)";
                
                using (var cmdCaja = new SqlCommand(sqlMovCaja, conn, transaction))
                {
                    cmdCaja.Parameters.AddWithValue("@cajaId", idCajaAbierta);
                    cmdCaja.Parameters.AddWithValue("@fecha", DateTime.Now);
                    cmdCaja.Parameters.AddWithValue("@monto", montoTotal);
                    cmdCaja.Parameters.AddWithValue("@concepto", "Cobro Total Cta. Cte. Cliente ID: " + idCliente);
                    cmdCaja.Parameters.AddWithValue("@tipo", 1); // 1 = Ingreso
                    cmdCaja.ExecuteNonQuery();
                }

                // 4. Actualizamos el MontoEsperado en la tabla Cajas
                string sqlUpdateCaja = "UPDATE Cajas SET MontoEsperado = MontoEsperado + @monto WHERE Id = @cajaId";
                using (var cmdUpdCaja = new SqlCommand(sqlUpdateCaja, conn, transaction))
                {
                    cmdUpdCaja.Parameters.AddWithValue("@monto", montoTotal);
                    cmdUpdCaja.Parameters.AddWithValue("@cajaId", idCajaAbierta);
                    cmdUpdCaja.ExecuteNonQuery();
                }

                transaction.Commit();
                return (true, "");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return (false, ex.Message);
            }
        }
    }
}


        public List<object> ObtenerBalanceMovimientosPorFecha(DateTime desde, DateTime hasta)
        {
            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                string sql = @"SELECT CAST(Fecha AS DATE), SUM(CASE WHEN Tipo = 0 THEN Monto ELSE -Monto END)
                       FROM MovimientosCaja
                       WHERE CAST(Fecha AS DATE) BETWEEN @desde AND @hasta AND Concepto NOT LIKE '%Apertura%'
                       GROUP BY CAST(Fecha AS DATE) ORDER BY CAST(Fecha AS DATE) ASC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@desde", desde.Date);
                    cmd.Parameters.AddWithValue("@hasta", hasta.Date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new
                            {
                                Fecha = Convert.ToDateTime(reader[0]).ToString("yyyy-MM-dd"),
                                Total = Convert.ToDecimal(reader[1])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public List<object> ObtenerBalanceMovimientosPorMes(DateTime desde, DateTime hasta)
        {
            // Forzamos rango del mes completo
            DateTime inicio = new DateTime(desde.Year, desde.Month, 1);
            DateTime fin = new DateTime(hasta.Year, hasta.Month, 1).AddMonths(1).AddDays(-1);

            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                string sql = @"SELECT MONTH(Fecha), YEAR(Fecha), SUM(CASE WHEN Tipo = 0 THEN Monto ELSE -Monto END)
                       FROM MovimientosCaja
                       WHERE CAST(Fecha AS DATE) BETWEEN @desde AND @hasta AND Concepto NOT LIKE '%Apertura%'
                       GROUP BY YEAR(Fecha), MONTH(Fecha) ORDER BY YEAR(Fecha), MONTH(Fecha)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@desde", inicio);
                    cmd.Parameters.AddWithValue("@hasta", fin);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new
                            {
                                Fecha = reader[0].ToString().PadLeft(2, '0') + "/" + reader[1].ToString(),
                                Total = Convert.ToDecimal(reader[2])
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public List<object> ObtenerBalanceMovimientosPorAnio(DateTime desde, DateTime hasta)
        {
            // Forzamos rango del año completo
            DateTime inicio = new DateTime(desde.Year, 1, 1);
            DateTime fin = new DateTime(hasta.Year, 12, 31);

            List<object> lista = new List<object>();
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                string sql = @"SELECT YEAR(Fecha), SUM(CASE WHEN Tipo = 0 THEN Monto ELSE -Monto END)
                       FROM MovimientosCaja
                       WHERE CAST(Fecha AS DATE) BETWEEN @desde AND @hasta AND Concepto NOT LIKE '%Apertura%'
                       GROUP BY YEAR(Fecha) ORDER BY YEAR(Fecha)";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@desde", inicio);
                    cmd.Parameters.AddWithValue("@hasta", fin);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new
                            {
                                Fecha = reader[0].ToString(),
                                Total = Convert.ToDecimal(reader[1])
                            });
                        }
                    }
                }
            }
            return lista;
        }
        public int ObtenerProximoAutoIncremento(string tabla, string baseDeDatos)
        {
            // El argumento 'baseDeDatos' (gestionventas) ya no es necesario para IDENT_CURRENT
            // pero lo dejamos en la firma del método para no romper otras llamadas.

            using (var conn = ObtenerConexion())
            {
                conn.Open();

                // *** Consulta SQL Server (Reemplazando la de MySQL) ***
                // IDENT_CURRENT devuelve el último valor de identidad generado para la tabla.
                string sql = "SELECT IDENT_CURRENT(@tabla);";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    // Usamos el parámetro @tabla para pasar el nombre 'facturas'
                    cmd.Parameters.AddWithValue("@tabla", tabla);

                    var result = cmd.ExecuteScalar();

                    // Si la tabla está vacía, IDENT_CURRENT puede devolver NULL.
                    if (result == DBNull.Value || result == null)
                    {
                        // Si no hay filas, el primer ID será 1.
                        return 1;
                    }
                    else
                    {
                        // El resultado es el ÚLTIMO ID usado. El próximo será el resultado + 1.
                        int ultimoId = Convert.ToInt32(result);
                        return ultimoId + 1;
                    }
                }
            }
        }


        public void GuardarItemFactura(int idFactura, int idItem, string nombreProd, int cantidad, decimal precio)
        {
            try
            {
                using (SqlConnection conn = ObtenerConexion())
                {
                    conn.Open();

                    string query = @"INSERT INTO facturaitem (idFactura, idItem, nombreProd, cantidad, precio) 
                             VALUES (@idFactura, @idItem, @nombreProd, @cantidad, @precio);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idFactura", idFactura);
                        cmd.Parameters.AddWithValue("@idItem", idItem);
                        cmd.Parameters.AddWithValue("@nombreProd", nombreProd);
                        cmd.Parameters.AddWithValue("@cantidad", cantidad);
                        cmd.Parameters.AddWithValue("@precio", precio);

                        cmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine("✅ Item factura guardado OK.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERROR al guardar item factura: " + ex.ToString());
                throw; // Volvemos a lanzar para que el controlador lo atrape
            }
        }


        public int GuardarVenta(Venta venta)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 1. CAMBIO: Añadimos "; SELECT SCOPE_IDENTITY();" a la query
                string query = "INSERT INTO Ventas (diaVenta, montoVenta) VALUES (@dia, @monto); SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@dia", venta.DiaVenta);
                cmd.Parameters.AddWithValue("@monto", venta.MontoVenta);

                // 2. CAMBIO: Usamos ExecuteScalar y convertimos el resultado a int
                // Este comando ejecuta el INSERT y luego el SELECT, devolviendo el ID.
                object result = cmd.ExecuteScalar();

                // Verificamos si la operación fue exitosa antes de convertir
                if (result != null && result != DBNull.Value)
                {
                    // El ID devuelto es un decimal, por lo que Convert.ToInt32 es seguro.
                    return Convert.ToInt32(result);
                }

                // Si no se pudo obtener el ID (ej: la tabla no tiene IDENTITY), devolvemos 0 o -1
                return -1;
            }
        }

        public int ObtenerUltimaFactura()
        {
            int ultimo = 0;
            using (SqlConnection conn = ObtenerConexion())
            {
                conn.Open();
                string query = "SELECT MAX(idFactura) FROM Ventas";
                SqlCommand cmd = new SqlCommand(query, conn);
                object result = cmd.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    ultimo = Convert.ToInt32(result);
                }
            }
            return ultimo;
        }

        public int ObtenerStockActual(int idProducto)
        {
            int stock = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT stockActual FROM productos WHERE idProducto = @idProducto";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@idProducto", idProducto);
                    object result = cmd.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out int cantidad))
                    {
                        stock = cantidad;
                    }
                }
            }

            return stock;
        }


       public List<Factura> ObtenerFacturas()
{
    List<Factura> lista = new List<Factura>();

    using (var conn = ObtenerConexion())
    {
        conn.Open();
        string query = @"
            SELECT f.idFactura, f.diaVenta, f.montoVenta, f.vendedor, f.descuento, c.nombreCliente AS nombreCliente
            FROM facturas f
            JOIN clientes c ON f.idCliente = c.idCliente
            ORDER BY f.idFactura DESC";

        using (var cmd = new SqlCommand(query, conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                lista.Add(new Factura
                {
                    IdFactura = Convert.ToInt32(reader["idFactura"]),
                    DiaVenta = Convert.ToDateTime(reader["diaVenta"]),
                    MontoVenta = Convert.ToDecimal(reader["montoVenta"]),
                    Vendedor = reader["vendedor"].ToString(),
                    NombreCliente = reader["nombreCliente"].ToString(),
                    // Cargamos el descuento (si es NULL en DB, ponemos 0)
                    Descuento = reader["descuento"] != DBNull.Value ? Convert.ToDecimal(reader["descuento"]) : 0
                });
            }
        }
    }
    return lista;
}


     public Factura ObtenerFacturaConItems(int idFactura)
{
    Factura factura = null;

    using (var conn = ObtenerConexion())
    {
        conn.Open();

        string queryFactura = @"
            SELECT f.idFactura, f.diaVenta, f.montoVenta, f.vendedor, f.descuento, f.medioPago,
                   c.nombreCliente, c.dniCliente, c.domicilio, c.localidad, c.telefonoCliente
            FROM facturas f
            JOIN clientes c ON f.idCliente = c.idCliente
            WHERE f.idFactura = @id";

        using (var cmd = new SqlCommand(queryFactura, conn))
        {
            cmd.Parameters.AddWithValue("@id", idFactura);
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    factura = new Factura
                    {
                        IdFactura = Convert.ToInt32(reader["idFactura"]),
                        DiaVenta = Convert.ToDateTime(reader["diaVenta"]),
                        MontoVenta = Convert.ToDecimal(reader["montoVenta"]),
                        Vendedor = reader["vendedor"].ToString(),
                        NombreCliente = reader["nombreCliente"].ToString(),
                        DniCliente = reader["dniCliente"].ToString(),
                        Domicilio = reader["domicilio"].ToString(),
                        Localidad = reader["localidad"].ToString(),
                        TelefonoCliente = reader["telefonoCliente"].ToString(),
                        MedioPago = reader["medioPago"].ToString(), // Aprovechamos a traer el medio de pago
                        // CLAVE: Traemos el descuento
                        Descuento = reader["descuento"] != DBNull.Value ? Convert.ToDecimal(reader["descuento"]) : 0,
                        Items = new List<FacturaItem>()
                    };
                }
            }
        }

        if (factura != null)
        {
            string queryItems = @"
                SELECT fi.nombreProd, fi.cantidad, fi.precio
                FROM facturaitem fi
                WHERE fi.idFactura = @id";

            using (var cmd = new SqlCommand(queryItems, conn))
            {
                cmd.Parameters.AddWithValue("@id", idFactura);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        factura.Items.Add(new FacturaItem
                        {
                            NombreProd = reader["nombreProd"].ToString(),
                            Cantidad = Convert.ToInt32(reader["cantidad"]),
                            Precio = Convert.ToDecimal(reader["precio"])
                        });
                    }
                }
            }
        }
    }

    return factura;
}


        public void EliminarFactura(int idFactura)
        {
            using (var conexion = ObtenerConexion())
            {
                conexion.Open();

                // Primero eliminamos los items asociados
                using (var cmd = new SqlCommand("DELETE FROM facturaitem WHERE idFactura = @id", conexion))
                {
                    cmd.Parameters.AddWithValue("@id", idFactura);
                    cmd.ExecuteNonQuery();
                }

                // Luego eliminamos la factura
                using (var cmd = new SqlCommand("DELETE FROM facturas WHERE idFactura = @id", conexion))
                {
                    cmd.Parameters.AddWithValue("@id", idFactura);
                    cmd.ExecuteNonQuery();
                }
            }
        }

public int ActualizarPreciosPorProveedor(string proveedor, decimal porcentaje)
{
    using (SqlConnection conn = ObtenerConexion())
    {
        conn.Open();
        
        // Explicación de la fórmula:
        // 1. Multiplicamos el PrecioCosto por el factor de aumento (ej: 1.10 para un 10%)
        // 2. Recalculamos el PrecioVenta usando el NUEVO PrecioCosto y el Recargo actual
        string sql = @"UPDATE productos 
                       SET PrecioCosto = PrecioCosto * (1 + (@porcentaje / 100)),
                           PrecioVenta = (PrecioCosto * (1 + (@porcentaje / 100))) * (1 + (RecargoPorcentaje / 100))
                       WHERE NombreProveedor = @proveedor";

        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            // Importante: El porcentaje viene como número entero (ej: 10)
            cmd.Parameters.AddWithValue("@porcentaje", porcentaje);
            cmd.Parameters.AddWithValue("@proveedor", proveedor);
            
            return cmd.ExecuteNonQuery();
        }
    }
}


        public bool VerificarSiHayCajaAbierta()
        {
            using (var conn = ObtenerConexion())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Cajas WHERE FechaCierre IS NULL";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }



    }
}