﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Diagnostics;
using TextReplace.MVVM.Model;
using Wordprocessing = DocumentFormat.OpenXml.Wordprocessing;

namespace TextReplace.Core.AhoCorasick
{
    public class AhoCorasickHelper
    {
        // delimiters that decide what seperates whole words
        private const string WORD_DELIMITERS = " \t/\\()\"'-:,.;<>~!@#$%^&*|+=[]{}?│";

        /// <summary>
        /// Uses the Aho-Corasick algorithm to search a file for any substring matches
        /// </summary>
        /// <param name="replacePhrases"></param>
        /// <param name="line"></param>
        /// <param name="matcher"></param>
        /// <param name="wholeWord"></param>
        /// <param name="preserveCase"></param>
        /// <param name="numOfMatches"></param>
        /// <returns>The string with replacements made.</returns>
        public static string SubstituteMatches(
            Dictionary<string, string> replacePhrases,
            string line,
            AhoCorasickStringSearcher matcher,
            bool wholeWord,
            bool preserveCase,
            out int numOfMatches)
        {
            // search the current line for any text that should be replaced
            var matches = matcher.Search(line);
            numOfMatches = 0;

            // save an offset to remember how much the position of each replacement
            // should be shifted if a replacement was already made on the same line
            int offset = 0;
            string updatedLine = line;
            foreach (var m in matches)
            {
                if (wholeWord && IsMatchWholeWord(line, m.Text, m.Position) == false)
                {
                    continue;
                }

                numOfMatches += 1;
                string replacement = (preserveCase) ?
                    SetMatchCase(replacePhrases[m.Text], char.IsUpper(updatedLine[m.Position + offset])) :
                    replacePhrases[m.Text];

                updatedLine = updatedLine.Remove(m.Position + offset, m.Text.Length)
                                         .Insert(m.Position + offset, replacement);

                offset += replacePhrases[m.Text].Length - m.Text.Length;
            }
            return updatedLine;
        }

        /// <summary>
        /// This algorithm takes in an OpenXml paragraph that consists of one or more OpenXML Wordprocessing runs.
        /// From this paragraph, it creates a list indices that represent where each run within the paragraph begins.
        /// It also constructs a string that represents the entire text within the paragraph.
        /// It performs find-and-replacements on the string using the Aho-Corasick algorithm.
        /// It then reconstructs the original runs with the replacements that were made.
        /// If optional styling is needed for the replacements, they will have their own runs created for them as well.
        /// 
        /// the construction of the new runs works as follows:
        ///     there is the paragraph string and a list of pointers that point to the indices of the string where
        ///     each run begins. there is also another pointer that traverses the string and decides what text
        ///     should be put in each run. every time a run is added, this pointer moves up the length of the run's
        ///     string, except in the case of a replacement's run, it moves up the length of the replaced word
        ///     (since we're not performing the replacement on the paragraph string itself)
        /// </summary>
        /// <param name="paragraph"></param>
        /// <param name="replacePhrases"></param>
        /// <param name="matcher"></param>
        /// <param name="replaceStyling"></param>
        /// <param name="wholeWord"></param>
        /// <param name="preserveCase"></param>
        /// <param name="numOfMatches"></param>
        /// <returns>List of new runs beloning to the paragraph that contain the text replacements.</returns>
        public static List<Wordprocessing.Run> GenerateDocxRuns(
            Wordprocessing.Paragraph paragraph,
            Dictionary<string, string> replacePhrases,
            AhoCorasickStringSearcher matcher,
            OutputFileStyling replaceStyling,
            bool wholeWord,
            bool preserveCase,
            out int numOfMatches)
        {
            var newRuns = new List<Wordprocessing.Run>();
            numOfMatches = 0;

            string paragraphText = "";
            var runs = paragraph.Descendants<Wordprocessing.Run>().ToList();
            var runPtrs = new List<int>();

            // if there are no runs in the paragraph, return an empty List
            if (runs.Count() == 0)
            {
                return newRuns;
            }

            // build out the paragraph text while keeping track of where each run ends
            foreach (var run in runs)
            {
                runPtrs.Add(paragraphText.Length);
                paragraphText += run.InnerText;
            }
            // add one final pointer that marks the end of the paragraph text
            // it makes the algorithm keep working when the main for loop that
            // iterates through the runs is on its final index
            runPtrs.Add(paragraphText.Length);

            // search the paragraph text for any text that should be replaced
            var matches = matcher.Search(paragraphText).ToList();
            if (matches.Count == 0)
            {
                return runs.ToList();
            }

            // this points to the position where the next run should be made
            int runPtr = 0;
            int matchIndex = 0;

            // iterate through the runs
            for (int i = 0; i < runs.Count; i++)
            {
                // while there is a match left inside of the current run
                while (matchIndex < matches.Count && matches[matchIndex].Position < runPtrs[i+1])
                {
                    // if we are looking for whole word matches only,
                    // check if this match is a whole word
                    if (wholeWord && IsMatchWholeWord(paragraphText, matches[matchIndex].Text, matches[matchIndex].Position) == false)
                    {
                        matchIndex++;
                        continue;
                    }

                    numOfMatches++;

                    int lengthBeforeReplacement = matches[matchIndex].Position - runPtr;

                    // create a new run containing text before the replacement
                    // unless the replacement is at the start of the run
                    if (lengthBeforeReplacement > 0)
                    {
                        var beforeReplaceRun = new Wordprocessing.Run();
                        var beforeReplaceRunProps = runs[i].RunProperties?.CloneNode(true);
                        if (beforeReplaceRunProps != null)
                        {
                            beforeReplaceRun.Append(beforeReplaceRunProps);
                        }
                        var beforeReplaceRunText = new Wordprocessing.Text(paragraphText.Substring(runPtr, lengthBeforeReplacement));
                        beforeReplaceRunText.Space = SpaceProcessingModeValues.Preserve;
                        beforeReplaceRun.AppendChild(beforeReplaceRunText);
                        newRuns.Add(beforeReplaceRun);

                        // move the run ptr forward the length of this run
                        runPtr += lengthBeforeReplacement;
                    }

                    // create new run containing the text from the replacement
                    var replaceRun = new Wordprocessing.Run();
                    var replaceRunProps = runs[i].RunProperties?.CloneNode(true);
                    // set custom styling properties
                    var newReplaceRunProps = (replaceRunProps != null) ?
                        OutputFileStyling.StyleRunProperties((Wordprocessing.RunProperties)replaceRunProps, replaceStyling) :
                        OutputFileStyling.StyleRunProperties(new Wordprocessing.RunProperties(), replaceStyling);
                    replaceRun.Append(newReplaceRunProps);
                    // preserve the original case of the word that is being replaced if that option is set
                    string replacement = (preserveCase) ?
                        SetMatchCase(replacePhrases[matches[matchIndex].Text], char.IsUpper(paragraphText[matches[matchIndex].Position])) :
                        replacePhrases[matches[matchIndex].Text];
                    var replaceRunText = new Wordprocessing.Text(replacement)
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    };
                    replaceRun.AppendChild(replaceRunText);
                    newRuns.Add(replaceRun);

                    // move the runPtr forward the length of the text before a replacement is made
                    // since we're not performing the replacement on the paragraphText, just creating a new run
                    runPtr += matches[matchIndex].Text.Length;

                    // move on to the next replacement match
                    matchIndex++;
                }

                // create a run for the text that comes after the last replaced run
                // if no replacement runs were made (the while loop was never entered), this will always be true
                // if a replacement run was made but extended beyond the next item in runIndices, this will be false

                // the replacement subrun exceeded the start of the next run
                if (runPtr >= runPtrs[i+1])
                {
                    continue;
                }

                // there is still text in this run that should be turned into a subrun
                int lengthBeforeNextRun = runPtrs[i + 1] - runPtr;

                // create a run with the text after the replacement
                // (or the full run text if no replacement was made
                var newRun = new Wordprocessing.Run();
                var newRunProps = runs[i].RunProperties?.CloneNode(true);
                if (newRunProps != null)
                {
                    newRun.Append(newRunProps);
                }
                var newRunText = new Wordprocessing.Text(paragraphText.Substring(runPtr, lengthBeforeNextRun));
                newRunText.Space = SpaceProcessingModeValues.Preserve;
                newRun.AppendChild(newRunText);
                newRuns.Add(newRun);

                // move the subrun pointer forward the length of this run
                runPtr += lengthBeforeNextRun;
            }


            return newRuns;
        }

        /// <summary>
        /// Performs the same processing as GenerateDocxRuns, except this combines the replacement runs with the
        /// run before them in order to cut down on the number or runs that are returned.
        /// </summary>
        /// <param name="paragraph"></param>
        /// <param name="replacePhrases"></param>
        /// <param name="matcher"></param>
        /// <param name="wholeWord"></param>
        /// <param name="preserveCase"></param>
        /// <param name="numOfMatches"></param>
        /// <returns>List of new runs beloning to the paragraph that contain the text replacements.</returns>
        public static List<Wordprocessing.Run> GenerateDocxRunsOriginalStyling(
            Wordprocessing.Paragraph paragraph,
            Dictionary<string, string> replacePhrases,
            AhoCorasickStringSearcher matcher,
            bool wholeWord,
            bool preserveCase,
            out int numOfMatches)
        {
            var newRuns = new List<Wordprocessing.Run>();
            numOfMatches = 0;

            string paragraphText = "";
            var runs = paragraph.Descendants<Wordprocessing.Run>().ToList();
            var runPtrs = new List<int>();

            // if there are no runs in the paragraph, return an empty List
            if (runs.Count() == 0)
            {
                return newRuns;
            }

            // build out the paragraph text while keeping track of where each run ends
            foreach (var run in runs)
            {
                runPtrs.Add(paragraphText.Length);
                paragraphText += run.InnerText;
            }
            // add one final pointer that marks the end of the paragraph text
            // it makes the algorithm keep working when the main for loop that
            // iterates through the runs is on its final index
            runPtrs.Add(paragraphText.Length);

            // search the paragraph text for any text that should be replaced
            var matches = matcher.Search(paragraphText).ToList();
            if (matches.Count == 0)
            {
                return runs.ToList();
            }

            // this points to the position where the next run should be made
            int runPtr = 0;
            int matchIndex = 0;

            // iterate through the runs
            for (int i = 0; i < runs.Count; i++)
            {
                // while there is a match left inside of the current run
                while (matchIndex < matches.Count && matches[matchIndex].Position < runPtrs[i + 1])
                {
                    // if we are looking for whole word matches only,
                    // check if this match is a whole word
                    if (wholeWord && IsMatchWholeWord(paragraphText, matches[matchIndex].Text, matches[matchIndex].Position) == false)
                    {
                        matchIndex++;
                        continue;
                    }

                    numOfMatches++;

                    int lengthBeforeReplacement = matches[matchIndex].Position - runPtr;

                    // create new run containing the text from the replacement
                    var replaceRun = new Wordprocessing.Run();
                    var replaceRunProps = runs[i].RunProperties?.CloneNode(true);
                    if (replaceRunProps != null)
                    {
                        replaceRun.Append(replaceRunProps);
                    }
                    string beforeReplacement = (lengthBeforeReplacement > 0) ? paragraphText.Substring(runPtr, lengthBeforeReplacement) : string.Empty;
                    // preserve the original case of the word that is being replaced if that option is set
                    string replacement = (preserveCase) ?
                        SetMatchCase(replacePhrases[matches[matchIndex].Text], char.IsUpper(paragraphText[matches[matchIndex].Position])) :
                        replacePhrases[matches[matchIndex].Text];
                    var replaceRunText = new Wordprocessing.Text(beforeReplacement + replacement)
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    };
                    replaceRun.AppendChild(replaceRunText);
                    newRuns.Add(replaceRun);

                    // move the runPtr forward the length of the text before a replacement is made
                    // since we're not performing the replacement on the paragraphText, just creating a new run
                    runPtr += lengthBeforeReplacement + matches[matchIndex].Text.Length;

                    // move on to the next replacement match
                    matchIndex++;
                }

                // create a run for the text that comes after the last replaced run
                // if no replacement runs were made (the while loop was never entered), this will always be true
                // if a replacement run was made but extended beyond the next item in runIndices, this will be false

                // the replacement subrun exceeded the start of the next run
                if (runPtr >= runPtrs[i + 1])
                {
                    continue;
                }

                // there is still text in this run that should be turned into a subrun
                int lengthBeforeNextRun = runPtrs[i + 1] - runPtr;

                // create a run with the text after the replacement
                // (or the full run text if no replacement was made
                var newRun = new Wordprocessing.Run();
                var newRunProps = runs[i].RunProperties?.CloneNode(true);
                if (newRunProps != null)
                {
                    newRun.Append(newRunProps);
                }
                var newRunText = new Wordprocessing.Text(paragraphText.Substring(runPtr, lengthBeforeNextRun));
                newRunText.Space = SpaceProcessingModeValues.Preserve;
                newRun.AppendChild(newRunText);
                newRuns.Add(newRun);

                // move the subrun pointer forward the length of this run
                runPtr += lengthBeforeNextRun;
            }


            return newRuns;
        }

        /// <summary>
        /// Generates a list of docx runs from a line of text, with custom styling on the replacements.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="replacePhrases"></param>
        /// <param name="matcher"></param>
        /// <param name="replaceStyling"></param>
        /// <param name="wholeWord"></param>
        /// <param name="preserveCase"></param>
        /// <param name="numOfMatches"></param>
        /// <returns>A list of docx runs containing the styled replacements</returns>
        public static List<Wordprocessing.Run> GenerateDocxRunsFromText(
            string text,
            Dictionary<string, string> replacePhrases,
            AhoCorasickStringSearcher matcher,
            OutputFileStyling replaceStyling,
            bool wholeWord,
            bool preserveCase,
            out int numOfMatches)
        {
            var newRuns = new List<Wordprocessing.Run>();
            numOfMatches = 0;

            // search the paragraph text for any text that should be replaced
            var matches = matcher.Search(text).ToList();

            // this points to the position where the next run should be made
            int runPtr = 0;

            // loop over the matches
            for (int i = 0; i < matches.Count; i++)
            {
                // if we are looking for whole word matches only,
                // check if this match is a whole word
                if (wholeWord && IsMatchWholeWord(text, matches[i].Text, matches[i].Position) == false)
                {
                    continue;
                }

                numOfMatches++;

                int lengthBeforeReplacement = matches[i].Position - runPtr;

                // create a new run containing text before the replacement
                // unless the replacement is at the start of the run
                if (lengthBeforeReplacement > 0)
                {
                    var beforeReplaceRun = new Wordprocessing.Run();
                    var beforeReplaceRunProperties = new Wordprocessing.RunProperties();
                    beforeReplaceRun.AppendChild(beforeReplaceRunProperties);
                    var beforeReplaceRunText = new Wordprocessing.Text(text.Substring(runPtr, lengthBeforeReplacement))
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    };
                    beforeReplaceRun.AppendChild(beforeReplaceRunText);
                    newRuns.Add(beforeReplaceRun);

                    // move the run ptr forward the length of this run
                    runPtr += lengthBeforeReplacement;
                }

                // create new run containing the text from the replacement
                var replaceRun = new Wordprocessing.Run();
                // set custom styling properties
                var replaceRunProps = OutputFileStyling.StyleRunProperties(new Wordprocessing.RunProperties(), replaceStyling);
                replaceRun.Append(replaceRunProps);
                // preserve the original case of the word that is being replaced if that option is set
                string replacement = (preserveCase) ?
                    SetMatchCase(replacePhrases[matches[i].Text], char.IsUpper(text[matches[i].Position])) :
                    replacePhrases[matches[i].Text];
                var replaceRunText = new Wordprocessing.Text(replacement)
                {
                    Space = SpaceProcessingModeValues.Preserve
                };
                replaceRun.AppendChild(replaceRunText);
                newRuns.Add(replaceRun);

                // move the runPtr forward the length of the text before a replacement is made
                // since we're not performing the replacement on the paragraphText, just creating a new run
                runPtr += matches[i].Text.Length;
            }

            int lengthUntilEndOfText = text.Length - runPtr;

            // create the last run after the replacements
            var lastRun = new Wordprocessing.Run();
            var lastRunProperties = new Wordprocessing.RunProperties();
            lastRun.AppendChild(lastRunProperties);
            var lastRunText = new Wordprocessing.Text(text.Substring(runPtr, lengthUntilEndOfText))
            {
                Space = SpaceProcessingModeValues.Preserve
            };
            lastRun.AppendChild(lastRunText);
            newRuns.Add(lastRun);

            return newRuns;
        }

        /// <summary>
        /// Checks to see if a match found by the AhoCorasickStringSearcher
        /// Search() method is a whole word.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <returns>False if the character before and after the match exists and isnt a delimiter.</returns>
        private static bool IsMatchWholeWord(string line, string match, int pos)
        {
            // yes, i know this is ugly. this can be boiled down to the following:
            //
            // match is *not* a whole word if:
            //     there is a char before the match AND it is not in the list of word delimiters
            //     OR
            //     there is a char after the match AND it is not found in the list of word delimiters
            int indexBefore = pos - 1;
            int indexAfter = pos + match.Length;
            if ((indexBefore >= 0 && WORD_DELIMITERS.Contains(line[indexBefore]) == false) ||
                (indexAfter < line.Length && WORD_DELIMITERS.Contains(line[indexAfter]) == false))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Sets the case of the first letter of a string based off of a bool
        /// </summary>
        /// <param name="match"></param>
        /// <param name="toUppercase"></param>
        /// <returns>The original string with the new case</returns>
        private static string SetMatchCase(string match, bool toUppercase)
        {
            if (match == string.Empty)
            {
                return string.Empty;
            }

            return (toUppercase) ?
                char.ToUpper(match[0]) + match.Substring(1) :
                char.ToLower(match[0]) + match.Substring(1);
        }
    }
}
