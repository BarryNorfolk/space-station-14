using System;
using System.Globalization;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.ProtoEditor.ViewModels.Properties;

[ViewModelFor([typeof(float), typeof(double)])]
[UsedImplicitly]
public sealed partial class FloatingPointPropertyViewModel : PropertyViewModel
{
    /// <summary>
    /// Current value of this property.
    /// </summary>
    [ObservableProperty]
    private double? _value;

    /// <summary>
    /// Used for rendering to
    /// </summary>
    private string StringValue
    {
        get => Value.HasValue ? Value.Value.ToString(CultureInfo.InvariantCulture) : "Null";
        set
        {
            if (double.TryParse(value, out var d))
                Value = d;
        }
    }

    public FloatingPointPropertyViewModel(MemberInfo info, object? value) : base(info)
    {
        if (value == null)
            return;

        Value = Convert.ToDouble(value);

        switch (TypeCode)
        {
            case TypeCode.Single:
                Tooltip.Add("A floating point with 7 digit precision");
                break;
            case TypeCode.Double:
                Tooltip.Add("A floating point with 15 digit precision");
                break;
        }
    }

    public override void SaveToInstance(IPrototype instance)
    {
        switch (TypeCode)
        {
            case TypeCode.Single:
                Save(instance, Convert.ToSingle(Value));
                break;
            case TypeCode.Double:
                Save(instance, Value);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
