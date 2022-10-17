// Copyright Koninklijke Philips N.V. 2022

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Represents the Suppression File in a CS Project
    /// </summary>
    public sealed class SuppressionFile {
        private IEnumerable<string> _suppressions;

        internal SuppressionFile(IEnumerable<string> suppressions) {
            _suppressions = suppressions.ToArray();
        }

        /// <summary>
        /// Serializes the Suppression in the current instance to a file.
        /// </summary>
        /// <param name="filePath">Absolute path of the file to which the suppression statements
        /// should be return to.Existing file is overwritten.</param>
        public void SerializeToFile(string filePath) {
            if (string.IsNullOrEmpty(filePath)) {
                throw new ArgumentException("'filePath' must have a valid absolute path value.");
            }
            SuppressionFileParser.AddUsingStatement(filePath);
            using (var stream = File.Open(filePath, FileMode.Append)) {
                using (var writer = new StreamWriter(stream)) {
                    foreach (var suppression in _suppressions) {
                        writer.WriteLine(suppression);
                    }
                }
            }
        }
    }
}
