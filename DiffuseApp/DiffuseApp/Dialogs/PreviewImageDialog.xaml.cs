// Copyright (c) TensorStack. All rights reserved.
// Licensed under the Apache 2.0 License.
using Diffuse.Common;
using Diffuse.Services;
using System.Threading.Tasks;
using System.Windows.Data;
using TensorStack.Image;
using TensorStack.WPF;
using TensorStack.WPF.Controls;

namespace Diffuse.Dialogs
{
    /// <summary>
    /// Interaction logic for PreviewImageDialog.xaml
    /// </summary>
    public partial class PreviewImageDialog : DialogControl
    {
        private ImageInput _currentImage;

        public PreviewImageDialog(Settings settings, IHistoryService historyService)
        {
            Settings = settings;
            HistoryService = historyService;
            CancelCommand = new AsyncRelayCommand(CancelAsync);
            PrevCommand = new AsyncRelayCommand(PrevAsync, CanMovePrev);
            NextCommand = new AsyncRelayCommand(NextAsync, CanMoveNext);
            ImageCollection = new ListCollectionView(HistoryService.HistoryCollection)
            {
                Filter = (obj) =>
                {
                    if (obj is not IHistoryItem item)
                        return false;
                    return item.MediaType == MediaType.Image;
                }
            };
            InitializeComponent();
        }

        public Settings Settings { get; }
        public IHistoryService HistoryService { get; }
        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand PrevCommand { get; }
        public AsyncRelayCommand NextCommand { get; }
        public ListCollectionView ImageCollection { get; }

        public ImageInput CurrentImage
        {
            get { return _currentImage; }
            set { SetProperty(ref _currentImage, value); }
        }


        public Task<bool> ShowDialogAsync(IHistoryItem selectedItem)
        {
            ImageCollection.MoveCurrentTo(selectedItem);
            SetCurrentImage();
            Height = CurrentImage.Height + 150;
            Width = CurrentImage.Width + 20;
            return base.ShowDialogAsync();
        }


        private Task PrevAsync()
        {
            if (CanMoveNext())
            {
                ImageCollection.MoveCurrentToPrevious();
                SetCurrentImage();
            }

            return Task.CompletedTask;
        }


        private bool CanMovePrev()
        {
            return !ImageCollection.IsCurrentBeforeFirst 
                 && ImageCollection.CurrentPosition > 0;
        }


        private Task NextAsync()
        {
            if (CanMoveNext())
            {
                ImageCollection.MoveCurrentToNext();
                SetCurrentImage();
            }

            return Task.CompletedTask;
        }


        private bool CanMoveNext()
        {
            return !ImageCollection.IsCurrentAfterLast 
                 && ImageCollection.CurrentPosition < ImageCollection.Count - 1;
        }


        private void SetCurrentImage()
        {
            var currentItem = ImageCollection.CurrentItem as IHistoryItem;
            if (currentItem == null)
                return;

            CurrentImage = new ImageInput(currentItem.MediaPath);
        }
    }
}
