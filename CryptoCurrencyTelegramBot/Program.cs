namespace CryptoCurrencyTelegramBot;
//TODO:Добавить боту большее количество валют.
///TODO:Добавить боту новую команду, прислать информацию о рыночной капитализации (market_cap) выбранной валюты. Документация по работе https://coinmarketcap.com/api/documentation/v1/#operation/getV1CryptocurrencyListingsHistorical.
public static class Program
{
    private static void Main()
    {
        var currencyBot = new CurrencyBot(ApiConstants.BOT_API);
        currencyBot.CreateCommands();
        currencyBot.StartReceiving();

        Console.ReadKey();
    }
}