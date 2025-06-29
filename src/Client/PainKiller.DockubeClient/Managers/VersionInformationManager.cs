namespace PainKiller.DockubeClient.Managers;
public static class VersionInformationManager
{
    public static DockubeVersionInformation GetVersionInformation()
    {
        var sslVersion = GetSslVersion();
        var currentEnvironment = KubeEnvironmentManager.GetTarget();
        var version = KubeEnvironmentManager.GetVersion();
        var helmVersion = KubeEnvironmentManager.GetHelmVersion();
        return new DockubeVersionInformation(sslVersion, version, helmVersion, currentEnvironment);
    }

    private static string GetSslVersion()
    {
        var versionInfo = SslService.Default.GetVersion().Trim().Split(' ').Take(2);
        var retVal = $"{string.Join(' ', versionInfo).Trim()}";
        return retVal;
    }
}