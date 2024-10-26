using HeartPlayer.Models;
using HeartPlayer.Services;

namespace HeartPlayer.Behaviors
{
    public class ThumbnailLoaderBehavior : Behavior<Image>
    {
        public static readonly BindableProperty VideoFileProperty =
            BindableProperty.Create(nameof(VideoFile), typeof(VideoFile), typeof(ThumbnailLoaderBehavior), null, propertyChanged: OnVideoFileChanged);

        public ThumbnailLoaderBehavior()
        {
            Console.WriteLine("ThumbnailLoaderBehavior instantiated");
        }

        public VideoFile VideoFile
        {
            get => (VideoFile)GetValue(VideoFileProperty);
            set => SetValue(VideoFileProperty, value);
        }

        private static async void OnVideoFileChanged(BindableObject bindable, object oldValue, object newValue)
        {
            Console.WriteLine($"OnVideoFileChanged called. New value: {newValue}");

            var behavior = (ThumbnailLoaderBehavior)bindable;
            if (behavior.AssociatedObject != null && newValue is VideoFile videoFile)
            {
                await behavior.LoadThumbnailAsync(videoFile);
            }
        }

        protected override void OnAttachedTo(Image bindable)
        {
            base.OnAttachedTo(bindable);
            AssociatedObject = bindable;
        }

        protected override void OnDetachingFrom(Image bindable)
        {
            base.OnDetachingFrom(bindable);
            AssociatedObject = null;
        }

        private Image AssociatedObject { get; set; }

        private async Task LoadThumbnailAsync(VideoFile videoFile)
        {
            if (AssociatedObject == null) return;

            var thumbnailService = Application.Current.Handler.MauiContext.Services.GetService<ThumbnailService>();
            if (thumbnailService == null) return;

            var thumbnail = await thumbnailService.GetThumbnailAsync(videoFile);
            await MainThread.InvokeOnMainThreadAsync(
                      () =>
                      {
                          videoFile.Thumbnail = thumbnail;
                      });
        }
    }
}
