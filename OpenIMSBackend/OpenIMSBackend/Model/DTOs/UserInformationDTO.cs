namespace OpenIMSBackend.Core.Model.DTOs
{
    /// <summary>Represents the information about the currently authenticated user which the frontend needs to render itself.</summary>
    public class UserInformationDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }

        public UserInformationDTO(string id, string name, bool isAdmin)
        {
            this.Id = id;
            this.Name = name;
            this.IsAdmin = isAdmin;
        }
    }
}
