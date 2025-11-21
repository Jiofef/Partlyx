using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.Core.VisualsInfo;
using Partlyx.Infrastructure.Events;
using Partlyx.Services;
using Partlyx.Services.Dtos;
using Partlyx.Services.PartsEventClasses;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.UIStates;

namespace Partlyx.ViewModels.GraphicsViewModels.IconViewModels
{
    public class ImageViewModel : UpdatableViewModel<ImageDto>, IDisposable
    {
        private readonly ImageUiItemStateFactoryViewModel _stateFactoryViewModel;
        private IDisposable _imageUpdatedSubscription;
        public ImageViewModel(ImageDto dto, ImageUiItemStateFactoryViewModel uiItemFactory, IEventBus bus)
        {
            Uid = dto.Uid;
            _name = dto.Name;
            Mime = dto.Mime;
            Hash = dto.Hash;
            Compressed = dto.CompressedContent;
            Original = dto.Content;

            _stateFactoryViewModel = uiItemFactory;
            _imageUpdatedSubscription = bus.Subscribe<ImageUpdatedEvent>(OnImageUpdated);
        }

        // Info updating
        protected override Dictionary<string, Action<ImageDto>> ConfigureUpdaters() => new()
        {
            { nameof(ImageDto.Name), dto => Name = dto.Name },
        };
        private void OnImageUpdated(ImageUpdatedEvent ev)
        {
            if (Uid != ev.Image.Uid) return;

            Update(ev.Image, ev.ChangedProperties);
        }

        // Main info
        public Guid Uid { get; }

        private string _name;
        public string Name { get => _name; private set => SetProperty(ref _name, value); }

        public string Mime { get; }

        public byte[] Hash { get; }

        private byte[]? _compressed;
        public byte[]? Compressed 
        {
            get => _compressed;
            set
            {
                SetProperty(ref _compressed, value);
                UpdateToDrawBytes();
            }
        }

        private byte[]? _original;
        public byte[]? Original
        {
            get => _original;
            set
            {
                SetProperty(ref _original, value);
                UpdateToDrawBytes();
            }
        }

        private void UpdateToDrawBytes()
        {
            if (Original != null)
                ToDraw = Original;
            else if (Compressed != null)
                ToDraw = Compressed;
            else
                ToDraw = null;
        }
        private byte[]? _toDraw;

        public byte[]? ToDraw { get => _toDraw; set => SetProperty(ref _toDraw, value);}

        public void Dispose()
        {
            _imageUpdatedSubscription?.Dispose();
        }

        // For UI
        public ImageUiItemStateViewModel UiItem => _stateFactoryViewModel.GetOrCreateItemUi(this);
    }
}
