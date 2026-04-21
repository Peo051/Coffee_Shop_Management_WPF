using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class BaoCaoViewModel : BaseViewModel
{
    private readonly IBaoCaoService _baoCaoService;
    private readonly RelayCommand _taiBaoCaoCommand;
    private readonly RelayCommand _lamMoiCommand;

    private DateTime _fromDate = DateTime.Today.AddDays(-7);
    private DateTime _toDate = DateTime.Today;

    private int _tongSoHoaDon;
    private decimal _tongDoanhThuThuan;
    private int _tongSoLuongBan;
    private decimal _tongDoanhThuSanPham;

    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public BaoCaoViewModel(IBaoCaoService baoCaoService)
    {
        _baoCaoService = baoCaoService;

        BaoCaoDonGianRows = new ObservableCollection<BaoCaoDonGianDong>();
        BaoCaoNangCaoRows = new ObservableCollection<BaoCaoNangCaoDong>();

        _taiBaoCaoCommand = new RelayCommand(ExecuteTaiBaoCao, () => !IsBusy);
        _lamMoiCommand = new RelayCommand(ExecuteLamMoi, () => !IsBusy);
    }

    public ObservableCollection<BaoCaoDonGianDong> BaoCaoDonGianRows { get; }

    public ObservableCollection<BaoCaoNangCaoDong> BaoCaoNangCaoRows { get; }

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

    public int TongSoHoaDon
    {
        get => _tongSoHoaDon;
        private set => SetProperty(ref _tongSoHoaDon, value);
    }

    public decimal TongDoanhThuThuan
    {
        get => _tongDoanhThuThuan;
        private set => SetProperty(ref _tongDoanhThuThuan, value);
    }

    public int TongSoLuongBan
    {
        get => _tongSoLuongBan;
        private set => SetProperty(ref _tongSoLuongBan, value);
    }

    public decimal TongDoanhThuSanPham
    {
        get => _tongDoanhThuSanPham;
        private set => SetProperty(ref _tongDoanhThuSanPham, value);
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
                _taiBaoCaoCommand.RaiseCanExecuteChanged();
                _lamMoiCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand TaiBaoCaoCommand => _taiBaoCaoCommand;

    public ICommand LamMoiCommand => _lamMoiCommand;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await TaiBaoCaoAsync(cancellationToken);
    }

    private async void ExecuteTaiBaoCao()
    {
        await TaiBaoCaoAsync();
    }

    private async Task TaiBaoCaoAsync(CancellationToken cancellationToken = default)
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
            var result = await _baoCaoService.GetTongHopAsync(FromDate, ToDate, cancellationToken);
            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            BaoCaoDonGianRows.Clear();
            foreach (var row in result.Data.BaoCaoDonGian.OrderByDescending(x => x.Ngay))
            {
                BaoCaoDonGianRows.Add(row);
            }

            BaoCaoNangCaoRows.Clear();
            foreach (var row in result.Data.BaoCaoNangCao)
            {
                BaoCaoNangCaoRows.Add(row);
            }

            TongSoHoaDon = result.Data.TongSoHoaDon;
            TongDoanhThuThuan = result.Data.TongDoanhThuThuan;
            TongSoLuongBan = result.Data.TongSoLuongBan;
            TongDoanhThuSanPham = result.Data.TongDoanhThuSanPham;

            SuccessMessage = result.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Không thể tải báo cáo: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteLamMoi()
    {
        FromDate = DateTime.Today.AddDays(-7);
        ToDate = DateTime.Today;

        await TaiBaoCaoAsync();
    }
}
