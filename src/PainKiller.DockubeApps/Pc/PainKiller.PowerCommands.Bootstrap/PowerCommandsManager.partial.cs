using PainKiller.PowerCommands.Core.Services;
using PainKiller.PowerCommands.ReadLine;
using PainKiller.PowerCommands.ReadLine.Events;

namespace PainKiller.PowerCommands.Bootstrap
{
    public partial class PowerCommandsManager
    {
        private bool _showInitialTipAtStartupDone;
        private string[] Labels = new []{"Tip use command ->","cd --bookmark","To navigate to api spec files."};
        private void RunCustomCode()
        {
            if (!_showInitialTipAtStartupDone)
            {
                DialogService.DrawToolbar(Labels);
                _showInitialTipAtStartupDone = true;
                ReadLineService.CmdLineTextChanged += ReadLineServiceOnCmdLineTextChanged;
            }
        }
        private void ReadLineServiceOnCmdLineTextChanged(object? sender, CmdLineTextChangedArgs e)
        {
            ReadLineService.CmdLineTextChanged -= ReadLineServiceOnCmdLineTextChanged;
            DialogService.ClearToolbar(Labels);
        }
    }
}