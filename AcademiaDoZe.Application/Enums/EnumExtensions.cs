using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace AcademiaDoZe.Application.Enums;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        if (value == null) return string.Empty;
        var type = value.GetType();
        var name = value.ToString();
        // Handle Flags
        if (type.GetCustomAttribute<FlagsAttribute>() != null)
        {
            var parts = name.Split(", ", StringSplitOptions.RemoveEmptyEntries);
            return string.Join(", ", parts.Select(p => GetSingleDisplayName(type, p)));
        }
        return GetSingleDisplayName(type, name);
    }

    private static string GetSingleDisplayName(Type type, string name)
    {
        var member = type.GetMember(name).FirstOrDefault();
        if (member == null) return name;
        var attr = member.GetCustomAttribute<DisplayAttribute>();
        return attr?.Name ?? name;
    }
}
