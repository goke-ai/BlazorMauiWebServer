using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GokeApp.PageModels;

public partial class MainPageModel(ModalErrorHandler errorHandler) : BasePageModel(errorHandler)
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountText))]
	int _count;

    public string CountText => $"Clicked {Count} times";


    [RelayCommand]
    private void Add()
    {
        Count++;
    }
}