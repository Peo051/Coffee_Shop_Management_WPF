using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CoffeeShop.Wpf.ViewModels;

namespace CoffeeShop.Wpf.Views;

public partial class BaoCaoView : UserControl
{
    private BaoCaoViewModel? _viewModel;

    public BaoCaoView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = e.NewValue as BaoCaoViewModel;

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdatePdfViewer();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BaoCaoViewModel.FilePathPdf))
        {
            UpdatePdfViewer();
        }
    }

    private void UpdatePdfViewer()
    {
        if (_viewModel == null || pdfWebViewer == null) return;

        try
        {
            string path = _viewModel.FilePathPdf;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                pdfWebViewer.Navigate(new Uri(path));
            }
            else
            {
                pdfWebViewer.Navigate("about:blank");
            }
        }
        catch (Exception)
        {
            // Ignore navigation exceptions
        }
    }
}
