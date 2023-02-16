using System;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpirationTime { get; set; }
        
        public string AccessToken { get; set; }
        
        public DateTime CreationDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public bool IsTransient()
        {
            return Id == default;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Entity))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (GetType() != obj.GetType())
                return false;

            var item = (Entity)obj;

            if (item.IsTransient() || IsTransient())
                return false;
            
            return item.Id == Id;
        }

        public static bool operator == (User left, User right)
        {
            return left?.Equals(right) ?? Equals(right, null);
        }

        public static bool operator != (User left, User right)
        {
            return left != right;
        }
    }
}