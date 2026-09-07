using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace YamanCam.Web.Infrastructure;

/// <summary>
/// decimal / double / float alanlarını her zaman <see cref="CultureInfo.InvariantCulture"/>
/// ("." ondalık ayırıcı) ile çözer.
///
/// HTML5 <c>&lt;input type="number"&gt;</c> alanları, sayfanın/tarayıcının dilinden bağımsız
/// olarak değeri her zaman nokta ile gönderir (örn. kullanıcı "10,0000" görse bile form'a
/// "10.0000" gider). Sunucu kültürü tr-TR olduğundan varsayılan model binding bu değeri
/// "." grup ayırıcı sanıp "10.0000" -> 100000 gibi çözüyordu ve fatura/fiş tutarları hatalı
/// hesaplanıyordu. Bu binder önce InvariantCulture, olmazsa CurrentCulture dener.
/// </summary>
public sealed class InvariantNumericModelBinder : IModelBinder
{
    private const NumberStyles DecimalStyles =
        NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite |
        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    private const NumberStyles FloatStyles = DecimalStyles | NumberStyles.AllowExponent;

    private readonly Type _numberType;

    public InvariantNumericModelBinder(Type numberType) => _numberType = numberType;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var rawValue = valueProviderResult.FirstValue;
        var isNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) is not null;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            // Nullable -> null; non-nullable -> 0 (varsayılan). Sıfır/negatif kontrolleri
            // controller'ların Validate metodlarında Türkçe mesajlarla ayrıca yapılır.
            bindingContext.Result = isNullable
                ? ModelBindingResult.Success(null)
                : ModelBindingResult.Success(Activator.CreateInstance(_numberType));
            return Task.CompletedTask;
        }

        var value = rawValue.Trim();

        if (TryParse(value, out var parsed))
        {
            bindingContext.Result = ModelBindingResult.Success(parsed);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                modelName,
                $"'{rawValue}' geçerli bir sayı değil.");
        }

        return Task.CompletedTask;
    }

    private bool TryParse(string value, out object? result)
    {
        if (_numberType == typeof(decimal))
        {
            if (decimal.TryParse(value, DecimalStyles, CultureInfo.InvariantCulture, out var d) ||
                decimal.TryParse(value, DecimalStyles, CultureInfo.CurrentCulture, out d))
            {
                result = d;
                return true;
            }
        }
        else if (_numberType == typeof(double))
        {
            if (double.TryParse(value, FloatStyles, CultureInfo.InvariantCulture, out var d) ||
                double.TryParse(value, FloatStyles, CultureInfo.CurrentCulture, out d))
            {
                result = d;
                return true;
            }
        }
        else if (_numberType == typeof(float))
        {
            if (float.TryParse(value, FloatStyles, CultureInfo.InvariantCulture, out var f) ||
                float.TryParse(value, FloatStyles, CultureInfo.CurrentCulture, out f))
            {
                result = f;
                return true;
            }
        }

        result = null;
        return false;
    }
}

/// <summary>
/// decimal/double/float (ve nullable karşılıkları) için <see cref="InvariantNumericModelBinder"/>
/// sağlar. Program.cs içinde <c>ModelBinderProviders</c> listesinin başına eklenir.
/// </summary>
public sealed class InvariantNumericModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var modelType = context.Metadata.UnderlyingOrModelType;
        if (modelType == typeof(decimal) || modelType == typeof(double) || modelType == typeof(float))
        {
            return new InvariantNumericModelBinder(modelType);
        }

        return null;
    }
}
