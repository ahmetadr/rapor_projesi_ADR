using System;
using System.Collections.Generic;

namespace KorRaporOnline.API.Models
{
    public class User : BaseEntity
    {
        public User()
        {
            SavedReports = new HashSet<SavedReport>();
            UserRoles = new HashSet<UserRole>();
            DatabaseConnections = new HashSet<DatabaseConnection>();
            ReportExecutions = new HashSet<ReportExecution>();
        }



        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<ReportExecution> ReportExecutions { get; set; }
        public virtual ICollection<SavedReport> SavedReports { get; set; }


        
        
        public string FullName { get; set; }
         
       

        
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<DatabaseConnection> DatabaseConnections { get; set; }
         
    }
}