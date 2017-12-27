using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using ExitGames.Logging;

namespace ExitGames.Configuration
{
    /// <summary>
    /// This class reads a configuration section for a profile name and merges it with profile section 'Common'. 
    /// </summary>
    public class ProfileReader : NameValueCollectionReader
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Configuration.ProfileReader"/> class.
        /// </summary>
        /// <param name="settings"> The settings.</param>
        public ProfileReader(NameValueCollection settings)
            : base(settings)
        {
        }

        /// <summary>
        ///   Creates a new instance from the given pofile name.
        /// </summary>
        /// <param name="profile"> The profile.</param>
        /// <param name="profilesSection"> The profiles section.</param>
        /// <returns>A new instance of <see cref="T:ExitGames.Configuration.ProfileReader"/>.</returns>
        /// <exception cref="T:ExitGames.Configuration.ConfigurationException">
        /// Profile not found.
        /// </exception>
        public static ProfileReader Create(string profile, string profilesSection)
        {
            string str = string.Format("{0}/{1}", profilesSection, profile);
            NameValueCollection settings = HttpUtility.ParseQueryString(str);
            if (settings == null)
            {
                throw new ConfigurationException(string.Format("Section '{0}' not found", str));
            }
            str = string.Format("{0}/Common", profilesSection);
            NameValueCollection values2 = HttpUtility.ParseQueryString(str);
            if (values2 == null)
            {
                return new ProfileReader(settings);
            }
            NameValueCollection values3 = new NameValueCollection(settings);
            foreach (var v2 in values2.Keys)
            {
                string str2 = v2.ToString();
                if (values3.GetValues(str2) == null)
                {
                    values3.Add(str2, values2[str2]);
                }
                else if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Profile section '{1}' hides common configuration setting '{0}'", str2, profile);
                }
            }
            //IEnumerator enumerator = .~(values2);
            //try
            //{
            //    while (.~(enumerator))
            //    {
            //        string str2 = (string) .~(enumerator);
            //        if (.~(values3, str2) == null)
            //        {
            //            .~(values3, str2, .~(values2, str2));
            //        }
            //        else if (log.IsDebugEnabled)
            //        {
            //            log.DebugFormat("Profile section '{1}' hides common configuration setting '{0}'", new object[] { str2, profile });
            //        }
            //    }
            //}
            //finally
            //{
            //    IDisposable disposable = enumerator as IDisposable;
            //    if (disposable != null)
            //    {
            //        .~(disposable);
            //    }
            //}
            return new ProfileReader(values3);
        }

        /// <summary>
        /// Reads the profile name from AppSettings.Profile.
        /// </summary>
        /// <returns>     The profile name.</returns>
        /// <exception cref="T:ExitGames.Configuration.ConfigurationException">
        /// Key 'Profile' missing in AppSettings.
        /// </exception>
        public static string ReadCurrentProfileName()
        {
            string str = ConfigurationManager.AppSettings["Profile"];
            if (str == null)
            {
                throw new ConfigurationException("'Profile' missing in AppSettings");
            }
            return str;
        }
    }
}
