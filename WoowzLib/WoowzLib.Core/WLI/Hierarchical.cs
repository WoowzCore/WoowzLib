using WLO;

namespace WLI;

public interface Hierarchical<T> where T : class{
    HierarchyNode<T> Node{ get; }
}