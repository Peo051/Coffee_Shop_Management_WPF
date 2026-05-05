using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CoffeeShop.Wpf.Models;

/// <summary>
/// Đại diện cho một nhóm menu có thể thu gọn/mở rộng trong sidebar
/// </summary>
public sealed class MenuGroupModel : INotifyPropertyChanged
{
    private bool _isExpanded;

    public MenuGroupModel(string title, string icon = "📁")
    {
        Title = title;
        Icon = icon;
        Items = new ObservableCollection<MenuItemModel>();
    }

    /// <summary>Tên nhóm menu (không có số thứ tự)</summary>
    public string Title { get; }

    /// <summary>Icon đại diện cho nhóm (hiển thị khi sidebar collapsed)</summary>
    public string Icon { get; }

    /// <summary>Tooltip hiển thị khi hover vào icon (khi collapsed)</summary>
    public string Tooltip => Title;

    /// <summary>Danh sách menu con trong nhóm</summary>
    public ObservableCollection<MenuItemModel> Items { get; }

    /// <summary>Trạng thái mở/đóng của nhóm</summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
