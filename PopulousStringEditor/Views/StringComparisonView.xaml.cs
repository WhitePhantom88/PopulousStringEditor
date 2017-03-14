using System.Windows.Controls;

namespace PopulousStringEditor.Views
{
    /// <summary>
    /// Interaction logic for StringComparisonView.xaml
    /// </summary>
    public partial class StringComparisonView : UserControl
    {
        /// <summary>Gets or sets the visibility of the referenced strings.</summary>
        public System.Windows.Visibility ReferenceStringsVisiblity
        {
            get { return ReferenceStringsColumn.Visibility; }
            set { ReferenceStringsColumn.Visibility = value; }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StringComparisonView()
        {
            InitializeComponent();
        }
    }
}
