using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class TopSanPhamBanChayViewModel : BaseViewModel
{
    private readonly ITopSanPhamService _topSanPhamService;
    private readonly RelayCommand _taiTopCommand;
    private readonly RelayCommand _lamMoiCommand;

    private DateTime _fromDate = DateTime.Today.AddDays(-7);
    private DateTime _toDate = DateTime.Today;
    private string _topN = "10";
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public TopSanPhamBanChayViewModel(ITopSanPhamService topSanPhamService)
    {
        _topSanPhamService = topSanPhamService;

        TopSanPhamBanChay = new ObservableCollection<ThongKeTopSanPhamDong>();

        _taiTopCommand = new RelayCommand(ExecuteTaiTop, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<ThongKeTopSanPhamDong> TopSanPhamBanChay { get; }

    public DateTime FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public DateTime ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
    }

    public string TopN
    {
        get => _topN;
        set => SetProperty(ref _topN, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        private set => SetProperty(ref _successMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                _taiTopCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TaiTopCommand => _taiTopCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await TaiTopAsync(cancellationToken);
    }

    private async void ExecuteTaiTop()
    {
        await TaiTopAsync();
    }

    private async void ExecuteLamMoi()
    {
        FromDate = DateTime.Today.AddDays(-7);
        ToDate = DateTime.Today;
        TopN = "10";
        await TaiTopAsync();
    }

    private async Task TaiTopAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (!int.TryParse(TopN, out var topNValue))
        {
            ErrorMessage = "Top N phải là số nguyên.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _topSanPhamService.GetTopSanPhamBanChayAsync(
                FromDate,
                ToDate,
                topNValue,
                cancellationToken);

            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            TopSanPhamBanChay.Clear();
            foreach (var item in result.Data)
            {
                TopSanPhamBanChay.Add(item);
            }

            SuccessMessage = TopSanPhamBanChay.Count == 0
                ? "Không có dữ liệu top sản phẩm trong khoảng thời gian đã chọn."
                : $"Đã tải {TopSanPhamBanChay.Count} sản phẩm bán chạy.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải top sản phẩm bán chạy: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
