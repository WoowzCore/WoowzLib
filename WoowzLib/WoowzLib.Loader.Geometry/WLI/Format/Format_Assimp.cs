using WLO;
using WLO.Math;

namespace WLI.Format;

public abstract class Format_Assimp : Format<WLO.Geometry>{
    public string LinkedID{ get; set; }

    protected string AssimpID;
    
    public abstract bool __Is(byte[] Data);
    
    public Geometry __Load(byte[] Data){
        Assimp.AssimpContext Context = new Assimp.AssimpContext();

        using MemoryStream Stream = new MemoryStream(Data);

        Assimp.Scene Scene = Context.ImportFileFromStream(Stream,
            Assimp.PostProcessSteps.Triangulate |
            Assimp.PostProcessSteps.GenerateNormals |
            Assimp.PostProcessSteps.FlipUVs |
            Assimp.PostProcessSteps.LimitBoneWeights |
            Assimp.PostProcessSteps.OptimizeMeshes,
            AssimpID
        );

        if(Scene == null || !Scene.HasMeshes){ throw new ExceptionWL("Файл модели пуст или не распознан!"); }

        Geometry Result = new Geometry();

        uint TotalID = 0;
        
        foreach(Assimp.Mesh? Mesh in Scene.Meshes){
            uint VertexOffset = (uint)Result.Vertices.Count;

            for(int i = 0; i < Mesh.VertexCount; i++){
                Assimp.Vector3D V_Position = Mesh.Vertices[i];
                Vector3F Position = new Vector3F(V_Position.X, V_Position.Y, V_Position.Z);

                Vector3F Normal = new Vector3F();
                if(Mesh.HasNormals){
                    Assimp.Vector3D V_Normal = Mesh.Normals[i];
                    Normal = new Vector3F(V_Normal.X, V_Normal.Y, V_Normal.Z);
                }

                Vector2F UV = new Vector2F();
                if(Mesh.HasTextureCoords(0)){
                    Assimp.Vector3D V_UV = Mesh.TextureCoordinateChannels[0][i];
                    UV = new Vector2F(V_UV.X, V_UV.Y);
                }
                
                Color4B Color = Color4B.White;
                if(Mesh.HasVertexColors(0)){
                    Assimp.Color4D V_Color = Mesh.VertexColorChannels[0][i];
                    Color = new Color4B(
                        (byte)(V_Color.R * 255),
                        (byte)(V_Color.G * 255),
                        (byte)(V_Color.B * 255),
                        (byte)(V_Color.A * 255)
                    );
                }

                uint ID = TotalID++;
                
                Result.Vertices.Add(new Vertex(Position, UV, Normal, Color, ID));
            }

            foreach(Assimp.Face? Face in Mesh.Faces){
                if(Face.IndexCount == 3){
                    Result.Indices.Add(VertexOffset + (uint)Face.Indices[0]);
                    Result.Indices.Add(VertexOffset + (uint)Face.Indices[1]);
                    Result.Indices.Add(VertexOffset + (uint)Face.Indices[2]);
                }
            }
        }
        
        return Result;
    }
}