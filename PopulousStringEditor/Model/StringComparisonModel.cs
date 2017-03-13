using System.ComponentModel;

namespace PopulousStringEditor.Model
{
    /// <summary>
    /// Represents a string comparison between a Populous strings file and an optional, read-only reference Populous strings file.
    /// </summary>
    public class StringComparison : INotifyPropertyChanged
    {
        #region Fields
        /// <summary>The editable string.</summary>
        private string editableString = string.Empty;
        #endregion

        #region Properties
        /// <summary>Gets the optional, read-only, reference string used for comparison.</summary>
        public string ReferenceString { get; private set; } = null;
        /// <summary>Gets or sets the editable string.</summary>
        public string EditableString
        {
            get
            {
                return editableString;
            }
            set
            {
                editableString = value;
                RaisePropertyChanged("LanguageString");
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>Handles the actions that should take place when a property is changed.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Triggers the property changed event handler.
        /// </summary>
        /// <param name="property">The property that was changed.</param>
        private void RaisePropertyChanged(string property)
        {
            // VERIFY THAT THE PROPERTY CHANGED EVNET HANDLER IS CONFIGURED.
            if (PropertyChanged != null)
            {
                // TRIGGER THE PROPERTY CHANGED EVENT HANDLER.
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Constructs a string comparison from the provided arguments.
        /// </summary>
        /// <param name="editableString">The optional editable string.  A null or empty string means that no string was specified.</param>
        /// <param name="referenceString">The optional reference string.  A null or empty string means that no string was specified.</param>
        public StringComparison(string editableString, string referenceString)
        {
            if (!string.IsNullOrEmpty(editableString))
            {
                EditableString = editableString;
            }

            if (!string.IsNullOrEmpty(referenceString))
            {
                ReferenceString = referenceString;
            }
        }
        #endregion
    }
}
