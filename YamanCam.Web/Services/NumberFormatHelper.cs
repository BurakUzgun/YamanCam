namespace YamanCam.Web.Services;

/// <summary>
/// Ayarlar ekranindaki (App_Setting, Grup=Küsürat) ondalik basamak degerlerini
/// Razor view'larda ve JS'e aktarilacak adimlarda (step) kullanilabilir hale getirir.
/// </summary>
public static class NumberFormatHelper
{
    /// <summary>
    /// Küsürat ayarları artık serbest metin olarak girilebildiğinden (bkz. App_Setting.Value),
    /// dönen hane sayısı veritabanındaki azami ondalık hassasiyetle (10 hane) sınırlanır.
    /// Bu sayede Math.Round(deger, hane) gibi çağrılar geçersiz/aşırı bir değerle patlamaz.
    /// </summary>
    public static int Get(IReadOnlyDictionary<string, int>? map, string explanation, int defaultValue = 2)
    {
        var value = map is not null && map.TryGetValue(explanation, out var found) ? found : defaultValue;
        return Math.Clamp(value, 0, 10);
    }

    public static string Format(int decimals) => "N" + Math.Max(0, decimals);

    public static string Step(int decimals)
    {
        if (decimals <= 0)
        {
            return "1";
        }

        return "0." + new string('0', decimals - 1) + "1";
    }
}
