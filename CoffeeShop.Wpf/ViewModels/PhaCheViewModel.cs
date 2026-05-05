using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class PhaCheViewModel : BaseViewModel
{
    private readonly IPhaCheService _phaCheService;
    private readonly IExportPrintService _exportPrintService;
    private readonly SessionService _sessionService;

    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;
    private PhaCheDonHangDong? _selectedDon;

    public PhaCheViewModel(
        IPhaCheService phaCheService,
        IExportPrintService exportPrintService,
        SessionService sessionService)
    {
        _phaCheService = phaCheService;
        _exportPrintService = exportPrintService;
        _sessionService = sessionService;

        DonPhaChe = [];
        LamMoiCommand = new RelayCommand(() => _ = LoadAsync());
        BatDauPhaCheCommand = new RelayCommand(
            () => _ = CapNhatTrangThaiAsync(TrangThaiPhaCheConst.DangPhaChe),
            () => SelectedDon?.TrangThaiPhaChe == TrangThaiPhaCheConst.ChoPhaChe);
        HoanThanhCommand = new RelayCommand(
            () => _ = CapNhatTrangThaiAsync(TrangThaiPhaCheConst.DaHoanThanh),
            () => SelectedDon?.TrangThaiPhaChe == TrangThaiPhaCheConst.DangPhaChe);
        GiaoKhachCommand = new RelayCommand(
            () => _ = CapNhatTrangThaiAsync(TrangThaiPhaCheConst.DaGiaoKhach),
            () => SelectedDon?.TrangThaiPhaChe == TrangThaiPhaCheConst.DaHoanThanh);
        InPhieuPhaCheCommand = new RelayCommand(ExecuteInPhieuPhaChe, () => !IsBusy && SelectedDon is not null);
    }

    public ObservableCollection<PhaCheDonHangDong> DonPhaChe { get; }

    public PhaCheDonHangDong? SelectedDon
    {
        get => _selectedDon;
        set
        {
            if (SetProperty(ref _selectedDon, value))
            {
                ((RelayCommand)BatDauPhaCheCommand).RaiseCanExecuteChanged();
                ((RelayCommand)HoanThanhCommand).RaiseCanExecuteChanged();
                ((RelayCommand)GiaoKhachCommand).RaiseCanExecuteChanged();
                ((RelayCommand)InPhieuPhaCheCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public ICommand LamMoiCommand { get; }
    public ICommand BatDauPhaCheCommand { get; }
    public ICommand HoanThanhCommand { get; }
    public ICommand GiaoKhachCommand { get; }
    public ICommand InPhieuPhaCheCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _phaCheService.GetDonCanPhaCheAsync(cancellationToken);

            if (!result.IsSuccess || result.Data is null)
            {
                ErrorMessage = result.Message;
                return;
            }

            DonPhaChe.Clear();
            foreach (var don in result.Data)
            {
                DonPhaChe.Add(don);
            }

            SuccessMessage = $"Đã tải {DonPhaChe.Count} đơn pha chế.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi tải danh sách pha chế: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CapNhatTrangThaiAsync(string trangThaiMoi, CancellationToken cancellationToken = default)
    {
        if (IsBusy || SelectedDon is null) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _phaCheService.CapNhatTrangThaiAsync(
                SelectedDon,
                trangThaiMoi,
                cancellationToken);

            if (result.IsSuccess)
            {
                // Cập nhật trực tiếp object đang được bind để UI tự động cập nhật
                SelectedDon.TrangThaiPhaChe = trangThaiMoi;

                // Cập nhật thời gian tương ứng với trạng thái
                var now = DateTime.Now;
                switch (trangThaiMoi)
                {
                    case TrangThaiPhaCheConst.DangPhaChe:
                        SelectedDon.ThoiGianBatDauPhaChe = now;
                        break;
                    case TrangThaiPhaCheConst.DaHoanThanh:
                        SelectedDon.ThoiGianHoanThanhPhaChe = now;
                        break;
                    case TrangThaiPhaCheConst.DaGiaoKhach:
                        SelectedDon.ThoiGianGiaoKhach = now;
                        // Xóa đơn khỏi danh sách khi đã giao khách
                        DonPhaChe.Remove(SelectedDon);
                        SelectedDon = null;
                        break;
                }

                SuccessMessage = result.Message;

                // Refresh CanExecute của các command
                ((RelayCommand)BatDauPhaCheCommand).RaiseCanExecuteChanged();
                ((RelayCommand)HoanThanhCommand).RaiseCanExecuteChanged();
                ((RelayCommand)GiaoKhachCommand).RaiseCanExecuteChanged();
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi cập nhật trạng thái: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExecuteInPhieuPhaChe()
    {
        if (IsBusy || SelectedDon is null)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var currentUserId = _sessionService.CurrentUser?.UserId;
            var result = await _exportPrintService.InPhieuPhaCheAsync(
                SelectedDon.HoaDonBanId,
                null,
                currentUserId);

            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi in phiếu pha chế: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
