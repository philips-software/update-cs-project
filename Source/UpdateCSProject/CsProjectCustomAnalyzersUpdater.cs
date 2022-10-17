// Copyright Koninklijke Philips N.V. 2021

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Build.Evaluation;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Add Philips.Platform.Analyzers analyzer if not already present
    /// </summary>
    internal class CsProjectCustomAnalyzersUpdater : ICsProjectUpdater {
        public UpdateStatus Update(CsProject csProject, Arguments arguments) {
            var project = csProject.Project;
            var analyzerPropertyCollection = project.GetItems("Analyzer");
            if (!IsCustomAnalyzerAlreadyExists(analyzerPropertyCollection)) return UpdateStatus.Skipped;
            var analyzerRelativePath = Path.GetFullPath(GetAnalyzerAssemblyPath(arguments.Path));
            project.AddItem("Analyzer", analyzerRelativePath, new Dictionary<string, string>());
            return UpdateStatus.Success;
        }

        private static bool IsCustomAnalyzerAlreadyExists(ICollection<ProjectItem> analyzerPropertyCollection) {
            if (analyzerPropertyCollection != null) {
                foreach (var analyzerProperty in analyzerPropertyCollection) {
                    if (analyzerProperty.EvaluatedInclude.EndsWith("Philips.Platform.Analyzers.dll",
                        StringComparison.OrdinalIgnoreCase)) {
                        return false;
                    }
                }
            }

            return true;
        }

        private string GetAnalyzerAssemblyPath(string path) {
            var viewRoot = GetViewRoot(path);
            if (string.IsNullOrEmpty(viewRoot)) {
                return @"C:\views\trunk\Build\Output\Bin\Philips.Platform.Analyzers.dll";
            } else {
                return Path.Combine(viewRoot, @"Build\Output\Bin\Philips.Platform.Analyzers.dll");
            }
        }

        private string GetViewRoot(string viewSubDir) {
            var found = BuildMenuExeExists(viewSubDir);
            if (found) {
                return viewSubDir;
            } else {
                var parent = new DirectoryInfo(viewSubDir).Parent;
                if (parent == null) {
                    return string.Empty;
                } else {
                    return GetViewRoot(parent.FullName);
                }
            }
        }

        private static bool BuildMenuExeExists(string dir) {
            return File.Exists(Path.Combine(dir, "BuildMenu.exe"));
        }
    }
}
