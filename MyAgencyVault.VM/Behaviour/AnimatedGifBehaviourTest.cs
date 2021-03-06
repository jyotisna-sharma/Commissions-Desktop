using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System;

namespace MyAgencyVault.ViewModel.Behaviour
{
    public static class SupportAnimatedGIFBehviour
    {
        private static readonly int MILLISCONDS_PER_FRAME = 75;

        #region SupportAnimatedGif Attached Property

        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static bool GetSupportAnimatedGif(Image image)
        {
            return (bool)image.GetValue(SupportAnimatedGifProperty);
        }

        public static void SetSupportAnimatedGif(Image image, bool value)
        {
            image.SetValue(SupportAnimatedGifProperty, value);
        }

        /// <summary>
        /// An Attached Property for Animated GIF support.
        /// </summary>
        public static readonly DependencyProperty SupportAnimatedGifProperty =
            DependencyProperty.RegisterAttached(
            "SupportAnimatedGif",
            typeof(bool),
            typeof(SupportAnimatedGIFBehviour),
            new UIPropertyMetadata(false, OnSupportAnimatedGifChanged));

        /// <summary>
        /// Register \ UnRegister to visibility changes and image source changes.
        /// </summary>
        /// <param name="depObj">The image object.</param>
        /// <param name="e">The SupportAnimatedGif Property values.</param>
        private static void OnSupportAnimatedGifChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                bool isSupportingAnimatedGIF = (bool)e.NewValue;
                Image image = (Image)depObj;

                if (isSupportingAnimatedGIF)
                {
                    RegisterForRelevantImagePropertyChanges(image);

                    if (image.Visibility == Visibility.Visible)
                    {
                        image.StartFramesAnimation();
                    }
                }
                else
                {
                    UnRegisterForRelevantImagePropertyChanges(image);
                    StopFramesAnimation(image);
                }
            }
            catch { }
        }

        private static DependencyPropertyDescriptor VisibilityDPDescriptor
        {
            get
            {
                return DependencyPropertyDescriptor.FromProperty(UIElement.VisibilityProperty, typeof(UIElement));
            }
        }

        private static DependencyPropertyDescriptor ImageSourceDPDescriptor
        {
            get
            {
                return DependencyPropertyDescriptor.FromProperty(Image.SourceProperty, typeof(Image));
            }
        }

        private static void UnRegisterForRelevantImagePropertyChanges(DependencyObject depObj)
        {
            VisibilityDPDescriptor.RemoveValueChanged(depObj, OnVisibilityChanged);
            ImageSourceDPDescriptor.RemoveValueChanged(depObj, OnImageSourceChanged);
        }

        private static void RegisterForRelevantImagePropertyChanges(DependencyObject depObj)
        {
            VisibilityDPDescriptor.AddValueChanged(depObj, OnVisibilityChanged);
            ImageSourceDPDescriptor.AddValueChanged(depObj, OnImageSourceChanged);
        }

        #endregion

        #region CurrentFrameIndex Dependency Property

        private static readonly DependencyProperty CurrentFrameIndexProperty = DependencyProperty.Register(
            "CurrentFrameIndex",
            typeof(int),
            typeof(Image),
            new PropertyMetadata(0, OnCurrentFrameIndexChanged));

        #endregion

        #region IsAnimationChangingFrame Dependency Property

        /// <summary>
        /// IsAnimationChangingFrame Dependency Property
        /// </summary>
        private static readonly DependencyProperty IsAnimationChangingFrameProperty = DependencyProperty.Register(
            "IsAnimationChangingFrame",
            typeof(bool),
            typeof(Image),
            new PropertyMetadata(false));

        /// <summary>
        /// DummyImage Dependency Property - for keeping the original source binding when animation is applied
        /// </summary>
        private static readonly DependencyProperty DummyImageProperty = DependencyProperty.Register(
            "DummyImage",
            typeof(ImageSource),
            typeof(Image),
            new PropertyMetadata(null, OnDummyImagePropertyChanged));

        /// <summary>
        /// Gets or sets the IsAnimationChangingFrame property. This dependency property
        /// indicates that the animation is currently changing the image source.
        /// </summary>
        private static bool GetIsAnimationChangingFrame(this Image image)
        {
            return (bool)image.GetValue(IsAnimationChangingFrameProperty);
        }
        private static void SetIsAnimationChangingFrame(this Image image, bool isAnimationChangingFrame)
        {
            image.SetValue(IsAnimationChangingFrameProperty, isAnimationChangingFrame);
        }

        #endregion

        private static void OnImageSourceChanged(object sender, EventArgs e)
        {
            try
            {
                Image image = (Image)sender;

                if (!image.GetIsAnimationChangingFrame())//If the image source is changing by an outer source(not the animation).
                {
                    //stop old animation
                    image.SetIsAnimationChangingFrame(true);
                    image.StopFramesAnimation();
                    image.SetIsAnimationChangingFrame(false);

                    //start new animation - only if frames count is bigger than one.
                    image.StartFramesAnimation();
                }
            }
            catch { }
        }

        /// <summary>
        /// Starts frame animation only if frames count is bigger than 1.
        /// </summary>
        /// <param name="image"></param>
        private static void StartFramesAnimation(this Image image)
        {
            try
            {
                BitmapFrame bitmapFrame = image.Source as BitmapFrame;
                if (bitmapFrame != null)
                {
                    int framesCount = bitmapFrame.Decoder.Frames.Count;

                    if (framesCount > 1)
                    {
                        Int32Animation gifAnimation =
                            new Int32Animation(
                                0, // "From" value
                                framesCount - 1, // "To" value
                                new Duration(TimeSpan.FromMilliseconds(MILLISCONDS_PER_FRAME * framesCount))
                            );

                        gifAnimation.RepeatBehavior = RepeatBehavior.Forever;

                        image.BeginAnimation(CurrentFrameIndexProperty, gifAnimation, HandoffBehavior.SnapshotAndReplace);
                    }
                }
            }
            catch
            { }
        }

        private static void StopFramesAnimation(this Image image)
        {
            image.BeginAnimation(CurrentFrameIndexProperty, null);
        }

        private static void OnVisibilityChanged(object sender, EventArgs e)
        {
            try
            {
                Image image = (Image)sender;

                if (image.Visibility != Visibility.Visible)
                {
                    image.StopFramesAnimation();
                }
                else
                {
                    image.StartFramesAnimation();
                }
            }
            catch { }
        }

        private static void OnDummyImagePropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Image animatedImage = (Image)dpo;

                if (!animatedImage.GetIsAnimationChangingFrame())
                {
                    BindingBase originalBinding = BindingOperations.GetBindingBase(dpo, DummyImageProperty);
                    if (originalBinding != null)
                    {
                        BindingOperations.SetBinding(dpo, Image.SourceProperty, originalBinding);
                        BindingOperations.ClearBinding(animatedImage, DummyImageProperty);
                    }
                    animatedImage.SetIsAnimationChangingFrame(false);
                }
                else
                {
                    animatedImage.SetIsAnimationChangingFrame(false);
                }

                animatedImage.SetIsAnimationChangingFrame(false);
            }
            catch { }
        }

        /// <summary>
        /// Update the current image source to the relevenat frame.
        /// </summary>
        /// <param name="dpo"></param>
        /// <param name="e"></param>
        private static void OnCurrentFrameIndexChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                Image animatedImage = (Image)dpo;

                if (!animatedImage.GetIsAnimationChangingFrame())
                {
                    animatedImage.SetIsAnimationChangingFrame(true);

                    bool hasBinding = BindingOperations.IsDataBound(animatedImage, Image.SourceProperty);
                    if (hasBinding)
                    {
                        BindingBase originalBinding = BindingOperations.GetBindingBase(animatedImage, Image.SourceProperty);
                        BindingOperations.SetBinding(animatedImage, DummyImageProperty, originalBinding);
                    }
                    animatedImage.Source = ((BitmapFrame)animatedImage.Source).Decoder.Frames[(int)e.NewValue];

                    if (!hasBinding)
                    {
                        animatedImage.SetIsAnimationChangingFrame(false);
                    }
                }
            }
            catch { }
        }
    }
}