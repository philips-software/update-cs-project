// Copyright Koninklijke Philips N.V. 2021

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Identifies all csproj files under a given folder and updates each of them via passed
    /// <see cref="ICsProjectUpdater"/> instance
    /// </summary>
    internal class BulkCsProjectUpdater {
        public BulkCsProjectUpdater(Arguments arguments, ICsProjectUpdater csProjectUpdater) {
            _arguments = arguments;
            this._csProjectUpdater = csProjectUpdater;
            InitializeResults();
        }

        private void InitializeResults() {
            Completed = new List<string>();
            Failures = new List<string>();
            Warnings = new List<string>();
            Skipped = new List<string>();
        }

        public void Update() {
            _ = GetAllCsProjects(_arguments.Path).Any(p =>
            {
                UpdateProject(p);
                return false;
            });
        }

        private void UpdateProject(CsProject csProject) {
            ConsoleWriter.WriteMessage($"Processing {csProject.Path}");
            if (!csProject.CanLoad()) {
                Failures.Add(csProject.Path);
                return;
            }

            csProject.Load();
            var updateStatus = _csProjectUpdater.Update(csProject, _arguments);
            switch (updateStatus) {
                case UpdateStatus.Fail:
                    Failures.Add(csProject.Name);
                    break;
                case UpdateStatus.Warn:
                    Warnings.Add(csProject.Name);
                    break;
                case UpdateStatus.Skipped:
                    Skipped.Add(csProject.Name);
                    break;
            }

            if (updateStatus == UpdateStatus.Success) {
                if (!csProject.CanSave()) {
                    Failures.Add(csProject.Name);
                    return;
                }

                if (!csProject.Save()) {
                    Failures.Add(csProject.Name);
                    return;
                }

                Completed.Add(csProject.Name);
            }
        }

        private static IEnumerable<CsProject> GetAllCsProjects(string path) {
            var csProjectFiles= Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);
            return csProjectFiles.Select(x => new CsProject(x));
        }

        public List<string> Skipped { get; private set; }

        public List<string> Warnings { get; private set; }

        public List<string> Failures { get; private set; }

        public List<string> Completed { get; private set; }

        private readonly Arguments _arguments;
        private readonly ICsProjectUpdater _csProjectUpdater;
    }
}