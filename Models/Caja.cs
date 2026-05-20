using System;
using System.Collections.Generic;

namespace GestionVentas.Models
{
    // Agregamos esto para que el sistema reconozca el tipo
    public enum TipoMovimiento
    {
        Ingreso,
        Egreso
    }

    public class Caja
    {
        public int Id { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal MontoInicial { get; set; }
        public decimal? MontoFinalReal { get; set; }
        public decimal MontoEsperado { get; set; }
        public bool EstaAbierta { get; set; }
        public string UsuarioId { get; set; }
        

        // Tip: Agregá esta propiedad para que la relación sea más fácil de usar
        public virtual ICollection<MovimientoCaja> Movimientos { get; set; }
    }

    public class MovimientoCaja
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; }
        public decimal Monto { get; set; }
        public TipoMovimiento Tipo { get; set; } // Ahora sí lo reconoce
         public string? Usuario { get; set; }
        public virtual Caja Caja { get; set; }
    }
}