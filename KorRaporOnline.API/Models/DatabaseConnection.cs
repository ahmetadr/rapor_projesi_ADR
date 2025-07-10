namespace KorRaporOnline.API.Models
{
    public class DatabaseConnection : BaseEntity
    {
        public DatabaseConnection()
        {
            SavedReports = new List<SavedReport>();
        }

        public int ConnectionID { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string Provider { get; set; }
        public bool IsActive { get; set; }

        // Navigation property
        public virtual ICollection<SavedReport> SavedReports { get; set; }

       
        public int UserID { get; set; }
        public string ConnectionName { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
       

        public virtual User User { get; set; }
        
    }
}