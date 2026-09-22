using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OpenIMSBackend.Core.Constants
{
    public partial class CodeUnitSpecificConstants
    {
        internal const string UsernameAdmin = "admin";
        internal const string RolenameUsers = "Users";
        internal const string RolenameAdmins = "Administrators";

        /// <summary>Key of the setting which controls whether a new user-account can be created. The configuration-value of the same name is only the default which applies until an administrator changed it.</summary>
        internal const string SettingKeyRegistrationIsEnabled = "RegistrationIsEnabled";

        /// <summary>Key of the user-setting which holds the color-scheme the user chose.</summary>
        internal const string UserSettingKeyTheme = "Theme";

        /// <summary>The color-scheme of a user who did not choose one. It follows the setting of the operating-system of that user.</summary>
        internal const string ThemeSystem = "system";

        /// <summary>The color-scheme which is always light, regardless of the setting of the operating-system.</summary>
        internal const string ThemeLight = "light";

        /// <summary>The color-scheme which is always dark, regardless of the setting of the operating-system.</summary>
        internal const string ThemeDark = "dark";

        /// <summary>All values which are accepted as color-scheme of a user.</summary>
        internal static readonly ISet<string> Themes = new HashSet<string>() { ThemeSystem, ThemeLight, ThemeDark };

        internal const string BusinessMetricsPrefix = $"{GeneralConstants.CodeUnitName}_business_";
        internal const string MetricsNameAmountOfUsers = $"{BusinessMetricsPrefix}_AmountOfUsers";

        [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9]*$")]
        public static partial Regex UserNameRegex();
    }
}
