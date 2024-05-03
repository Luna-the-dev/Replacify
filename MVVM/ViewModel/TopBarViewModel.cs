﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TextReplace.Core;
using TextReplace.MVVM.Model;
using TextReplace.MVVM.View;

namespace TextReplace.MVVM.ViewModel
{
    class TopBarViewModel : ObservableObject
    {
        private Visibility _hasHeader = Visibility.Hidden;
        public Visibility HasHeader
        {
            get { return _hasHeader; }
            set
            {
                _hasHeader = value;
                ReplaceData.HasHeader = (value == Visibility.Visible) ? true : false;
                OnPropertyChanged();
            }
        }
        private string _delimiter = string.Empty;
        public string Delimiter
        {
            get { return _delimiter; }
            set
            {
                _delimiter = value;
                ReplaceData.Delimiter = value;
                OnPropertyChanged();
            }
        }
        private Visibility _caseSensitive = Visibility.Hidden;
        public Visibility CaseSensitive
        {
            get { return _caseSensitive; }
            set
            {
                _caseSensitive = value;
                OnPropertyChanged();
            }
        }
        private Visibility _wholeWord = Visibility.Hidden;
        public Visibility WholeWord
        {
            get { return _wholeWord; }
            set
            {
                _wholeWord = value;
                OnPropertyChanged();
            }
        }
        private string _outputFilePath = string.Empty;
        public string OutputFilePath
        {
            get { return _outputFilePath; }
            set { _outputFilePath = value; }
        }


        // visibility flags for top bar components
        private Visibility _replaceFileReadSuccess = Visibility.Hidden;
        public Visibility ReplaceFileReadSuccess
        {
            get { return _replaceFileReadSuccess; }
            set
            {
                _replaceFileReadSuccess = value;
                OnPropertyChanged();
            }
        }
        private Visibility _replaceFileReadFail = Visibility.Hidden;
        public Visibility ReplaceFileReadFail
        {
            get { return _replaceFileReadFail; }
            set
            {
                _replaceFileReadFail = value;
                OnPropertyChanged();
            }
        }
        private Visibility _sourceFileReadSuccess = Visibility.Hidden;
        public Visibility SourceFileReadSuccess
        {
            get { return _sourceFileReadSuccess; }
            set
            {
                _sourceFileReadSuccess = value;
                OnPropertyChanged();
            }
        }
        private Visibility _sourceFileReadFail = Visibility.Hidden;
        public Visibility SourceFileReadFail
        {
            get { return _sourceFileReadFail; }
            set
            {
                _sourceFileReadFail = value;
                OnPropertyChanged();
            }
        }
        private Visibility _replaceisClickable = Visibility.Hidden;
        public Visibility ReplaceisClickable
        {
            get { return _replaceisClickable; }
            set
            {
                _replaceisClickable = value;
                OnPropertyChanged();
            }
        }
        private Visibility _replaceisUnclickable = Visibility.Visible;
        public Visibility ReplaceisUnclickable
        {
            get { return _replaceisUnclickable; }
            set
            {
                _replaceisUnclickable = value;
                OnPropertyChanged();
            }
        }

        private const string INVALID_DELIMITER_CHARS = "\n";

        // commands
        public RelayCommand ReplaceFile => new RelayCommand(o => ReplaceFileCmd());
        public RelayCommand SourceFiles => new RelayCommand(o => SourceFilesCmd());
        public RelayCommand Replace => new RelayCommand(o => ReplaceCmd());
        public RelayCommand ToggleHasHeader => new RelayCommand(o => ToggleHasHeaderCmd());
        public RelayCommand ToggleCaseSensitive => new RelayCommand(o => ToggleCaseSensitiveCmd());
        public RelayCommand ToggleWholeWord => new RelayCommand(o => ToggleWholeWordCmd());
        public RelayCommand ChangeOutputDirectory => new RelayCommand(o => ChangeOutputDirectoryCmd());

        private void ReplaceFileCmd()
        {
            // open a file dialogue for the user and update the replace file
            bool? result = ReplaceData.SetNewReplaceFileFromUser();

            if (result == true)
            {
                Debug.Write("Replace file name:\t");
                Debug.WriteLine(ReplaceData.FileName);
                ReplaceFileReadSuccess = Visibility.Visible;
                ReplaceFileReadFail = Visibility.Hidden;
            }
            else if (result == false) 
            {
                Debug.WriteLine("ReplaceFile either could not be read or parsed.");
                ReplaceFileReadSuccess = Visibility.Hidden;
                ReplaceFileReadFail = Visibility.Visible;
            }

            SetReplaceButtonClickability();
        }

        private void SourceFilesCmd()
        {
            // open a file dialogue for the user and update the source files
            bool? result = SourceFilesData.SetNewSourceFilesFromUser();

            if (result == true)
            {
                Debug.Write("Source file name(s):");
                SourceFilesData.FileNames.ForEach(i => Debug.WriteLine($"\t{i}"));
                SourceFileReadSuccess = Visibility.Visible;
                SourceFileReadFail = Visibility.Hidden;
            }
            else if (result == false)
            {
                Debug.WriteLine("SourceFile could not be read.");
                SourceFileReadSuccess = Visibility.Hidden;
                SourceFileReadFail = Visibility.Visible;
            }

            SetReplaceButtonClickability();
        }

        private void ReplaceCmd()
        {
            if (ReplaceData.FileName == string.Empty || SourceFilesData.FileNames.Count == 0)
            {
                Debug.WriteLine("Replace file or source files were empty. This should never be reached...");
                return;
            }

            bool caseSensitive = (CaseSensitive == Visibility.Visible) ? true : false;
            ReplaceData replaceData = new ReplaceData(caseSensitive);

            string suffix = "replacify"; // TODO let the user change this with GUI later

            // create a list of destination file names
            List<string> destFileNames = SourceFilesData.GenerateDestFileNames(suffix);

            // perform the text replacements
            bool wholeWord = (WholeWord == Visibility.Visible) ? true : false;
            bool result = replaceData.PerformReplacements(SourceFilesData.FileNames, destFileNames, wholeWord);
            SourceFilesData.FileNames.ForEach(o => Debug.WriteLine(o));
            destFileNames.ForEach(o => Debug.WriteLine(o));

            if (result == false)
            {
                Debug.WriteLine("A replacement could not be made.");
            }
            else
            {
                Debug.WriteLine("Replacements successfully performed.");
            }
        }

        private void ToggleHasHeaderCmd()
        {
            HasHeader = (HasHeader == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
        }

        private void ToggleCaseSensitiveCmd()
        {
            CaseSensitive = (CaseSensitive == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
        }

        private void ToggleWholeWordCmd()
        {
            WholeWord = (WholeWord == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden;
        }

        private void ChangeOutputDirectoryCmd()
        {
            // configure open file dialog box
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.Title = "Select Folder";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                Debug.WriteLine("Change default file path window was closed");
                return;
            }
            
            SourceFilesData.OutputDirectory = dialog.FileName;
        }

        /// <summary>
        /// Checks to see if the delimiter is valid, and then sets the delimiter
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public bool SetDelimiter(string delimiter)
        {
            if (IsDelimiterValid(delimiter))
            {
                Delimiter = delimiter;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if the delimiter string contains any invalid characters.
        /// </summary>
        /// <param name="delimiter"></param>
        /// <returns>True if the string is empty or does not contain any invalid characters.</returns>
        private bool IsDelimiterValid(string delimiter)
        {
            if (delimiter == string.Empty)
            {
                return true;
            }
            return INVALID_DELIMITER_CHARS.Contains(delimiter) ? false : true;
        }

        /// <summary>
        /// Sets the replace button visibility based on whether replace/source files were successfully uploaded.
        /// </summary>
        private void SetReplaceButtonClickability()
        {
            if (ReplaceFileReadSuccess == Visibility.Visible &&
                SourceFileReadSuccess == Visibility.Visible)
            {
                ReplaceisClickable = Visibility.Visible;
                ReplaceisUnclickable = Visibility.Hidden;
            }
            else
            {
                ReplaceisClickable = Visibility.Hidden;
                ReplaceisUnclickable = Visibility.Visible;
            }
        }
    }
}