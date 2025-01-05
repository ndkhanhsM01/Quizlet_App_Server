using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Src.Models.OtherFeature.Cipher;

namespace Quizlet_App_Server.Src.DataSettings
{
    [System.Serializable]
    public class AppConfigResource
    {
        public AesConfig Aes;
        public JwtConfig Jwt;
        public UserStoreDatabaseSetting UserStoreDatabaseSetting;

        [System.Serializable]
        public class JwtConfig
        {
            public string Issuer;
            public string Audience;
            public string Key;
            public int TokenValidityMins;
        }
    }
}
