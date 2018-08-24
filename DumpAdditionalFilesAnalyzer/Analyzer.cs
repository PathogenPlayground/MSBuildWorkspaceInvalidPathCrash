using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DumpAdditionalFilesAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Analyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor diagnostic = new DiagnosticDescriptor
        (
            "AdditionalFilesAnalyzer",
            "Additional file",
            "Additional file: '{0}'",
            "AdditionalFilesAnalyzer",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(diagnostic);

        public override void Initialize(AnalysisContext context)
            => context.RegisterCompilationAction(DumpAdditionalFiles);

        private static void DumpAdditionalFiles(CompilationAnalysisContext context)
        {
            foreach (AdditionalText additionalFile in context.Options.AdditionalFiles)
            {
                context.ReportDiagnostic(Diagnostic.Create(diagnostic, Location.None, additionalFile.Path));
            }
        }
    }
}
