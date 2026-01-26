using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace HotelManagementSystem.ModelBinders
{
    public class DateTimeModelBinder : IModelBinder
    {
        private readonly string[] _dateFormats = new[] { "dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            DateTime dateTime;
            if (DateTime.TryParseExact(value, _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                bindingContext.Result = ModelBindingResult.Success(dateTime);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(modelName, $"Invalid date format. Please use dd-MM-yyyy format.");
            }

            return Task.CompletedTask;
        }
    }
}
