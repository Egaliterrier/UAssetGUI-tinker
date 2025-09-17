using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using UAssetAPI;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetManager.Controls.Editors;
using UAssetManager.Utils;

namespace UAssetManager.Controls;
public partial class PropertyEditor
{
    #region Constructor
    private ICollectionView? _dataView;

    public PropertyEditor()
    {
        InitializeComponent();
    }
    #endregion

    #region Properties

    public static readonly DependencyProperty AssetProperty = DependencyProperty.Register(
        nameof(Asset), typeof(UAsset), typeof(PropertyEditor), new PropertyMetadata(default(UAsset)));

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source), typeof(object), typeof(PropertyEditor), new PropertyMetadata(default, OnSourceChanged));

    public UAsset Asset
    {
        get => (UAsset?)GetValue(AssetProperty) ?? throw new ArgumentNullException(nameof(Asset));
        set => SetValue(AssetProperty, value);
    }

    public object Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    #endregion

    #region Methods
    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctl = (PropertyEditor)d;
        ctl.UpdateItems(e.NewValue);
    }

    private void UpdateItems(object obj)
    {
        if (obj == null) return;

        IEnumerable<PropertyData>? properties;
        if (obj is IEnumerable<PropertyData> enumerable) properties = enumerable;
        else if (obj is PropertyData property) properties = [property];
        else return;

        ItemsControl.ItemsSource = _dataView = CollectionViewSource.GetDefaultView(
            properties.Select(CreatePropertyItem)
            .Do(item => item.InitElement()));
    }

    protected virtual PropertyItem CreatePropertyItem(PropertyData property) => new()
    {
        PropertyName = property.Name.Value.Value,
        Description = GetType(property),
        Editor = ResolveEditor(Asset, property),
        Value = property,
    };

    private static string GetType(PropertyData property) => property switch
    {
        BytePropertyData bp when bp.ByteType == BytePropertyType.FName => bp.EnumType.ToString(),
        _ => property.PropertyType.Value,
    };

    public static PropertyEditorBase ResolveEditor(UAsset asset, PropertyData property) => property switch
    {
        ArrayPropertyData => new ArrayPropertyEditor(asset),
        StructPropertyData => new StructPropertyEditor(asset),
        BytePropertyData => new BytePropertyEditor(asset),
        EnumPropertyData => new EnumPropertyEditor(asset),
        ObjectPropertyData => new ObjectPropertyEditor(asset),

        BoolPropertyData => new BoolPropertyEditor(),
        IntPropertyData => new IntPropertyEditor(),
        FloatPropertyData => new FloatPropertyEditor(),
        DoublePropertyData => new FloatPropertyEditor(),
        StrPropertyData => new StrPropertyEditor(),
        NamePropertyData => new NamePropertyEditor(),
        VectorPropertyData => new VectorPropertyEditor(),
        Vector2DPropertyData => new Vector2DPropertyEditor(),
        Vector4PropertyData => new Vector4PropertyEditor(),
        RotatorPropertyData => new RotatorPropertyEditor(),
        LinearColorPropertyData => new LinearColorPropertyEditor(),
        ColorPropertyData => new ColorPropertyEditor(),
        QuatPropertyData => new QuatPropertyEditor(),
        _ => new ReadOnlyTextPropertyEditor()
    };
    #endregion
}