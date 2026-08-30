using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using WLI.GPU;
using WLO.Render;
using WLO.Render.Hardware;

namespace WLO.GPU;

public partial class GLShader : WLI.GPU.GLResource, WLI.GPU.Shader{
    public WLI.GPU.Shader.Type Stage{ get; }
    public Builder.Metadata? Metadata{ get; }

    public GLShader(OpenGL Render, WLI.GPU.Shader.Type Stage, string Source) : base(Render){
        this.Stage = Stage;
        ID = OpenGL.CompileGLSL(Owner.API, Stage, Source);
        Metadata = null;
    }

    public GLShader(OpenGL Render, Builder Builder) : base(Render){
        Stage = Builder.Type;
        (string Source, Builder.Metadata Metadata) = Builder.Compile();
        ID = OpenGL.CompileGLSL(Owner.API, Stage, Source);
        this.Metadata = Metadata;
    }

    public override void OnDestroy() => Owner.API.DeleteShader(ID);
    
    public partial struct Builder{
        // Настройки компилятора
        public const string Version = "430 core";
        // Игнорировать не существующие Uniform's?
        public /*static*/const bool IgnoreNotExistUniforms = false; // todo, говно, потому-что а как указать ID тогда?
        // Делает уникальные названия у функций
        public static bool UniqueFunctionNames = true;
        // Удаляет не используемые функции
        public static bool ClearUnusedFunctions = true;
        // Удаляет комментарии
        public static bool ClearComments = true;
        // Входная функция
        public static string EntryPoint = "Main";
        // Удаляет не используемые Uniform's
        public static bool ClearUnusedUniforms = true;
        // Удаляет не используемые Texture's
        public static bool ClearUnusedTextures = true;

        /// Что-бы вписать ссылку на другой код, к примеру вставить код по Key = "Example", будет: "#Example#", и он буквально заменит его на указанный код по ключу, если "#Example", то вставит но в конце добавил новую строку, работает рекурсивно
        public static void AddCode(string Key, string RawCode){
            Key = Key.Trim();
            try{
                if(string.IsNullOrWhiteSpace(Key)){ throw new ExceptionWL("Ключ не может быть пустым!"); }
                __Codes[Key] = RawCode;
            }catch(Exception e){
                throw new ExceptionWL($"Произошла ошибка при добавлении кода в компилятор шейдеров WL! AddCode(\"{Key}\")\nКод:\n{RawCode}", e);
            }
        }
        private static readonly Dictionary<string, string> __Codes = [];



        /// Записывает доступные Uniform's, уникальное название, тип, уникальный ID, потом в коде пишите так, например: Type = (TODO), Name = "MyTestUniform" то будет: "uniform TODO MyTestUniform;"
        public static void AddUniform(string Name, ValueType Type, int Location){
            try{
                if(!new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$").IsMatch(Name)) { throw new ExceptionWL("Указано недопустимое имя Uniform!"); }
                
                if(__Uniforms.TryGetValue(Name, out (ValueType Type, int Location) ExistingUniform)){ throw new ExceptionWL($"Uniform с таким названием уже есть! Вот же он [\"{Name}\", {ExistingUniform.Type}, {ExistingUniform.Location}]!"); }

                if(__Uniforms.Values.Any(V => V.Location == Location)){ throw new ExceptionWL("Указанный Location уже занят!"); }
                
                __Uniforms[Name] = (Type, Location);
            }catch(Exception e){
                throw new ExceptionWL($"Произошла ошибка при добавлении Uniform в компилятор шейдеров WL! AddUniform(\"{Name}\", {Type}, {Location})", e);
            }
        }
        private static readonly Dictionary<string, (ValueType Type, int Location)> __Uniforms = [];

        

        
        // todo, binding не уникальный!
        public static void AddUniformBlock<T>(string Name, uint Binding) where T : struct{
            try{
                if(!new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$").IsMatch(Name)) { throw new ExceptionWL("Указано недопустимое имя UniformBlock!"); }
                
                if(__UniformBlocks.TryGetValue(Name, out (Type StructType, uint Binding) ExistingUniformBlock)){ throw new ExceptionWL($"UniformBlock с таким названием уже есть! Вот же он [\"{Name}\", {ExistingUniformBlock.Binding}]!"); }
                
                __UniformBlocks[Name] = (typeof(T), Binding);
            }catch(Exception e){
                throw new ExceptionWL($"Произошла ошибка при добавлении UniformBlock в компилятор шейдеров WL! AddUniformBlock<{typeof(T).Name}>(\"{Name}\", {Binding})", e);
            }
        }
        private static readonly Dictionary<string, (Type StructType, uint Binding)> __UniformBlocks = [];


        

        // todo, нужно писать "layout NAME;"
        public static void AddLayout(string Name, VertexLayout Layout){
            try{
                if(!new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$").IsMatch(Name)) { throw new ExceptionWL("Указано недопустимое имя Layout!"); }
                
                if(__Layouts.TryGetValue(Name, out VertexLayout ExistingLayout)){ throw new ExceptionWL($"Layout с таким названием уже есть! Вот же он [\"{Name}\", {ExistingLayout}]!"); }
                
                __Layouts[Name] = Layout;
            }catch(Exception e){
                throw new ExceptionWL($"Произошла ошибка при добавлении Layout в компилятор шейдеров WL! AddLayout(\"{Name}\", {Layout})", e);
            }
        }
        private static readonly Dictionary<string, VertexLayout> __Layouts = [];
        
        
        
       public static void AddTexture(string Name, ValueType Type, uint Slot){
            try{
                if(!new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$").IsMatch(Name)) { throw new ExceptionWL("Указано недопустимое имя Texture!"); }
                
                if(__Textures.TryGetValue(Name, out (ValueType Type, uint Slot) ExistingTexture)){ throw new ExceptionWL($"Texture с таким названием уже есть! Вот же он [\"{Name}\", {ExistingTexture.Type}, {ExistingTexture.Slot}]!"); }

                __Textures[Name] = (Type, Slot);
            }catch(Exception e){
                throw new ExceptionWL($"Произошла ошибка при добавлении Texture в компилятор шейдеров WL! AddTexture(\"{Name}\", {Type}, {Slot})", e);
            }
        }
        private static readonly Dictionary<string, (ValueType Type, uint Slot)> __Textures = [];
        
        
        // ----------------------------------------------------------------------
        
        public WLI.GPU.Shader.Type Type;
        
        public Builder(WLI.GPU.Shader.Type Type){ this.Type = Type; RawCode = ""; }
        
        private string RawCode;
        
        public Builder SetCode(string RawCode){ this.RawCode = RawCode; return this; }
        
        public (string, Metadata) Compile(){
            string RawCode = this.RawCode;
            string __RawFullCode = "Не обработано";
            
            try{
                Metadata Metadata = new Metadata{ Properties = [] };

                #region Компилятор

                    #region Пред-компиляция

                        // Замена "#Example" на код
                        string ExpandCodes(string Source){
                            bool Found = true;
                            string Result = Source;
                            int SafetyCounter = 0;

                            while(Found && SafetyCounter < 100){
                                Found = false;
                                foreach(KeyValuePair<string, string> KVP in __Codes){
                                    string MarkerDirect  = $"#{KVP.Key}#";
                                    if(Result.Contains(MarkerDirect)){
                                        Found = true;
                                        Result = Regex.Replace(Result, MarkerDirect, KVP.Value);
                                    }

                                    Result = Regex.Replace(Result, $"#{KVP.Key}(?:\\r?\\n)*", M => {
                                        Found = true;
                                        return KVP.Value + new string('\n', System.Math.Max(2, M.Value.Count(C => C == '\n')));
                                    });
                                }
                                SafetyCounter++;
                            }
                            
                            return Result;
                        }
                        __RawFullCode = ExpandCodes(RawCode);
                        RawCode = __RawFullCode;

                        // Удаляет комментарии
                        if(ClearComments){
                            RawCode = Regex.Replace(RawCode, @"/\*[\s\S]*?\*/|//.*", "");
                        }
                    
                        if(string.IsNullOrWhiteSpace(RawCode)){ throw new ExceptionWL("Указан пустой код!"); }
                        
                    #endregion

                    #region Компиляция

                        StringBuilder SB = new StringBuilder();

                        // Везде где надо писать "new"
                        HashSet<string> ConstructorTypes = ["Vector2F", "Vector3F", "Vector4F", "Matrix2F", "Matrix3F", "Matrix4F"];
                        
                        Dictionary<string, string> TypeMapping = new Dictionary<string, string>{
                            {"int", "int"},
                            {"uint", "uint"},
                            
                            {"float", "float"},
                            
                            {"Vector2F", "vec2"},
                            {"Vector3F", "vec3"},
                            {"Vector4F", "vec4"},
                            
                            {"Matrix4F", "mat4"},
                            {"Matrix3F", "mat3"},
                            {"Matrix2F", "mat2"},
                            
                            {"Texture2D", "sampler2D"}
                        };
                        
                        // todo, use TypeMapping
                        ValueType StringToValueType(string String) => String switch{
                            "int"      => ValueType.Int,
                            "uint"     => ValueType.UInt,
                            
                            "float"    => ValueType.Float,
                            
                            "Vector2F" => ValueType.Vector2F,
                            "Vector3F" => ValueType.Vector3F,
                            "Vector4F" => ValueType.Vector4F,
                            
                            "Matrix4F" => ValueType.Matrix4F,
                            "Matrix3F" => ValueType.Matrix3F,
                            "Matrix2F" => ValueType.Matrix2F,
                            
                            "Texture2D" => ValueType.Texture2D,
                            
                            var _ => throw new ArgumentOutOfRangeException(nameof(String), String, null) // todo, тут нужна ошибка норм
                        };

                        HashSet<string> Keywords = ["if", "for", "while", "switch", "return", "else", "discard", "new", "in", "out"];
                        
                        Dictionary<string, string> GLSLFunctionMapping = new Dictionary<string, string>{
                            { "Texture", "texture" },
                            { "Normalize", "normalize" },
                            { "Max", "max" },
                            { "Min", "min" },
                            { "Dot", "dot" },
                            { "Cross", "cross" },
                            { "Mix", "mix" },
                            { "Clamp", "clamp" },
                            { "Abs", "abs" },
                            { "Length", "length" },
                            { "Distance", "distance" },
                            { "Sin", "sin" },
                            { "Cos", "cos" },
                            { "Tan", "tan" },
                            { "Pow", "pow" },
                            { "Exp", "exp" },
                            { "Log", "log" },
                            { "Sqrt", "sqrt" },
                            { "Floor", "floor" },
                            { "Ceil", "ceil" },
                            { "Fract", "fract" }
                        };

                        HashSet<string> AllowedGLSLNames = [
                            ..TypeMapping.Keys,
                            ..GLSLFunctionMapping.Values,
                            ..__UniformBlocks.Keys,
                            "main", "void", "discard",
                            "gl_Position", "gl_PointSize", "gl_VertexID", "gl_InstanceID", "gl_FragCoord", "gl_FrontFacing"
                        ];
                        
                        HashSet<string> ReservedNames = [
                            ..Keywords,
                            ..TypeMapping.Keys,
                            "void", "true", "false"
                        ];
                        
                        // Начало компиляции
                        
                        string Code = RawCode;
                        
                        
                        
                        // Убирает синтаксис CS
                        void FixCSharpSyntax(){
                            foreach(string TypeName in ConstructorTypes){
                                if(Regex.IsMatch(Code,$@"(?<!new\s+)\b{TypeName}\s*\(")){
                                    throw new ExceptionWL($"Отсутствует у конструктора [{TypeName}] в начале \"new\"!");
                                }
                            }
                            
                            foreach(string TypeName in ConstructorTypes){
                                Code = Regex.Replace(Code, $@"\bnew\s+({TypeName})\b", "$1");
                            }

                            Code = Regex.Replace(Code, @"(\d+\.\d+)[fF]", "$1");
                            Code = Regex.Replace(Code, @"(?<!\.)\b(\d+)[fF]\b", "$1.0");
                        }
                        FixCSharpSyntax();
                        
                        
                        
                        // Заменяет GL.*() на стандартные функции GLSL (GL.Fract() => fract())
                        void FixGLSLFunctions() {
                            foreach(KeyValuePair<string, string> KVP in GLSLFunctionMapping) {
                                Code = Regex.Replace(Code, $@"\bGL\.{KVP.Key}\b", KVP.Value);
                            }
                        }
                        FixGLSLFunctions();
                        
                        
                        
                        
                        // Проверка на повторы функций, и удаление не используемых, и делает уникальные названия
                        void CheckFunctionsRepeats(bool Unique){
                            MatchCollection InitialMatches = Regex.Matches(Code, @"\b[a-zA-Z_]\w*\s+([a-zA-Z_]\w*)\s*\(");
                            HashSet<string> AllDefinedFunctions = [];

                            foreach(Match M in InitialMatches){
                                string Name = M.Groups[1].Value;
                                if(ReservedNames.Contains(Name)){ throw new ExceptionWL($"Название функции [{Name}] зарезервировано!"); }
                                if(!AllDefinedFunctions.Add(Name)){ throw new ExceptionWL($"Функция [{Name}] определена больше одного раза!"); }
                            }

                            if(ClearUnusedFunctions){
                                bool Changed = true;
                                while(Changed){
                                    Changed = false;
                                    
                                    MatchCollection CurrentMatches = Regex.Matches(Code, @"\b[a-zA-Z_]\w*\s+([a-zA-Z_]\w*)\s*\(");
                                    
                                    foreach(Match M in CurrentMatches){
                                        string Name = M.Groups[1].Value;
                                        if(Name == EntryPoint){ continue; }

                                        if(Regex.Matches(Code,  $@"\b{Name}\b").Count <= 1){
                                            int Start = M.Index;
                                            int BraceLevel = 0;
                                            int End = -1;
                                            for(int i = Start; i < Code.Length; i++){
                                                if(Code[i] == '{'){ BraceLevel++; }
                                                else if(Code[i] == '}'){
                                                    BraceLevel--;
                                                    if(BraceLevel == 0){ End = i; break; }
                                                }
                                            }

                                            if(End != -1){
                                                Code = Code.Remove(Start, End - Start + 1);
                                                AllDefinedFunctions.Remove(Name);
                                                Changed = true;
                                        
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            
                            MatchCollection AllCalls = Regex.Matches(Code, @"\b([a-zA-Z_]\w*)\s*\(");
                            HashSet<string> CalledNames = [];
                            foreach(Match? M in AllCalls){
                                string Name = M.Groups[1].Value;

                                bool IsUserFunction = AllDefinedFunctions.Contains(Name);
                                bool IsGLSL         = AllowedGLSLNames.Contains(Name);
                                bool IsKeyword      = Keywords.Contains(Name);
                                
                                if(!IsUserFunction && !IsGLSL && !IsKeyword){
                                    throw new ExceptionWL($"Вызов не существующей функции [{Name}]!");
                                }

                                CalledNames.Add(Name);
                            }
                            
                            foreach(string Name in AllDefinedFunctions){
                                if(Name == EntryPoint){ continue; }

                                Code = Regex.Replace(Code, $@"\b{Name}\b", Unique ? $"__WL_{Guid.NewGuid().ToString("N")[..8]}" : $"WL_{Name}");
                            }
                        }
                        CheckFunctionsRepeats(UniqueFunctionNames);
                        
                        
                        
                        
                        // Превращает EntryPoint в "main"
                        void FixEntryPoint(){
                            if(!Regex.IsMatch(Code, $@"\bvoid\s+{EntryPoint}\s*\(")){ throw new ExceptionWL($"Не найдена точка входа {EntryPoint}()!"); }
                            Code = Regex.Replace(Code, $@"\b{EntryPoint}\b", "main");
                        }
                        FixEntryPoint();


                        

                        // Переделывает Uniform Block's
                        void ReplaceUniformBlocks(){
                            Regex BlockRegex = new Regex(@"\buniformblock\s+([a-zA-Z0-9_]+)\s*;", RegexOptions.Compiled);

                            while(true){
                                Match M = BlockRegex.Match(Code);
                                if(!M.Success){ break; }
                                
                                string Name = M.Groups[1].Value;

                                try{
                                    if(!__UniformBlocks.TryGetValue(Name, out (Type StructType, uint Binding) Registered)){
                                        throw new ExceptionWL("Указан не зарегистрированный UniformBlock!");
                                    }

                                    StringBuilder SB = new StringBuilder();
                                    SB.AppendLine($"layout (std140, binding={Registered.Binding}) uniform {Name}{{");

                                    FieldInfo[] Fields = Registered.StructType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

                                    List<string> FieldNames = [];
                                    
                                    foreach(FieldInfo Field in Fields){
                                        if(Field.Name.StartsWith("__")){ continue; }

                                        string GLSLType = Field.FieldType.Name switch{
                                            "Single" or "float" => "float",
                                            "Int32"  or "int"   => "int",
                                            "Vector2F" => "vec2",
                                            "Vector3F" => "vec3",
                                            "Vector4F" => "vec4",
                                            "Matrix2F" => "mat2",
                                            "Matrix3F" => "mat3",
                                            "Matrix4F" => "mat4",
                                            var _ => throw new ExceptionWL($"todo, Тип {Field.FieldType.Name} в структуре {Registered.StructType.Name} не поддерживается в UniformBlock!")
                                        };

                                        FieldNames.Add(Field.Name);
                                        SB.AppendLine($"\t{GLSLType} {Name}_{Field.Name};");
                                    }
                                    
                                    SB.Append("};");
                                    
                                    Code = Code.Remove(M.Index, M.Length).Insert(M.Index, SB.ToString());
                                    
                                    foreach(string FieldName in FieldNames){
                                        string Pattern = $@"(?<!{Name}\.)\b{FieldName}\b";
                                        if(Regex.IsMatch(__RawFullCode, Pattern)){
                                            throw new ExceptionWL($"Неверное обращение к полю UniformBlock! Нужно писать {Name}.{FieldName}!");
                                        }

                                        Code = Regex.Replace(Code, $@"\b{Name}\.{FieldName}\b", $"{Name}_{FieldName}");
                                    }
                                }catch(Exception e){
                                    throw new ExceptionWL($"Произошла ошибка при обработке UniformBlock [\"{Name}\"]!", e);
                                }
                            }
                        }
                        ReplaceUniformBlocks();
                        
                        
                        
                        // Переделывает Uniform's
                        void ReplaceUniforms(){
                            Regex UniformRegex = new Regex(@"\buniform\s+([a-zA-Z0-9_]+)\s+([a-zA-Z0-9_]+)\s*;", RegexOptions.Compiled);
                            HashSet<string> FoundInCodeNames = [];

                            string OriginalCode = Code;
                            Code = UniformRegex.Replace(Code, M => {
                                string VType = M.Groups[1].Value;
                                string Name  = M.Groups[2].Value;

                                // Удаляем неиспользуемые Uniform's
                                if(ClearUnusedUniforms){
                                    if(Regex.Matches(OriginalCode, $@"\b{Name}\b").Count <= 1){ return ""; }
                                }

                                try{
                                    if(!__Uniforms.TryGetValue(Name, out (ValueType Type, int Location) Registered)){
                                        if(!IgnoreNotExistUniforms){
                                            throw new ExceptionWL("Указан не зарегистрированный Uniform!");
                                        }
                                    }

                                    if(!FoundInCodeNames.Add(Name)){ throw new ExceptionWL("Uniform повторяется в шейдере несколько раз!"); }

                                    if(!IgnoreNotExistUniforms){
                                        ValueType VType__ = StringToValueType(VType);
                                        if(VType__ != Registered.Type){ throw new ExceptionWL($"Типы не совпадают, ожидал [{Registered.Type}] а получил [{VType__}]!"); }
                                    }

                                    // todo, add metadata uniform
                                    
                                    // todo, учитывай что тут используешь Registration, ХОТЯ НЕ ДОЛЖЕН
                                    return $"layout (location={Registered.Location}) uniform {VType} {Name};";
                                }catch(Exception e){
                                    throw new ExceptionWL($"Произошла ошибка при обработке Uniform [\"{Name}\", {VType}]!", e);
                                }
                            });
                        }
                        ReplaceUniforms();

                        


                        // Переделывает Layout's
                        Shader.Type Type__ = Type;
                        void ReplaceLayouts(){
                            Regex LayoutRegex = new Regex(@"\blayout\s+([a-zA-Z0-9_]+)\s*;", RegexOptions.Compiled);

                            while(true){
                                Match M = LayoutRegex.Match(Code);
                                if(!M.Success){ break; }

                                if(Type__ != Shader.Type.Vertex){ throw new ExceptionWL("Layout's можно указывать только в Vertex шейдере!"); }

                                string Name = M.Groups[1].Value;

                                if(!__Layouts.TryGetValue(Name, out VertexLayout? Registered)){
                                    throw new ExceptionWL("Указан не зарегистрированный Layout!");
                                }

                                StringBuilder SB = new StringBuilder();
                                List<string> AttributeNames = [];

                                for(int i = 0; i < Registered.Attributes.Length; i++){
                                    VertexAttribute Attribute = Registered.Attributes[i];

                                    string GLSLType = "";
                                    if(Attribute.Normalized || Attribute.Type == VertexAttribute.AttributeType.Float){
                                        GLSLType = Attribute.Count switch {
                                            1 => "float",
                                            2 => "vec2",
                                            3 => "vec3",
                                            4 => "vec4",
                                            var _ => throw new ExceptionWL("todo, Неподдерживаемый размер атрибута")
                                        };
                                    }else if(Attribute.Type == VertexAttribute.AttributeType.UInt || Attribute.Type == VertexAttribute.AttributeType.UByte){
                                        GLSLType = Attribute.Count == 1 ? "uint" : $"uvec{Attribute.Count}";
                                    }else{
                                        GLSLType = Attribute.Count == 1 ? "int" : $"ivec{Attribute.Count}";
                                    }
                                    
                                    AttributeNames.Add(Attribute.Name);
                                    SB.AppendLine($"layout (location={i}) in {GLSLType} {Name}_{Attribute.Name};");
                                }
                                
                                Code = Code.Remove(M.Index, M.Length).Insert(M.Index, SB.ToString());
                                
                                foreach(string AttributeName in AttributeNames){
                                    if (Regex.IsMatch(__RawFullCode, $@"(?<!{Name}\.|in\s+|out\s+|uniform\s+|gl_)\b{AttributeName}\b")) {
                                        throw new ExceptionWL($"Неверное обращение к полю Layout! Нужно писать {Name}.{AttributeName}!");
                                    }

                                    Code = Regex.Replace(Code, $@"\b{Name}\.{AttributeName}\b", $"{Name}_{AttributeName}");
                                }
                            }
                        }
                        ReplaceLayouts();
                        
                        
                        
                        
                        // Переделывает Texture's
                        void ReplaceTextures(){
                            Regex TextureRegex = new Regex(@"\btexture\s+([a-zA-Z0-9_]+)\s+([a-zA-Z0-9_]+)\s*;", RegexOptions.Compiled);
                            HashSet<string> FoundInCodeNames = [];

                            string OriginalCode = Code;
                            Code = TextureRegex.Replace(Code, M => {
                                string VType = M.Groups[1].Value;
                                string Name  = M.Groups[2].Value;

                                // Удаляем неиспользуемые Texture's
                                if(ClearUnusedTextures){
                                    if(Regex.Matches(OriginalCode, $@"\b{Name}\b").Count <= 1){ return ""; }
                                }

                                try{
                                    if(!__Textures.TryGetValue(Name, out (ValueType Type, uint Slot) Registered)){
                                        throw new ExceptionWL("Указан не зарегистрированный Texture!");
                                    }

                                    if(!FoundInCodeNames.Add(Name)){ throw new ExceptionWL("Texture повторяется в шейдере несколько раз!"); }

                                    ValueType VType__ = StringToValueType(VType);
                                    if(VType__ != Registered.Type){ throw new ExceptionWL($"Типы не совпадают, ожидал [{Registered.Type}] а получил [{VType__}]!"); }

                                    // todo, add metadata uniform
                                    
                                    // todo, учитывай что тут используешь Registration, ХОТЯ НЕ ДОЛЖЕН
                                    return $"layout (binding={Registered.Slot}) uniform {VType} {Name};";
                                }catch(Exception e){
                                    throw new ExceptionWL($"Произошла ошибка при обработке Texture [\"{Name}\", {VType}]!", e);
                                }
                            });
                        }
                        ReplaceTextures();
                        
                        
                        
                        
                        // Заменяет свои типы на GLSL
                        void ReplaceTypes(){
                            foreach(KeyValuePair<string, string> KVP in TypeMapping){
                                Code = Regex.Replace(Code, $@"\b{KVP.Key}\b", KVP.Value);
                            }
                        }
                        ReplaceTypes();
                        
                        
                        
                        
                        // Исправляет .RGB -> .rgb
                        Code = Regex.Replace(Code, @"\.([RGBAXYZW]{1,4})\b", m => m.Value.ToLower());
                        
                        
                        
                        
                        // Установка нужной версии
                        SB.AppendLine($"#version {Version}");
                        SB.AppendLine();
                        SB.AppendLine(Code);
                        
                    #endregion
                    
                #endregion

                return (SB.ToString().Trim(), Metadata);
            }catch(Exception e){
                throw new ExceptionWL($"Произошла ошибка при WL компиляции GLShader!\nСырой код:\n{RawCode}\nСырой полный код:\n{__RawFullCode}\n", e);
            }
        }
        
        // ----------------------------------------------------------------------
        
        public enum ValueType{
            Int,
            UInt,
            
            Float,
            
            Vector2F,
            Vector3F,
            Vector4F,
            
            Matrix2F,
            Matrix3F,
            Matrix4F,
            
            Texture2D
        }
        
        public struct Property{
            public string    Name;
            
            // это для редактора снизу
            public string Label;
            public string Widget;
        }
        
        // todo, потом сделать, это нужно для редактора и значений шейдеров тыры пыры
        [Obsolete]
        public struct Metadata{
            public List<Property> Properties;
        }
    }
}