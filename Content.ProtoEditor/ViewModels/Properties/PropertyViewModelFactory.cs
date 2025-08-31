using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Content.ProtoEditor.ViewModels.Properties;

public sealed class PropertyViewModelFactory
{
    private readonly Dictionary<Type, Type> _viewModelRegistry = [];
    public PropertyViewModelFactory()
    {
        var assembly = typeof(PropertyViewModelFactory).Assembly;
        var baseType = typeof(PropertyViewModel);
        var fallbackType = typeof(FallbackPropertyViewModel);

        var modelTypes = assembly.GetTypes().Where(t => t != fallbackType && t.IsSubclassOf(baseType));
        foreach (var type in modelTypes)
        {
            var attr = type.GetCustomAttribute<ViewModelForAttribute>();
            if (attr != null)
            {
                foreach (var supportedType in attr.TargetTypes)
                {
                    if (_viewModelRegistry.ContainsKey(supportedType))
                    {
                        throw new Exception($"Already registered a view model for {supportedType}");
                    }

                    _viewModelRegistry[supportedType] = type;
                }
            }
        }
    }

    public PropertyViewModel MakeViewModel(string? name, Type objectType, object? value)
    {
        if (_viewModelRegistry.TryGetValue(objectType, out var implType))
        {
            return (PropertyViewModel)Activator.CreateInstance(implType, name, value)!;
        }

        var fallbackType = PrettyPrint.PrintUserFacingTypeShort(objectType, 2);
        return new FallbackPropertyViewModel(name, fallbackType);
    }

    public ArrayPropertyViewModel CreatePropertyArray(string name, MemberInfo info, Type arrayType, IPrototype instance)
    {
        List<PropertyViewModel> viewModels = [];

        if (GetValue(info, instance) is IEnumerable elements)
        {
            foreach (var item in elements)
            {
                var viewModel = MakeViewModel(null, arrayType, item);
                viewModel.IsArrayElement = true;
                viewModels.Add(viewModel);
            }
        }
        return new ArrayPropertyViewModel(name, arrayType, viewModels, OnAddArrayItem);
    }

    private static string GetName(MemberInfo info)
    {
        if (info.GetCustomAttribute<DataFieldAttribute>() is var field &&
            field != null &&
            field.Tag != null)
        {
            // Tags override names of the properties
            return field.Tag;
        }

        return info.Name;
    }

    private static object? GetValue(MemberInfo info, IPrototype instance)
    {
        return info.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)info).GetValue(instance),
            MemberTypes.Property => ((PropertyInfo)info).GetValue(instance),
            _ => throw new NotImplementedException(),
        };
    }

    public PropertyViewModel CreateFromField(FieldInfo info, IPrototype instance)
    {
        return CreatePropertyInternal(GetName(info), info.FieldType, info, instance);
    }

    public PropertyViewModel CreateFromProperty(PropertyInfo info, IPrototype instance)
    {
        return CreatePropertyInternal(GetName(info), info.PropertyType, info, instance);
    }

    private PropertyViewModel OnAddArrayItem(Type elementType)
    {
        return MakeViewModel(null, elementType, null);
    }

    private PropertyViewModel CreatePropertyInternal(string name, Type type, MemberInfo info, IPrototype instance)
    {
        if (type.BaseType == typeof(Enum))
        {
            // Override the actual type for Enums so they are properly handled as enums
            type = type.BaseType;
        }

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (elementType == null)
                return new FallbackPropertyViewModel(name, "");

            return CreatePropertyArray(name, info, elementType, instance);
        }
        else if (type.IsGenericType)
        {
            if (type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var genericTypes = type.GetGenericArguments();
                if (genericTypes.Length == 0 || genericTypes.First() == null)
                    return new FallbackPropertyViewModel(name, ""); // No idea what this is, maybe log an error?

                return CreatePropertyArray(name, info, genericTypes.First(), instance);
            }
            else if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                // TODO: Dictionary views
            }
        }

        return MakeViewModel(name, type, GetValue(info, instance));
    }
}
