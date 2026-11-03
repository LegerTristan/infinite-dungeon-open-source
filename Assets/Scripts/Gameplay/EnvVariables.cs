namespace LingeringHopes.Gameplay
{
    /// <summary>
    /// Keys used for basic local save data encryption.
    /// If you plan to reuse this projet, you should change them and implement your own secure encoding or key management system 
    /// as hardcoding encryption keys directly into the source code is insecure.
    /// </summary>
    public static class EnvVariables
    {
        public const string AES_KEY = "49fe232fd3d7d707ea7a7218cdf512bd";

        public const string AES_IV = "8540b143de1f8fab";
    }
}