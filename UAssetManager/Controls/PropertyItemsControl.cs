using System.Windows;
using System.Windows.Controls;
using UAssetAPI.PropertyTypes.Objects;

namespace UAssetManager.Controls;
public class PropertyItemsControl : ListBox
{
    protected override bool IsItemItsOwnContainerOverride(object item) => item is PropertyItem;

    public PropertyItemsControl()
    {
        VirtualizingPanel.SetIsVirtualizingWhenGrouping(this, true);
        VirtualizingPanel.SetScrollUnit(this, ScrollUnit.Pixel);
    }
}

public class PropertyItem : ListBoxItem
{
    public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(nameof(PropertyName), typeof(string), typeof(PropertyItem));
    public string PropertyName
    {
        get => (string)GetValue(PropertyNameProperty);
        set => SetValue(PropertyNameProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(PropertyData), typeof(PropertyItem));
    public PropertyData Value
    {
        get => (PropertyData)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(PropertyItem));
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(nameof(Category), typeof(string), typeof(PropertyItem));
    public string Category
    {
        get => (string)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    public static readonly DependencyProperty EditorProperty = DependencyProperty.Register(nameof(Editor), typeof(PropertyEditorBase), typeof(PropertyItem));
    public PropertyEditorBase Editor
    {
        get => (PropertyEditorBase)GetValue(EditorProperty);
        set => SetValue(EditorProperty, value);
    }

    public static readonly DependencyProperty EditorElementProperty = DependencyProperty.Register(nameof(EditorElement), typeof(FrameworkElement), typeof(PropertyItem));
    public FrameworkElement EditorElement
    {
        get => (FrameworkElement)GetValue(EditorElementProperty);
        set => SetValue(EditorElementProperty, value);
    }

    public virtual void InitElement()
    {
        if (Editor == null) return;
        EditorElement = Editor.CreateElement(Value);
        Editor.CreateBinding(Value, EditorElement);
    }
}