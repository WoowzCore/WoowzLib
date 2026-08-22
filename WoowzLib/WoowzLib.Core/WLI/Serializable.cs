namespace WLI;

public interface Serializable{
    Dictionary<string, object> Serialize();
    void Deserialize(Dictionary<string, object> Data);
}