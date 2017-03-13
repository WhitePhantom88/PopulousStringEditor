using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PopulousStringEditor.ViewModel
{
    class StringComparisonViewModel
    {
        #region Properties
        /// <summary>The observable collection of string comparisons.</summary>
        public ObservableCollection<Model.StringComparison> StringComparisons { get; set; } = new ObservableCollection<Model.StringComparison>();

        /// <summary>Gets the editable strings from the string comparisons.</summary>
        private List<string> EditableStrings
        {
            get
            {
                return StringComparisons.Select(
                    currentStringComparison =>
                        string.IsNullOrEmpty(currentStringComparison.EditableString) ?
                            string.Empty :
                            currentStringComparison.EditableString).ToList();
            }
        }

        /// <summary>Gets the reference strings from the string comparisons.</summary>
        private List<string> ReferenceStrings
        {
            get
            {
                return StringComparisons.Select(
                    currentStringComparison =>
                        string.IsNullOrEmpty(currentStringComparison.ReferenceString) ?
                            string.Empty :
                            currentStringComparison.ReferenceString).ToList();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Loads the strings from the specified file and populates the string comparisons.
        /// </summary>
        /// <param name="filePath">An absolute or relative path to the file to get strings from.</param>
        /// <exception cref="Exception">Thrown if an error occurs.</exception>
        public void LoadStrings(string filePath)
        {
            // RETRIEVE THE LISTS OF STRINGS TO POPULATE INTO THE STRING COMPARISON.
            List<string> newEditableStrings = PopulousString.BinaryStringsFile.Read(filePath);
            List<string> oldReferenceStrings = ReferenceStrings;

            // POPULATE THE STRING COMPARISONS.
            StringComparisons = GetStringComparisons(
                newEditableStrings,
                oldReferenceStrings);
        }

        /// <summary>
        /// Loads the reference strings from the specified file and populates the string comparisons.
        /// </summary>
        /// <param name="filePath">An absolute or relative path to the file to get reference strings from.</param>
        /// <exception cref="Exception">Thrown if an error occurs.</exception>
        public void LoadReferenceStrings(string filePath)
        {
            // RETRIEVE THE LISTS OF STRINGS TO POPULATE INTO THE STRING COMPARISON.
            List<string> oldEditableStrings = EditableStrings;
            List<string> newReferenceStrings = PopulousString.BinaryStringsFile.Read(filePath);

            // POPULATE THE STRING COMPARISONS.
            StringComparisons = GetStringComparisons(
                oldEditableStrings,
                newReferenceStrings);
        }

        /// <summary>
        /// Clears all of the strings from each string comparison.
        /// </summary>
        public void ClearStrings()
        {
            // CLEAR EACH OF THE STRING COMPARISON'S REFERENCE STRING.
            foreach (Model.StringComparison currentStringComparison in StringComparisons)
            {
                // CLEAR THE REFERENCE STRING.
                currentStringComparison.EditableString = string.Empty;
            }
        }

        /// <summary>
        /// Clear all of the string comparisons.
        /// </summary>
        public void ClearStringComparisons()
        {
            // CLEAR ALL OF THE STRING COMPARISONS.
            StringComparisons.Clear();
        }

        /// <summary>
        /// Saves the strings to the specified file.
        /// </summary>
        /// <param name="filePath">An absolute or relative path to the file where strings will be saved.</param>
        /// <exception cref="Exception">Thrown if an error occurs.</exception>
        public void SaveStrings(string filePath)
        {
            PopulousString.BinaryStringsFile.Write(filePath, EditableStrings);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the collection of string comparisons formed from the provided collections.
        /// </summary>
        /// <remarks>
        /// It is assumed that the strings from the reference strings and the editable strings will
        /// be in the same order for the reference strings to be of any use when comparing to the
        /// editable strings.
        /// </remarks>
        /// <param name="editableStringCollection">The editable strings to add to the string comparisons.</param>
        /// <param name="referenceStringCollection">The reference strings to add to the string comparisons.</param>
        /// <returns>The resulting collection of string comparisons.  The size of the collection will be the larger
        /// of the editable strings and reference strings collections.</returns>
        private ObservableCollection<Model.StringComparison> GetStringComparisons(
            List<string> editableStringCollection,
            List<string> referenceStringCollection)
        {
            // DETERMINE HOW MANY STRING COMPARISONS ARE NEEDED.
            int editableStringCollectionCount = editableStringCollection.Count;
            int referenceStringCollectionCount = referenceStringCollection.Count;
            int stringComparisonCount = System.Math.Max(editableStringCollectionCount, referenceStringCollectionCount);

            // ADD ALL OF THE STRING COMPARISONS.
            ObservableCollection<Model.StringComparison> stringComparisons = new ObservableCollection<Model.StringComparison>();
            for (int stringComparisonIndex = 0; stringComparisonIndex < stringComparisonCount; ++stringComparisonIndex)
            {
                // GET THE EDITABLE STRING, IF AVAILABLE.
                bool editableStringAvailable = (stringComparisonIndex < editableStringCollectionCount);
                string editableString = editableStringAvailable ?
                    editableStringCollection[stringComparisonIndex] :
                    null;

                // GET THE REFERENCE STRING, IF AVAILABLE.
                bool referenceStringAvailable = (stringComparisonIndex < referenceStringCollectionCount);
                string referenceString = referenceStringAvailable ?
                    referenceStringCollection[stringComparisonIndex] :
                    null;

                // ADD THE STRING COMPARISON TO THE COLLECTION OF STRING COMPARISONS.
                Model.StringComparison stringComparison = new Model.StringComparison(editableString, referenceString);
                stringComparisons.Add(stringComparison);
            }

            return stringComparisons;
        }
        #endregion
    }
}
