using System.Windows;
using System.Windows.Controls;

namespace CoffeeShop.Wpf.Infrastructure;

public static class PasswordBoxHelper
{
    public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached(
        "BoundPassword",
        typeof(string),
        typeof(PasswordBoxHelper),
        new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

    public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached(
        "Attach",
        typeof(bool),
        typeof(PasswordBoxHelper),
        new PropertyMetadata(false, OnAttachChanged));

    private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached(
        "IsUpdating",
        typeof(bool),
        typeof(PasswordBoxHelper),
        new PropertyMetadata(false));

    public static string GetBoundPassword(DependencyObject dependencyObject)
    {
        return (string)dependencyObject.GetValue(BoundPasswordProperty);
    }

    public static void SetBoundPassword(DependencyObject dependencyObject, string value)
    {
        dependencyObject.SetValue(BoundPasswordProperty, value);
    }

    public static bool GetAttach(DependencyObject dependencyObject)
    {
        return (bool)dependencyObject.GetValue(AttachProperty);
    }

    public static void SetAttach(DependencyObject dependencyObject, bool value)
    {
        dependencyObject.SetValue(AttachProperty, value);
    }

    private static bool GetIsUpdating(DependencyObject dependencyObject)
    {
        return (bool)dependencyObject.GetValue(IsUpdatingProperty);
    }

    private static void SetIsUpdating(DependencyObject dependencyObject, bool value)
    {
        dependencyObject.SetValue(IsUpdatingProperty, value);
    }

    private static void OnBoundPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox)
        {
            return;
        }

        if (GetIsUpdating(passwordBox))
        {
            return;
        }

        passwordBox.PasswordChanged -= PasswordBoxOnPasswordChanged;
        passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
        passwordBox.PasswordChanged += PasswordBoxOnPasswordChanged;
    }

    private static void OnAttachChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not PasswordBox passwordBox)
        {
            return;
        }

        var shouldAttach = (bool)e.NewValue;

        if (shouldAttach)
        {
            passwordBox.PasswordChanged += PasswordBoxOnPasswordChanged;
            return;
        }

        passwordBox.PasswordChanged -= PasswordBoxOnPasswordChanged;
    }

    private static void PasswordBoxOnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
        {
            return;
        }

        SetIsUpdating(passwordBox, true);
        SetBoundPassword(passwordBox, passwordBox.Password);
        SetIsUpdating(passwordBox, false);
    }
}
