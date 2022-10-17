// Copyright Koninklijke Philips N.V. 2021

using System;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Output to console
    /// </summary>
    internal static class ConsoleWriter {
        public static void WriteMessage(string message) {
            WriteColor(Console.ForegroundColor, message);
        }
        public static void WriteInfo(string message) {
            WriteColor(ConsoleColor.DarkCyan, message);
        }

        public static void WriteSuccess(string message) {
            WriteColor(ConsoleColor.DarkGreen, message);
        }

        public static void WriteError(string message) {
            WriteColor(ConsoleColor.DarkRed, message);
        }

        public static void WriteWarning(string message) {
            WriteColor(ConsoleColor.DarkYellow, message);
        }

        public static void WriteColor(ConsoleColor color, string message) {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}