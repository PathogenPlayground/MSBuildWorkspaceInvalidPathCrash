using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;

namespace DumpAdditionalFilesTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Load project
            MSBuildLocator.RegisterDefaults();
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Project project = workspace.OpenProjectAsync(@"../../../MalformedProject/MalformedProject.csproj").Result;

            // Print out additional files
            foreach (TextDocument additionalDocument in project.AdditionalDocuments)
            {
                Console.WriteLine($"Additional file: '{additionalDocument.FilePath}'");
            }

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
