using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MyAgencyVault.VM.VMLib.Adorners
{
    public class ResizeAdorner : Adorner
    {
        private VisualCollection visuals;
        private ResizeChrome chrome;

        protected override int VisualChildrenCount
        {
            get
            {
                return this.visuals.Count;
            }
        }

        public ResizeAdorner(ContentControl designerItem)
            : base(designerItem)
        {
            try
            {
                this.chrome = new ResizeChrome();
                this.visuals = new VisualCollection(this);
                this.visuals.Add(this.chrome);
                this.chrome.DataContext = designerItem;
            }
            catch { }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            this.chrome.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.visuals[index];
        }
    }
}
