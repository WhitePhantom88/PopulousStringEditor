using Microsoft.Win32;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace PopulousStringEditor
{
    #region Custom Command Definitions
    /// <summary>
    /// Contains the commands used by various child controls of the main window.
    /// </summary>
    public static class MainWindowCommands
    {
        /// <summary>The command for creating a new strings file.</summary>
        public static readonly RoutedUICommand NewStringsFile = new RoutedUICommand(
            text: "_New",
            name: "NewStringsFile",
            ownerType: typeof(PopulousStringEditor.MainWindowCommands),
            inputGestures: new InputGestureCollection { new KeyGesture(Key.N, ModifierKeys.Control) }
        );

        /// <summary>The command for opening the strings file.</summary>
        public static readonly RoutedUICommand OpenStringsFile = new RoutedUICommand(
            text: "_Open...",
            name: "OpenStringsFile",
            ownerType: typeof(PopulousStringEditor.MainWindowCommands),
            inputGestures: new InputGestureCollection { new KeyGesture(Key.O, ModifierKeys.Control) }
        );

        /// <summary>The command for opening the reference strings file.</summary>
        public static readonly RoutedUICommand OpenReferenceStringsFile = new RoutedUICommand(
            text: "Open _Reference Strings...",
            name: "OpenReferenceStringsFile",
            ownerType: typeof(PopulousStringEditor.MainWindowCommands),
            inputGestures: null
        );

        /// <summary>The command for closing all the files.</summary>
        public static readonly RoutedUICommand CloseAllFiles = new RoutedUICommand(
            text: "_Close All",
            name: "CloseAllFiles",
            ownerType: typeof(PopulousStringEditor.MainWindowCommands),
            inputGestures: null
        );

        /// <summary>The command for saving the strings file.</summary>
        public static readonly RoutedUICommand SaveStringsFile = new RoutedUICommand(
            text: "_Save",
            name: "SaveStringsFile",
            ownerType: typeof(PopulousStringEditor.MainWindowCommands),
            inputGestures: new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) }
        );

        /// <summary>The command for saving the strings file as another file..</summary>
        public static readonly RoutedUICommand SaveStringsFileAs = new RoutedUICommand(
            text: "Save _As...",
            name: "SaveStringsFileAs",
            ownerType: typeof(PopulousStringEditor.MainWindowCommands),
            inputGestures: null
        );

        /// <summary>The command for exiting the application.</summary>
        public static readonly RoutedUICommand ExitApplication = new RoutedUICommand(
            text: "E_xit",
            name: "ExitApplication",
            ownerType: typeof(PopulousStringEditor.MainWindowCommands),
            inputGestures: null
        );

    }
    #endregion

    /// <summary>
    /// Handles events which take place on the application's main window.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants
        /// <summary>The open/save file dialog files for string files.</summary>
        private const string StringFileDialogFilters = "String Files lang??.dat|lang??.dat|Data Files (*.dat)|*.dat|All files (*.*)|*.*";
        #endregion

        #region Fields
        /// <summary>The view model for populating the string comparison view.</summary>
        private ViewModel.StringComparisonViewModel stringComparisonViewModel = new ViewModel.StringComparisonViewModel();
        /// <summary>Indicates whether or not the strings file has unsaved changes./summary>
        private bool unsavedStringFileModificationsExist = false;
        /// <summary>Holds the path to the strings file that is currently being modified.  If this field is null,
        /// then no path has been specified yet.</summary>
        private string currentStringsFilePath = null;
        #endregion

        #region Properties
        /// <summary>Gets whether or not any string comparisons have been loaded.</summary>
        private bool AnyStringComparisons
        {
            get { return stringComparisonViewModel.StringComparisons.Any(); }
        }

        /// <summary>Gets whether or not any reference strings are available.</summary>
        private bool AnyReferenceStringsAvailable
        {
            get
            {
                return stringComparisonViewModel.StringComparisons.Any(
                    currentStringComparison => !string.IsNullOrEmpty(currentStringComparison.ReferenceString));
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the actions that should take place when this window is loaded.
        /// </summary>
        /// <param name="sender">The window that triggered his event.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void Window_Loaded(object sender, RoutedEventArgs eventArguments)
        {
            // HIDE THE REFERENCE STRINGS COLUMN.
            // No reference strings are being displayed, so there's no need to waste
            // screen space displaying a column for them.
            StringComparisonsControl.ReferenceStringsVisiblity = Visibility.Hidden;
        }

        /// <summary>
        /// Handles the actions that should take place when the string comparisons control is loaded.
        /// </summary>
        /// <param name="sender">The string comparisons control which triggered this event.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void StringComparisonsControl_Loaded(object sender, RoutedEventArgs eventArguments)
        {
            // HOOK UP THE STRING COMPARISON CONTROL'S DATA CONTEXT.
            // Once this data context is set, any changes made to the view model will appear in the control.
            StringComparisonsControl.DataContext = stringComparisonViewModel;
        }
        #endregion

        #region Command Handlers
        #region New Strings File Command
        /// <summary>
        /// An action for determining whether or not the new strings file command can execute.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void NewStringsFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs eventArguments)
        {
            // THIS COMAND SHOULD ALWAYS BE ABLE TO EXECUTE.
            eventArguments.CanExecute = true;
        }

        /// <summary>
        /// An action for handling execution of the new strings file action.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void NewStringsFileCommand_Executed(object sender, ExecutedRoutedEventArgs eventArguments)
        {
            // CHECK TO SEE IF THERE ARE ANY UNSAVED CHANGES.
            if (unsavedStringFileModificationsExist)
            {
                // WARN THE USER ABOUT UNSAVED CHANGES AND ASK THEM WHAT THEY WANT TO DO.
                const string UnsavedChangesMessage = "Your strings file has unsaved changes.  Save before making a new file?";
                bool userCanceledAction = false;
                PromptUserAboutUnsavedChangesAndHandleDecision(UnsavedChangesMessage, out userCanceledAction);

                // CHECK TO SEE IF THE USER CANCELED THEIR ACTION.
                if (userCanceledAction)
                {
                    // Exit early since the user canceled this action.
                    return;
                }
            }

            // CHECK TO SEE IF THE REFERENCE STRINGS HAVE BEEN LOADED.
            // A new strings file should only be allowed to be created if an existing reference file has
            // been loaded.
            bool anyReferenceStringsAlreadyLoaded = AnyReferenceStringsAvailable;
            if (anyReferenceStringsAlreadyLoaded)
            {
                // RESET THE DISPLAY OF THIS WINDOW FOR THE NEW STRINGS FILE.
                ResetForNewStringsFile();
            }
            else
            {
                // PROMPT THE USER TO SPECIFY A FILE TO OPEN.
                string referenceStringsFilePath = GetPathForOpeningReferenceStringsFileFromUser();
                bool referenceStringFilePathSpecified = !string.IsNullOrWhiteSpace(referenceStringsFilePath);
                if (!referenceStringFilePathSpecified)
                {
                    // A reference strings file must be loaded to create a new strings file.
                    // Alert the user since this sort of thing is not common in most applications.
                    const string NoReferenceStringsMessage = "New files can only be created when matched against a reference file.";
                    const string NoReferenceStringsTitle = "No Reference Strings";
                    MessageBox.Show(
                        this,
                        NoReferenceStringsMessage,
                        NoReferenceStringsTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return;
                }

                // RESET THE DISPLAY OF THIS WINDOW FOR THE NEW STRINGS FILE.
                ResetForNewStringsFile();

                // OPEN AND DISPLAY THE CONTENTS OF THE REFERENCE STRINGS FILE.
                OpenAndDisplayReferenceStringsFile(referenceStringsFilePath);
            }
        }
        #endregion

        #region Open Strings File Command
        /// <summary>
        /// An action for determining whether or not the open strings file command can execute.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void OpenStringsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs eventArguments)
        {
            // THIS COMAND SHOULD ALWAYS BE ABLE TO EXECUTE.
            eventArguments.CanExecute = true;
        }

        /// <summary>
        /// An action for handling execution of the open strings file action.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void OpenStringsFileCommand_Executed(object sender, ExecutedRoutedEventArgs eventArguments)
        {
            // PROMPT THE USER TO SPECIFY A FILE TO OPEN.
            string stringsFilePath = GetPathForOpeningStringFileFromUser();
            bool stringFilePathSpecified = !string.IsNullOrWhiteSpace(stringsFilePath);
            if (!stringFilePathSpecified)
            {
                // Exit early since the user canceled selecting a file to open.
                return;
            }

            // OPEN AND DISPLAY THE STRINGS FILE.
            OpenAndDisplayStringsFile(stringsFilePath);
        }
        #endregion

        #region Open Reference Strings File Command
        /// <summary>
        /// An action for determining whether or not the open reference strings file command can execute.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void OpenReferenceStringsFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs eventArguments)
        {
            // THIS COMAND SHOULD ALWAYS BE ABLE TO EXECUTE.
            eventArguments.CanExecute = true;
        }

        /// <summary>
        /// An action for handling execution of the open reference strings file action.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void OpenReferenceStringsFileCommand_Executed(object sender, ExecutedRoutedEventArgs eventArguments)
        {
            // CHECK TO SEE IF THERE ARE ANY UNSAVED CHANGES.
            if (unsavedStringFileModificationsExist)
            {
                // WARN THE USER ABOUT UNSAVED CHANGES AND ASK THEM WHAT THEY WANT TO DO.
                const string UnsavedChangesMessage = "Your strings file has unsaved changes.  Save before opening a different file?";
                bool userCanceledAction = false;
                PromptUserAboutUnsavedChangesAndHandleDecision(UnsavedChangesMessage, out userCanceledAction);

                // CHECK TO SEE IF THE USER CANCELED THEIR ACTION.
                if (userCanceledAction)
                {
                    // Exit early since the user canceled this action.
                    return;
                }
            }

            // PROMPT THE USER TO SPECIFY A FILE TO OPEN.
            string referenceStringsFilePath = GetPathForOpeningReferenceStringsFileFromUser();
            bool referenceStringFilePathSpecified = !string.IsNullOrWhiteSpace(referenceStringsFilePath);
            if (!referenceStringFilePathSpecified)
            {
                // Exit early since the user canceled selecting a file to open.
                return;
            }

            // OPEN AND DISPLAY THE CONTENTS OF THE REFERENCE STRINGS FILE.
            OpenAndDisplayReferenceStringsFile(referenceStringsFilePath);
        }
        #endregion

        #region Close Files Command
        /// <summary>
        /// An action for determining whether or not the close files command can execute.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void CloseAllFilesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs eventArguments)
        {
            // There is nothing to close if no string comparisons are present.
            eventArguments.CanExecute = AnyStringComparisons;
        }

        /// <summary>
        /// An action for handling execution of the new close files action.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void CloseAllFilesCommand_Executed(object sender, ExecutedRoutedEventArgs eventArguments)
        {
            // CHECK TO SEE IF THERE ARE ANY UNSAVED CHANGES.
            if (unsavedStringFileModificationsExist)
            {
                // WARN THE USER ABOUT UNSAVED CHANGES AND ASK THEM WHAT THEY WANT TO DO.
                const string UnsavedChangesMessage = "Your strings file has unsaved changes.  Save before closing files?";
                bool userCanceledAction = false;
                PromptUserAboutUnsavedChangesAndHandleDecision(UnsavedChangesMessage, out userCanceledAction);

                // CHECK TO SEE IF THE USER CANCELED THEIR ACTION.
                if (userCanceledAction)
                {
                    // Exit early since the user canceled this action.
                    return;
                }
            }

            // CLEAR ALL OF THE STRING COMPARISONS.
            stringComparisonViewModel.ClearStringComparisons();

            // UPDATE THE CURRENT STRING FILE PATH.
            currentStringsFilePath = null;

            // HIDE THE REFERENCE STRINGS COLUMN.
            // No reference strings are being displayed, so there's no need to waste
            // screen space displaying a column for them.
            StringComparisonsControl.ReferenceStringsVisiblity = Visibility.Hidden;
        }
        #endregion

        #region Save String File Command
        /// <summary>
        /// An action for determining whether or not the save string file command can execute.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void SaveStringsFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs eventArguments)
        {
            // There is nothing to save if no string comparisons are present.
            eventArguments.CanExecute = AnyStringComparisons;
        }

        /// <summary>
        /// An action for handling execution of the save string file action.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void SaveStringsFileCommand_Executed(object sender, ExecutedRoutedEventArgs eventArguments)
        {
            // DETERMINE IF A SAVE FILE PATH HAS ALREADY BEEN SPECIFIED.
            bool saveFileAlreadySpecified = !string.IsNullOrWhiteSpace(currentStringsFilePath);
            if (!saveFileAlreadySpecified)
            {
                // PROMPT THE USER TO SPECIFY A FILE TO SAVE UNDER.
                // Since the file path of the file to save has not been specified, treat this as a Save As operation
                // to get the file path from the user.
                currentStringsFilePath = GetPathForSavingStringsFileFromUser();
                bool stringFilePathSpecified = !string.IsNullOrWhiteSpace(currentStringsFilePath);
                if (!stringFilePathSpecified)
                {
                    // Exit early since the user canceled selecting a file to save.
                    return;
                }
            }

            // SAVE THE FILE.
            SaveStringsFile(currentStringsFilePath);
        }
        #endregion

        #region Save String File As Command
        /// <summary>
        /// An action for determining whether or not the save as command can execute.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void SaveStringsFileAsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs eventArguments)
        {
            // There is nothing to save if no string comparisons are present.
            eventArguments.CanExecute = AnyStringComparisons;
        }

        /// <summary>
        /// An action for handling execution of the save as action.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void SaveStringsFileAsCommand_Executed(object sender, ExecutedRoutedEventArgs eventArguments)
        {
            // PROMPT THE USER TO SPECIFY A FILE TO SAVE UNDER.
            string stringsFilePath = GetPathForSavingStringsFileFromUser();
            bool stringFilePathSpecified = !string.IsNullOrWhiteSpace(stringsFilePath);
            if (!stringFilePathSpecified)
            {
                // Exit early since the user canceled selecting a file to save.
                return;
            }

            // SAVE THE FILE.
            SaveStringsFile(stringsFilePath);
        }
        #endregion

        #region Exit Application Command
        /// <summary>
        /// An action for determining whether or not the exit application command can execute.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void ExitApplicationCommand_CanExecute(object sender, CanExecuteRoutedEventArgs eventArguments)
        {
            // THIS COMAND SHOULD ALWAYS BE ABLE TO EXECUTE.
            eventArguments.CanExecute = true;
        }

        /// <summary>
        /// An action for handling execution of the exit application action.
        /// </summary>
        /// <param name="sender">The object which triggered the command.</param>
        /// <param name="eventArguments">The event arguments.</param>
        private void ExitApplicationCommand_Executed(object sender, ExecutedRoutedEventArgs eventArguments)
        {
            // EXIT THE APPLICATION.
            const int NormalApplicationTerminationExitCode = 0;
            Application.Current.Shutdown(NormalApplicationTerminationExitCode);
        }
        #endregion
        #endregion

        #region Public Methods
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets error message details from the provided exception.
        /// </summary>
        /// <param name="exception">The exception to extract error message details from.</param>
        /// <returns>The error message.</returns>
        private static string GetErrorMessage(Exception exception)
        {
            // APPEND THE MESSAGE FROM THE OUTER EXCEPTION.
            StringBuilder errorMessage = new StringBuilder();
            errorMessage.Append(exception.Message);

            // CHECK TO SEE IF THERE IS AN INNER EXCEPTION.
            bool innerExceptionExists = (exception.InnerException != null);
            if (innerExceptionExists)
            {
                // APPEND THE MESSAGE FROM THE INNER EXCEPTION.
                errorMessage.Append(" ");
                errorMessage.Append(exception.InnerException.Message);
            }

            return errorMessage.ToString();
        }

        /// <summary>
        /// Prompts the user about unsaved changes remaining and handles his or her decision.
        /// </summary>
        /// <param name="unsavedChangesMessage">The message to display about unsaved changes.
        /// This should include the context of the user's most recent action.</param>
        /// <param name="userCanceledDialog">Indicates whether or not the user canceled their action.
        /// True if they canceled or false otherwise.</param>
        private void PromptUserAboutUnsavedChangesAndHandleDecision(
            string unsavedChangesMessage,
            out bool userCanceledDialog)
        {
            // SET THE OUTPUT PARAMETER TO A DEFAULT KNOWN VALUE.
            userCanceledDialog = false;

            // WARN THE USER ABOUT UNSAVED CHANGES.
            const string UnsavedChangesTitle = "Unsaved Changes";
            MessageBoxResult userSelection = MessageBox.Show(
                this,
                unsavedChangesMessage,
                UnsavedChangesTitle,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            switch (userSelection)
            {
                case MessageBoxResult.Yes:
                    // DETERMINE IF A SAVE FILE PATH HAS ALREADY BEEN SPECIFIED.
                    bool saveFileAlreadySpecified = !string.IsNullOrWhiteSpace(currentStringsFilePath);
                    if (!saveFileAlreadySpecified)
                    {
                        // PROMPT THE USER TO SPECIFY A FILE TO SAVE UNDER.
                        // Since the file path of the file to save has not been specified, treat this as a Save As operation
                        // to get the file path from the user.
                        currentStringsFilePath = GetPathForSavingStringsFileFromUser();
                        bool stringFilePathSpecified = !string.IsNullOrWhiteSpace(currentStringsFilePath);
                        if (!stringFilePathSpecified)
                        {
                            // Exit early since the user canceled selecting a file to save.
                            userCanceledDialog = true;
                            return;
                        }
                    }

                    // SAVE THE FILE.
                    SaveStringsFile(currentStringsFilePath);
                    break;
                case MessageBoxResult.No:
                    // Intentionally do nothing.
                    break;
                case MessageBoxResult.Cancel:
                    userCanceledDialog = true;
                    break;
                default:
                    // Nothing can be done since the user's selection is unknown.
                    break;
            }
        }

        /// <summary>
        /// Resets the display of this window for a new strings file.  The current strings file path
        /// and the unsaved changes flag are cleared.
        /// </summary>
        private void ResetForNewStringsFile()
        {
            // CLEAR THE DATA CONTEXT SO THAT IT APPEARS TO HAVE CHANGED.
            // Clearing the data context will allow it to be updated with new contents once the file is loaded.
            StringComparisonsControl.DataContext = null;

            // CLEAR ALL OF THE EXISTING STRINGS.
            stringComparisonViewModel.ClearStrings();

            // SET THE DATA CONTEXT.
            StringComparisonsControl.DataContext = stringComparisonViewModel;

            // UPDATE THE CURRENT STRING FILE PATH.
            currentStringsFilePath = null;

            // MARK THE FILE AS NO LONGER HAVING UNSAVED CHANGES.
            // Since the file is new and empty, there are no unsaved changes.
            unsavedStringFileModificationsExist = false;
        }

        /// <summary>
        /// Gets the path to the strings file to open from the user.
        /// </summary>
        /// <returns>The absolute path to the strings file to open or null
        /// if the user canceled selecting a file.</returns>
        private string GetPathForOpeningStringFileFromUser()
        {
            // DISPLAY THE FILE OPEN DIALOG.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = StringFileDialogFilters;
            openFileDialog.Title = "Open String File";
            bool? dialogResult = openFileDialog.ShowDialog(this);

            // DETERMINE WHAT THE USER CHOSE TO DO.
            bool fileSpecified = (dialogResult.HasValue && dialogResult.Value);
            if (!fileSpecified)
            {
                // Return indicating that the user canceled selecting a file to open.
                return null;
            }

            return openFileDialog.FileName;
        }

        /// <summary>
        /// Opens the strings file and updates the controls of this window to display the content of
        /// the opened file.  The current strings file path and unsaved changes flag are also updated.
        /// </summary>
        /// <param name="filePath">An absolute or relative path to the strings file to open.</param>
        private void OpenAndDisplayStringsFile(string filePath)
        {
            try
            {
                // CLEAR THE DATA CONTEXT SO THAT IT APPEARS TO HAVE CHANGED.
                // Clearing the data context will allow it to be updated with new contents once the file is loaded.
                StringComparisonsControl.DataContext = null;

                // LOAD THE FILE.
                stringComparisonViewModel.LoadStrings(filePath);

                // UPDATE THE CURRENT STRING FILE PATH.
                currentStringsFilePath = filePath;

                // SET THE DATA CONTEXT.
                StringComparisonsControl.DataContext = stringComparisonViewModel;

                // MARK THE FILE AS NO LONGER HAVING UNSAVED CHANGES.
                // Since the file has been freshly opened, there are no unsaved changes.
                unsavedStringFileModificationsExist = false;
            }
            catch (Exception exception)
            {
                const string MessageBoxCaption = "Open File Failed";
                MessageBox.Show(
                    this,
                    GetErrorMessage(exception),
                    MessageBoxCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Gets the path to the reference strings file to open from the user.
        /// </summary>
        /// <returns>The absolute path to the reference strings file to open or null
        /// if the user canceled selecting a file.</returns>
        private string GetPathForOpeningReferenceStringsFileFromUser()
        {
            // DISPLAY THE FILE OPEN DIALOG.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = StringFileDialogFilters;
            openFileDialog.Title = "Open Reference String File";
            bool? dialogResult = openFileDialog.ShowDialog(this);

            // DETERMINE WHAT THE USER CHOSE TO DO.
            bool fileSpecified = (dialogResult.HasValue && dialogResult.Value);
            if (!fileSpecified)
            {
                // Return indicating that the user canceled selecting a file.
                return null;
            }

            return openFileDialog.FileName;
        }

        /// <summary>
        /// Opens the reference strings file and updates the controls of this window to display the
        /// content of the opened file.
        /// </summary>
        /// <param name="filePath">An absolute or relative path to the reference strings file to open.</param>
        private void OpenAndDisplayReferenceStringsFile(string filePath)
        {
            try
            {
                // CLEAR THE DATA CONTEXT SO THAT IT APPEARS TO HAVE CHANGED.
                // Clearing the data context will allow it to be updated with new contents once the file is loaded.
                StringComparisonsControl.DataContext = null;

                // LOAD THE FILE.
                stringComparisonViewModel.LoadReferenceStrings(filePath);

                // SET THE DATA CONTEXT.
                StringComparisonsControl.DataContext = stringComparisonViewModel;

                // SHOW THE REFERENCE STRINGS COLUMN.
                StringComparisonsControl.ReferenceStringsVisiblity = Visibility.Visible;
            }
            catch (Exception exception)
            {
                const string MessageBoxCaption = "Open File Failed";
                MessageBox.Show(
                    this,
                    GetErrorMessage(exception),
                    MessageBoxCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Gets the path to the strings file to save from the user.
        /// </summary>
        /// <returns>The absolute path to the strings file to save or null
        /// if the user canceled selecting a file.</returns>
        private string GetPathForSavingStringsFileFromUser()
        {
            // DISPLAY THE FILE SAVE DIALOG.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = StringFileDialogFilters;
            saveFileDialog.Title = "Save As";
            bool? dialogResult = saveFileDialog.ShowDialog(this);

            // DETERMINE WHAT THE USER CHOSE TO DO.
            bool fileSpecified = (dialogResult.HasValue && dialogResult.Value);
            if (!fileSpecified)
            {
                // Exit indicating that the user canceled selecting a file.
                return null;
            }

            return saveFileDialog.FileName;
        }

        /// <summary>
        /// Saves the strings file.  The current strings file path and unsaved changes flag are also updated.
        /// </summary>
        /// <param name="filePath">An absolute or relative path to the strings file to save.</param>
        private void SaveStringsFile(string filePath)
        {
            try
            {
                // SAVE THE FILE.
                stringComparisonViewModel.SaveStrings(filePath);

                // MARK THE FILE AS NO LONGER HAVING UNSAVED CHANGES.
                unsavedStringFileModificationsExist = false;

                // UPDATE THE CURRENT STRING FILE PATH.
                currentStringsFilePath = filePath;
            }
            catch (Exception exception)
            {
                const string MessageBoxCaption = "Save File Failed";
                MessageBox.Show(
                    this,
                    GetErrorMessage(exception),
                    MessageBoxCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }
        #endregion
    }
}
