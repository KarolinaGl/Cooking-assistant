using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CookingAssistant.Tests
{
    public class SupplyWindowTests
    {
        [Theory]
        [InlineData("add", "", "", new string[] { "Make sure to fill in all of the textboxes before you add an ingredient", "Add ingredient" })]
        [InlineData("add", "ingredientName", "", new string[] { "Make sure to fill in all of the textboxes before you add an ingredient", "Add ingredient" })]
        [InlineData("add", "", "123", new string[] { "Make sure to fill in all of the textboxes before you add an ingredient", "Add ingredient" })]
        [InlineData("add", "ingredientName", "123", new string[] { null, null })]
        [InlineData("update", "", "", new string[] { "Make sure to fill in all of the textboxes before you update an ingredient", "Update ingredient" })]
        [InlineData("update", "ingredientName", "", new string[] { "Make sure to fill in all of the textboxes before you update an ingredient", "Update ingredient" })]
        [InlineData("update", "", "123", new string[] { "Make sure to fill in all of the textboxes before you update an ingredient", "Update ingredient" })]
        [InlineData("update", "ingredientName", "123", new string[] { null, null })]
        [InlineData("", "", "", new string[] { null, null })]
        [InlineData("", "ingredientName", "", new string[] { null, null })]
        [InlineData("", "", "123", new string[] { null, null })]
        [InlineData("", "ingredientName", "123", new string[] { null, null })]
        [InlineData("add", "ingredientName", "abc", new string[] { "Amount must be a number", "Add ingredient" })]
        [InlineData("update", "ingredientName", "abc", new string[] { "Amount must be a number", "Update ingredient" })]
        public void ValidateText_Test(string buttonType, string ingredientName, string ingredientAmount, string[] expectedResult)
        {
            string[] validationOutput = SupplyWindow.ValidateText(buttonType, ingredientName, ingredientAmount);

            Assert.Equal(expectedResult, validationOutput);
        }
    }
}
