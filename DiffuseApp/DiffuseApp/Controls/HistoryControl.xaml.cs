using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TensorStack.WPF;
using TensorStack.WPF.Controls;
using TensorStack.WPF.Utils;
using Diffuse.Common;

namespace Diffuse.Controls
{
    /// <summary>
    /// Interaction logic for HistoryControl.xaml
    /// </summary>
    public partial class HistoryControl : BaseControl
    {
        private ICollectionView _collectionView;
        private Point _dragStartPoint;

        public HistoryControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(nameof(ItemSource), typeof(ObservableCollection<IHistoryItem>), typeof(HistoryControl), new PropertyMetadata<HistoryControl>(x => x.OnItemSourceChanged()));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(IHistoryItem), typeof(HistoryControl));
   
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(HistoryControl));

        public static readonly DependencyProperty ItemsPanelTemplateProperty =
            DependencyProperty.Register(nameof(ItemsPanelTemplate), typeof(ItemsPanelTemplate), typeof(HistoryControl));

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
            DependencyProperty.Register(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(HistoryControl), new PropertyMetadata(ScrollBarVisibility.Disabled));

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(HistoryControl), new PropertyMetadata(ScrollBarVisibility.Visible));

        public static readonly DependencyProperty SortPropertyProperty =
            DependencyProperty.Register(nameof(SortProperty), typeof(string), typeof(HistoryControl), new PropertyMetadata<HistoryControl>(x => x.OnSortChanged()) { DefaultValue = nameof(IHistoryItem.Timestamp) });

        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.Register(nameof(SortDirection), typeof(ListSortDirection), typeof(HistoryControl), new PropertyMetadata<HistoryControl>(x => x.OnSortChanged()) { DefaultValue = ListSortDirection.Descending });

        public static readonly DependencyProperty PreviewItemCommandProperty = DependencyProperty.Register(nameof(PreviewItemCommand), typeof(AsyncRelayCommand<IHistoryItem>), typeof(HistoryControl));
        public static readonly DependencyProperty RemoveItemCommandProperty = DependencyProperty.Register(nameof(RemoveItemCommand), typeof(AsyncRelayCommand<IHistoryItem>), typeof(HistoryControl));

        public ObservableCollection<IHistoryItem> ItemSource
        {
            get { return (ObservableCollection<IHistoryItem>)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        public IHistoryItem SelectedItem
        {
            get { return (IHistoryItem)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public ItemsPanelTemplate ItemsPanelTemplate
        {
            get { return (ItemsPanelTemplate)GetValue(ItemsPanelTemplateProperty); }
            set { SetValue(ItemsPanelTemplateProperty, value); }
        }

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        public string SortProperty
        {
            get { return (string)GetValue(SortPropertyProperty); }
            set { SetValue(SortPropertyProperty, value); }
        }

        public ListSortDirection SortDirection
        {
            get { return (ListSortDirection)GetValue(SortDirectionProperty); }
            set { SetValue(SortDirectionProperty, value); }
        }

        public ICollectionView CollectionView
        {
            get { return _collectionView; }
            set { SetProperty(ref _collectionView, value); }
        }

        public AsyncRelayCommand<IHistoryItem> PreviewItemCommand
        {
            get { return (AsyncRelayCommand<IHistoryItem>)GetValue(PreviewItemCommandProperty); }
            set { SetValue(PreviewItemCommandProperty, value); }
        }

        public AsyncRelayCommand<IHistoryItem> RemoveItemCommand
        {
            get { return (AsyncRelayCommand<IHistoryItem>)GetValue(RemoveItemCommandProperty); }
            set { SetValue(RemoveItemCommandProperty, value); }
        }


        private Task OnItemSourceChanged()
        {
            CollectionView = new ListCollectionView(ItemSource) { IsLiveSorting = true };
            CollectionView.Filter = (obj) =>
            {
                if (obj is not IHistoryItem item)
                    return false;

                return true;
            };
            OnSortChanged();
            return Task.CompletedTask;
        }


        private Task OnSortChanged()
        {
            if (CollectionView?.SortDescriptions is not null)
            {
                CollectionView.SortDescriptions.Clear();
                CollectionView.SortDescriptions.Add(new SortDescription(SortProperty, SortDirection));
            }
            return Task.CompletedTask;
        }


        protected void ListBoxPreviewMouseMove(object sender, MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // check if drag threshold is passed
                var diff = e.GetPosition(null) - _dragStartPoint;
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (SelectedItem is not null)
                    {
                        var listBoxItem = (ListBoxItem)ListBoxControl.ItemContainerGenerator.ContainerFromItem(SelectedItem);
                        if (listBoxItem == null)
                            return;

                        var dropType = SelectedItem.MediaType switch
                        {
                            MediaType.Image => DragDropType.Image,
                            MediaType.Video => DragDropType.Video,
                            MediaType.Audio => DragDropType.Audio,
                            MediaType.Text => DragDropType.Text,
                            _ => throw new NotSupportedException()
                        };

                        DragDropHelper.DoDragDropFile(this, SelectedItem.MediaPath, dropType, listBoxItem);
                    }
                }
            }
            else if (e.LeftButton == MouseButtonState.Released)
            {
                _dragStartPoint = e.GetPosition(null); // reset for next drag
            }
        }


        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
            e.Handled = true;
        }


        private void ListBox_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }


        //protected override async void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        //{
        //    if (SelectedItem == null)
        //        return;

        //    if (SelectedItem.MediaType == MediaType.Image)
        //    {
        //        //var previewImageDialog = DialogService.GetDialog<PreviewImageDialog>();
        //        //await previewImageDialog.ShowDialogAsync(SelectedItem);
        //    }

        //    base.OnPreviewMouseDoubleClick(e);
        //}


        static HistoryControl()
        {
            // Create a default ItemsPanelTemplate with a VirtualizingStackPanel
            var factory = new FrameworkElementFactory(typeof(VirtualizingStackPanel));
            factory.SetValue(VirtualizingStackPanel.OrientationProperty, Orientation.Horizontal);
            factory.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);

            var template = new ItemsPanelTemplate(factory);
            template.Seal();
            ItemsPanelTemplateProperty.OverrideMetadata(typeof(HistoryControl), new FrameworkPropertyMetadata(template));
        }

    }

    public class MediaTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IHistoryItem historyItem)
            {
                return historyItem.MediaType switch
                {
                    MediaType.Image => ImageTemplate,
                    MediaType.Video => VideoTemplate,
                    _ => base.SelectTemplate(item, container)
                };
            }
            return base.SelectTemplate(item, container);
        }
    }
}
