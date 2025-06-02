# InfoPanel Module Version 1.0
Provides a flexible and dynamic information panel that reserves the top lines of the console for displaying continuously updating information.

## Overview
The InfoPanel module allows you to display important information or dynamic content in a reserved area at the top of the console. This makes it easy to keep essential data visible while still using the rest of the console for other tasks.
You write your own custom content by implementing the `IInfoPanelContent` interface.

### Features:
- Asynchronous updates: Continuously display information without blocking user interactions.
- Modular content: Each project can inject its own custom content.
- Flexible start and stop: Start, update, and stop the panel without affecting the console.
- Manually update content at any time.
- Updates the InfoPanel every time a Command has finished.
- Custom content formatting: The content is entirely controlled by the `IInfoPanelContent` implementation.

## Console Space Reservation
The InfoPanel reserves a fixed area at the **top of the console window** to display its content.
- **Default size:** 3 lines (can be adjusted if needed).
- This reserved area is maintained even when the rest of the console is updated.
- Regular console output will always start **below** this area.

### Why Reserve Space?
By reserving space at the top of the console, the InfoPanel ensures that its displayed content:
- Remains clearly visible at all times.
- Is not accidentally overwritten by other console output.
- Maintains a consistent and fixed position in the UI.

---

## Initialize the InfoPanel

### Where to register:
You can register and start the InfoPanel in a Command class in the `OnInitialized()` method.
Look in the example Command InfoPanelCommand class for a reference.

```csharp
public class InfoPanelCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override void OnInitialized() => InfoPanelService.Instance.RegisterContent(new SpectreInfoPanel(new DefaultInfoPanelContent(), Color.Magenta1, Color.BlueViolet));
    public override RunResult Run(ICommandLineInput input)
    {
        if(input.HasOption("stop")) InfoPanelService.Instance.Stop();
        else InfoPanelService.Instance.Update(); 
        return Ok();
    }
}
```