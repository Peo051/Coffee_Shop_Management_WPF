using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class DashboardViewModel : BaseViewModel
{
    private readonly IDashboardService _dashboardService;
    private Action<string>? _navigateToModuleAction;
    private readonly RelayCommand _taiLaiCommand;
    private readonly RelayCommand _moThongKeCommand;
    private readonly RelayCommand _moTonKhoThapCommand;
    private readonly RelayCommand _moTopSanPhamCommand;

    private decimal _doanhThuHomNay;
    private int _soHoaDonHomNay;
    private int _soSanPhamDangKinhDoanh;
    private int _soSanPhamSapHet;
    private decimal _tyLeSanPhamDangKinhDoanh;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        _navigateToModuleAction = null;

        TopSanPhamHomNay = new ObservableCollection<ThongKeTopSanPhamDong>();
        DanhSachTonKhoThap = new ObservableCollection<CanhBaoTonKhoThapDong>();
        DoanhThu7Ngay = new ObservableCollection<DashboardDoanhThuNgayDong>();

        _taiLaiCommand = new RelayCommand(ExecuteTaiLai, () => !IsBusy);
        _moThongKeCommand = new RelayCommand(ExecuteMoThongKe, () => !IsBusy);
        _moTonKhoThapCommand = new RelayCommand(ExecuteMoTonKhoThap, () => !IsBusy);
        _moTopSanPhamCommand = new RelayCommand(ExecuteMoTopSanPham, () => !IsBusy);
    }

    public void SetNavigationCallback(Action<string> navigateToModule)
    {
        _navigateToModuleAction = navigateToModule;
    }

    public decimal DoanhThuHomNay
    {
        get => _doanhThuHomNay;
        private set => SetProperty(ref _doanhThuHomNay, value);
    }

    public int SoHoaDonHomNay
    {
        get => _soHoaDonHomNay;
        private set => SetProperty(ref _soHoaDonHomNay, value);
    }

    public int SoSanPhamDangKinhDoanh
    {
        get => _soSanPhamDangKinhDoanh;
        private set => SetProperty(ref _soSanPhamDangKinhDoanh, value);
    }

    public int SoSanPhamSapHet
    {
        get => _soSanPhamSapHet;
        private set => SetProperty(ref _soSanPhamSapHet, value);
    }

    public decimal TyLeSanPhamDangKinhDoanh
    {
        get => _tyLeSanPhamDangKinhDoanh;
        private set => SetProperty(ref _tyLeSanPhamDangKinhDoanh, value);
    }

    public ObservableCollection<ThongKeTopSanPhamDong> TopSanPhamHomNay { get; }

    public ObservableCollection<CanhBaoTonKhoThapDong> DanhSachTonKhoThap { get; }

    public ObservableCollection<DashboardDoanhThuNgayDong> DoanhThu7Ngay { get; }

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
                _taiLaiCommand.RaiseCanExecuteChanged();
                _moThongKeCommand.RaiseCanExecuteChanged();
                _moTonKhoThapCommand.RaiseCanExecuteChanged();
                _moTopSanPhamCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TaiLaiCommand => _taiLaiCommand;

    public ICommand MoThongKeCommand => _moThongKeCommand;

    public ICommand MoTonKhoThapCommand => _moTonKhoThapCommand;

    public ICommand MoTopSanPhamCommand => _moTopSanPhamCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await TaiLaiAsync(cancellationToken);
    }

    private async void ExecuteTaiLai()
    {
        await TaiLaiAsync();
    }

    private void ExecuteMoThongKe()
    {
        _navigateToModuleAction?.Invoke("ThongKe");
    }

    private void ExecuteMoTonKhoThap()
    {
        _navigateToModuleAction?.Invoke("CanhBaoTonKho");
    }

    private void ExecuteMoTopSanPham()
    {
        _navigateToModuleAction?.Invoke("TopSanPhamBanChay");
    }

    private async Task TaiLaiAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var tongQuanResult = await _dashboardService.GetTongQuanAsync(cancellationToken);
            if (!tongQuanResult.IsSuccess || tongQuanResult.Data is null)
            {
                ErrorMessage = tongQuanResult.Message;
                return;
            }

            var topResult = await _dashboardService.GetTopSanPhamHomNayAsync(5, cancellationToken);
            if (!topResult.IsSuccess || topResult.Data is null)
            {
                ErrorMessage = topResult.Message;
                return;
            }

            var tonKhoResult = await _dashboardService.GetTonKhoThapAsync(20, cancellationToken);
            if (!tonKhoResult.IsSuccess || tonKhoResult.Data is null)
            {
                ErrorMessage = tonKhoResult.Message;
                return;
            }

            var doanhThu7NgayResult = await _dashboardService.GetDoanhThu7NgayAsync(cancellationToken);
            if (!doanhThu7NgayResult.IsSuccess || doanhThu7NgayResult.Data is null)
            {
                ErrorMessage = doanhThu7NgayResult.Message;
                return;
            }

            DoanhThuHomNay = tongQuanResult.Data.DoanhThuHomNay;
            SoHoaDonHomNay = tongQuanResult.Data.SoHoaDonHomNay;
            SoSanPhamDangKinhDoanh = tongQuanResult.Data.SoSanPhamDangKinhDoanh;
            SoSanPhamSapHet = tongQuanResult.Data.SoSanPhamSapHet;
            TyLeSanPhamDangKinhDoanh = tongQuanResult.Data.TyLeSanPhamDangKinhDoanh;

            TopSanPhamHomNay.Clear();
            foreach (var row in topResult.Data)
            {
                TopSanPhamHomNay.Add(row);
            }

            DanhSachTonKhoThap.Clear();
            foreach (var row in tonKhoResult.Data)
            {
                DanhSachTonKhoThap.Add(row);
            }

            DoanhThu7Ngay.Clear();
            foreach (var row in doanhThu7NgayResult.Data)
            {
                DoanhThu7Ngay.Add(row);
            }

            SuccessMessage = "Tải dữ liệu dashboard thành công.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải dữ liệu dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

