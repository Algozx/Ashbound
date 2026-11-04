using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Ashbound.Editor
{
    /// <summary>
    /// Builds the WebGL player into docs/, the folder GitHub Pages serves.
    /// Pass -buildOutput to write a throwaway build somewhere else.
    /// </summary>
    public static class WebGLBuild
    {
        [MenuItem("Ashbound/Build WebGL")]
        public static void Build()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string output = ReadArgument("-buildOutput") ?? Path.Combine(projectRoot, "docs");

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = output,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            });

            bool succeeded = report.summary.result == BuildResult.Succeeded;
            if (succeeded)
            {
                // Without this file Pages runs Jekyll and skips some of the build output.
                File.WriteAllText(Path.Combine(output, ".nojekyll"), string.Empty);
            }

            Debug.Log($"WebGL build {report.summary.result}: {report.summary.totalSize / (1024 * 1024)} MB in {output}");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(succeeded ? 0 : 1);
            }
        }

        private static string ReadArgument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
