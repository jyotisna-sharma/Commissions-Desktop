using System.Windows;
using System.Windows.Controls;
using MyAgencyVault.VM.VMLib.Adorners;

namespace MyAgencyVault.VM.VMLib.PayorForm
{
    public class Toolbox : ListView
    {
        private Size defaultItemSize = new Size(65, 65);
        public Size DefaultItemSize
        {
            get { return this.defaultItemSize; }
            set { this.defaultItemSize = value; }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ToolboxItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is ToolboxItem);
        }
    }
}
