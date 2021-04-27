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
        public void ValidateText_Test(string ingredientName, string ingredientAmount, string errorMessage)
        {
            string error = RecipeCRUDWindow.ValidateText(ingredientName, ingredientAmount);

            Assert.Equal(errorMessage, error);
        }
    }
}
