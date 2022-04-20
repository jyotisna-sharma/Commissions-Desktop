using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MyAgencyVault.ViewModel.PayorToolLib
{
	public class HoverAdorner : Adorner
	{
		public HoverAdorner(UIElement adornedElement) : base(adornedElement)
		{
			Container = new ContentPresenter();
		}

		protected override Size MeasureOverride(Size constraint)
		{
			Container.Measure(constraint);
			return Container.DesiredSize;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			double left = AdornedElement.RenderSize.Width - Container.DesiredSize.Width;
			Container.Arrange(new Rect(new Point(left, AdornedElement.RenderSize.Height/2), finalSize));
			return finalSize;
		}

		protected override System.Windows.Media.Visual GetVisualChild(int index)
		{
			return Container;
		}

		protected override int VisualChildrenCount
		{
			get { return 1; }
		}

		internal ContentPresenter Container { get; set; }
	}
}