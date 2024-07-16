﻿using TextReplace.MVVM.ViewModel;
using TextReplace.MVVM.Model;

namespace TextReplace.Tests.ViewModels
{
    public class ReplaceTests
    {
        [Fact]
        public void AddNewPhrase_AddPhrasesAtTop_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            // Act
            vm.AddNewPhrase("a", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.AddNewPhrase("c", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Top);

            // Assert
            Assert.Equal("c", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("a", vm.ReplacePhrases[2].Item1);
        }

        [Fact]
        public void AddNewPhrase_AddPhrasesAtBottom_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            // Act
            vm.AddNewPhrase("a", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Assert
            Assert.Equal("a", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);
        }

        [Fact]
        public void AddNewPhrase_AddPhrasesAboveSelected_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            // Act
            vm.AddNewPhrase("a", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.AboveSelection);
            vm.AddNewPhrase("c", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.AboveSelection);

            // Assert
            Assert.Equal("c", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("a", vm.ReplacePhrases[2].Item1);
        }

        [Fact]
        public void AddNewPhrase_AddPhrasesBelowSelected_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            // Act
            vm.AddNewPhrase("a", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.BelowSelection);
            vm.AddNewPhrase("c", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.BelowSelection);

            // Assert
            Assert.Equal("a", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);
        }

        [Fact]
        public void AddNewPhrase_AddDuplicateItem1_ReturnFalse()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;
            var duplicateItem1 = "Item1";

            // Act
            vm.AddNewPhrase(duplicateItem1, "this is fine", Core.Enums.InsertReplacePhraseAtEnum.Top);
            var actual = vm.AddNewPhrase(duplicateItem1, "this should return false", Core.Enums.InsertReplacePhraseAtEnum.Top);

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void EditSelectedPhrase_ChangeItem1AndItem2_SelectedPhraseUpdated()
        {
            // Arrange
            var expectedItem1 = "new Item1";
            var expectedItem2 = "new Item2";

            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase("original Item1", "original Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);

            // Act
            vm.EditSelectedPhrase(expectedItem1, expectedItem2);

            // Assert
            Assert.Equal(expectedItem1, vm.ReplacePhrases[0].Item1);
            Assert.Equal(expectedItem1, vm.SelectedPhrase.Item1);
            Assert.Equal(expectedItem2, vm.ReplacePhrases[0].Item2);
            Assert.Equal(expectedItem2, vm.SelectedPhrase.Item2);
        }

        [Fact]
        public void EditSelectedPhrase_ChangeOnlyItem2_SelectedPhraseUpdated()
        {
            // Arrange
            var expected = "new Item2";

            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase("Item1", "original Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);

            // Act
            vm.EditSelectedPhrase("Item1", expected);

            // Assert
            Assert.Equal(expected, vm.ReplacePhrases[0].Item2);
            Assert.Equal(expected, vm.SelectedPhrase.Item2);
        }

        [Fact]
        public void EditSelectedPhrase_EmptySelectedPhrase_ReturnFalse()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.SelectedPhrase = new();

            // Act
            var actual = vm.EditSelectedPhrase("aaa", "bbb");

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void RemoveSelectedPhrase_ValidSelectedPhrase_SelectedPhraseRemoved()
        {
            // Arrange
            var expectedItem1 = "expected Item1";

            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase(expectedItem1, "expected Item2", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("non-expected Item1", "non-expected Item2", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.RemoveSelectedPhrase();

            // Assert
            Assert.Single(vm.ReplacePhrases);
            Assert.Equal(expectedItem1, vm.ReplacePhrases[0].Item1);
        }

        [Fact]
        public void RemoveSelectedPhrase_EmptySelectedPhrase_ReturnFalse()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.SelectedPhrase = new();

            // Act
            var actual = vm.RemoveSelectedPhrase();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void RemoveSelectedPhrase_NonexistentSelectedPhrase_ReturnFalse()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);
            vm.SelectedPhrase = new("This phrase doesnt exist", "");

            // Act
            var actual = vm.RemoveSelectedPhrase();

            // Assert
            Assert.False(actual);
        }

        [Fact]
        public void RemoveAllPhrases_AtLeastOnePhrase_PhrasesAreEmpty()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase("Item1", "Item2", Core.Enums.InsertReplacePhraseAtEnum.Top);

            // Act
            ReplaceViewModel.RemoveAllPhrases();

            // Assert
            Assert.Empty(vm.ReplacePhrases);
        }

        [Fact]
        public void RemoveAllPhrases_NoPhrases_PhrasesAreEmpty()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();

            // Act
            ReplaceViewModel.RemoveAllPhrases();

            // Assert
            Assert.Empty(vm.ReplacePhrases);
        }

        [Fact]
        public void MoveReplacePhrase_LastToFirst_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("a", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("b", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.MoveReplacePhrase(oldIndex: 2, newIndex: 0);

            // Assert
            Assert.Equal("c", vm.ReplacePhrases[0].Item1);
            Assert.Equal("a", vm.ReplacePhrases[1].Item1);
            Assert.Equal("b", vm.ReplacePhrases[2].Item1);
        }

        [Fact]
        public void MoveReplacePhrase_FirstToMiddle_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("a", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("b", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.MoveReplacePhrase(oldIndex: 0, newIndex: 2);

            // Assert
            Assert.Equal("b", vm.ReplacePhrases[0].Item1);
            Assert.Equal("a", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);
        }

        [Fact]
        public void MoveReplacePhrase_FirstToFirst_PhrasesInCorrectOrder()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("a", "this should be first", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("b", "this should be second", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "this should be third", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.MoveReplacePhrase(oldIndex: 0, newIndex: 0);

            // Assert
            Assert.Equal("a", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);
        }

        [Fact]
        public void ToggleSort_IsToggled_PhrasesAreSorted()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = false;

            vm.AddNewPhrase("b", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("a", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("d", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.ToggleSortCommand.Execute(null);

            // Assert
            Assert.Equal("a", vm.ReplacePhrases[0].Item1);
            Assert.Equal("b", vm.ReplacePhrases[1].Item1);
            Assert.Equal("c", vm.ReplacePhrases[2].Item1);
            Assert.Equal("d", vm.ReplacePhrases[3].Item1);
        }

        [Fact]
        public void ToggleSort_IsToggled_PhrasesAreUnsorted()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();
            ReplaceData.IsSorted = true;

            vm.AddNewPhrase("b", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("a", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("d", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);
            vm.AddNewPhrase("c", "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.ToggleSortCommand.Execute(null);

            // Assert
            Assert.Equal("b", vm.ReplacePhrases[0].Item1);
            Assert.Equal("a", vm.ReplacePhrases[1].Item1);
            Assert.Equal("d", vm.ReplacePhrases[2].Item1);
            Assert.Equal("c", vm.ReplacePhrases[3].Item1);
        }

        [Fact]
        public void SetSelectedPhrase_ValidPhrase_PhraseIsSetAsSelectedPhrase()
        {
            // Arrange
            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();

            // Act
            vm.SetSelectedPhraseCommand.Execute(new ReplacePhraseWrapper("Item1", "Item2"));

            // Assert
            Assert.Equal("Item1", vm.SelectedPhrase.Item1);
        }

        [Fact]
        public void SetSelectedPhrase_InvalidPhrase_SelectedPhraseShouldNotChange()
        {
            // Arrange
            var expected = "original selected phrase Item1";

            var vm = new ReplaceViewModel();
            ReplaceViewModel.RemoveAllPhrases();

            vm.AddNewPhrase(expected, "", Core.Enums.InsertReplacePhraseAtEnum.Bottom);

            // Act
            vm.SetSelectedPhraseCommand.Execute(null);

            // Assert
            Assert.Equal(expected, vm.SelectedPhrase.Item1);
        }
    }
}
