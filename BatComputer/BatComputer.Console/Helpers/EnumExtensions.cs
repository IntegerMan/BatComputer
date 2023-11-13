using System.ComponentModel;
using System.Reflection;

namespace MattEland.BatComputer.ConsoleApp.Helpers;

public static class EnumExtensions
{
    public static string ToFriendlyString(this Enum value)
    {
        DescriptionAttribute? descriptionAttribute = value.GetType()
            .GetMember(value.ToString())
            .FirstOrDefault()
            ?.GetCustomAttribute<DescriptionAttribute>();

        return descriptionAttribute?.Description ?? value.ToString();
    }
}
