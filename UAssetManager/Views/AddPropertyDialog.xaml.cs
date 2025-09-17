using System.Windows;
using System.Windows.Controls;
using UAssetAPI;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;
using UAssetManager.Controls;

namespace UAssetManager.Views;
public partial class AddPropertyDialog : Window
{
    private readonly UAsset _asset;

    public PropertyData? Result { get; private set; }

    public AddPropertyDialog(UAsset asset, IEnumerable<string>? commonTypeHints = null)
    {
        InitializeComponent();
        _asset = asset;

        // Preload common types; user can still type freely
        var common = commonTypeHints ?? new[]
        {
            "BoolProperty",
            "IntProperty",
            "FloatProperty",
            "DoubleProperty",
            "StrProperty",
            "NameProperty",
            "ByteProperty",
            "EnumProperty",
            "ObjectProperty",
            "ArrayProperty",
            "StructProperty",
            "VectorProperty",
            "Vector2DProperty",
            "Vector4Property",
            "RotatorProperty",
            "LinearColorProperty",
            "ColorProperty",
            "QuatProperty"
        };
        foreach (var t in common) TypeBox.Items.Add(t);

        TypeBox.LostKeyboardFocus += (_, _) => RefreshEditor();
        TypeBox.SelectionChanged += (_, _) => RefreshEditor();
        TypeBox.IsEditable = true;
        TypeBox.IsTextSearchEnabled = true;
        TypeBox.IsTextSearchCaseSensitive = false;
    }

    private void RefreshEditor()
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(TypeBox.Text))
        {
            EditorHost.Content = null;
            return;
        }

        var property = CreatePropertyData(NameBox.Text.Trim(), TypeBox.Text.Trim());
        if (property == null)
        {
            EditorHost.Content = new TextBlock { Text = "不支持的属性类型", VerticalAlignment = VerticalAlignment.Center };
            return;
        }

        var editor = PropertyEditor.ResolveEditor(_asset, property);
        var element = editor.CreateElement(property);
        editor.CreateBinding(property, element);
        EditorHost.Content = element;
    }

    private PropertyData? CreatePropertyData(string name, string typeText)
    {
        var fname = FName.DefineDummy(_asset, name);
        return typeText.Trim() switch
        {
            "BoolProperty" => new BoolPropertyData(fname) { Value = false },
            "IntProperty" => new IntPropertyData(fname) { Value = 0 },
            "FloatProperty" => new FloatPropertyData(fname) { Value = 0f },
            "DoubleProperty" => new DoublePropertyData(fname) { Value = 0.0 },
            "StrProperty" => new StrPropertyData(fname),
            "NameProperty" => new NamePropertyData(fname) { Value = FName.DefineDummy(_asset, "None") },
            "ByteProperty" => new BytePropertyData(fname) { ByteType = BytePropertyType.Byte, Value = (byte)0 },
            "EnumProperty" => new BytePropertyData(fname) { ByteType = BytePropertyType.FName, EnumType = FName.DefineDummy(_asset, "None"), EnumValue = FName.DefineDummy(_asset, "None") },
            "ObjectProperty" => new ObjectPropertyData(fname),
            "ArrayProperty" => new ArrayPropertyData(fname),
            "StructProperty" => new StructPropertyData(fname, FName.DefineDummy(_asset, "Generic")),
            "VectorProperty" => new VectorPropertyData(fname),
            "Vector2DProperty" => new Vector2DPropertyData(fname),
            "Vector4Property" => new Vector4PropertyData(fname),
            "RotatorProperty" => new RotatorPropertyData(fname),
            "LinearColorProperty" => new LinearColorPropertyData(fname),
            "ColorProperty" => new ColorPropertyData(fname),
            "QuatProperty" => new QuatPropertyData(fname),
            _ => null,
        };
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text?.Trim();
        var type = TypeBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, "请输入属性名称", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            NameBox.Focus();
            return;
        }
        if (string.IsNullOrWhiteSpace(type))
        {
            MessageBox.Show(this, "请输入属性类型", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            TypeBox.Focus();
            return;
        }

        var property = CreatePropertyData(name!, type!);
        if (property == null)
        {
            MessageBox.Show(this, "不支持的属性类型", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Result = property;
        DialogResult = true;
        Close();
    }
}