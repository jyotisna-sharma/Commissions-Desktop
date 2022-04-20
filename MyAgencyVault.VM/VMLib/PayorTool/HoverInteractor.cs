using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MyAgencyVault.ViewModel.PayorToolLib
{
	public class HoverInteractor : DependencyObject
	{
		public static readonly DependencyProperty UseHoverProperty = DependencyProperty
			.RegisterAttached(
			"UseHover", typeof (bool), typeof (HoverInteractor),
			new PropertyMetadata(false, OnUseHoverChanged));

		private static readonly DependencyProperty AttachedAdornerProperty = DependencyProperty
			.Register(
			"AttachedAdorner", typeof (AdornerInfo), typeof (HoverInteractor));

		public static void SetUseHover(DependencyObject d, bool use)
		{
			d.SetValue(UseHoverProperty, use);
		}

		private static void OnUseHoverChanged(DependencyObject d,
		                                      DependencyPropertyChangedEventArgs e)
		{
			ListBox lb = d as ListBox;
			if (lb != null)
			{
				if ((bool) e.NewValue)
				{
					lb.MouseMove += ListBox_MouseMove;
					lb.MouseLeave += ListBox_MouseLeave;
				}
				else
				{
					lb.MouseEnter -= ListBox_MouseMove;
					lb.MouseLeave -= ListBox_MouseLeave;
				}
			}
		}

		private static void ListBox_MouseLeave(object sender, MouseEventArgs e)
		{
			ListBox lb = sender as ListBox;

			// Remove any previous Adorners
			AdornerInfo prevInfo = lb.GetValue(AttachedAdornerProperty) as AdornerInfo;
			AdornerLayer layer = AdornerLayer.GetAdornerLayer(lb);
			if (prevInfo != null)
			{
				if (layer != null)
				{
					layer.Remove(prevInfo.Adorner);
					lb.ClearValue(AttachedAdornerProperty);
				}
			}
		}

		private static void ListBox_MouseMove(object sender, MouseEventArgs e)
		{
			// Check that we are hovering on a ListBoxItem
			ListBox lb = sender as ListBox;
			ListBoxItem item =
				lb.ContainerFromElement(e.OriginalSource as Visual) as ListBoxItem;
			if (item == null)
			{
				return;
			}

			// Remove any previous Adorners
			AdornerInfo prevInfo = lb.GetValue(AttachedAdornerProperty) as AdornerInfo;
			AdornerLayer layer = AdornerLayer.GetAdornerLayer(lb);
			if (prevInfo != null)
			{
				if (prevInfo.ListItem == item) return;
				layer.Remove(prevInfo.Adorner);
				lb.ClearValue(AttachedAdornerProperty);
			}

			// Attach new adorner to current ListBoxItem
			HoverAdorner adorner = new HoverAdorner(item);
			adorner.Container.Content = lb.ItemContainerGenerator.ItemFromContainer(item);
			adorner.Container.ContentTemplate =
				item.FindResource("AdornerTemplate") as DataTemplate;
			layer.Add(adorner);

			AdornerInfo info = new AdornerInfo();
			info.Adorner = adorner;
			info.ListItem = item;
			lb.SetValue(AttachedAdornerProperty, info);
		}
	}

	internal class AdornerInfo
	{
		public HoverAdorner Adorner;
		public ListBoxItem ListItem;
	}
}