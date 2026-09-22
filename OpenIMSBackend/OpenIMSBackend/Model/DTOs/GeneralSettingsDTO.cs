namespace OpenIMSBackend.Core.Model.DTOs
{
    /// <summary>Data transfer object for the general (admin-configurable) OpenIMS-settings.</summary>
    public class GeneralSettingsDTO
    {
        /// <summary>Whether a new user-account can be created at all.</summary>
        public bool RegistrationIsEnabled { get; set; }

        /// <summary>Initializes a new instance of <see cref="GeneralSettingsDTO"/>.</summary>
        /// <param name="registrationIsEnabled">Whether the creation of a new user-account is enabled.</param>
        public GeneralSettingsDTO(bool registrationIsEnabled)
        {
            this.RegistrationIsEnabled = registrationIsEnabled;
        }
    }
}
