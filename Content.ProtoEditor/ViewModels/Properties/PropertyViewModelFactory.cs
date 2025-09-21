using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Robust.Shared.Prototypes;
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
            if (attr == null)
                continue;

            foreach (var supportedType in attr.TargetTypes)
            {
                if (!_viewModelRegistry.TryAdd(supportedType, type))
                    throw new Exception($"Already registered a view model for {supportedType}");
            }
        }
    }

    private PropertyViewModel MakeViewModel(MemberInfo info, Type objectType, object? value)
    {
        if (_viewModelRegistry.TryGetValue(objectType, out var implType))
            return (PropertyViewModel)Activator.CreateInstance(implType, info, value)!;

        var fallbackType = PrettyPrint.PrintUserFacingTypeShort(objectType, 2);
        return new FallbackPropertyViewModel(info, fallbackType);
    }

    private ArrayPropertyViewModel CreatePropertyArray(MemberInfo info, Type arrayType, IPrototype instance)
    {
        List<PropertyViewModel> viewModels = [];

        if (GetValue(info, instance) is not IEnumerable elements)
            return new ArrayPropertyViewModel(info, arrayType, viewModels, OnAddArrayItem);

        foreach (var item in elements)
        {
            var viewModel = MakeViewModel(info, arrayType, item); // TODO: Figure this bit out for saving
            viewModel.IsArrayElement = true;
            viewModels.Add(viewModel);
        }

        return new ArrayPropertyViewModel(info, arrayType, viewModels, OnAddArrayItem);
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
        return CreatePropertyInternal(info, info.FieldType, instance);
    }

    public PropertyViewModel CreateFromProperty(PropertyInfo info, IPrototype instance)
    {
        return CreatePropertyInternal(info, info.PropertyType, instance);
    }

    private PropertyViewModel OnAddArrayItem(MemberInfo info, Type elementType)
    {
        return MakeViewModel(info, elementType, null);
    }

    private PropertyViewModel CreatePropertyInternal(MemberInfo info, Type type, IPrototype instance)
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
                return new FallbackPropertyViewModel(info, "");

            return CreatePropertyArray(info, elementType, instance);
        }

        if (type.IsGenericType)
        {
            var generic = type.GetGenericTypeDefinition();
            if (generic == typeof(Nullable<>))
            {
                // Unwrap the nullable to its more basic type and recurse into it
                var vm = CreatePropertyInternal(info, type.GetUnderlyingType()!, instance);
                vm.IsNullable = true;
                return vm;
            }

            if (generic == typeof(List<>))
            {
                var genericTypes = type.GetGenericArguments();
                if (genericTypes.Length == 0)
                    return new FallbackPropertyViewModel(info, ""); // No idea what this is, maybe log an error?

                return CreatePropertyArray(info, genericTypes.First(), instance);
            }

            if (generic == typeof(Dictionary<,>))
            {
                // TODO: Dictionary views
            }
        }

        return MakeViewModel(info, type, GetValue(info, instance));
    }
}
