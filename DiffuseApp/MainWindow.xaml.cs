using System;
using System.Threading.Tasks;
using Diffuse.Common;
using Diffuse.Dialogs;
using Diffuse.Services;
using Diffuse.Views;
using TensorStack.WPF;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Services;

namespace Diffuse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowMainBase
    {
        private View _view;
        private ViewCategory _viewCategory;
        private View _defaultView = View.TextToImage;

        public MainWindow(Settings configuration, NavigationService navigation, IHistoryService historyService)
        {
            Navigation = navigation;
            HistoryService = historyService;
            NavigateCommand = new AsyncRelayCommand<View>(NavigateAsync, CanNavigate);
            NavigateCategoryCommand = new AsyncRelayCommand<ViewCategory>(NavigateCategoryAsync, CanNavigateCategory);
            RemoveHistoryItemCommand = new AsyncRelayCommand<HistoryItem>(RemoveHistoryItemAsync, CanRemoveHistoryItem);
            PreviewHistoryItemCommand = new AsyncRelayCommand<HistoryItem>(PreviewHistoryItemAsync, CanPreviewHistoryItem);
            InitializeComponent();
            NavigateCommand.Execute(_defaultView);
        }



        public NavigationService Navigation { get; }
        public AsyncRelayCommand<View> NavigateCommand { get; }
        public AsyncRelayCommand<ViewCategory> NavigateCategoryCommand { get; }
        public AsyncRelayCommand<HistoryItem> RemoveHistoryItemCommand { get; }
        public AsyncRelayCommand<HistoryItem> PreviewHistoryItemCommand { get; }
        public IHistoryService HistoryService { get; }


        public View View
        {
            get { return _view; }
            set { SetProperty(ref _view, value); }
        }

        public ViewCategory ViewCategory
        {
            get { return _viewCategory; }
            set { SetProperty(ref _viewCategory, value); }
        }


        private async Task NavigateAsync(View view)
        {
            View = view;
            ViewCategory = UpdateViewMap(view);
            await Navigation.NavigateAsync((int)view);
        }


        private bool CanNavigate(View view)
        {
            return true;
        }


        private async Task NavigateCategoryAsync(ViewCategory category)
        {
            if (_viewCategory == category)
                return;

            var previousView = ViewManager.GetCurrentView(category);
            await NavigateAsync(previousView);
        }


        private bool CanNavigateCategory(ViewCategory category)
        {
            return true;
        }


        public override void OnDragBegin(DragDropType type)
        {
            base.OnDragBegin(type);
            Navigation.CurrentView.DragDropType = type;
            Navigation.CurrentView.IsDragDrop = true;
        }


        public override void OnDragEnd()
        {
            base.OnDragEnd();
            Navigation.CurrentView.IsDragDrop = false;
            Navigation.CurrentView.DragDropType = DragDropType.None;
        }


        private ViewCategory UpdateViewMap(View view)
        {
            return ViewManager.SetCurrentView(view);
        }


        private async Task RemoveHistoryItemAsync(HistoryItem item)
        {
            await HistoryService.DeleteAsync(item);
        }

        private bool CanRemoveHistoryItem(HistoryItem item)
        {
            return true;
        }


        private async Task PreviewHistoryItemAsync(HistoryItem item)
        {
            if (item.MediaType == MediaType.Image)
            {
                var dialog = DialogService.GetDialog<PreviewImageDialog>();
                await dialog.ShowDialogAsync(item);
            }
            else if (item.MediaType == MediaType.Video)
            {
                var dialog = DialogService.GetDialog<PreviewVideoDialog>();
                await dialog.ShowDialogAsync(item);
            }
        }

        private bool CanPreviewHistoryItem(HistoryItem item)
        {
            return true;
        }
    }
}