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
            string? License,
            Project[] Dependencies,
            bool ContainsWoowzLib = true
        ){
            this.Name        = Name        ?? "Неизвестный проект";
            this.Version     = Version     ?? 0;
            this.Author      = Author      ?? "Аноним";
            this.Description = Description ?? "У этого проекта нету описания.";
            this.URL         = URL         ?? "";
            this.License     = License     ?? "MIT";
            __Dependencies   = [..Dependencies];
            if(ContainsWoowzLib && !__Dependencies.Contains(WL.WoowzLib.ProjectInfo)){
                __Dependencies.Insert(0, WL.WoowzLib.ProjectInfo);
            }
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
        
        /// <summary>
        /// Зависимости проекта
        /// </summary>
        public IReadOnlyList<Project> Dependencies => __Dependencies.AsReadOnly();
        private readonly List<Project> __Dependencies;

        // ----------------------------------------------------------------------
        
        public bool IsValid             () => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Author);
        public bool AllDependenciesValid() => __Dependencies.All(D => D.IsValid());

        public bool HasDependency(string Name                ) => __Dependencies.Any(D => D.Name == Name                        );
        public bool HasDependency(string Name, ushort Version) => __Dependencies.Any(D => D.Name == Name && D.Version == Version);
        public bool HasDependency(Project ProjectInfo        ) => __Dependencies.Any(D => D == ProjectInfo                      );
        
        public Project? GetDependency(string Name) => __Dependencies.FirstOrDefault(D => D.Name == Name);

        public bool HasWoowzLib() => HasDependency(WL.WoowzLib.ProjectInfo);
        
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
            private string?        __Name               ;
            private ushort?        __Version            ;
            private string?        __Author             ;
            private string?        __Description        ;
            private string?        __URL                ;
            private string?        __License            ;
            private List<Project>? __Dependencies       ;
            private bool           __NotContainsWoowzLib;

            /// <summary>
            /// Название проекта
            /// </summary>
            public Builder Name(string? Name){ __Name = Name; return this; }
            
            /// <summary>
            /// Версия проекта
            /// </summary>
            public Builder Version(ushort? Version){ __Version = Version; return this; }
            
            /// <summary>
            /// Автор проекта
            /// </summary>
            public Builder Author(string? Author){ __Author = Author; return this; }
            
            /// <summary>
            /// Описание проекта
            /// </summary>
            public Builder Description(string? Description){ __Description = Description; return this; }
            
            /// <summary>
            /// Главная ссылка проекта (Можно ссылку на репозиторий или сайт)
            /// </summary>
            public Builder URL(string? URL){ __URL = URL; return this; }
            
            /// <summary>
            /// Лицензия проекта
            /// </summary>
            public Builder License(string? License){ __License = License; return this; }

            /// <summary>
            /// Делает проект не содержащим в себе WoowzLib
            /// </summary>
            public Builder NotContainsWoowzLib(){ __NotContainsWoowzLib = true; return this; }

            /// <summary>
            /// Добавляет зависимость
            /// </summary>
            public Builder AddDependency(Project ProjectInfo){
                __Dependencies ??= [];
                __Dependencies.Add(ProjectInfo); return this;
            }

            /// <summary>
            /// Добавляет зависимости
            /// </summary>
            public Builder AddDependencies(params Project[] ProjectInfos){
                __Dependencies ??= [];
                __Dependencies.AddRange(ProjectInfos); return this;
            }

            /// <summary>
            /// Превращает в Project
            /// </summary>
            public Project Build() => new Project(
                __Name,
                __Version,
                __Author,
                __Description,
                __URL,
                __License,
                __Dependencies != null ? __Dependencies.ToArray() : [],
                !__NotContainsWoowzLib
            );
        }
    }
}