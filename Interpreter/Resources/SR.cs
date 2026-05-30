using System.Globalization;
using System.Resources;

namespace Esolang.Brainfuck.Interpreter;

static class SR
{
    static readonly ResourceManager ResourceManager =
        new("Esolang.Brainfuck.Interpreter.Resources.Strings", typeof(SR).Assembly);

    internal static string Get(string key)
        => ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
}
