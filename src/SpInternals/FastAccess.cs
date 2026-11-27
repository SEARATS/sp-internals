using System.Linq.Expressions;
using System.Reflection;

namespace SP.Internals;

/// <summary>Creates delegates to cache and reuse on hot paths.</summary>
public static class FastAccess
{
    public delegate void RefSetter<TTarget, in TValue>(ref TTarget target, TValue value) where TTarget : struct;

    public static TDelegate Bind<TDelegate>(this MethodInfo method) where TDelegate : Delegate =>
        (TDelegate)method.CreateDelegate(typeof(TDelegate));

    public static TDelegate Bind<TDelegate>(this MethodInfo method, object target) where TDelegate : Delegate =>
        (TDelegate)method.CreateDelegate(typeof(TDelegate), target);

    public static TDelegate Factory<TDelegate>(this ConstructorInfo constructor) where TDelegate : Delegate
    {
        var signature = typeof(TDelegate).GetMethod("Invoke")!;
        var parameters = signature.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
        return Expression.Lambda<TDelegate>(Expression.Convert(Expression.New(constructor, parameters), signature.ReturnType), parameters).Compile();
    }

    public static Func<TTarget, TValue> Getter<TTarget, TValue>(this FieldInfo field)
    {
        var target = Expression.Parameter(typeof(TTarget), "target");
        return Expression.Lambda<Func<TTarget, TValue>>(Read<TValue>(InstanceField(field, target)), target).Compile();
    }

    public static Func<TValue> StaticGetter<TValue>(this FieldInfo field) =>
        Expression.Lambda<Func<TValue>>(Read<TValue>(Expression.Field(null, field))).Compile();

    public static Action<TTarget, TValue> Setter<TTarget, TValue>(this FieldInfo field) where TTarget : class
    {
        if (field.DeclaringType!.IsValueType)
            throw new ArgumentException("Use RefSetterFor for a struct field.", nameof(field));

        var target = Expression.Parameter(typeof(TTarget), "target");
        var value = Expression.Parameter(typeof(TValue), "value");
        return Expression.Lambda<Action<TTarget, TValue>>(Write(InstanceField(field, target), value), target, value).Compile();
    }

    public static RefSetter<TTarget, TValue> RefSetterFor<TTarget, TValue>(this FieldInfo field) where TTarget : struct
    {
        var target = Expression.Parameter(typeof(TTarget).MakeByRefType(), "target");
        var value = Expression.Parameter(typeof(TValue), "value");
        return Expression.Lambda<RefSetter<TTarget, TValue>>(Write(Expression.Field(target, field), value), target, value).Compile();
    }

    public static Action<TValue> StaticSetter<TValue>(this FieldInfo field)
    {
        var value = Expression.Parameter(typeof(TValue), "value");
        return Expression.Lambda<Action<TValue>>(Write(Expression.Field(null, field), value), value).Compile();
    }

    private static MemberExpression InstanceField(FieldInfo field, ParameterExpression target) =>
        Expression.Field(Expression.Convert(target, field.DeclaringType!), field);

    private static UnaryExpression Read<TValue>(MemberExpression field) => Expression.Convert(field, typeof(TValue));

    private static BlockExpression Write(MemberExpression field, ParameterExpression value) =>
        Expression.Block(Expression.Assign(field, Expression.Convert(value, field.Type)), Expression.Empty());
}
