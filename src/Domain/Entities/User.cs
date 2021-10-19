using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        private int? _requestedHashCode;
        private List<INotification> _domainEvents;
        public UserRefreshToken UserRefreshToken { get; set; }
        
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

        public override int GetHashCode()
        {
            if (IsTransient()) return base.GetHashCode();
            
            _requestedHashCode ??= Id.GetHashCode() ^ 31;
            return _requestedHashCode.Value;

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