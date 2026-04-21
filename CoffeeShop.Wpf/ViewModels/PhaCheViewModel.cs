using System.Collections.ObjectModel;
using System.Windows.Input;
using CoffeeShop.Wpf.Commands;
using CoffeeShop.Wpf.Models;
using CoffeeShop.Wpf.Services;

namespace CoffeeShop.Wpf.ViewModels;

public sealed class PhaCheViewModel : BaseViewModel
{
    private readonly IPhaCheService _phaCheService;

    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;
    private PhaCheDonHangDong? _selectedDon;

    public PhaCheViewModel(IPhaCheService phaCheService)
    {
        _phaCheService = phaCheService;

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
        if (IsBusy) return;

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
                SuccessMessage = result.Message;
                await LoadAsync(cancellationToken);
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
}
