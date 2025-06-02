using System.ComponentModel;
using System.Reflection;
using PainKiller.CommandPrompt.CoreLib.Core.Enums;

namespace PainKiller.CommandPrompt.CoreLib.Core.Extensions;

public static class UIExtensions
{
    public static string Icon(this Emo symbol)
    {
        var member = symbol.GetType().GetMember(symbol.ToString());
        var attr = member[0].GetCustomAttribute<DescriptionAttribute>();
        return attr?.Description ?? symbol.ToString();
    }
    public static string ToCheck(this bool val, string trueCheck = "✅", string falseCheck = "") => val ? trueCheck : falseCheck;
}