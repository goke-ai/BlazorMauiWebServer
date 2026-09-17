using GokeApp.Extensions;
using System.Windows.Input;

namespace GokeApp.Pages.Controls
{
    [ContentProperty(nameof(ScrollContent))]
    public partial class ScrollViewPage : AuthorizePage
    {
        private const double DefaultProgressBarHeight = 4d;

        public static readonly BindableProperty ScrollContentProperty =
            BindableProperty.Create(
                nameof(ScrollContent),
                typeof(View),
                typeof(ScrollViewPage),
                null,
                propertyChanged: OnScrollContentChanged);

        public static readonly BindableProperty RefreshCommandProperty =
            BindableProperty.Create(
                nameof(RefreshCommand),
                typeof(ICommand),
                typeof(ScrollViewPage));

        public static readonly BindableProperty IsRefreshingProperty =
            BindableProperty.Create(
                nameof(IsRefreshing),
                typeof(bool),
                typeof(ScrollViewPage),
                false,
                propertyChanged: OnIsRefreshingChanged);

        public static readonly BindableProperty IsBusyOverlayVisibleProperty =
            BindableProperty.Create(
                nameof(IsBusyOverlayVisible),
                typeof(bool),
                typeof(ScrollViewPage),
                false,
                propertyChanged: OnBusyChanged);

        public static readonly BindableProperty ProgressBarColorProperty =
            BindableProperty.Create(
                nameof(ProgressBarColor),
                typeof(Color),
                typeof(ScrollViewPage),
                Colors.DeepSkyBlue,
                propertyChanged: OnProgressBarColorChanged);

        public View ScrollContent
        {
            get => (View)GetValue(ScrollContentProperty);
            set => SetValue(ScrollContentProperty, value);
        }


        public ICommand RefreshCommand
        {
            get => (ICommand)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }

        public bool IsRefreshing
        {
            get => (bool)GetValue(IsRefreshingProperty);
            set => SetValue(IsRefreshingProperty, value);
        }

        public bool IsBusyOverlayVisible
        {
            get => (bool)GetValue(IsBusyOverlayVisibleProperty);
            set => SetValue(IsBusyOverlayVisibleProperty, value);
        }

        public Color ProgressBarColor
        {
            get => (Color)GetValue(ProgressBarColorProperty);
            set => SetValue(ProgressBarColorProperty, value);
        }

        private readonly Grid _rootGrid;
        private readonly Grid _busyOverlay;
        private readonly Grid _refreshProgressBar;
        private readonly BoxView _progressFill;
        private readonly RefreshView _refreshView;
        private readonly ScrollView _scrollView;

        private bool _isInitialized;
        private int _refreshAnimationVersion;

        public ScrollViewPage(
            Thickness? padding = null,
            double spacing = 10,
            ScrollOrientation orientation = ScrollOrientation.Vertical)
        {
            var initialContent = CreateDefaultContent(orientation, padding ?? default, spacing);

            _scrollView = new ScrollView
            {
                Orientation = orientation,
                Content = initialContent
            };

            _refreshView = new RefreshView
            {
                Command = new Command(ExecuteRefresh),
                Content = _scrollView
            };

            _progressFill = new BoxView
            {
                WidthRequest = 0,
                HeightRequest = DefaultProgressBarHeight,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Fill
            };

            _refreshProgressBar = new Grid
            {
                IsVisible = false,
                Opacity = 0,
                HeightRequest = DefaultProgressBarHeight,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill,
                Children = { _progressFill }
            };

            _busyOverlay = new Grid
            {
                IsVisible = false,
                InputTransparent = false,
                BackgroundColor = Colors.Black.WithAlpha(0.4f),
                Children =
                {
                    new ActivityIndicator
                    {
                        IsRunning = true,
                        Color = Colors.White,
                        WidthRequest = 60,
                        HeightRequest = 60,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                }
            };

            _rootGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(DefaultProgressBarHeight) },
                    new RowDefinition { Height = GridLength.Star }
                }
            };

            _rootGrid.Add(_refreshProgressBar);
            Grid.SetRow(_refreshProgressBar, 0);

            _rootGrid.Add(_refreshView);
            Grid.SetRow(_refreshView, 1);

            _rootGrid.Add(_busyOverlay);
            Grid.SetRowSpan(_busyOverlay, 2);

            Content = _rootGrid;
            ScrollContent = initialContent;

            _isInitialized = true;

            UpdateScrollContent(ScrollContent);
            UpdateProgressBarColor(ProgressBarColor);
            UpdateBusyOverlayVisibility(IsBusyOverlayVisible);
        }

        protected override Task OnAuthorizedAppearingAsync()
        {
            return base.OnAuthorizedAppearingAsync();
        }


        private static void OnScrollContentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ScrollViewPage page)
            {
                View? view = newValue as View;
                if (view is not null)
                {
                    page.UpdateScrollContent(view);
                }
            }
        }

        private static async void OnIsRefreshingChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is not ScrollViewPage page || !page._isInitialized)
            {
                return;
            }

            if ((bool)newValue)
            {
                await page.StartRefreshAnimationAsync();
            }
            else
            {
                await page.StopRefreshAnimationAsync();
            }
        }

        private static void OnBusyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ScrollViewPage page)
            {
                page.UpdateBusyOverlayVisibility((bool)newValue);
            }
        }

        private static void OnProgressBarColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ScrollViewPage page && newValue is Color color)
            {
                page.UpdateProgressBarColor(color);
            }
        }

        private void ExecuteRefresh()
        {
            if (IsRefreshing)
            {
                return;
            }

            RefreshCommand?.Execute(null);
        }

        private void UpdateScrollContent(View view)
        {
            if (!_isInitialized || view is null)
            {
                return;
            }

            _scrollView.Content = view;
        }

        private void UpdateBusyOverlayVisibility(bool isVisible)
        {
            if (!_isInitialized)
            {
                return;
            }

            _busyOverlay.IsVisible = isVisible;
        }

        private void UpdateProgressBarColor(Color color)
        {
            if (!_isInitialized)
            {
                return;
            }

            _progressFill.BackgroundColor = color;
        }

        private static View CreateDefaultContent(ScrollOrientation orientation, Thickness padding, double spacing)
        {
            return orientation == ScrollOrientation.Vertical
                ? new VerticalStackLayout
                {
                    Padding = padding,
                    Spacing = spacing
                }
                : new HorizontalStackLayout
                {
                    Padding = padding,
                    Spacing = spacing
                };
        }

        private async Task StartRefreshAnimationAsync()
        {
            var version = ++_refreshAnimationVersion;

            _progressFill.AbortAnimation(nameof(StartRefreshAnimationAsync));
            _refreshView.IsRefreshing = true;
            _refreshProgressBar.IsVisible = true;
            _refreshProgressBar.Opacity = 0;
            _progressFill.WidthRequest = 0;

            await _refreshProgressBar.FadeToAsync(1, 200);

            if (version != _refreshAnimationVersion || !IsRefreshing)
            {
                return;
            }

            var totalWidth = _refreshProgressBar.Width > 0 ? _refreshProgressBar.Width : _rootGrid.Width;
            if (totalWidth > 0)
            {
                await _progressFill.WidthRequestTo(totalWidth, 600, Easing.CubicInOut);
            }
        }

        private async Task StopRefreshAnimationAsync()
        {
            _refreshAnimationVersion++;
            _progressFill.AbortAnimation(nameof(StartRefreshAnimationAsync));

            await _refreshProgressBar.FadeToAsync(0, 200);
            _refreshProgressBar.IsVisible = false;
            _progressFill.WidthRequest = 0;
            _refreshView.IsRefreshing = false;
        }
    }
}