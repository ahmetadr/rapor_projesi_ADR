namespace KorRaporOnline.API.Models.DTOs
{
    public class DatabaseConnectionDto
    {
        public int ConnectionID { get; set; }
        public string ConnectionName { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
    }
}