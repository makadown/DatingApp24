namespace API.DTOs
{
    /// <summary>
    /// Este modelo es el contenido para el usuario miembro al ver sus fotos
    /// </summary>
    public class PhotoDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        public bool IsApproved { get; set; }
    }
}