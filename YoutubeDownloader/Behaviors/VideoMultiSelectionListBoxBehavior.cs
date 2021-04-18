using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Behaviors
{
    public class VideoMultiSelectionListBoxBehavior : Behavior<ListBox>
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(nameof(SelectedItems), typeof(IList),
                typeof(VideoMultiSelectionListBoxBehavior),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var behavior = (VideoMultiSelectionListBoxBehavior) sender;
            if (behavior._modelHandled) return;

            if (behavior.AssociatedObject is null)
                return;

            behavior._modelHandled = true;
            behavior.SelectItems();
            behavior._modelHandled = false;
        }

        private bool _viewHandled;
        private bool _modelHandled;

        public IList? SelectedItems
        {
            get => (IList?) GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        // Propagate selected items from model to view
        private void SelectItems()
        {
            _viewHandled = true;

            AssociatedObject.SelectedItems.Clear();
            if (SelectedItems is not null)
            {
                foreach (var item in SelectedItems)
                    AssociatedObject.SelectedItems.Add(item);
            }

            _viewHandled = false;
        }

        // Propagate selected items from view to model
        private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (_viewHandled) return;
            if (AssociatedObject.Items.SourceCollection is null) return;

            SelectedItems = AssociatedObject.SelectedItems.Cast<IVideo>().ToArray();
        }

        // Re-select items when the set of items changes
        private void OnListBoxItemsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (_viewHandled) return;
            if (AssociatedObject.Items.SourceCollection is null) return;
            SelectItems();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnListBoxSelectionChanged;
            ((INotifyCollectionChanged) AssociatedObject.Items).CollectionChanged += OnListBoxItemsChanged;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject is not null)
            {
                AssociatedObject.SelectionChanged -= OnListBoxSelectionChanged;
                ((INotifyCollectionChanged) AssociatedObject.Items).CollectionChanged -= OnListBoxItemsChanged;
            }
        }
    }
}