using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.CodeGen
{
    /// <summary>
    /// The CodeGeneratorValidator is a static class that contains methods for validating, and writing code generated by <see cref="Writer"/> structs.
    /// </summary>
    public static class CodeGeneratorValidator
    {
        /// <summary>
        /// Represents Code to be validated by the validator
        /// </summary>
        public struct ValidationData
        {
            /// <summary>
            /// The code to validate
            /// </summary>
            public Writer code;

            /// <summary>
            /// The path where the file will be written to.
            /// </summary>
            public string desiredOutputPath;
        }

        /// <summary>
        /// Validates and Writes the code found within <paramref name="data"/> synchronously
        /// </summary>
        /// <param name="data">The data to validate and write</param>
        /// <returns>True if the file was written to, false if the data in the writer is not different from the data that's already written</returns>
        public static bool Validate(ValidationData data)
        {
            var code = data.code.ToString();

            if (File.Exists(data.desiredOutputPath))
            {
                var existingCode = File.ReadAllText(data.desiredOutputPath);
                if (existingCode == code || WithAllWhitespaceStripped(existingCode) == WithAllWhitespaceStripped(code))
                    return false;
            }

            CheckOut(data.desiredOutputPath, code);
            return true;
        }

        /// <summary>
        /// Validates and Writes the code found within <paramref name="data"/> asynchronously using a Coroutine
        /// </summary>
        /// <param name="data">The data to validate and write</param>
        /// <returns>True if the file was written to, false if the data in the writer is not different from the data that's already written. null if its waiting for the file to be written</returns>
        public static IEnumerator<bool?> ValidateCoroutine(ValidationData data)
        {
            var code = data.code.ToString();
            if(File.Exists(data.desiredOutputPath))
            {
                var existingCode = File.ReadAllText(data.desiredOutputPath);
                if (existingCode == code || WithAllWhitespaceStripped(existingCode) == WithAllWhitespaceStripped(code))
                {
                    yield return false;
                    yield break;
                }
            }

            var subroutine = CheckOutCoroutine(data.desiredOutputPath, code);
            while(subroutine.MoveNext())
            {
                yield return null;
            }
            yield return true;
        }

        private static void CheckOut(string path, string code)
        {
            if (string.IsNullOrEmpty(path))
                throw new NullReferenceException("data.desiredPath");

            // Make path relative to project folder.
            var projectPath = Application.dataPath;
            if (path.StartsWith(projectPath) && path.Length > projectPath.Length &&
                (path[projectPath.Length] == '/' || path[projectPath.Length] == '\\'))
                path = path.Substring(0, projectPath.Length + 1);
            AssetDatabase.MakeEditable(path);

            File.WriteAllText(path, code);
        }


        private static IEnumerator CheckOutCoroutine(string path, string code)
        {
            if (path.IsNullOrEmptyOrWhiteSpace())
                throw new NullReferenceException("data.desiredPath");

            var projectPath = Application.dataPath;
            if (path.StartsWith(projectPath) && path.Length > projectPath.Length &&
                (path[projectPath.Length] == '/' || path[projectPath.Length] == '\\'))
                path = path.Substring(0, projectPath.Length + 1);
                AssetDatabase.MakeEditable(path);

            var task = File.WriteAllTextAsync(path, code);
            while(!task.IsCompleted)
            {
                yield return null;
            }
        }


        private static string WithAllWhitespaceStripped(string str)
        {
            var buffer = new StringBuilder();
            foreach (var ch in str)
                if (!char.IsWhiteSpace(ch))
                    buffer.Append(ch);
            return buffer.ToString();
        }
    }
}