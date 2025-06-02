using DockubeCommands.Managers;
using PainKiller.PowerCommands.Core.Services;

namespace PainKiller.PowerCommands.Bootstrap
{
    public partial class PowerCommandsManager
    {
        private bool _dockerWarningShowedOnce;
        private void RunCustomCode()
        {
            if (!KubernetesManager.IsKubernetesAvailable().Result && !_dockerWarningShowedOnce)
            {
                DialogService.DrawToolbar(new string[] { "You need to start Docker Desktop run ->", "dd" });
                _dockerWarningShowedOnce = true;
            }
        }
    }
}