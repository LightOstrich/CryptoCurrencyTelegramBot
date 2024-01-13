using System.Text.Json.Nodes;

namespace CryptoCurrencyTelegramBot;

/// <summary>
/// Класс для работы с CoinMarketApi
/// </summary>
public static class CoinMarket
{
    private static readonly string API_KEY = ApiConstants.COIN_MARKET_API;

    /// <summary>
    /// Метод для получения прайса выбранной валюты
    /// </summary>
    /// <param name="currencyCode"> Код выбранной валюты </param>
    /// <returns> Цену валюты </returns>
    public static async Task<decimal> GetPriceAsync(string currencyCode)
    {
        // Создаем конструкцию using, чтобы по окончанию метода, освободить память
        // Создаем новый объект класса HttpClient, чтобы отправить запрос на сайт coinmarketcap.com
        using (var httpClient = new HttpClient())
        {
            // Добавляем заголовок с апи ключем, чтобы сервер не отклонил запрос
            httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", API_KEY);
            // Делаем запрос выбранной валюты
            var response = await httpClient.GetAsync(
                $"https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol={currencyCode}&convert=USD");
            // Переводим полученный ответ в тип данных string
            var responseString = await response.Content.ReadAsStringAsync();
            // Распарсим запрос в формат JSON
            var jsonResponse = JsonNode.Parse(responseString);
            var price = (decimal)jsonResponse["data"][currencyCode]["quote"]["USD"]["price"];
            return price;
        }
    }
}