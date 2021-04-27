using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CookingAssistant;
using Xunit;

namespace CookingAssistant.Tests
{
    public class RecipeCRUDWindowTests
    {
        [Theory]
        [InlineData("","", "Make sure to fill in all of the textboxes before you add an ingredient")]
        [InlineData("test","", "Make sure to fill in all of the textboxes before you add an ingredient")]
        [InlineData("", "test", "Make sure to fill in all of the textboxes before you add an ingredient")]
        [InlineData("", "123", "Make sure to fill in all of the textboxes before you add an ingredient")]
        [InlineData("test", "test", "Amount must be a number")]
        [InlineData("test", "123", "")]
        [InlineData("test", "123.4", "")]
        public void ValidateText_Test(string ingredientName, string ingredientAmount, string errorMessage)
        {
            string validationOutput = RecipeCRUDWindow.ValidateText(ingredientName, ingredientAmount);

            Assert.Equal(errorMessage, validationOutput);
        }
    }
}
