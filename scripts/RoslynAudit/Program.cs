using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;
using System.Text.RegularExpressions;

if (args.Length != 1) throw new ArgumentException("Expected repository root");
string baseDir = args[0];
var report = new List<Metric>();
var literals = new List<object>();
var comments = new List<object>();
foreach (string folder in new[] {"SportEvents.Core", "SportEvents.Console"})
foreach (string file in Directory.EnumerateFiles(Path.Combine(baseDir,folder), "*.cs", SearchOption.AllDirectories))
{
    if (file.Contains("\\obj\\") || file.Contains("\\bin\\")) continue;
    string rel=Path.GetRelativePath(baseDir,file).Replace('\\','/');
    var syntax=CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();
    foreach (var method in syntax.DescendantNodes().OfType<BaseMethodDeclarationSyntax>())
    {
        var nodes=method.DescendantNodes(n=> n==method || n is not AnonymousFunctionExpressionSyntax && n is not LocalFunctionStatementSyntax).ToList();
        int cc=1+nodes.Count(n=>n is IfStatementSyntax or ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax or CatchClauseSyntax or ConditionalExpressionSyntax or CaseSwitchLabelSyntax or CasePatternSwitchLabelSyntax or SwitchExpressionArmSyntax || (n is BinaryExpressionSyntax b && (b.IsKind(SyntaxKind.LogicalAndExpression)||b.IsKind(SyntaxKind.LogicalOrExpression)||b.IsKind(SyntaxKind.CoalesceExpression))));
        int maxdepth=Depth(method,0);
        int line=method.GetLocation().GetLineSpan().StartLinePosition.Line+1;
        int logical=method.ToFullString().Split('\n').Count(s=> {string t=s.Trim();return t.Length>0 && t!="{" && t!="}" && !t.StartsWith("//");});
        string name=method switch {MethodDeclarationSyntax m=>m.Identifier.ValueText,ConstructorDeclarationSyntax c=>c.Identifier.ValueText,_=>method.Kind().ToString()};
        report.Add(new Metric(rel,name,line,cc,maxdepth,logical));
    }
    foreach(var lit in syntax.DescendantNodes().OfType<LiteralExpressionSyntax>().Where(n=>n.IsKind(SyntaxKind.NumericLiteralExpression)))
    {
        if (lit.Token.ValueText is "0" or "1") continue;
        if(lit.Ancestors().OfType<FieldDeclarationSyntax>().Any(f=>f.Modifiers.Any(t=>t.IsKind(SyntaxKind.ConstKeyword))))continue;
        if(lit.Ancestors().Any(n=>n is AttributeSyntax || n is EnumMemberDeclarationSyntax))continue;
        literals.Add(new {file=rel,line=lit.GetLocation().GetLineSpan().StartLinePosition.Line+1,number=lit.Token.ValueText,context=lit.Parent?.ToString()});
    }
    foreach(var c in syntax.DescendantTrivia().Where(t=>t.IsKind(SyntaxKind.SingleLineCommentTrivia)))
    {
        string t=c.ToString().TrimStart('/',' ');
        if(Regex.IsMatch(t,@"^(if\s*\(|var\s+|return\s+|throw\s+|foreach\s*\(|Console\.|\w+\s*=)"))
            comments.Add(new {file=rel,line=c.GetLocation().GetLineSpan().StartLinePosition.Line+1,text=t});
    }
}
Console.WriteLine(JsonSerializer.Serialize(new {
    scope="Core+Console (Roslyn AST)",
    methods=report.Count,
    max_cc=report.Max(x=>x.Cc),
    max_depth=report.Max(x=>x.Depth),
    max_logical_lines=report.Max(x=>x.Logical),
    top=report.OrderByDescending(x=>x.Cc).ThenByDescending(x=>x.Logical).Take(25),
    candidates_for_magic_numbers=literals,
    candidate_commented_code=comments
},new JsonSerializerOptions{WriteIndented=true}));
static int Depth(SyntaxNode n,int depth)
{
    int current=depth+(n is IfStatementSyntax or ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax or SwitchStatementSyntax or CatchClauseSyntax ? 1:0);
    int max=current;
    foreach(var c in n.ChildNodes())
        if(c is not AnonymousFunctionExpressionSyntax && c is not LocalFunctionStatementSyntax)
            max=Math.Max(max,Depth(c,current));
    return max;
}
record Metric(string File,string Name,int Line,int Cc,int Depth,int Logical);
