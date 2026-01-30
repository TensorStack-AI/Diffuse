// Copyright (c) TensorStack. All rights reserved.
// Licensed under the Apache 2.0 License.
using Diffuse.Common;
using Diffuse.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Data;
using TensorStack.Image;
using TensorStack.Video;
using TensorStack.WPF;
using TensorStack.WPF.Controls;

namespace Diffuse.Dialogs
{
    /// <summary>
    /// Interaction logic for MediaPreviewDialog.xaml
    /// </summary>
    public partial class MediaPreviewDialog : DialogControl
    {
        private ImageInput _currentImage;
        private VideoInputStream _currentVideoStream;

        public MediaPreviewDialog(Settings settings, IHistoryService historyService)
        {

            Settings = settings;
            HistoryService = historyService;
            CancelCommand = new AsyncRelayCommand(CancelAsync);
            PrevCommand = new AsyncRelayCommand(PrevAsync, CanMovePrev);
            NextCommand = new AsyncRelayCommand(NextAsync, CanMoveNext);
            Progress = new ProgressInfo();
            HistoryCollection = new ListCollectionView(HistoryService.HistoryCollection)
            {
                Filter = (obj) =>
                {
                    if (obj is not IHistoryItem item)
                        return false;
                    return item.MediaType == MediaType.Image || item.MediaType == MediaType.Video;
                }
            };
            Loaded += (s, e) => { MaxWidth = double.PositiveInfinity; MaxHeight = double.PositiveInfinity; };
            InitializeComponent();
        }

        public Settings Settings { get; }
        public IHistoryService HistoryService { get; }
        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand PrevCommand { get; }
        public AsyncRelayCommand NextCommand { get; }
        public ListCollectionView HistoryCollection { get; }
        public ProgressInfo Progress { get; }

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


        public async Task<bool> ShowDialogAsync(IHistoryItem selectedItem)
        {
            HistoryCollection.MoveCurrentTo(selectedItem);
            await SetCurrentImage();
            return await base.ShowDialogAsync();
        }


        private async Task PrevAsync()
        {
            if (CanMovePrev())
            {
                HistoryCollection.MoveCurrentToPrevious();
                await SetCurrentImage();
            }
        }


        private bool CanMovePrev()
        {
            return !HistoryCollection.IsCurrentBeforeFirst
                 && HistoryCollection.CurrentPosition > 0;
        }


        private async Task NextAsync()
        {
            if (CanMoveNext())
            {
                HistoryCollection.MoveCurrentToNext();
                await SetCurrentImage();
            }
        }


        private bool CanMoveNext()
        {
            return !HistoryCollection.IsCurrentAfterLast
                 && HistoryCollection.CurrentPosition < HistoryCollection.Count - 1;
        }


        private async Task SetCurrentImage()
        {
            var currentItem = HistoryCollection.CurrentItem as IHistoryItem;
            if (currentItem == null)
                return;

            try
            {
                Progress.Indeterminate();

                CurrentImage = default;
                CurrentVideoStream = default;
                if (currentItem.MediaType == MediaType.Image)
                    CurrentImage = await ImageInput.CreateAsync(currentItem.MediaPath);
                if (currentItem.MediaType == MediaType.Video)
                    CurrentVideoStream = await VideoInputStream.CreateAsync(currentItem.MediaPath);
            }
            finally
            {
                Progress.Clear();
            }
        }
    }
}
