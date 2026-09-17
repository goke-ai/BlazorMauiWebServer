using CommunityToolkit.Mvvm.ComponentModel;
using Goke.Core.Interfaces;

namespace GokeApp.Pages.Controls
{
    public partial class FormFactorViewModel : ObservableObject
    {
        private readonly IFormFactor formFactorService;

        [ObservableProperty]
        string _factor;

        [ObservableProperty]
        string _platform;

        public FormFactorViewModel(IFormFactor FormFactorService)
        {
            formFactorService = FormFactorService;

            Factor = formFactorService.GetFormFactor();
            Platform = formFactorService.GetPlatform();
        }
    }

       
}
