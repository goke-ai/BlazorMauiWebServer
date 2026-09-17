using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GokeApp.Services;

namespace GokeApp.PageModels;

public partial class BasePageModel(ModalErrorHandler errorHandler) : ObservableObject
{
	private bool _isNavigatedTo;
	private bool _dataLoaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool _isBusy;

    [ObservableProperty]
    bool _isRefreshing;

    [ObservableProperty]
    private string _today = DateTime.Now.ToString("dddd, MMM d");


    public bool IsNotBusy => !IsBusy;

    private async Task LoadData()
	{
		try
		{
			IsBusy = true;

            //await Task.Delay(2000); // Simulate a delay for the data loading operation

            await OnLoadDataAsync();
            
        }
        finally
		{
			IsBusy = false;
		}
	}

    protected virtual async Task OnLoadDataAsync()
    {

    }

    private async Task InitData()
	{
        await OnInitDataAsync();
        //await Refresh();
	}

    protected virtual async Task OnInitDataAsync()
    {

    }

    [RelayCommand]
	private async Task Refresh()
	{
		try
		{
			IsRefreshing = true;

			//await Task.Delay(2000); // Simulate a delay for the refresh operation

            await LoadData();
		}
		catch (Exception e)
		{
			errorHandler.HandleError(e);
		}
		finally
		{
			IsRefreshing = false;
		}
	}

	[RelayCommand]
	private void NavigatedTo() =>
		_isNavigatedTo = true;

	[RelayCommand]
	private void NavigatedFrom() =>
		_isNavigatedTo = false;

	[RelayCommand]
	private async Task Appearing()
	{
		if (!_dataLoaded)
		{
            await InitData();
            _dataLoaded = true;
			await Refresh();
		}
		// This means we are being navigated to
		else if (!_isNavigatedTo)
		{
			await Refresh();
		}
	}

}