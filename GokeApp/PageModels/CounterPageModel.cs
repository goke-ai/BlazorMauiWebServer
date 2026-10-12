using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GokeApp.Services;

namespace GokeApp.PageModels
{
    public partial class CounterPageModel(ModalErrorHandler errorHandler) : BasePageModel(errorHandler)
    {
        [ObservableProperty]
        int _currentCount = 0;


        [RelayCommand]
        private void IncrementCount()
        {
            CurrentCount++;
        }

    }



}
