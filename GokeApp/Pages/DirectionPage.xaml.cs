using System.ComponentModel;
using System.Text.Json;
using Goke.Core.Models;
using GokeApp.PageModels;
using GokeApp.Pages.Controls;

namespace GokeApp.Pages;

public partial class DirectionPage : ScrollViewPage
{
    public DirectionPage(DirectionPageModel vModel)
    {
        InitializeComponent();

        BindingContext = vModel;
    }

}