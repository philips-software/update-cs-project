// Copyright Koninklijke Philips N.V. 2022

using System;
using System.Diagnostics;
using System.IO;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Delete or moves the GlobalSuppressions file from \Properties\ folder to the root.
    /// </summary>
    /// <remarks> Argument : --updateinvalidsuppressionpath </remarks>
    internal class CsProjectSuppressionFilePathUpdater : ICsProjectUpdater {
        private const string SuppressionsFileName = "GlobalSuppressions.cs";
        private const string PropertiesFolderName = "Properties";

        public UpdateStatus Update(CsProject csProject, Arguments arguments) {
            Fix(csProject);
            return UpdateStatus.Success;
        }

        private static void Fix(CsProject csProject) {
            if (csProject.Name.Contains("Prerequisites")) {
                return;
            }

            var basePath = Path.GetDirectoryName(csProject.Path);
            var rootSuppressionFilePath = Path.Combine(basePath, SuppressionsFileName);
            var rootSuppressionFileRelativePath = SuppressionsFileName;
            var invalidSuppressionFilePath = Path.Combine(basePath, PropertiesFolderName, SuppressionsFileName);
            var invalidSuppressionFileRelativePath = $"{PropertiesFolderName}\\{SuppressionsFileName}";

            // Step 1 : Delete the GlobalSuppressions.cs exists under "Properties" folder.
            if (File.Exists(invalidSuppressionFilePath)) {
                if (File.Exists(rootSuppressionFilePath)) {
                    // Extract contents from both the Suppression files.
                    var invalidSuppressionFile = SuppressionFileParser.Parse(invalidSuppressionFilePath);

                    // Merge the content with the root GlobalSuppressions.cs.
                    invalidSuppressionFile.SerializeToFile(rootSuppressionFilePath);

                    // Delete from SVN source control.
                    var command = $"svn delete {invalidSuppressionFilePath}";
                    Process.Start("powershell.exe", command);

                    // Remove the GlobalSuppressions.cs from .csproj.
                    csProject.ExcludeFromProject("Compile", invalidSuppressionFileRelativePath);
                    ConsoleWriter.WriteInfo($"Excluding file {invalidSuppressionFileRelativePath} from the project: {csProject.Name}.");
                } else {
                    // SVN move to the root folder where the project file exists.
                    var command = $"svn move {invalidSuppressionFilePath} {rootSuppressionFilePath}";
                    Process.Start("powershell.exe", command);

                    // Remove the invalid GlobalSuppressions.cs from .csproj.
                    csProject.ExcludeFromProject("Compile", invalidSuppressionFileRelativePath);

                    // Add the GlobalSuppressions.cs to csproj.
                    csProject.IncludeInProject("Compile", rootSuppressionFileRelativePath);
                    ConsoleWriter.WriteInfo(
                        $"{invalidSuppressionFileRelativePath} has been moved to root of the project - {csProject.Name}.");
                    Console.WriteLine($"Moved file {invalidSuppressionFilePath} to {csProject.Path}.");
                }
            }
        }
    }
}
