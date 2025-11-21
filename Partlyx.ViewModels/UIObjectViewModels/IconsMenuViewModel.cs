using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Partlyx.Core.Contracts;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class IconsMenuViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly IPartlyxImageService _imageService;
        private readonly IFileDialogService _fileDialogService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _loc;

        public bool EnableIconSelection { get; set; } = false;
        public string DialogIdentifier { get; set; } = IDialogService.DefaultDialogIdentifier;

        public ObservableCollection<object> ImagesToShow { get; } = new();
        public IImagesStoreViewModel Images { get; }

        private object? _selectedIcon;

        public object? SelectedIcon { get => _selectedIcon; set => SetSelectedIcon(value); }

        private bool _canSelect;
        public bool CanSelect { get => _canSelect; set => _canSelect = SetProperty(ref _canSelect, value); }
        private void SetSelectedIcon(object? value)
        {
            SetProperty(ref _selectedIcon, value, nameof(SelectedIcon));
            CanSelect = SelectedIcon is ImageViewModel or IconFigureContentViewModel;
        }
        public IconsMenuViewModel(IDialogService dialogService, IPartlyxImageService imageService, IImagesStoreViewModel imagesContainer, IFileDialogService fds, INotificationService ns,
            ILocalizationService loc)
        {
            _dialogService = dialogService;
            _imageService = imageService;
            _fileDialogService = fds;
            _notificationService = ns;
            _loc = loc;

            Images = imagesContainer;

            var imagesCreatorButton = new CreateNewElementButton();
            ImagesToShow.Add(imagesCreatorButton);
            ImagesToShow.AddRange(Images.Images);
            Images.Images.CollectionChanged += OnImagesCollectionChanged;
            imagesCreatorButton.ClickedCommand = new(async () => await OpenImageLoadingDialog());
        }
        private void OnImagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // all positions in ImagesToShow are shifted by +1 due to the CreateNewElementButton
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex >= 0)
                    {
                        int insertIndex = 1 + e.NewStartingIndex;
                        foreach (var newItem in e.NewItems!.Cast<object>())
                            ImagesToShow.Insert(insertIndex++, newItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex >= 0)
                    {
                        for (int i = 0; i < e.OldItems!.Count; i++)
                            ImagesToShow.RemoveAt(1 + e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.NewStartingIndex >= 0)
                        ImagesToShow[1 + e.NewStartingIndex] = e.NewItems![0]!;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ImagesToShow.Clear();
                    ImagesToShow.Add(new CreateNewElementButton());
                    ImagesToShow.AddRange(Images.Images);
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex >= 0 && e.NewStartingIndex >= 0)
                    {
                        var item = ImagesToShow[1 + e.OldStartingIndex];
                        ImagesToShow.RemoveAt(1 + e.OldStartingIndex);
                        ImagesToShow.Insert(1 + e.NewStartingIndex, item);
                    }
                    break;
            }
        }
        [RelayCommand]
        public async Task OpenImageLoadingDialog()
        {
            var path = await _fileDialogService.ShowOpenFileDialogAsync(new FileDialogOptions()
            {
                Title = _loc["Load_image"],
                Filter = _loc["filter_Image_files"],
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            });

            if (path == null) return;

            try
            {
                var result = await _imageService.TryLoadFromDiskToDbAsync(path);
            }
            catch
            {
                Trace.WriteLine("Image import error");
                await _notificationService.ShowErrorAsync(NotificationPresets.InvalidImageLoadingErrorPreset);
            }
        }

        [RelayCommand]
        public void TryStartRenamingSelectedImage()
        {
            if (SelectedIcon is ImageViewModel image)
            {
                image.UiItem.StartRenaming();
            }
        }

        [RelayCommand]
        public async Task TryDeleteSelectedImage()
        {
            if (SelectedIcon is ImageViewModel image)
            {
                await image.UiItem.DeleteImage();
            }
        }

        [RelayCommand]
        public void CloseWindow(object? arg)
        {
            _dialogService.Close(DialogIdentifier, arg);
        }

        [RelayCommand]
        public void Ok()
        {
            CloseWindow(null);
        }
        [RelayCommand]
        public void Select()
        {
            CloseWindow(SelectedIcon);
        }
        [RelayCommand]
        public void Cancel()
        {
            CloseWindow(null);
        }
    }
}