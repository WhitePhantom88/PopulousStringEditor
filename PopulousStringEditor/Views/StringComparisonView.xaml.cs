using System.Windows.Controls;

namespace PopulousStringEditor.Views
{
    /// <summary>
    /// Interaction logic for StringComparisonView.xaml
    /// </summary>
    public partial class StringComparisonView : UserControl
    {
        #region Fields
        /// <summary>Keeps track of whether or not the contents of this control have been edited by a user.</summary>
        private bool hasBeenEdited = false;
        #endregion

        #region Properties
        /// <summary>Gets whether or not the contents of this control have been edited by a user.</summary>
        public bool HasBeenEdited { get { return hasBeenEdited; } }
        
        /// <summary>Gets or sets the visibility of the referenced strings.</summary>
        public System.Windows.Visibility ReferenceStringsVisiblity
        {
            get { return ReferenceStringsColumn.Visibility; }
            set { ReferenceStringsColumn.Visibility = value; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Default constructor.
        /// </summary>
        public StringComparisonView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Clears the has been edited flag.
        /// </summary>
        public void ClearHasBeenEdited()
        {
            hasBeenEdited = false;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles that actions that should take place when a data grid cell is finished being edited.
        /// </summary>
        /// <param name="sender">The data grid that triggered this event.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs eventArguments)
        {
            // INDICATE THAT THE CONTROL'S CONTENTS HAS BEEN EDITED.
            hasBeenEdited = true;
        }
        #endregion
    }
}
