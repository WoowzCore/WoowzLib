namespace WLO;

public class HierarchyNode<T> where T : class{
    public readonly T Owner;

    public HierarchyNode<T>? Parent{ get; private set; }

    public List<HierarchyNode<T>> Children{ get; } = [];

    public event Action<HierarchyNode<T>, HierarchyNode<T>?, HierarchyNode<T>?>? OnParentChanged;
    public event Action<HierarchyNode<T>>? OnChildAdded;
    public event Action<HierarchyNode<T>>? OnChildRemoved;

    public HierarchyNode(T Owner){ this.Owner = Owner; }

    public void SetParent(HierarchyNode<T>? NewParent){
        if(Parent == NewParent){ return; }
        if(NewParent != null && (NewParent == this || NewParent.IsDescendantOf(this))){ throw new ExceptionWL("todo 2"); }

        HierarchyNode<T>? OldParent = Parent;

        if(Parent != null){
            Parent.Children.Remove(this);
            Parent.OnChildRemoved?.Invoke(this);
        }

        Parent = NewParent;

        if(Parent != null){
            Parent.Children.Add(this);
            Parent.OnChildAdded?.Invoke(this);
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
}