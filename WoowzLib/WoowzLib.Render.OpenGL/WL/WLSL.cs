using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using WLO;
using WLO.Render;

namespace WL;

public struct WLSL{
    // Настройки компилятора
    public const string Version = "430 core";
    // Удаляет комментарии
    public static bool ClearComments = true;
    // Удаляет не используемые Uniform's
    public static bool ClearUnusedUniforms = true;
    // Удаляет не используемые Texture's
    public static bool ClearUnusedTextures = true;
    
    // ----------------------------------------------------------------------
    
    /// Что-бы вписать ссылку на другой код, к примеру вставить код по Key = "Example", будет: "#Example#", и он буквально заменит его на указанный код по ключу, если "#Example", то вставит но в конце добавил новую строку, работает рекурсивно
    public static void AddCode(string Key, string RawCode){
        Key = Key.Trim();
        try{
            if(string.IsNullOrWhiteSpace(Key)){ throw new ExceptionWL("Ключ не может быть пустым!"); }
            __Codes[Key] = RawCode;
        }catch(Exception e){
            throw new ExceptionWL($"Произошла ошибка при добавлении кода в WLSL! AddCode(\"{Key}\")\nКод:\n{RawCode}", e);
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
            throw new ExceptionWL($"Произошла ошибка при добавлении Uniform в WLSL! AddUniform(\"{Name}\", {Type}, {Location})", e);
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
            throw new ExceptionWL($"Произошла ошибка при добавлении UniformBlock в WLSL! AddUniformBlock<{typeof(T).Name}>(\"{Name}\", {Binding})", e);
        }
    }
    private static readonly Dictionary<string, (Type StructType, uint Binding)> __UniformBlocks = [];


    

    // todo, нужно писать "layout NAME;"
    public static void AddLayout(string Name, VertexLayout Layout){
        try{
            if(!new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$").IsMatch(Name)) { throw new ExceptionWL("Указано недопустимое имя Layout!"); }
            
            if(__Layouts.TryGetValue(Name, out VertexLayout? ExistingLayout)){ throw new ExceptionWL($"Layout с таким названием уже есть! Вот же он [\"{Name}\", {ExistingLayout}]!"); }
            
            __Layouts[Name] = Layout;
        }catch(Exception e){
            throw new ExceptionWL($"Произошла ошибка при добавлении Layout в WLSL! AddLayout(\"{Name}\", {Layout})", e);
        }
    }
    private static readonly Dictionary<string, VertexLayout> __Layouts = [];
    
    
    
   public static void AddTexture(string Name, ValueType Type, uint Slot){
        try{
            if(!new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$").IsMatch(Name)) { throw new ExceptionWL("Указано недопустимое имя Texture!"); }
            
            if(__Textures.TryGetValue(Name, out (ValueType Type, uint Slot) ExistingTexture)){ throw new ExceptionWL($"Texture с таким названием уже есть! Вот же он [\"{Name}\", {ExistingTexture.Type}, {ExistingTexture.Slot}]!"); }

            __Textures[Name] = (Type, Slot);
        }catch(Exception e){
            throw new ExceptionWL($"Произошла ошибка при добавлении Texture в WLSL! AddTexture(\"{Name}\", {Type}, {Slot})", e);
        }
    }
    private static readonly Dictionary<string, (ValueType Type, uint Slot)> __Textures = [];
    
    // ----------------------------------------------------------------------
    
    public static Result Compile(string WLSL, WLI.GPU.Shader.Type? Type = null){
        string __RawFullCode = "Не обработано";
        
        try{
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
                    __RawFullCode = ExpandCodes(WLSL);
                    WLSL = __RawFullCode;

                    // Удаляет комментарии
                    if(ClearComments){
                        WLSL = Regex.Replace(WLSL, @"/\*[\s\S]*?\*/|//.*", "");
                    }
                
                    if(string.IsNullOrWhiteSpace(WLSL)){ throw new ExceptionWL("Указан пустой код!"); }
                    
                #endregion

                #region Компиляция

                    StringBuilder SB = new StringBuilder();
                    
                    ValueType StringToValueType(string String) => String switch{
                        "int"  => ValueType.Int,
                        "uint" => ValueType.UInt,
                        
                        "float" => ValueType.Float,
                        
                        "vec2" => ValueType.Vec2,
                        "vec3" => ValueType.Vec3,
                        "vec4" => ValueType.Vec4,
                        
                        "ivec2" => ValueType.IVec2,
                        "ivec3" => ValueType.IVec3,
                        "ivec4" => ValueType.IVec4,
                        
                        "uvec2" => ValueType.UVec2,
                        "uvec3" => ValueType.UVec3,
                        "uvec4" => ValueType.UVec4,
                        
                        "mat2" => ValueType.Mat2,
                        "mat3" => ValueType.Mat3,
                        "mat4" => ValueType.Mat4,
                        
                        "sampler2D" => ValueType.Sampler2D,
                        
                        var _ => throw new ArgumentOutOfRangeException(nameof(String), String, null) // todo, тут нужна ошибка норм
                    };
                    
                    // Начало компиляции
                    
                    string Code = WLSL;


                    
                    // Добавляет flat полям in, out где нужно
                    void AddFlatQualifiers() {
                        Code = Regex.Replace(Code, @"(?<!flat\s+)\b(in|out)\s+\b(u?int|[iu]vec[2-4])\b", "flat $1 $2");
                    }
                    AddFlatQualifiers();
            
                    
                    

                    // Переделывает Uniform Block's
                    void ReplaceUniformBlocks(){
                        Regex BlockRegex = new Regex(@"\buniform_block\s+([a-zA-Z0-9_]+)\s*;", RegexOptions.Compiled);

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
                                        "UInt32" or "uint"  => "uint",
                                        "Vector2F"          => "vec2",
                                        "Vector3F"          => "vec3",
                                        "Vector4F"          => "vec4",
                                        "Vector2I"          => "ivec2",
                                        "Vector3I"          => "ivec3",
                                        "Vector4I"          => "ivec4",
                                        "Matrix2F"          => "mat2",
                                        "Matrix3F"          => "mat3",
                                        "Matrix4F"          => "mat4",
                                        var _ => throw new ExceptionWL($"todo, Тип {Field.FieldType.Name} в структуре {Registered.StructType.Name} не поддерживается в UniformBlock!")
                                    };

                                    FieldNames.Add(Field.Name);
                                    SB.AppendLine($"\t{GLSLType} {Name}_{Field.Name};");
                                }
                                
                                SB.Append("};");
                                
                                Code = Code.Remove(M.Index, M.Length).Insert(M.Index, SB.ToString());
                                
                                foreach(string FieldName in FieldNames){
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
                                    throw new ExceptionWL("Указан не зарегистрированный Uniform!");
                                }

                                if(!FoundInCodeNames.Add(Name)){ throw new ExceptionWL("Uniform повторяется в шейдере несколько раз!"); }

                                ValueType VType__ = StringToValueType(VType);
                                if(VType__ != Registered.Type){ throw new ExceptionWL($"Типы не совпадают, ожидал [{Registered.Type}] а получил [{VType__}]!"); }

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
                    void ReplaceLayouts(){
                        Regex LayoutRegex = new Regex(@"\blayout\s+([a-zA-Z0-9_]+)\s*;", RegexOptions.Compiled);

                        while(true){
                            Match M = LayoutRegex.Match(Code);
                            if(!M.Success){ break; }

                            if(Type != WLI.GPU.Shader.Type.Vertex){ throw new ExceptionWL("Layout's можно указывать только в Vertex шейдере!"); }

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
                                    GLSLType = Attribute.Count switch{
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
                    
                    
                    
                    
                    // Установка нужной версии
                    SB.AppendLine($"#version {Version}");
                    SB.AppendLine();
                    SB.AppendLine(Code);
                    
                #endregion
                
            #endregion

            return new Result{
                GLSL = SB.ToString().Trim(),
                Type = Type!.Value
            };
        }catch(Exception e){
            throw new ExceptionWL($"Произошла ошибка при WLSL компиляции!\nСырой код:\n{WLSL}\nСырой полный код:\n{__RawFullCode}\n", e);
        }
    }
    
    public struct Result{
        public string             GLSL;
        public WLI.GPU.Shader.Type Type;
    }
    
    public enum ValueType{
        Int,
        UInt,
            
        Float,
            
        Vec2,
        Vec3,
        Vec4,
            
        IVec2,
        IVec3,
        IVec4,
            
        UVec2,
        UVec3,
        UVec4,
            
        Mat2,
        Mat3,
        Mat4,
            
        Sampler2D
    }
}