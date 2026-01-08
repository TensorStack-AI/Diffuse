// Copyright (c) TensorStack. All rights reserved.
// Licensed under the Apache 2.0 License.
using Diffuse.Common;
using Diffuse.Services;
using System.Threading.Tasks;
using System.Windows.Data;
using TensorStack.Video;
using TensorStack.WPF;
using TensorStack.WPF.Controls;

namespace Diffuse.Dialogs
{
    /// <summary>
    /// Interaction logic for PreviewVideoDialog.xaml
    /// </summary>
    public partial class PreviewVideoDialog : DialogControl
    {
        private VideoInputStream _currentVideoStream;

        public PreviewVideoDialog(Settings settings, IHistoryService historyService)
        {
            Settings = settings;
            HistoryService = historyService;
            CancelCommand = new AsyncRelayCommand(CancelAsync);
            PrevCommand = new AsyncRelayCommand(PrevAsync, CanMovePrev);
            NextCommand = new AsyncRelayCommand(NextAsync, CanMoveNext);
            VideoCollection = new ListCollectionView(HistoryService.HistoryCollection)
            {
                Filter = (obj) =>
                {
                    if (obj is not IHistoryItem item)
                        return false;
                    return item.MediaType == MediaType.Video;
                }
            };
            InitializeComponent();
        }

        public Settings Settings { get; }
        public IHistoryService HistoryService { get; }
        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand PrevCommand { get; }
        public AsyncRelayCommand NextCommand { get; }
        public ListCollectionView VideoCollection { get; }

        public VideoInputStream CurrentVideoStream
        {
            get { return _currentVideoStream; }
            set { SetProperty(ref _currentVideoStream, value); }
        }


        public Task<bool> ShowDialogAsync(IHistoryItem selectedItem)
        {
            VideoCollection.MoveCurrentTo(selectedItem);
            SetCurrentVideoStream();
            Height = CurrentVideoStream.Height + 160;
            Width = CurrentVideoStream.Width + 20;
            return base.ShowDialogAsync();
        }


        private Task PrevAsync()
        {
            if (CanMovePrev())
            {
                VideoCollection.MoveCurrentToPrevious();
                SetCurrentVideoStream();
            }
            return Task.CompletedTask;
        }


        private bool CanMovePrev()
        {
            return !VideoCollection.IsCurrentBeforeFirst
                 && VideoCollection.CurrentPosition > 0;
        }


        private Task NextAsync()
        {
            if (CanMoveNext())
            {
                VideoCollection.MoveCurrentToNext();
                SetCurrentVideoStream();
            }
            return Task.CompletedTask;
        }


        private bool CanMoveNext()
        {
            return !VideoCollection.IsCurrentAfterLast
                 && VideoCollection.CurrentPosition < VideoCollection.Count - 1;
        }


        private void SetCurrentVideoStream()
        {
            var currentItem = VideoCollection.CurrentItem as IHistoryItem;
            if (currentItem == null)
                return;

            CurrentVideoStream = new VideoInputStream(currentItem.MediaPath);
        }

    }
}
