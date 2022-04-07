#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;
using Xelion.Features.MainWindow.Desktop;


namespace Xelion.Controls
{
    /// <summary>
    /// A Content Control that can be dragged around.
    /// </summary>
    [TemplatePart(Name = _RootPartName, Type = typeof(Grid))]
    [TemplatePart(Name = _DragDropHandlePartName, Type = typeof(Polygon))]
    [TemplatePart(Name = _DragDropVisualPartName, Type = typeof(Polygon))]
    [TemplatePart(Name = _CloseButtonPartName, Type = typeof(Button))]
    [TemplatePart(Name = _MoveWindowPartName, Type = typeof(Border))]
    [TemplatePart(Name = _ResizeBothPartName, Type = typeof(Border))]
    [TemplatePart(Name = _ResizeVerticalPartName, Type = typeof(Border))]
    [TemplatePart(Name = _ResizeHorizontalPartName, Type = typeof(Border))]

    public class MdiWindow : ContentControl
    {
        private const string _RootPartName = "PART_Root";
        private const string _DragDropHandlePartName = "PART_DragDropHandle";
        private const string _DragDropVisualPartName = "PART_DragDropVisual";
        private const string _CloseButtonPartName = "PART_CloseButton";
        private const string _ResizeBothPartName = "PART_ResizeBoth";
        private const string _ResizeVerticalPartName = "PART_ResizeVertical";
        private const string _ResizeHorizontalPartName = "PART_ResizeHorizontal";
        private const string _MoveWindowPartName = "PART_MoveWindow";

        private string? _originatingPaneName;
        private Grid? _root;
        private Polygon? _dragHandle;
        private Polygon? _dragVisual;
        public static readonly DependencyProperty BoundaryProperty =
            DependencyProperty.Register(
                "Boundary",
                typeof(MdiWindowBoundary),
                typeof(MdiWindow),
                new PropertyMetadata(MdiWindowBoundary.DragHandleAlwaysUsable));

        public static readonly DependencyProperty StartAtBottomRightProperty =
            DependencyProperty.Register("StartAtBottomRight", typeof(bool), typeof(MdiWindow), new PropertyMetadata(false));

        public static readonly DependencyProperty DragDropSupportedProperty =
            DependencyProperty.Register("DragDropSupported", typeof(bool), typeof(MdiWindow), new PropertyMetadata(true));

        /// <summary>
        /// Initializes a new instance of the <see cref="MdiWindow"/> class.
        /// </summary>
        public MdiWindow()
        {
            DefaultStyleKey = typeof(MdiWindow);
        }

        public MdiWindowBoundary Boundary
        {
            get => (MdiWindowBoundary)GetValue(BoundaryProperty);
            set => SetValue(BoundaryProperty, value);
        }

        public bool StartAtBottomRight
        {
            get => (bool)GetValue(StartAtBottomRightProperty);
            set => SetValue(StartAtBottomRightProperty, value);
        }

        public bool DragDropSupported
        {
            get => (bool)GetValue(DragDropSupportedProperty);
            set => SetValue(DragDropSupportedProperty, value);
        }

        internal void Show(UserControl control, string? originatingPaneName = null)
        {
            try
            {
                Visibility = Visibility.Visible;
                Content = control;
                _originatingPaneName = originatingPaneName;

                MakeActiveWindow();
            }
            catch
            {
                Content = null;
                _originatingPaneName = null;
            }


            UpdateDragHandle();
        }




        /// <summary>
        /// Invoked whenever application code or internal processes (such as a rebuilding layout pass) call ApplyTemplate.
        /// In simplest terms, this means the method is called just before a UI element displays in your app.
        /// Override this method to influence the default post-template logic of a class.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            // Save root
            _root = GetTemplateChild(_RootPartName) as Grid;

            //
            // Move
            if (GetTemplateChild(_MoveWindowPartName) is Border moveHandle)
            {
                moveHandle.ManipulationDelta += MoveHandleManipulationDelta;
                moveHandle.IsDoubleTapEnabled = true;
                moveHandle.Tapped += MoveHandleTapped;
                moveHandle.DoubleTapped += (_, e) => { e.Handled = true;  };
            }

            //
            // Drag/drop
            _dragHandle = GetTemplateChild(_DragDropHandlePartName) as Polygon;
            _dragVisual = GetTemplateChild(_DragDropVisualPartName) as Polygon;

            if (_dragHandle != null)
            {
                _dragHandle.DragStarting += OnDragStarting;
                _dragHandle.DropCompleted += OnDropCompleted;
                var toolTip = new ToolTip
                {
                    Content = "Drag to empty pane"
                };
                ToolTipService.SetToolTip(_dragHandle, toolTip);

                _dragHandle.PointerPressed += OnDragHandlePressed;
            }

            //
            // Resize
            if (GetTemplateChild(_ResizeBothPartName) is Border resizeBothBorder)
            {
                resizeBothBorder.ManipulationDelta += OnSizeManipulationDelta;
            }

            if (GetTemplateChild(_ResizeHorizontalPartName) is Border resizeHorizontalBorder)
            {
                resizeHorizontalBorder.ManipulationDelta += OnSizeManipulationDelta;
            }

            if (GetTemplateChild(_ResizeVerticalPartName) is Border resizeVerticalBorder)
            {
                resizeVerticalBorder.ManipulationDelta += OnSizeManipulationDelta;
            }


            //
            // Close
            if (GetTemplateChild(_CloseButtonPartName) is Button actionButton)
            {
                actionButton.Click += (sender, __) =>
                {
                    CloseByUser();
                };
            }


            Loaded += Floating_Loaded;
        }

        private async void OnDragHandlePressed(object sender, PointerRoutedEventArgs e)
        {
            if (_dragHandle == null)
                return;

            e.Handled = true;

            var pointerPoint = e.GetCurrentPoint(sender as UIElement);
            if (pointerPoint.Properties.IsLeftButtonPressed)
            {
                // If this window does not have a name, assign one
                if (string.IsNullOrEmpty(Name))
                {
                    Name = "mdiwindow-" + Guid.NewGuid().ToString();
                }

                // Start dragging
                var _ = await _dragHandle.StartDragAsync(pointerPoint);
            }
        }

        private void MoveHandleTapped(object sender, TappedRoutedEventArgs e)
        {
            MakeActiveWindow();
        }


      

        public UserControl? GetContent()
        {
            return Content as UserControl;
        }

        /// <summary>
        /// Removes the content from the floating window and returns it.
        /// </summary>
        internal UserControl? DetachContent()
        {
            if (Content is UserControl control)
            {
                Content = null; // Unload not called...
                return control;
            }

            return null;
        }

        //private async void TearOff()
        //{
        //    var content = DetachContent();
        //    if (content != null)
        //    {
        //        var window = await XelionWindow.TryCreate();
        //        if (window == null)
        //        {
        //            return;
        //        }

        //        await window.TryShow(content);

        //        CloseByUser();
        //    }
        //}

        public void CloseByUser()
        {
            // TODO: Decide if we need to close the tab or not when the user closes the window
            // Code for not closing the tab:
            //// if content is in a tab, unselect it
            //var tabUnselected = DesktopSurface.Window.UnselectTabItem(Content as FrameworkElement);

            //if (!tabUnselected)
            //{
            //    // Only remove content when it is not in a tab
            //    CleanupView();
            //    Content = null;
            //    _originatingPaneName = null;
            //}

            //// Remove this window from its parent panel
            //DesktopSurface.HideMdiWindow(this);


            CleanupAndClose();
        }

     
        public void CleanupAndClose()
        {
            // if content is in a tab, remove it
            DesktopSurface.Window.RemoveTabItem(Content as UserControl);

            CleanupView();
            Content = null;
            _originatingPaneName = null;

            // Remove this window from its parent panel
            DesktopSurface.HideMdiWindow(this);
        }

        private void CleanupView()
        {
        }

        //private void SizeThumbManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        //{
        //    var newWidth = ActualWidth + e.Delta.Translation.X;
        //    var newHeight = ActualHeight + e.Delta.Translation.Y;

        //    Width = newWidth;
        //    Height = newHeight;
        //    e.Handled = true; // don't handle manipulation of the border
        //}

        private  void OnDragStarting(UIElement sender, DragStartingEventArgs args)
        {
            var deferral = args.GetDeferral();
            try
            {
                Visibility = Visibility.Collapsed;
            }
            catch
            { }
            finally
            {
                deferral.Complete();
            }
        }

        private void OnDropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            if (Content != null)
                Visibility = Visibility.Visible;
        }


        public void DefaultPositionAndDimensions(double availableWidth, double availableHeight, double desiredWidth, double desiredHeight)
        {
            var wWidth = availableWidth;
            var wHeight = availableHeight;

            // Adapt size to available space (with margin 120)
            Width = Math.Min(desiredWidth, wWidth - 120);
            Height = Math.Min(desiredHeight, wHeight - 120);

            // Get left, top for centered positioning
            var centeredLeft = (wWidth - Width) / 2;
            var centeredTop = (wHeight - Height) / 2;

            // But don't go too far from the left, top
            var left = Math.Min(centeredLeft, 100);
            var top = Math.Min(centeredTop, 100);

            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);
        }


        private static int _iForm = 0;

        public void DefaultPositionAndDimensions(double availableWidth, double availableHeight, double desiredWidth, double desiredHeight, bool isForm)
        {
            var wWidth = availableWidth;
            var wHeight = availableHeight;

            // Adapt size to available space (with margin 120)
            Width = Math.Min(desiredWidth, wWidth - 120);
            Height = Math.Min(desiredHeight, wHeight - 120);

            // Get left, top for centered positioning
            var centeredLeft = (wWidth - Width) / 2;
            var centeredTop = (wHeight - Height) / 2;

            // But don't go too far from the left, top
            var left = Math.Min(centeredLeft, 100);
            var top = Math.Min(centeredTop, 100);

            // Put a form a bit to the right/top
            if (isForm)
            {
                _iForm++;
                if (_iForm == 5)
                    _iForm = 1;

                left += _iForm * 18;
                top -= _iForm * 18;
            }

            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);
        }

        internal void MakeActiveWindow()
        {
            var canvas = this.FindParent<Canvas>();
            if (canvas == null)
                return;

            Canvas.SetZIndex(this, 10000);

            // Decrease z index of non-active windows
            foreach (var child in canvas.Children)
            {
                if (child != this)
                {
                    var childZindex = Canvas.GetZIndex(child);
                    Canvas.SetZIndex(child, childZindex - 1);
                }
            }

        }



        private void Floating_Loaded(object sender, RoutedEventArgs e)
        {
            var el = GetClosestParentWithSize();
            if (el == null)
            {
                return;
            }

            el.SizeChanged += Floating_SizeChanged;

            // WB:
            // position the border at the right/bottom of the canvas
            if (StartAtBottomRight)
            {
                Canvas.SetLeft(this, el.ActualWidth - ActualWidth);
                Canvas.SetTop(this, el.ActualHeight - ActualHeight);
            }

            UpdateDragHandle();
        }

        private void UpdateDragHandle()
        {
            var visibility = DragDropSupported ? Visibility.Visible : Visibility.Collapsed;

            if (_dragHandle != null)
            {
                _dragHandle.Visibility = visibility;
            }

            if (_dragVisual != null)
            {
                _dragVisual.Visibility = visibility;
            }
        }

        private void Floating_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_root == null)
                return;

            var left = Canvas.GetLeft(this);
            var top = Canvas.GetTop(this);

            var rect = new Rect(left, top, _root.ActualWidth, _root.ActualHeight);

            AdjustCanvasPosition(rect);
        }

        /// <summary>
        /// Move the window
        /// </summary>
        private void MoveHandleManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var left = Canvas.GetLeft(this) + e.Delta.Translation.X;
            var top = Canvas.GetTop(this) + e.Delta.Translation.Y;

            var rect = new Rect(left, top, ActualWidth, ActualHeight);
            var moved = AdjustCanvasPosition(rect);

            if (!moved)
            {
                // Not intuitive:
                // We hit the boundary. Stop the inertia.
                //e.Complete();
            }

        }

        /// <summary>
        /// Resize the window
        /// </summary>
        private void OnSizeManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                var newWidth = Math.Max(100, ActualWidth + e.Delta.Translation.X);
                var newHeight = Math.Max(40, ActualHeight + e.Delta.Translation.Y);

                Width = newWidth;
                Height = newHeight;
            }
            catch (Exception ex)
            {
              //
            }
            e.Handled = true; // don't handle manipulation of the border
        }

        /// <summary>
        /// Adjusts the canvas position according to the IsBoundBy* properties.
        /// </summary>
        private bool AdjustCanvasPosition(Rect rect)
        {
            // Free floating.
            if (Boundary == MdiWindowBoundary.None)
            {
                Canvas.SetLeft(this, rect.Left);
                Canvas.SetTop(this, rect.Top);

                return true;
            }



            var el = GetClosestParentWithSize();

            // No parent
            if (el == null)
            {
                // We probably never get here.
                return false;
            }

            if(Boundary == MdiWindowBoundary.DragHandleAlwaysUsable)
            {
                // top cannot move beyond canvas top
                var top = Math.Max(0, rect.Top);

                // top cannot move too low
                top = Math.Min(el.ActualHeight - 40, top);

                // 80% of window can be moved out of the left canvas bound
                var left = Math.Max(-0.8 * rect.Width, rect.Left);

                // 80% of window can be moved out of the right canvas bound
                left = Math.Min(el.ActualWidth - 0.2 * rect.Width, left);

                Canvas.SetLeft(this, left);
                Canvas.SetTop(this, top);

                return true;
            }

            var position = new Point(rect.Left, rect.Top);


            if (Boundary == MdiWindowBoundary.Parent)
            {
                var parentRect = new Rect(0, 0, el.ActualWidth, el.ActualHeight);
                position = AdjustedPosition(rect, parentRect);
            }
           
      
            // Set new position
            Canvas.SetLeft(this, position.X);
            Canvas.SetTop(this, position.Y);

            return position == new Point(rect.Left, rect.Top);
        }

        /// <summary>
        /// Returns the adjusted the topleft position of a rectangle so that is stays within a parent rectangle.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="parentRect">The parent rectangle.</param>
        /// <returns></returns>
        private Point AdjustedPosition(Rect rect, Rect parentRect)
        {
            var left = rect.Left;
            var top = rect.Top;

            if (left < -parentRect.Left)
            {
                // Left boundary hit.
                left = -parentRect.Left;
            }
            else if (left + rect.Width > parentRect.Width)
            {
                // Right boundary hit.
                left = parentRect.Width - rect.Width;
            }

            if (top < -parentRect.Top)
            {
                // Top hit.
                top = -parentRect.Top;
            }
            else if (top + rect.Height > parentRect.Height)
            {
                // Bottom hit.
                top = parentRect.Height - rect.Height;
            }

            return new Point(left, top);
        }

        /// <summary>
        /// Gets the closest parent with a real size.
        /// </summary>
        private FrameworkElement? GetClosestParentWithSize()
        {
            var element = Parent as FrameworkElement;
            while (element != null && (element.ActualHeight == 0 || element.ActualWidth == 0))
            {
                // Crawl up the Visual Tree.
                element = element.Parent as FrameworkElement;
            }

            return element;
        }

    }
}