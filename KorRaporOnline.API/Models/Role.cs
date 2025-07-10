using System.Collections.Generic;

namespace KorRaporOnline.API.Models
{
    public class Role : BaseEntity
    {
        public Role()
        {
            UserRoles = new HashSet<UserRole>();
        }

        public int RoleID { get; set; }
        public string Name { get; set; }  // RoleName yerine sadece Name kullanıyoruz
        public string Description { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}