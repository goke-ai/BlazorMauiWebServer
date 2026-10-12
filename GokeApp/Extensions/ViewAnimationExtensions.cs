namespace GokeApp.Extensions;

public static class ViewAnimationExtensions
    {
        public static Task WidthRequestTo(
            this VisualElement view,
            double to,
            uint length = 250,
            Easing? easing = null)
        {
            easing ??= Easing.Linear;

            var tcs = new TaskCompletionSource<bool>();
            var from = view.WidthRequest < 0 ? view.Width : view.WidthRequest;

            var animation = new Animation(
                callback: value => view.WidthRequest = value,
                start: from,
                end: to,
                easing: easing);

            animation.Commit(
                owner: view,
                name: "WidthRequestTo",
                rate: 16,
                length: length,
                finished: (_, _) => tcs.TrySetResult(true));

            return tcs.Task;
        }
    }
