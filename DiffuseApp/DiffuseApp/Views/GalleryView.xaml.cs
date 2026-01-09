using Diffuse.Common;
using Diffuse.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using TensorStack.Image;
using TensorStack.Video;
using TensorStack.WPF;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Services;

namespace Diffuse.Views
{
    /// <summary>
    /// Interaction logic for GalleryView.xaml
    /// </summary>
    public partial class GalleryView : ViewBase
    {
        private const string String_AllModels = "All Models";
        private IHistoryItem _selectedItem;
        private ImageInput _currentImage;
        private VideoInputStream _currentVideoStream;
        private string _filterText;
        private string _filterModelName;
        private MediaType? _filterMediaType;
        private View? _filterProcessType;

        public GalleryView(Settings settings, NavigationService navigationService, IEnvironmentService environmentService, IHistoryService historyService)
            : base(settings, navigationService, environmentService, historyService)
        {
            RemoveFiltersCommand = new AsyncRelayCommand(RemoveFilters, CanRemoveFilters);
            DeleteItemCommand = new AsyncRelayCommand<IHistoryItem>(RemoveItemAsync, (x) => SelectedItem is not null);
            DeleteItemsCommand = new AsyncRelayCommand(RemoveItemsAsync, () => HistoryCollection.Count > 0);
            FilterModelNames = new ObservableCollection<string>();
            FilterProcessTypes = Enum.GetValues<View>().Except([View.Home, View.Settings, View.Gallery, View.History]).ToArray();
            HistoryCollection = new ListCollectionView(HistoryService.HistoryCollection)
            {
                Filter = HistoryCollectionFilter()
            };
            Populate();
            InitializeComponent();
        }

        public override int Id => (int)View.Gallery;
        public ListCollectionView HistoryCollection { get; }
        public View[] FilterProcessTypes { get; }
        public AsyncRelayCommand RemoveFiltersCommand { get; }
        public AsyncRelayCommand<IHistoryItem> DeleteItemCommand { get; }
        public AsyncRelayCommand DeleteItemsCommand { get; }
        public ObservableCollection<string> FilterModelNames { get; }

        public IHistoryItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    SetCurrentItem();
                }
            }
        }

        public ImageInput CurrentImage
        {
            get { return _currentImage; }
            set { SetProperty(ref _currentImage, value); }
        }

        public VideoInputStream CurrentVideoStream
        {
            get { return _currentVideoStream; }
            set { SetProperty(ref _currentVideoStream, value); }
        }

        public string FilterText
        {
            get { return _filterText; }
            set { SetProperty(ref _filterText, value); HistoryCollection?.Refresh(); }
        }

        public string FilterModelName
        {
            get { return _filterModelName; }
            set { SetProperty(ref _filterModelName, value); HistoryCollection?.Refresh(); }
        }

        public MediaType? FilterMediaType
        {
            get { return _filterMediaType; }
            set { SetProperty(ref _filterMediaType, value); HistoryCollection?.Refresh(); }
        }

        public View? FilterProcessType
        {
            get { return _filterProcessType; }
            set { SetProperty(ref _filterProcessType, value); HistoryCollection?.Refresh(); }
        }


        public override Task OpenAsync(OpenViewArgs args = null)
        {
            return base.OpenAsync(args);
        }

        private void SetCurrentItem()
        {
            if (_selectedItem == null)
                return;

            CurrentImage = _selectedItem.MediaType != MediaType.Image ? default : new ImageInput(_selectedItem.MediaPath);
            CurrentVideoStream = _selectedItem.MediaType != MediaType.Video ? default : new VideoInputStream(_selectedItem.MediaPath);
        }


        private Predicate<object> HistoryCollectionFilter()
        {
            return (obj) =>
            {
                if (obj is not IHistoryItem historyItem)
                    return false;

                if (historyItem is RecentHistory)
                    return false;

                var isValid = true;
                if (_filterMediaType.HasValue)
                    isValid = historyItem.MediaType == _filterMediaType.Value;
                if (_filterProcessType.HasValue)
                    isValid = historyItem.Source == _filterProcessType.Value;
                if (_filterModelName?.Equals(String_AllModels) == false)
                    isValid = isValid && historyItem.Model == _filterModelName;

                if (!string.IsNullOrEmpty(_filterText))
                {
                    if (historyItem is DiffusionHistory diffusionHistory)
                    {
                        if (!string.IsNullOrEmpty(diffusionHistory.Options?.Prompt))
                            isValid = isValid && diffusionHistory.Options.Prompt.Contains(_filterText, StringComparison.OrdinalIgnoreCase);
                        if (!string.IsNullOrEmpty(diffusionHistory.Options?.NegativePrompt))
                            isValid = isValid && diffusionHistory.Options.NegativePrompt.Contains(_filterText, StringComparison.OrdinalIgnoreCase);
                    }
                }

                return isValid;
            };
        }


        private void Populate()
        {
            FilterModelNames.Clear();
            FilterModelNames.Add(String_AllModels);
            foreach (var modelName in HistoryService.HistoryCollection.Select(x => x.Model).Distinct().OrderBy(x => x))
            {
                FilterModelNames.Add(modelName);
            }
            FilterModelName = FilterModelNames[0];
        }


        private async Task RemoveItemAsync(IHistoryItem historyItem)
        {
            if (!HistoryCollection.MoveCurrentToNext())
                HistoryCollection.MoveCurrentToPrevious();

            await HistoryService.DeleteAsync(historyItem);
        }


        private async Task RemoveItemsAsync()
        {
            var message = HistoryCollection.Count < HistoryService.HistoryCollection.Count
                ? $"Are you sure you want you delete {HistoryCollection.Count} filtered gallery items?"
                : $"Are you sure you want you delete all {HistoryCollection.Count} gallery items?";
            if (await DialogService.ShowMessageAsync("Delete Items?", message, TensorStack.WPF.Dialogs.MessageDialogType.YesNo, TensorStack.WPF.Dialogs.MessageBoxIconType.Question, TensorStack.WPF.Dialogs.MessageBoxStyleType.Warning))
            {
                var toRemove = new List<IHistoryItem>();
                foreach (var item in HistoryCollection)
                {
                    if (item is IHistoryItem historyItem)
                        toRemove.Add(historyItem);
                }

                foreach (var historyItem in toRemove)
                {
                    await HistoryService.DeleteAsync(historyItem);
                }
                toRemove.Clear();
                await RemoveFilters();
            }
        }


        private Task RemoveFilters()
        {
            _filterText = null;
            _filterMediaType = null;
            _filterProcessType = null;
            _filterModelName = String_AllModels;

            HistoryCollection.Refresh();
            NotifyPropertyChanged(nameof(FilterText));
            NotifyPropertyChanged(nameof(FilterModelName));
            NotifyPropertyChanged(nameof(FilterMediaType));
            NotifyPropertyChanged(nameof(FilterProcessType));
            return Task.CompletedTask;
        }


        private bool CanRemoveFilters()
        {
            return !string.IsNullOrWhiteSpace(_filterText)
                || _filterModelName?.Equals(String_AllModels) == false
                || _filterMediaType.HasValue
                || _filterProcessType.HasValue;
        }
    }
}