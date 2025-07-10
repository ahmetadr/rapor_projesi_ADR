using System;

namespace KorRaporOnline.API.Models
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
    }
}