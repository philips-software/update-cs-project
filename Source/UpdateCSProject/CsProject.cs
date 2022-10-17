// Copyright Koninklijke Philips N.V. 2021

using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Philips.Tools.UpdateCsProject.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Project = Microsoft.Build.Evaluation.Project;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// In-memory representation of a csproj file entity
    /// </summary>
    internal class CsProject {
        private Project _project;

        public CsProject(string path) {
            Path = path;
        }

        public string Path { get; }

        public void Load() {
            try {
                _project = Project.FromFile(Path, new ProjectOptions() {
                    LoadSettings = ProjectLoadSettings.IgnoreMissingImports | ProjectLoadSettings.IgnoreEmptyImports | ProjectLoadSettings.DoNotEvaluateElementsWithFalseCondition | ProjectLoadSettings.IgnoreInvalidImports,
                    ProjectCollection = new ProjectCollection()
                });
            } catch (Exception) {
                _project = null;
            }
        }

        public bool Save() {
            var saved = true;
            try {
                Project.Save();
            } catch (Exception) {
                _project = null;
                saved = false;
            }
            return saved;
        }

        public bool CanSave() {
            return CanLoad() && Project != null;
        }

        public bool CanLoad() {
            if (string.IsNullOrEmpty(Path))
                return false;
            if (!File.Exists(Path))
                return false;
            return true;
        }

        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        public Project Project => _project;

        public bool HasItemInProject(string itemType, string include) {
            bool found = false;
            var projectItems = _project.GetItems(itemType);
            var comparer = StringComparer.OrdinalIgnoreCase;
            var comparison = StringComparison.OrdinalIgnoreCase;
            var items = projectItems.Select(item => item.UnevaluatedInclude);
            found = items.Any(item => item.EndsWith(include, comparison));
            return found;
        }

        public bool HasPropertyInProject(string propertyName)
        {
            return _project.GetProperty(propertyName) != null;
        }

        public void IncludeInProject(string itemType, string relativePath, Dictionary<string, string> metadata = null) {
            if (string.IsNullOrWhiteSpace(itemType)) {
                throw new ArgumentNullException(nameof(itemType));
            }
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentNullException(nameof(relativePath));
            }
            var projectItems = _project.GetItems(itemType);

            // Before adding, check if the file path exists
            if (projectItems.Any(projectItem => projectItem.EvaluatedInclude.EqualsIgnoreCase(relativePath))) {
                return;
            }
            
            _project.AddItem(itemType, relativePath, metadata);
        }

        public void ExcludeFromProject(string itemType, string relativePath) {
            if (string.IsNullOrWhiteSpace(itemType)) {
                throw new ArgumentNullException(nameof(itemType));
            }
            if (string.IsNullOrWhiteSpace(relativePath)) {
                throw new ArgumentNullException(nameof(relativePath));
            }
            var projectItems = ImmutableArray.CreateRange(_project.GetItems(itemType));

            // Before removing, check if the file path exists
            var projectItem = projectItems.SingleOrDefault(item => item.EvaluatedInclude.EqualsIgnoreCase(relativePath) && !item.IsImported);
            if (projectItem == null) {
                return;
            }

            _project.RemoveItem(projectItem);
        }

        public void RemovePropertyInProject(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) {
                throw new ArgumentNullException(nameof(propertyName));
            }

            _project.RemoveProperty(_project.GetProperty(propertyName));
        }


        public void RemovePropertyInProjectWithValue(string propertyName, string propertyValue) {
            if (string.IsNullOrWhiteSpace(propertyName)) {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (string.IsNullOrWhiteSpace(propertyValue)) {
                throw new ArgumentNullException(nameof(propertyValue));
            }

            var property = _project.GetProperty(propertyName);
            if (property.EvaluatedValue.Contains(propertyValue)) {
                _project.RemoveProperty(property);
            }
        }
    }
}
