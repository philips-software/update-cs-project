// Copyright Koninklijke Philips N.V. 2022

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Adds a reference to the required Analyzers. These are currently:
    /// - Philips.Platform.Analyzers
    /// - FxCopAnalyzers
    /// - BannedApiAnalyzer
    /// </summary>
    /// <remarks> Argument : --updateanalyzerreference </remarks>
    internal class CsProjectAnalyzerReferenceUpdater : ICsProjectUpdater {
        private const string PhilipsAnalyzer = @"Build\Output\Bin\Philips.Platform.Analyzers.dll";
        private const string FxCopNuGetPackageName = "Microsoft.CodeAnalysis.FxCopAnalyzers";
        private const string FxCopNuGetPackageVersion = "$(MicrosoftCodeAnalysisFxCopAnalyzersPackageVersion)";
        private const string BannedApiNuGetPackageName = "Microsoft.CodeAnalysis.BannedApiAnalyzers";
        private const string BannedApiNuGetPackageVersion = "$(MicrosoftCodeAnalysisBannedApiAnalyzersPackageVersion)";
        private const string BannedApiAdditionalFiles = @"Build_\Config\BannedSymbols.txt";

        public UpdateStatus Update(CsProject csProject, Arguments arguments) {
            Fix(csProject);
            return UpdateStatus.Success;
        }

        private static void Fix(CsProject csProject) {
            if (csProject.Name.Contains("Prerequisites")) {
                return;
            }

            var projectPath = Path.GetDirectoryName(csProject.Path);
            var rootRepoPath = GetRepoRootPath(projectPath);

            // Refresh Philips Analyzers
            if (!csProject.HasItemInProject("Analyzer", PhilipsAnalyzer))
            {
                var absolutePhilipsAnalyzersPath = Path.Combine(rootRepoPath, PhilipsAnalyzer);
                var relativePhilipsAnalyzerPath = PathNetCore.GetRelativePath(projectPath, absolutePhilipsAnalyzersPath);
                csProject.IncludeInProject("Analyzer", relativePhilipsAnalyzerPath);
                ConsoleWriter.WriteInfo($"Adding Philips Analyzer to the project: {csProject.Name}.");
            }
            if (!csProject.HasItemInProject("PackageReference", FxCopNuGetPackageName))
            {
                var metadata = new Dictionary<string, string>(1);
                metadata.Add("Version", FxCopNuGetPackageVersion);
                csProject.IncludeInProject("PackageReference", FxCopNuGetPackageName, metadata);
                ConsoleWriter.WriteInfo($"Adding FxCop Analyzer to the project: {csProject.Name}.");
            }
            if (!csProject.HasItemInProject("PackageReference", BannedApiNuGetPackageName)) {
                var metadata = new Dictionary<string, string>(1);
                metadata.Add("Version", BannedApiNuGetPackageVersion);
                csProject.IncludeInProject("PackageReference", BannedApiNuGetPackageName, metadata);
                ConsoleWriter.WriteInfo($"Adding Banned API Analyzer to the project: {csProject.Name}.");
            }
            if (!csProject.HasItemInProject("AdditionalFiles", BannedApiAdditionalFiles)) {
                var absoluteBannedApiFilePath = Path.Combine(
                    rootRepoPath,
                    BannedApiAdditionalFiles);
                var relativeBannedApiFilePath = PathNetCore.GetRelativePath(projectPath, absoluteBannedApiFilePath);
                csProject.IncludeInProject("AdditionalFiles", relativeBannedApiFilePath);
                ConsoleWriter.WriteInfo($"Adding Banned API additional file to the project: {csProject.Name}.");
            }
        }

        private static string GetRepoRootPath(string innerPath) {
            var absolutePath = Path.GetFullPath(innerPath);
            var dir = new DirectoryInfo(absolutePath);
            while (!dir.GetDirectories("Build_").Any()) {
                dir = dir.Parent;
            }
            return dir.FullName;
        }
    }
}
