namespace RecetArreWeb.DTOs
{
    public class RecetaRankingDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double PromedioCalificacion { get; set; }
        public int TotalCalificaciones { get; set; }
        public int Posicion { get; set; }
    }
}
