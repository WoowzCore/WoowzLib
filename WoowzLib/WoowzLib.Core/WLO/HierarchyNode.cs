namespace WLO;

public class HierarchyNode<T> : WLI.Packable where T : class{
    private T __Owner;
    public  T Owner => __Owner;

    public HierarchyNode<T>? Parent{ get; private set; }

    public List<HierarchyNode<T>> Children{ get; } = [];

    public Func<Dictionary<string, object>, T>? ChildFactory = null!;
    
    public event Action<HierarchyNode<T>, HierarchyNode<T>?, HierarchyNode<T>?>? OnParentChanged;
    public event Action<HierarchyNode<T>, HierarchyNode<T>>? OnChildAdded;
    public event Action<HierarchyNode<T>, HierarchyNode<T>>? OnChildRemoved;

    private HierarchyNode(){}
    public HierarchyNode(T Owner){ __Owner = Owner; }

    public void SetParent(HierarchyNode<T>? NewParent){
        if(Parent == NewParent){ return; }
        if(NewParent != null && (NewParent == this || NewParent.IsDescendantOf(this))){ throw new ExceptionWL("todo 2"); }

        HierarchyNode<T>? OldParent = Parent;

        if(Parent != null){
            Parent.Children.Remove(this);
            Parent.OnChildRemoved?.Invoke(Parent, this);
        }

        Parent = NewParent;

        if(Parent != null){
            Parent.Children.Add(this);
            Parent.OnChildAdded?.Invoke(Parent, this);
        }
        
        OnParentChanged?.Invoke(this, OldParent, NewParent);
    }

    public bool IsDescendantOf(HierarchyNode<T> PotentialAncestor){
        HierarchyNode<T>? Current = Parent;
        while(Current != null){
            if(Current == PotentialAncestor){ return true; }
            Current = Current.Parent;
        }
        return false;
    }

    public void Traverse(Action<T> Action){
        Action(Owner);
        foreach(HierarchyNode<T> Child in Children){
            Child.Traverse(Action);
        }
    }

    public Dictionary<string, object?> __Pack() => new Dictionary<string, object?>{
        ["Children"] = Children.Select(C => WL.Packer.Pack(C.Owner)).ToList()
    };

    public void __Unpack(Dictionary<string, object?> Data){
        List<object>? ChildrenData = WL.Packer.Get<List<object>>(Data, "Children");

        if(ChildrenData != null){
            foreach(object NodeData in ChildrenData){
                T? Child = null;

                if(NodeData is T AlreadyUnpacked){
                    Child = AlreadyUnpacked;
                }else if(NodeData is Dictionary<string, object> Dictionary && ChildFactory != null){
                    Child = ChildFactory(Dictionary);
                }else if(NodeData is Dictionary<string, object?> DictionaryQ && ChildFactory != null){
                    Child = ChildFactory(DictionaryQ!);
                }

                if(Child is WLI.Hierarchical<T> Hierarchical){
                    Hierarchical.Node.SetParent(this);
                }
            }
        }
    }
}