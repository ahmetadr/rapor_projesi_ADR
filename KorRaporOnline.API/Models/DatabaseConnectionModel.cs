namespace KorRaporOnline.API.Models
{
    public class DatabaseConnectionModel
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