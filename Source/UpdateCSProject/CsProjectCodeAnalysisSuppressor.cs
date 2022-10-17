// Copyright Koninklijke Philips N.V. 2021

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Generates global suppression file
    /// </summary>
    internal class CsProjectCodeAnalysisSuppressor : ICsProjectUpdater {
        private const string SuppressionsFileName = "GlobalSuppressions.cs";
        private bool regenerateSuppressionsFile;
        private CsProject csProject;

        private static readonly Assembly[] analyzersAssemblies = {
            LoadNuGetPackage("microsoft.codeanalysis.bannedapianalyzers", "3.3.2", @"analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll"),
            LoadNuGetPackage("microsoft.codeanalysis.bannedapianalyzers", "3.3.2", @"analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll"),
            LoadNuGetPackage("microsoft.codeanalysis.versioncheckanalyzer", "3.3.1", @"analyzers\dotnet\Microsoft.CodeAnalysis.VersionCheckAnalyzer.dll"),
            LoadNuGetPackage("microsoft.codeanalysis.analyzers", "3.3.2", @"analyzers\dotnet\cs\Microsoft.CodeAnalysis.Analyzers.dll"),
            LoadNuGetPackage("microsoft.codeanalysis.analyzers", "3.3.2", @"analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.Analyzers.dll"),
            LoadNuGetPackage("microsoft.netcore.analyzers", "3.3.1", @"analyzers\dotnet\cs\Microsoft.NetCore.Analyzers.dll"),
            LoadNuGetPackage("microsoft.netcore.analyzers", "3.3.1", @"analyzers\dotnet\cs\Microsoft.NetCore.CSharp.Analyzers.dll"),
            LoadNuGetPackage("microsoft.netframework.analyzers", "3.3.1", @"analyzers\dotnet\cs\Microsoft.NetFramework.Analyzers.dll"),
            LoadNuGetPackage("microsoft.netframework.analyzers", "3.3.1", @"analyzers\dotnet\cs\Microsoft.NetFramework.CSharp.Analyzers.dll"),
        };

        private static Assembly LoadNuGetPackage(
            string packageName,
            string packageVersion,
            string localPath
        ) {
            var fullPath = 
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                @"\.nuget\packages\" + packageName + @"\" + packageVersion + @"\" +
                localPath;
            return Assembly.LoadFrom(fullPath);
        }

        public UpdateStatus Update(CsProject project, Arguments arguments) {
            if (arguments.Input?.Length == 1 &&
                arguments.Input[0].Equals("--regenerate", StringComparison.OrdinalIgnoreCase))
            {
                regenerateSuppressionsFile = true;
            }
            csProject = project;
            return Fix();
        }

        private UpdateStatus Fix() {
            if (csProject.Name.Contains("Prerequisites")) {
                return UpdateStatus.Skipped;
            }

            bool hadCompilerErrors = false;
            using(var workspace = MSBuildWorkspace.Create()) {
                Project roslynProject = workspace.OpenProjectAsync(csProject.Path).Result;
                
                var basePath = Path.GetDirectoryName(csProject.Path);
                // ReSharper disable once AssignNullToNotNullAttribute
                var absoluteSuppressionsFile = Path.Combine(basePath, SuppressionsFileName);

                List<string> bannedApiAnalyzerSuppressions = new List<string>();
                if (regenerateSuppressionsFile) {
                    bannedApiAnalyzerSuppressions = GetExistingBannedApiAnalyzerSuppressions(absoluteSuppressionsFile);
                    CreateBlankGlobalSuppressionsFile(absoluteSuppressionsFile);
                }

                // Step 1: Generate Diagnostics
                var diagnostics = GetDiagnostics(roslynProject, out hadCompilerErrors);
                if (diagnostics.Any()) {
                    // Step 2: Generate new GlobalSuppressions.cs
                    if (!File.Exists(absoluteSuppressionsFile)) {
                        // Create a new file. Warn user to add the file to the project.
                        CreateBlankGlobalSuppressionsFile(absoluteSuppressionsFile);

                        Console.WriteLine(
                            $"Created new GlobalSuppressions.cs file for project {csProject.Path}.");

                        // Add new file to SVN source control.
                        string command = $"svn add {absoluteSuppressionsFile}";
                        Process.Start("powershell.exe", command);

                        // Add the GlobalSuppressions.cs to csproj (if not already there).
                        if (!csProject.HasItemInProject("Compile", SuppressionsFileName)) {
                            csProject.IncludeInProject("Compile", SuppressionsFileName);
                        }
                    }

                    // Step 3: Write diagnostics into GlobalSuppressions.cs
                    Write(absoluteSuppressionsFile, diagnostics, bannedApiAnalyzerSuppressions);
                }
            }

            return (hadCompilerErrors) ? UpdateStatus.Warn : UpdateStatus.Success;
        }

        private static List<string> GetExistingBannedApiAnalyzerSuppressions(string absoluteSuppressionsFile)
        {
            List<string> bannedApiAnalyzerSuppressions = new List<string>();
            if (File.Exists(absoluteSuppressionsFile))
            {
                var allSuppressions = File.ReadAllLines(absoluteSuppressionsFile);
                foreach (var suppression in allSuppressions)
                {
                    if (suppression.Contains("\"RS0030\"") || suppression.Contains("\"RS0031\""))
                    {
                        bannedApiAnalyzerSuppressions.Add(suppression);
                    }
                }
            }

            return bannedApiAnalyzerSuppressions;
        }

        private static void CreateBlankGlobalSuppressionsFile(string absoluteSuppressionsFile)
        {
            using (var stream = File.Create(absoluteSuppressionsFile))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    WriteCopyrightHeader(writer);
                    WriteUsing(writer);
                }
            }
        }

        private static void WriteCopyrightHeader(TextWriter writer) {
            writer.WriteLine($"// Copyright Koninklijke Philips N.V. {DateTime.Now.Year}");
            writer.WriteLine("");
        }

        private static void WriteUsing(TextWriter writer) {
            writer.WriteLine("using System.Diagnostics.CodeAnalysis;");
            writer.WriteLine("");
        }

        /// <summary>
        /// Writes suppressions to passed suppresions file
        /// </summary>
        /// <param name="suppresionsFile">File to write</param>
        /// <param name="diagnostics">Diagnostics to write</param>
        /// <param name="additionalSuppressions">Additional diagnostic, if required</param>
        private void Write(string suppresionsFile, List<Tuple<Diagnostic, SyntaxNode, SemanticModel>> diagnostics,
            List<string> additionalSuppressions) {
            SuppressionFileParser.AddUsingStatement(suppresionsFile);
            using (var stream = new FileStream(suppresionsFile, FileMode.Append)) {
                using (var writer = new StreamWriter(stream)) {
                    // HashSet will remove duplicates
                    var lines = new HashSet<string>(diagnostics.Count);
                    foreach (var diagnostic in diagnostics) {
                        var node = diagnostic.Item2;
                        var model = diagnostic.Item3;
                        bool handled = false;
                        string code = diagnostic.Item1.Id;
                        string suppression = null;
                        while (!handled && node != null) {
                            switch (node.Kind()) {
                                case SyntaxKind.FieldDeclaration:
                                    suppression = SuppressMessageHelper.WriteFieldSuppression(code, (FieldDeclarationSyntax)node);
                                    handled = true;
                                    break;
                                case SyntaxKind.PropertyDeclaration:
                                    suppression = SuppressMessageHelper.WritePropertySuppression(code, (PropertyDeclarationSyntax)node);
                                    handled = true;
                                    break;
                                case SyntaxKind.MethodDeclaration:
                                    suppression = SuppressMessageHelper.WriteMethodSuppression(code, (MethodDeclarationSyntax)node, model);
                                    handled = true;
                                    break;
                                case SyntaxKind.EventDeclaration:
                                    suppression = SuppressMessageHelper.WriteEventSuppression(code, (EventDeclarationSyntax)node);
                                    handled = true;
                                    break;
                                case SyntaxKind.DelegateDeclaration:
                                    suppression = SuppressMessageHelper.WriteDelegateSuppression(code, (DelegateDeclarationSyntax)node);
                                    handled = true;
                                    break;
                                case SyntaxKind.InterfaceDeclaration:
                                    suppression = SuppressMessageHelper.WriteInterfaceSuppression(code, (InterfaceDeclarationSyntax)node);
                                    handled = true;
                                    break;
                                case SyntaxKind.EnumDeclaration:
                                suppression = SuppressMessageHelper.WriteEnumSuppression(code, (EnumDeclarationSyntax)node);
                                    handled = true;
                                break;
                                case SyntaxKind.StructDeclaration:
                                    suppression = SuppressMessageHelper.WriteClassSuppression(code, (StructDeclarationSyntax)node);
                                    handled = true;
                                    break;
                                case SyntaxKind.ClassDeclaration:
                                    suppression = SuppressMessageHelper.WriteClassSuppression(code, (ClassDeclarationSyntax)node);
                                    handled = true;
                                    break;
                                case SyntaxKind.NamespaceDeclaration:
                                    suppression = SuppressMessageHelper.WriteNamespaceSuppression(code, (NamespaceDeclarationSyntax)node);
                                    handled = true;
                                    break;
                                default:
                                    node = node.Parent;
                                    break;
                            }
                        }

                        if (!string.IsNullOrEmpty(suppression)) {
                            lines.Add(suppression);
                        }
                    }
                    foreach (var line in lines) {
                        writer.Write(line);
                    }

                    foreach (var additionalSuppression in additionalSuppressions)
                    {
                        writer.WriteLine(additionalSuppression);
                    }

                    ConsoleWriter.WriteInfo($"{lines.Count} diagnostics were suppressed for {csProject.Name}");
                }
            }
        }

        private static ImmutableArray<DiagnosticAnalyzer> GetAllAnalyzers(Project project) {
            // NuGet Analyzer packages (hardcoded as API to get NuGet references is missing).
            var analyzers =
                analyzersAssemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(IsAnalyzerType)
                    .Select(Activator.CreateInstance)
                    .Cast<DiagnosticAnalyzer>()
                    .ToList();
            // Inline Analyzer references.
            analyzers.AddRange(
                project.AnalyzerReferences.SelectMany(reference => reference.GetAnalyzers(project.Language)).Distinct());
            return ImmutableArray.CreateRange(analyzers);
        }

        private static List<Tuple<Diagnostic, SyntaxNode, SemanticModel>> GetDiagnostics(Project project, out bool hadCompilerErrors) {
            hadCompilerErrors = false;
            var diagnostics = new List<Tuple<Diagnostic, SyntaxNode, SemanticModel>>();
            var documents = project.Documents.ToArray();
            var compilationResult = project.GetCompilationAsync().Result;
            // ReSharper disable once AssignNullToNotNullAttribute
            var compilationResultWithAnalyzers = compilationResult.WithAnalyzers(GetAllAnalyzers(project));
            var diags = compilationResultWithAnalyzers.GetAllDiagnosticsAsync().Result;
            foreach (var diag in diags) {
                // Only suppress errors.
                if (diag.Severity != DiagnosticSeverity.Error) {
                    continue;
                }
                // Ignore the deprecation error for FxCop
                if (diag.Id == "CA9998") {
                    continue;
                }
                // Ignore compilation errors
                if (diag.Id.StartsWith("CS", StringComparison.OrdinalIgnoreCase)) {
                    ConsoleWriter.WriteWarning($"Compiler error {diag.Id} in {diag.Location.GetLineSpan().Path}.");
                    hadCompilerErrors = true;
                    continue;
                }
                if (diag.Location == Location.None || diag.Location.IsInMetadata) {
                    diagnostics.Add(new Tuple<Diagnostic, SyntaxNode, SemanticModel>(diag, null, null));
                } else {
                    var tree = diag.Location.SourceTree;
                    if (tree != null) {
                        var diagPath = diag.Location.SourceTree?.FilePath;
                        var diagFile = Path.GetFileName(diagPath);
                        for (int i = 0; i < documents.Length; i++) {
                            var document = documents[i];

                            if (document.Name == diagFile) {
                                var model = document.GetSemanticModelAsync().Result;
                                var node = FindNodeAtLocation(tree, diag.Location);
                                diagnostics.Add(new Tuple<Diagnostic, SyntaxNode, SemanticModel>(diag, node, model));
                                break;
                            }
                        }
                    }
                }
            }

            return diagnostics;
        }

        private static SyntaxNode FindNodeAtLocation(SyntaxTree tree, Location loc) {
            return tree.GetRoot().FindNode(loc.SourceSpan);
        }

        private static bool IsAnalyzerType(Type type) {
            return !type.IsAbstract && type.IsSubclassOf(typeof(DiagnosticAnalyzer));
        }
    }
}
