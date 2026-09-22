using System.Collections.Generic;

namespace OpenIMSBackend.Core.Model.DTOs
{
    /// <summary>Represents a user together with the names of the roles currently assigned to it. Used for the administrative user-overview.</summary>
    public class UserOverviewDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ISet<string> Roles { get; set; }

        public UserOverviewDTO(string id, string name, ISet<string> roles)
        {
            this.Id = id;
            this.Name = name;
            this.Roles = roles;
        }
    }
}
