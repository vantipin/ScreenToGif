﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Shapes;
using ScreenToGif.Util;

namespace ScreenToGif.Controls
{
    /// <summary>
    /// Light Window used by the Recorder.
    /// </summary>
    public class LightWindow : Window
    {
        #region Dependency Property

        public static readonly DependencyProperty FrameCountProperty = DependencyProperty.Register("FrameCount", typeof(int), typeof(LightWindow),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register("Child", typeof(UIElement), typeof(LightWindow),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty MaxSizeProperty = DependencyProperty.Register("MaxSize", typeof(double), typeof(LightWindow),
            new FrameworkPropertyMetadata(26.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BackVisibilityProperty = DependencyProperty.Register("BackVisibility", typeof(Visibility), typeof(LightWindow),
            new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty MinimizeVisibilityProperty = DependencyProperty.Register("MinimizeVisibility", typeof(Visibility), typeof(LightWindow),
            new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsRecordingProperty = DependencyProperty.Register("IsRecording", typeof(bool), typeof(LightWindow),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsThinProperty = DependencyProperty.Register("IsThin", typeof(bool), typeof(LightWindow),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsFullScreenProperty = DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(LightWindow),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Property Accessor

        /// <summary>
        /// The frame count of the current recording.
        /// </summary>
        [Bindable(true), Category("Common"), Description("The frame count of the current recording.")]
        public int FrameCount
        {
            get { return (int)GetValue(FrameCountProperty); }
            set { SetValue(FrameCountProperty, value); }
        }

        /// <summary>
        /// The Image of the caption bar.
        /// </summary>
        [Bindable(true), Category("Common"), Description("The Image of the caption bar.")]
        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetCurrentValue(ChildProperty, value); }
        }

        /// <summary>
        /// The maximum size of the image.
        /// </summary>
        [Bindable(true), Category("Common"), Description("The maximum size of the image.")]
        public double MaxSize
        {
            get { return (double)GetValue(MaxSizeProperty); }
            set { SetCurrentValue(MaxSizeProperty, value); }
        }

        /// <summary>
        /// Back button visibility.
        /// </summary>
        [Bindable(true), Category("Common"), Description("Back button visibility.")]
        public Visibility BackVisibility
        {
            get { return (Visibility)GetValue(BackVisibilityProperty); }
            set { SetCurrentValue(BackVisibilityProperty, value); }
        }

        /// <summary>
        /// Minimize button visibility.
        /// </summary>
        [Bindable(true), Category("Common"), Description("Minimize button visibility.")]
        public Visibility MinimizeVisibility
        {
            get { return (Visibility)GetValue(MinimizeVisibilityProperty); }
            set { SetCurrentValue(MinimizeVisibilityProperty, value); }
        }

        /// <summary>
        /// If in recording mode.
        /// </summary>
        [Bindable(true), Category("Common"), Description("If in recording mode.")]
        public bool IsRecording
        {
            get { return (bool)GetValue(IsRecordingProperty); }
            set { SetCurrentValue(IsRecordingProperty, value); }
        }

        /// <summary>
        /// Thin mode (hides the title bar).
        /// </summary>
        [Bindable(true), Category("Common"), Description("Thin mode (hides the title bar).")]
        public bool IsThin
        {
            get { return (bool)GetValue(IsThinProperty); }
            set { SetCurrentValue(IsThinProperty, value); }
        }

        /// <summary>
        /// Fullscreen mode (hides the title bar).
        /// </summary>
        [Bindable(true), Category("Common"), Description("Fullscreen mode (hides the title bar and a few other things).")]
        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetCurrentValue(IsFullScreenProperty, value); }
        }

        #endregion

        #region Constructors

        static LightWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LightWindow), new FrameworkPropertyMetadata(typeof(LightWindow)));
        }

        #endregion

        #region Click Events

        private void BackClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = false;
        }

        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void RestoreClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;

                var button = sender as Button;
                if (button != null) button.Content = FindResource("Vector.Restore");
            }
            else
            {
                WindowState = WindowState.Normal;

                var button = sender as Button;
                if (button != null) button.Content = FindResource("Vector.Maximize");
            }
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        #endregion

        #region Initializers

        public override void OnApplyTemplate()
        {
            var backButton = GetTemplateChild("BackButton") as ImageButton;
            if (backButton != null)
                backButton.Click += BackClick;

            var minimizeButton = GetTemplateChild("MinimizeButton") as Button;
            if (minimizeButton != null)
                minimizeButton.Click += MinimizeClick;

            var closeButton = GetTemplateChild("CloseButton") as Button;
            if (closeButton != null)
                closeButton.Click += CloseClick;

            var moveGrid = GetTemplateChild("MoveGrid") as Grid;
            if (moveGrid != null)
                moveGrid.PreviewMouseDown += MoveGrid_PreviewMouseDown;

            var resizeGrid = GetTemplateChild("MainGrid") as Grid;
            if (resizeGrid != null)
            {
                foreach (var element in resizeGrid.Children.OfType<Rectangle>())
                {
                    element.PreviewMouseDown += ResizeRectangle_PreviewMouseDown;
                }
            }

            base.OnApplyTemplate();
        }

        private HwndSource _hwndSource;

        protected override void OnInitialized(EventArgs e)
        {
            SourceInitialized += OnSourceInitialized;

            base.OnInitialized(e);
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }

        #endregion

        #region Drag

        private void MoveGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        #endregion

        #region Resize

        private void ResizeRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            var rectangle = sender as Rectangle;

            if (rectangle == null) return;

            switch (rectangle.Name)
            {
                case "TopRectangle":
                    ResizeWindow(ResizeDirection.Top);
                    break;
                case "BottomRectangle":
                    ResizeWindow(ResizeDirection.Bottom);
                    break;
                case "LeftRectangle":
                    ResizeWindow(ResizeDirection.Left);
                    break;
                case "RightRectangle":
                    ResizeWindow(ResizeDirection.Right);
                    break;
                case "TopLeftRectangle":
                    ResizeWindow(ResizeDirection.TopLeft);
                    break;
                case "TopRightRectangle":
                    ResizeWindow(ResizeDirection.TopRight);
                    break;
                case "BottomLeftRectangle":
                    ResizeWindow(ResizeDirection.BottomLeft);
                    break;
                case "BottomRightRectangle":
                    ResizeWindow(ResizeDirection.BottomRight);
                    break;
            }
        }

        private void ResizeWindow(ResizeDirection direction)
        {
            Native.SendMessage(_hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
        }

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8
        }

        #endregion
    }
}
