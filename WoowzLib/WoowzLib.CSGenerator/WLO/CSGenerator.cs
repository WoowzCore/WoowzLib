using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualBasic.FileIO;

namespace WLO;

public class CSGenerator{
    public readonly StringBuilder SB = new StringBuilder();
    
    public CSGenerator(){ Clear(); }
    public CSGenerator Clear(){
        SB.Clear();
        return this;
    }

    public override string ToString() => SB.ToString();

    public static string Refract(string Code){
        SyntaxTree Tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(Code);

        string Formatted = Tree.GetRoot().NormalizeWhitespace(indentation: "\t", eol: "\n").ToFullString();

        Formatted = Regex.Replace(Formatted, @"\r?\n\s*\{", "{");

        Formatted = Regex.Replace(Formatted, @"\{\s*\}", "{}");

        string[] Lines = Formatted.Split('\n');
        StringBuilder SB = new StringBuilder();

        foreach(string Line in Lines){
            string CurrentLine = Line;
            bool HasSpaceMarker = CurrentLine.Contains("/* __CSG_SPACE__ */");

            if(HasSpaceMarker){
                CurrentLine = CurrentLine.Replace("/* __CSG_SPACE__ */", "").TrimEnd();
            }

            if(!string.IsNullOrWhiteSpace(CurrentLine)){
                SB.AppendLine(CurrentLine);
            }

            if(HasSpaceMarker){
                SB.AppendLine();
            }
        }
        
        return SB.ToString().Trim();
    }

    // ----------------------------------------------------------------------
    
    public enum E_Declaration{ Class, Struct, Interface, Enum, Record, Delegate }
    public static string ToString_Declaration(E_Declaration E) => E switch{
        E_Declaration.Class     => "class",
        E_Declaration.Struct    => "struct",
        E_Declaration.Interface => "interface",
        E_Declaration.Enum      => "enum",
        E_Declaration.Record    => "record",
        E_Declaration.Delegate  => "delegate",
    };
    
    public enum E_AM_Declaration{ Public, Private, Protected, Internal }
    public static string ToString_AM_Declaration(E_AM_Declaration E) => E switch{
        E_AM_Declaration.Public    => "public",
        E_AM_Declaration.Private   => "private",
        E_AM_Declaration.Protected => "protected",
        E_AM_Declaration.Internal  => "internal",
    };
    
    public enum E_AM{ Public, PublicStatic, PublicConst, Private, PrivateStatic, Protected, Internal, Const }
    public static string ToString_AM(E_AM E) => E switch{
        E_AM.Public        => "public",
        E_AM.PublicStatic  => "public static",
        E_AM.Private       => "private",
        E_AM.PrivateStatic => "private static",
        E_AM.Protected     => "protected",
        E_AM.Internal      => "internal",
        E_AM.Const         => "const",
        E_AM.PublicConst   => "public const"
    };

    public static string FormatParams(string[] Params){
        if(Params.Length == 0){ return string.Empty; }

        List<string> Pairs = [];
        for(int i = 0; i < Params.Length; i += 2){
            string Type = Params[i];
            string Name = (i + 1 < Params.Length) ? Params[i + 1] : "";
            Pairs.Add($"{Type} {Name}".Trim());
        }
        return string.Join(", ", Pairs);
    }
    
    // ----------------------------------------------------------------------

    public string CurrentDeclaration = "";
    
    // ----------------------------------------------------------------------

    public void Space() => SB.Append("/* __CSG_SPACE__ */");



    public void Comment(string Comment) => SB.Append($"/* {Comment} */");
    
    
    
    public void Namespace(string Name, Action Content){
        SB.Append($"namespace {Name}{{");
        Content.Invoke();
        SB.Append("}");
    }

    public void Namespace(string Name){
        SB.Append($"namespace {Name};");
    }



    public void Declaration(E_AM_Declaration AM, E_Declaration Declaration, string Name, string Inheritance, Action Content){
        CurrentDeclaration = Name;
        
        string Base = $"{ToString_AM_Declaration(AM)} {ToString_Declaration(Declaration)} {Name}";
        if(string.IsNullOrEmpty(Inheritance)){
            SB.Append($"{Base}{{");
        }else{
            SB.Append($"{Base} : {Inheritance}{{");
        }

        Content.Invoke();
        
        SB.Append("}");
    }

    

    public void Class(E_AM_Declaration AM, string Name, string Inheritance, Action Content) => Declaration(AM, E_Declaration.Class, Name, Inheritance, Content);
    public void Class(E_AM_Declaration AM, string Name, Action Content) => Class(AM, Name, "", Content);



    public void AddField(E_AM AM, string ValueType, string Name, string Default){
        SB.Append($"{ToString_AM(AM)} {ValueType} {Name}{(string.IsNullOrEmpty(Default) ? "" : $" = {Default}")};");
    }
    public void AddField(E_AM AM, string ValueType, string Name) => AddField(AM, ValueType, Name, "");



    public void AddProperty(E_AM AM, string ValueType, string Name, string Logic, string Default){
        SB.Append($"{ToString_AM(AM)} {ValueType} {Name}{Logic}{(string.IsNullOrEmpty(Default) ? "" : $" = {Default}")}{(Logic.StartsWith("=>") ? ";" : "")}");
    }
    public void AddProperty(E_AM AM, string ValueType, string Name, string Logic) => AddProperty(AM, ValueType, Name, Logic, "");



    public void AddConstructor(E_AM AM, string[] Params, string Base, string Logic){
        SB.Append($"{ToString_AM(AM)} {CurrentDeclaration}({FormatParams(Params)}){(string.IsNullOrEmpty(Base) ? "" : " : " + Base)}{Logic}");
    }
    public void AddConstructor(E_AM AM, string[] Params, string Logic) => AddConstructor(AM, Params, "", Logic);
}