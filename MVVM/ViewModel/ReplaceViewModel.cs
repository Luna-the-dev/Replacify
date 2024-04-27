﻿using System.Diagnostics;
using System.IO;
using TextReplace.Core;
using TextReplace.MVVM.Model;

namespace TextReplace.MVVM.ViewModel
{
    class ReplaceViewModel
    {
        // commands
        public RelayCommand Replace {  get; set; }

        public ReplaceViewModel()
        {
            Replace = new RelayCommand(o =>
            {
                if (ReplaceData() == false)
                {
                    Debug.WriteLine("Something went wrong in the replace command.");
                }
            });
        }

        /// <summary>
        /// Utility function used by the Replace command.
        /// </summary>
        /// <returns>Returns false if something went wrong.</returns>
        private bool ReplaceData()
        {
            ReplaceData replaceData = new ReplaceData();
            string suffix = "replacify"; // TODO let the user change this with GUI later

            // open a file dialogue for the user and update the source files
            bool result = replaceData.SaveReplacePhrases();

            if (result == false)
            {
                Debug.WriteLine("Replace phrases could not be parsed.");
                return false;
            }

            // create a list of destination file names
            List<string> destFileNames = SourceFiles.GenerateDestFileNames(suffix);

            // perform the text replacements
            result = replaceData.PerformReplacements(SourceFiles.FileNames, destFileNames);

            if (result == false)
            {
                Debug.WriteLine("A replacement could not be made.");
                return false;
            }

            return true;
        }
    }
}