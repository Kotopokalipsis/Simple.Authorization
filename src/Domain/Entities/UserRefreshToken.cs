using System;

namespace Domain.Entities
{
    public class UserRefreshToken : Entity
    {
        public User User { get; set; }
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; }
    }
}