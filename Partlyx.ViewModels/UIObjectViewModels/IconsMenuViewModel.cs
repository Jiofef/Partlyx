using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Partlyx.Core.Contracts;
using Partlyx.Core.VisualsInfo;
using Partlyx.Services.Dtos;
using Partlyx.Services.ServiceInterfaces;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.UIServices;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class IconsMenuViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly IPartlyxImageService _imageService;
        private readonly IFileDialogService _fileDialogService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _loc;
        private readonly IIconVectorCatalog _iconVectorCatalog;

        private readonly CreateNewElementButton _createImageButton;

        public bool EnableIconSelection { get; set; } = false;
        public string DialogIdentifier { get; set; } = IDialogService.DefaultDialogIdentifier;

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplySearchFilter();
                }
            }
        }

        private bool _canSetColor;
        public bool CanSetColor { get => _canSetColor; private set => SetProperty(ref _canSetColor, value); }

        private ObservableCollection<StoreVectorIconContentViewModel>? _allTheVectorIconsCache;
        public ObservableCollection<StoreVectorIconContentViewModel> AllVectorIcons
        {
            get
            {
                if (_allTheVectorIconsCache != null)
                    return _allTheVectorIconsCache;
                else
                {
                    _allTheVectorIconsCache = new(_iconVectorCatalog.GetAllIconsContentForStore());
                    _allTheVectorIconsDic = _allTheVectorIconsCache.ToDictionary(i => i.FigureType);
                    return _allTheVectorIconsCache;
                }
            }
        }
        private Dictionary<string, StoreVectorIconContentViewModel>? _allTheVectorIconsDic;
        public ObservableCollection<StoreVectorIconContentViewModel> BaseVectorIcons { get; }
        private Dictionary<string, StoreVectorIconContentViewModel> _baseVectorIconsDic;

        private ObservableCollection<StoreVectorIconContentViewModel> _vectorIcons;
        public ObservableCollection<StoreVectorIconContentViewModel> VectorIcons { get => _vectorIcons; set => SetProperty(ref _vectorIcons, value); }

        public ObservableCollection<object> ImagesToShow { get; } = new();
        public IImagesStoreViewModel Images { get; }

        private bool _showAllTheIcons;
        public bool ShowAllTheIcons { get => _showAllTheIcons; set => SetShowAllTheIcons(value); }

        private void SetShowAllTheIcons(bool value)
        {
            if (!SetProperty(ref _showAllTheIcons, value, nameof(ShowAllTheIcons)))
                return;

            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            var sourceVectors = ShowAllTheIcons ? AllVectorIcons : BaseVectorIcons;
            var sourceVectorsDic = ShowAllTheIcons ? _allTheVectorIconsDic : _baseVectorIconsDic;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                VectorIcons = sourceVectors;
            }
            else
            {
                var culture = CultureInfo.CurrentCulture;
                TextInfo textInfo = culture.TextInfo;
                var correctedSearchText = textInfo.ToTitleCase(SearchText).Replace(" ", "");
                var lowerSearchText = correctedSearchText.ToLower();

                var filtered = sourceVectors
                    .Where(v => v.SearchKeys != null &&
                                v.SearchKeys.Any(key =>
                                    key.Contains(lowerSearchText, StringComparison.OrdinalIgnoreCase)))
                    .ToList();


                StoreVectorIconContentViewModel? existingIcon;

                if (sourceVectorsDic!.TryGetValue(correctedSearchText, out existingIcon))
                {
                    filtered.Remove(existingIcon);
                    filtered.Insert(0, existingIcon);
                }

                VectorIcons = new ObservableCollection<StoreVectorIconContentViewModel>(filtered);
            }

            RefillImagesToShow();
        }

        private void RefillImagesToShow()
        {
            ImagesToShow.Clear();

            ImagesToShow.Add(_createImageButton);

            IEnumerable<object> imagesSource = Images.Images;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                imagesSource = Images.Images
                    .Cast<ImageViewModel>()
                    .Where(img => img.Name != null && img.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var img in imagesSource)
            {
                ImagesToShow.Add(img);
            }
        }

        private object? _selectedIconContent;

        public object? SelectedIconContent { get => _selectedIconContent; set => TrySelectIconContent(value); }

        public void SelectItemWith(IIconContentViewModel content)
        {
            switch (content.ContentIconType)
            {
                case IconTypeEnumViewModel.Vector:
                    var vector = (IconVectorContentViewModel)content;
                    IconVectorContentViewModel? vectorToSelect = null;

                    vectorToSelect =
                        BaseVectorIcons.FirstOrDefault(v => v.FigureType == vector.FigureType)
                        ?? AllVectorIcons.FirstOrDefault(v => v.FigureType == vector.FigureType);

                    if (vectorToSelect != null)
                    {
                        TrySelectIconContent(vectorToSelect);
                        SelectedColor = vector.FigureColor;
                    }
                    break;
                default:
                    TrySelectIconContent(content);
                    break;
            }
        }
        private void TrySelectIconContent(object? value)
        {
            if (value is not IIconContentViewModel iconContent)
                return;

            switch (iconContent.ContentIconType)
            {
                case IconTypeEnumViewModel.Vector:
                    var vector = (IconVectorContentViewModel)iconContent;
                    var vectorClone = vector.Clone();
                    vectorClone.FigureColor = SelectedColor;
                    ResultIcon.Content = vectorClone;
                    SetProperty(ref _selectedIconContent, vector, nameof(SelectedIconContent));
                    CanSetColor = true;
                    break;
                case IconTypeEnumViewModel.Image:
                    ResultIcon.Content = iconContent;
                    SetProperty(ref _selectedIconContent, iconContent, nameof(SelectedIconContent));
                    CanSetColor = false;
                    break;
                default:
                    ResultIcon.Content = iconContent;
                    SetProperty(ref _selectedIconContent, iconContent, nameof(SelectedIconContent));
                    CanSetColor = false;
                    break;
            }

            ResultIcon.IconType = iconContent.ContentIconType;
        }

        private Color _selectedColor = StandardVisualSettings.StandardMainPartlyxColor;
        public Color SelectedColor { get => _selectedColor; set => SetSelectedColor(value); }

        private void SetSelectedColor(Color color)
        {
            if (ResultIcon.Content is IconVectorContentViewModel figureContent)
            {
                SetProperty(ref _selectedColor, color, nameof(SelectedColor));
                figureContent.FigureColor = color;
            }
        }

        private IconViewModel _resultIcon = new();

        public IconViewModel ResultIcon { get => _resultIcon; }

        public IconsMenuViewModel(IDialogService dialogService, IPartlyxImageService imageService, IImagesStoreViewModel imagesContainer, IFileDialogService fds, INotificationService ns,
            ILocalizationService loc, IIconVectorCatalog vectorIconsCatalog)
        {
            _dialogService = dialogService;
            _imageService = imageService;
            _fileDialogService = fds;
            _notificationService = ns;
            _loc = loc;
            _iconVectorCatalog = vectorIconsCatalog;

            Images = imagesContainer;

            _createImageButton = new CreateNewElementButton();
            _createImageButton.ClickedCommand = new(async () => await OpenImageLoadingDialog());

            var baseVectorIconsList = _iconVectorCatalog.GetBaseIconsContentForStore();
            BaseVectorIcons = new(baseVectorIconsList);
            _vectorIcons = new ObservableCollection<StoreVectorIconContentViewModel>(baseVectorIconsList);
            _baseVectorIconsDic = baseVectorIconsList.ToDictionary(i => i.FigureType);

            RefillImagesToShow();

            Images.Images.CollectionChanged += OnImagesCollectionChanged;
        }

        private void OnImagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefillImagesToShow();
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
            if (SelectedIconContent is ImageViewModel image)
            {
                image.UiItem.StartRenaming();
            }
        }

        [RelayCommand]
        public async Task TryDeleteSelectedImage()
        {
            if (SelectedIconContent is ImageViewModel image)
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
            CloseWindow(ResultIcon);
        }
        [RelayCommand]
        public void Cancel()
        {
            CloseWindow(null);
        }
    }

    public class StoreVectorIconContentViewModel : IconVectorContentViewModel
    {
        public StoreVectorIconContentViewModel(FigureIconDto dto) : base(dto){}

        public StoreVectorIconContentViewModel(string figureType) : base(figureType){}

        public StoreVectorIconContentViewModel(string figureType, Color figureColor) : base(figureType, figureColor){}

        private List<string> _searchKeys = new();
        public List<string> SearchKeys { get => _searchKeys; set => SetProperty(ref _searchKeys, value); }
    }
}