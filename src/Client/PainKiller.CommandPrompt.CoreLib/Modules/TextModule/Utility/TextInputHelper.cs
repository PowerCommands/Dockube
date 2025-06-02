namespace PainKiller.CommandPrompt.CoreLib.Modules.TextModule.Utility;

using System.Text;

public static class TextInputHelper
{
    public static string CaptureText()
    {
        var builder = new StringBuilder();
        Console.WriteLine("Paste or enter your text (press ESC to finish):");

        while (true)
        {
            var sb = new StringBuilder();

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    return builder.ToString();
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Length--;
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
            }

            var line = sb.ToString();
            builder.AppendLine(line);
        }
    }
}