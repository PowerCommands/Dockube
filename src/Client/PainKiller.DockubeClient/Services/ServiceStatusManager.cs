namespace PainKiller.DockubeClient.Services;

public class ServiceStatusManager
{
    public static ServiceConfiguration[] GetServicesStatus(DockubeConfiguration configuration)
    {
        var retVal = new List<ServiceConfiguration>();
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        foreach (var service in configuration.ServiceStatusChecks)
        {
            var start = service.Tsl ? "https://" : "http://";
            var url = $"{start}{service.Host}.{configuration.DefaultDomain}";
            bool available;
            try
            {
                var response = httpClient.GetAsync(url).GetAwaiter().GetResult();
                available = response.IsSuccessStatusCode;
            }
            catch
            {
                available = false;
            }
            service.SetIsAvailable(available);
            retVal.Add(service);
        }
        return retVal.ToArray();
    }
}