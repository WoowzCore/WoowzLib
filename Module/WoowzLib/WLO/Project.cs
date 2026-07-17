using System.Diagnostics.CodeAnalysis;

namespace WLO{
    /// <summary>
    /// Информация об проекте
    /// </summary>
    public readonly struct Project : IEquatable<Project>{
        public Project(
            string? Name,
            ushort? Version,
            string? Author,
            string? Description,
            string? URL,
            string? License
        ){
            this.Name        = Name        ?? "Unknown project";
            this.Version     = Version     ?? 0;
            this.Author      = Author      ?? "Anonymous";
            this.Description = Description ?? "This project does not have a description.";
            this.URL         = URL         ?? "";
            this.License     = License     ?? "MIT";
        }

        public readonly string Name;
        public readonly ushort Version;
        public readonly string Author;
        public readonly string Description;
        public readonly string URL;
        public readonly string License;

        /*private readonly List<Project> __Dependencies;
        public IReadOnlyList<Project> Dependencies => __Dependencies.AsReadOnly();

        private readonly Dictionary<string, object> __Metadata;
        public IReadOnlyDictionary<string, object> Metadata => __Metadata.AsReadOnly();*/

        // ----------------------------------------------------------------------
        
        public bool IsValid() => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Author);
        
        // ----------------------------------------------------------------------
        
        public override string ToString() => $"WLO.Project(\"{Name}\", {Version}, \"{Author}\")";

        public override bool Equals(object? Object) => Object is Project Other && Equals(Other);

        public bool Equals(Project Other) => Name == Other.Name && Version == Other.Version && Author == Other.Author;

        public override int GetHashCode() => HashCode.Combine(Name, Version, Author);

        public static bool operator ==(Project Left, Project Right) =>  Left.Equals(Right);
        public static bool operator !=(Project Left, Project Right) => !Left.Equals(Right);

        // ----------------------------------------------------------------------
        
        public static Builder Create() => new Builder();
        
        public struct Builder{
            private string? __Name;
            private ushort? __Version;
            private string? __Author;
            private string? __Description;
            private string? __URL;
            private string? __License;

            public Builder Name       (string? Name       ){ __Name        = Name       ; return this; }
            public Builder Version    (ushort? Version    ){ __Version     = Version    ; return this; }
            public Builder Author     (string? Author     ){ __Author      = Author     ; return this; }
            public Builder Description(string? Description){ __Description = Description; return this; }
            public Builder URL        (string? URL        ){ __URL         = URL        ; return this; }
            public Builder License    (string? License    ){ __License     = License    ; return this; }

            public Project Build() => new Project(
                __Name,
                __Version,
                __Author,
                __Description,
                __URL,
                __License
            );
        }
    }
}