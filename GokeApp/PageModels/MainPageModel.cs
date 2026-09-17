using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GokeApp.PageModels;

public partial class MainPageModel : ObservableObject
{
	private bool _isNavigatedTo;
	private bool _dataLoaded;
	private readonly ModalErrorHandler _errorHandler;

	[ObservableProperty]
	bool _isBusy;

    [ObservableProperty]
    bool _isRefreshing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountText))]
	int _count;

    public string CountText => $"Clicked {Count} times";

    public MainPageModel(ModalErrorHandler errorHandler)
	{
		_errorHandler = errorHandler;
	}

	private async Task LoadData()
	{
		try
		{
			IsBusy = true;
            
        }
        finally
		{
			IsBusy = false;
		}
	}

	private async Task InitData()
	{
		await Refresh();
	}

	[RelayCommand]
	private async Task Add()
	{
        Count++;
    }

	[RelayCommand]
    private async Task Counter()
    {
        Count++;

        //if (Count == 1)
        //    CountText = $"Clicked {Count} time";
        //else
        //    CountText = $"Clicked {Count} times";

        SemanticScreenReader.Announce(CountText);
    }

	[RelayCommand]
    private void GoToLogin()
    {
        Shell.Current.GoToAsync("//Login");

    }

    [RelayCommand]
	private async Task Refresh()
	{
		try
		{
			IsRefreshing = true;
			await LoadData();
		}
		catch (Exception e)
		{
			_errorHandler.HandleError(e);
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