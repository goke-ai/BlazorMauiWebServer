using System.ComponentModel;
using System.Text.Json;
using Goke.Core.Models;
using GokeApp.PageModels;
using GokeApp.Pages.Controls;

namespace GokeApp.Pages;

public partial class LocationPage : ScrollViewPage
{
    public LocationPage(LocationPageModel vModel)
    {
        InitializeComponent();

        BindingContext = vModel;
    }

}