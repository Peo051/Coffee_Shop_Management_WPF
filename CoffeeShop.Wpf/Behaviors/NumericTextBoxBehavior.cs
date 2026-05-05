using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace CoffeeShop.Wpf.Behaviors;

/// <summary>
/// Behavior để xử lý TextBox chỉ nhận số nguyên dương
/// </summary>
public sealed class NumericTextBoxBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewTextInput += OnPreviewTextInput;
        AssociatedObject.KeyDown += OnKeyDown;
        DataObject.AddPastingHandler(AssociatedObject, OnPaste);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
        AssociatedObject.KeyDown -= OnKeyDown;
        DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Chỉ cho phép nhập số
        e.Handled = !IsTextNumeric(e.Text);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Cho phép các phím điều hướng và xóa
        if (e.Key == Key.Back || e.Key == Key.Delete || 
            e.Key == Key.Left || e.Key == Key.Right || 
            e.Key == Key.Tab || e.Key == Key.Enter)
        {
            return;
        }

        // Nếu nhấn Enter, chuyển focus
        if (e.Key == Key.Enter && sender is TextBox textBox)
        {
            textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            e.Handled = true;
        }
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = (string)e.DataObject.GetData(typeof(string));
            if (!IsTextNumeric(text))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    private static bool IsTextNumeric(string text)
    {
        return !string.IsNullOrEmpty(text) && text.All(char.IsDigit);
    }
}
