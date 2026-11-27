using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SP.Internals;

#if SP_INTERNALS_EMBEDDED
internal
#else
public
#endif
class AssemblyBinding(string name, Guid moduleId)
{
    private readonly Lazy<Module> module = new(() =>
    {
        var loaded = Assembly.Load(new AssemblyName(name)).ManifestModule;
        if (loaded.ModuleVersionId != moduleId)
            throw new InvalidOperationException(
                $"Assembly '{name}' has module ID {loaded.ModuleVersionId}, expected {moduleId}. Rebuild SP.Internals against this install.");
        return loaded;
    });

    public Module Module => module.Value;
}

#if SP_INTERNALS_EMBEDDED
internal
#else
public
#endif
class TypeBinding(AssemblyBinding assembly, int token)
{
    private readonly Lazy<Type> type = new(() => assembly.Module.ResolveType(token));

    public Type Info => type.Value;
}

#if SP_INTERNALS_EMBEDDED
internal
#else
public
#endif
class MemberBinding<TMember>(TypeBinding owner, int token) where TMember : MemberInfo
{
    private const BindingFlags Declared = BindingFlags.DeclaredOnly | BindingFlags.Public |
        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    private readonly Lazy<TMember> member = new(() => Find(owner.Info, token));

    public TMember Info => member.Value;

    /// <summary>Resolves a member on a constructed version of its generic declaring type.</summary>
    public TMember On(Type declaringType)
    {
        if (declaringType == owner.Info)
            return Info;
        if (!declaringType.IsConstructedGenericType || declaringType.GetGenericTypeDefinition() != owner.Info)
            throw new ArgumentException($"Expected a constructed type of {owner.Info}.", nameof(declaringType));
        return Find(declaringType, token);
    }

    private static TMember Find(Type type, int token)
    {
        // Use declared members so the reflection objects carry the constructed declaring type.
        IEnumerable<MemberInfo> members = typeof(TMember) switch
        {
            var kind when kind == typeof(FieldInfo) => type.GetFields(Declared),
            var kind when kind == typeof(MethodInfo) => type.GetMethods(Declared),
            var kind when kind == typeof(ConstructorInfo) => type.GetConstructors(Declared),
            var kind when kind == typeof(PropertyInfo) => type.GetProperties(Declared),
            var kind when kind == typeof(EventInfo) => type.GetEvents(Declared),
            _ => throw new NotSupportedException(typeof(TMember).FullName)
        };

        return members.FirstOrDefault(member => member.MetadataToken == token) as TMember
            ?? throw new MissingMemberException(type.FullName, $"metadata token 0x{token:X8}");
    }
}
