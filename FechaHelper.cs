namespace GestionVentas.Helpers
{
    public static class FechaHelper
    {
        public static DateTime AhoraArgentina()
        {
            return DateTime.UtcNow.AddHours(-3);
        }
    }
}