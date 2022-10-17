// Copyright Koninklijke Philips N.V. 2022

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Class to parse contents of a Suppression file
    /// </summary>
    internal static class SuppressionFileParser {
        private const string SuppressMessageFqn = "System.Diagnostics.CodeAnalysis.SuppressMessage";
        private const string SuppressMessageClassName = "SuppressMessage";
        private const StringComparison ordinal = StringComparison.OrdinalIgnoreCase;

        /// <summary>
        /// Parses the content of a Suppressions file.
        /// </summary>
        /// <param name="filePath">Absolute path of the Suppressions file</param>
        /// <returns>
        /// An instance of SuppressionFile containing the contents of the parsed file as a string array.
        /// </returns>
        public static SuppressionFile Parse(string filePath) {
            ValidateFilePath(filePath);

            var contents = File.ReadAllLines(filePath);
            var currentLineIndex = 0;
            var suppressionBuilder = new StringBuilder(20);
            var suppressionStatements = new List<string>();

            while (currentLineIndex < contents.Length) {
                // Add the whole line if the suppression statement is present as a single line
                if (
                    contents[currentLineIndex].StartsWith("[", ordinal) && 
                    contents[currentLineIndex].EndsWith("]", ordinal)) {
                    suppressionStatements.Add(FormatSuppressionStatement(contents[currentLineIndex++]));
                    continue;
                }

                // Collect all segments of a suppression statement broken into multiple lines together and
                // flush it out as a single line
                if (
                    contents[currentLineIndex].StartsWith("[", ordinal) && 
                    !contents[currentLineIndex].EndsWith("]", ordinal)) {
                    do {
                        suppressionBuilder.Append(contents[currentLineIndex++].Trim());
                    } while (
                        currentLineIndex < contents.Length &&
                        !contents[currentLineIndex].StartsWith("[", ordinal)
                    );

                    var formattedStatement = FormatSuppressionStatement(suppressionBuilder.ToString());
                    suppressionStatements.Add(formattedStatement);
                    suppressionBuilder.Clear();
                    continue;
                }

                currentLineIndex++;
            }

            return new SuppressionFile(suppressionStatements.ToArray());
        }

        /// <summary>
        /// Add CodeAnaysis using statement is not already present.
        /// </summary>
        /// <param name="filePath">Absolute path of the Suppressions file</param>
        public static void AddUsingStatement(string filePath) {
            var contents = File.ReadAllLines(filePath);
            var copyWriteLine= "";
            for (int i = 0; i < contents.Length; i++) {
                var line = contents[i];
                if(line.Contains("Copyright Koninklijke Philips")) {
                    copyWriteLine = line;
                    break;
                }
            }
            
            var fileContents = File.ReadAllText(filePath);
            const string usingStatement = "using System.Diagnostics.CodeAnalysis;";
            const string fullyQualifiedSuppressText = "System.Diagnostics.CodeAnalysis.SuppressMessage(";
            if (!fileContents.Contains("using System.Diagnostics.CodeAnalysis;")) {
                // Add the namespace.
                var textToReplace = copyWriteLine + Environment.NewLine + usingStatement + Environment.NewLine;
                fileContents = fileContents.Replace(copyWriteLine, textToReplace);
            }
            // Remove the diagnostics namespace from existing suppression.
            if (fileContents.Contains(fullyQualifiedSuppressText)) {
                const string namespaceRemovedText = "SuppressMessage(";
                fileContents = fileContents.Replace(fullyQualifiedSuppressText, namespaceRemovedText);
            }
            File.WriteAllText(filePath, fileContents);
        }

        private static void ValidateFilePath(string filePath) {
            if (string.IsNullOrWhiteSpace(filePath)) {
                throw new ArgumentException("'filePath' should not be a null or empty.");
            }

            if (!File.Exists(filePath)) {
                throw new InvalidOperationException($"Invalid file parsing attempted - \"{filePath}\". " +
                                                    $"Ensure the path is correct and the file exists."
                );
            }
        }

        private static string FormatSuppressionStatement(string suppressionStatement) {
            var formattedString = Regex.Replace(suppressionStatement, SuppressMessageFqn, SuppressMessageClassName);
            formattedString = Regex.Replace(formattedString, ",[ ]{2,}", ", ");
            formattedString = Regex.Replace(formattedString, "assembly:Suppress", "assembly: Suppress");
            formattedString = Regex.Replace(formattedString, "\\([ ]{1,}\"", "(\"");

            return formattedString;
        }
    }
}
