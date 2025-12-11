using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class IconViewModel : PartlyxObservable
    {
        private IconTypeEnumViewModel _iconType = IconTypeEnumViewModel.Null;
        public IconTypeEnumViewModel IconType { get => _iconType; set => SetProperty(ref _iconType, value); }

        private IIconContentViewModel? _content;

        public IIconContentViewModel? Content { get => _content; set => SetContent(value); }

        private void SetContent(IIconContentViewModel? value)
        {
            SetProperty(ref _content, value, nameof(Content));
            IconType = Content?.ContentIconType ?? IconTypeEnumViewModel.Null;
        }

        public IconViewModel()
        {
            var isEmptyCheckSubscription = this.WhenAnyValue(t => t.Content!.IsEmpty)
                .Subscribe(_ => 
                {
                    IsEmpty = Content?.IsEmpty ?? true;
                });
            Disposables.Add(isEmptyCheckSubscription);
        }
        private bool _isEmpty = true;
        public bool IsEmpty { get => _isEmpty; private set => SetProperty(ref _isEmpty, value); }

        public bool IsIdentical(IconViewModel other)
        {
            return Content == other.Content || Content != null && other.Content != null && Content.IsIdentical(other.Content);
        }
    }
}