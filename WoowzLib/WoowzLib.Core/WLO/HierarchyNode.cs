namespace WLO;

public class HierarchyNode<T> : WLI.Serializable where T : class{
    public readonly T Owner;

    public HierarchyNode<T>? Parent{ get; private set; }

    public List<HierarchyNode<T>> Children{ get; } = [];

    public Func<Dictionary<string, object>, T>? ChildFactory = null!;
    
    public event Action<HierarchyNode<T>, HierarchyNode<T>?, HierarchyNode<T>?>? OnParentChanged;
    public event Action<HierarchyNode<T>, HierarchyNode<T>>? OnChildAdded;
    public event Action<HierarchyNode<T>, HierarchyNode<T>>? OnChildRemoved;

    public HierarchyNode(T Owner){ this.Owner = Owner; }

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
    
    public Dictionary<string, object> Serialize() => new Dictionary<string, object> {
        ["Children"] = Children.Select(C => {
            if(C.Owner is WLI.Serializable s){ return s.Serialize(); }
            throw new ExceptionWL($"Объект типа {typeof(T)} не является Serializable!  TODO");
        }).ToList()
    };
    
    
    public void Deserialize(Dictionary<string, object> Data){
        if (ChildFactory == null){ return; }

        if(Data.TryGetValue("Children", out object? V_Children__) && V_Children__ is IEnumerable<object> V_Children){
            foreach(object V_Child__ in V_Children){
                if (V_Child__ is Dictionary<string, object> V_Child){
                    T Child = ChildFactory(V_Child);
                    
                    if(Child is WLI.Hierarchical<T> ChildHierarchical){
                        ChildHierarchical.Node.SetParent(this);
                    }
                }
            }
        }
    }
}