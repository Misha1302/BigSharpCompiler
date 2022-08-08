namespace Lexer;

public class Dependence
{
    public readonly List<Dependence> Children;
    public DependenceEnum DependenceEnum;
    public readonly string? Name;

    private readonly Dependence? _internalParent;

    public Dependence(string name, DependenceEnum dependenceEnum, Dependence? parent = null, List<Dependence>? children = default)
    {
        Children = children ?? new List<Dependence>();
        Parent = parent;
        Name = name;
        DependenceEnum = dependenceEnum;
    }

    public Dependence? Parent
    {
        get => _internalParent;
        init
        {
            value?.AddChildren(this);
            _internalParent = value;
        }
    }

    public void AddChildren(Dependence children)
    {
        Children.Add(children);
    }
}