// Copyright Koninklijke Philips N.V. 2021

using System;
using System.Collections.Generic;
using System.IO;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Parses and validates command line arguments
    /// </summary>
    internal class Arguments {
        private readonly string[] arguments;
        private static readonly List<string> knownCommands = new List<string>();

        public string Command { get; private set; }
        public string Path { get; private set; }
        public string[] Input { get; private set; }

        // Use lower case here. Input can be any case
        public const string DummyCommand = "--dummy";
        public const string DebugCommand = "--debug";
        public const string AnalyzersCommand = "--analyzers";
        public const string SuppressCommand = "--suppress";
        public const string UpdateInvalidSuppressionFilePathCommand = "--updateinvalidsuppressionpath";
        public const string UpdateAnalyzerReferenceCommand = "--updateanalyzerreference";
        public const string RemoveProjectPropertyCommand = "--removeproperty";

        static Arguments() {
            knownCommands.Add(DummyCommand);
            knownCommands.Add(DebugCommand);
            knownCommands.Add(AnalyzersCommand);
            knownCommands.Add(SuppressCommand);
            knownCommands.Add(UpdateInvalidSuppressionFilePathCommand);
            knownCommands.Add(UpdateAnalyzerReferenceCommand);
            knownCommands.Add(RemoveProjectPropertyCommand);
        }
        public Arguments(string[] arguments) {
            this.arguments = arguments;
        }
        public static void PrintSupportedArguments() {
            ConsoleWriter.WriteInfo("Add Philips.Platform.Analyzers analyzers to project(s) if not already present");
            ConsoleWriter.WriteWarning("UpdateCSProject.exe <Directory_containing_c#_project(s)> --analyzers");
            Console.WriteLine(Environment.NewLine);

            ConsoleWriter.WriteInfo("Generates GlobalSuppressions file for the project(s)");
            ConsoleWriter.WriteInfo("Use --regenerate is specified, any existing GlobalSuppressions file will be trimmed and regenerated");
            ConsoleWriter.WriteWarning("UpdateCSProject.exe <Directory_containing_c#_project(s)> --suppress");
            ConsoleWriter.WriteWarning("UpdateCSProject.exe <Directory_containing_c#_project(s)> --suppress [--regenerate]");
            Console.WriteLine(Environment.NewLine);

            ConsoleWriter.WriteInfo("Delete or moves the GlobalSuppressions file from Properties folder to the root");
            ConsoleWriter.WriteWarning("UpdateCSProject.exe <Directory_containing_c#_project(s)> --updateinvalidsuppressionpath");
            Console.WriteLine(Environment.NewLine);

            ConsoleWriter.WriteInfo("Adds or updates the References to Roslyn Analyzers");
            ConsoleWriter.WriteWarning("UpdateCSProject.exe <Directory_containing_c#_project(s)> --updateanalyzerreference");
            Console.WriteLine(Environment.NewLine);

            ConsoleWriter.WriteInfo("Removes specified project property");
            ConsoleWriter.WriteWarning("UpdateCSProject.exe <Directory_containing_c#_project(s)> --removeproperty [Property Name] [Property Value]");
        }

        public bool Validate() {
            if (arguments == null || arguments.Length == 0) {
                ConsoleWriter.WriteWarning($"No input provided");
                return false;
            }
            if (arguments.Length == 1) {
                ConsoleWriter.WriteWarning($"Incomplete input");
                return false;
            }

            var pathIndex = 0;
            var commandIndex = 1;
            var inputIndex = 2;

            if (string.Equals(DebugCommand, arguments[commandIndex].ToLowerInvariant())) {
                commandIndex++;
                inputIndex++;
                System.Diagnostics.Debugger.Launch();
            }

            Command = arguments[commandIndex].ToLowerInvariant();
            if (!knownCommands.Contains(Command)) {
                ConsoleWriter.WriteWarning($"Unknown command {Command}");
                return false;
            }
            Path = arguments[pathIndex];
            if (!Directory.Exists(Path)) {
                if (File.Exists(Path)) {
                    ConsoleWriter.WriteError($"{Path} is not a directory");
                    return false;
                }
                ConsoleWriter.WriteError($"{Path} does not exists");
                return false;
            }
            if (arguments.Length > inputIndex) {
                Input = new string[arguments.Length - inputIndex];
                Array.Copy(arguments, inputIndex, Input, 0, arguments.Length - inputIndex);
            }

            return true;
        }
    }
}