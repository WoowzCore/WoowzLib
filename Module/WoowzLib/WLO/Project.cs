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
            this.Name        = Name        ?? "Неизвестный проект";
            this.Version     = Version     ?? 0;
            this.Author      = Author      ?? "Аноним";
            this.Description = Description ?? "У этого проекта нету описания.";
            this.URL         = URL         ?? "";
            this.License     = License     ?? "MIT";
        }

        /// <summary>
        /// Название проекта
        /// </summary>
        public readonly string Name;
        
        /// <summary>
        /// Версия проекта
        /// </summary>
        public readonly ushort Version;
        
        /// <summary>
        /// Автор проекта
        /// </summary>
        public readonly string Author;
        
        /// <summary>
        /// Описание проекта
        /// </summary>
        public readonly string Description;
        
        /// <summary>
        /// Главная ссылка проекта (Можно ссылку на репозиторий или сайт)
        /// </summary>
        public readonly string URL;
        
        /// <summary>
        /// Лицензия проекта
        /// </summary>
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

            /// <summary>
            /// Название проекта
            /// </summary>
            public Builder Name       (string? Name       ){ __Name        = Name       ; return this; }
            
            /// <summary>
            /// Версия проекта
            /// </summary>
            public Builder Version    (ushort? Version    ){ __Version     = Version    ; return this; }
            
            /// <summary>
            /// Автор проекта
            /// </summary>
            public Builder Author     (string? Author     ){ __Author      = Author     ; return this; }
            
            /// <summary>
            /// Описание проекта
            /// </summary>
            public Builder Description(string? Description){ __Description = Description; return this; }
            
            /// <summary>
            /// Главная ссылка проекта (Можно ссылку на репозиторий или сайт)
            /// </summary>
            public Builder URL        (string? URL        ){ __URL         = URL        ; return this; }
            
            /// <summary>
            /// Лицензия проекта
            /// </summary>
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