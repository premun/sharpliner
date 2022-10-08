using System;

namespace Sharpliner.Tools;

public static class EnvironmentVariables
{
    public static class Names
    {
        public const string DISABLE_COLOR_OUTPUT = "DISABLE_COLORED_OUTPUT";
        public const string LOG_TIMESTAMPS = "LOG_WITH_TIMESTAMPS";
    }

    public static bool IsTrue(string varName) =>
        Environment.GetEnvironmentVariable(varName)?.ToLower().Equals("true") ?? false;
}

