using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptoCurrencyTelegramBot;

public class CurrencyBot
{
    private readonly TelegramBotClient _telegramBotClient;

    private readonly List<string> _currencyCodes = new()
    {
        CurrencyCode.BTC, CurrencyCode.BNB, CurrencyCode.ETH, CurrencyCode.DOGE
    };

    public CurrencyBot(string token)
    {
        _telegramBotClient = new TelegramBotClient(token);
    }

    public void CreateCommands()
    {
        _telegramBotClient.SetMyCommandsAsync(new List<BotCommand>()
        {
            new()
            {
                Command = CustomBotCommands.START,
                Description = "Запуск бота."
            },
            new()
            {
                Command = CustomBotCommands.SHOW_CURRENCIES,
                Description = "Вывод сообщения с выбором 1 из 4 валют, для получения ее цены в данный момент"
            }
        });
    }

    public void StartReceiving()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new UpdateType[]
            {
                UpdateType.Message,
                UpdateType.CallbackQuery
            }
        };
        // Начинаем отслеживание сообщений от пользователя
        _telegramBotClient.StartReceiving(HandleUpdateAsync, HandleError, receiverOptions, cancellationToken);
    }

    /// <summary>
    /// The method handles bot errors while tracking messages from the user 
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="exception">Error Type</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(exception);
        return Task.CompletedTask;
    }

    /// <summary>
    /// The method handles events that occur with the bot.
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update">Information about the event that took place</param>
    /// <param name="cancellationToken"></param>
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                await HandleMessageAsync(update, cancellationToken);
                break;
            case UpdateType.CallbackQuery:
                await HandleCallbackQueryAsync(update, cancellationToken);
                break;
        }
    }

    private async Task HandleMessageAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message == null)
        {
            return;
        }

        var chatId = update.Message.Chat.Id;
        await DeleteMessage(chatId, update.Message.MessageId, cancellationToken);
        if (update.Message.Text == null)
        {
            await _telegramBotClient.SendTextMessageAsync(chatId: chatId, text: "Бот принимает только команды из меню.",
                cancellationToken: cancellationToken);
            return;
        }

        var messageText = update.Message.Text;
        if (IsStartCommand(messageText))
        {
            await SendStartMessageAsync(chatId, cancellationToken);
            return;
        }

        if (IsShowCommand(messageText))
        {
            await ShowCurrencySelectionAsync(chatId, cancellationToken);
        }
    }

    private async Task DeleteMessage(long chatId, int messageId, CancellationToken cancellationToken)
    {
        // Используем try-catch в случае, если пользователь сам удалил сообщение, раньше бота.
        try
        {
            await _telegramBotClient.DeleteMessageAsync(chatId, messageId, cancellationToken);
        }
        catch (ApiRequestException exception)
        {
            // В случае ошибки с кодом 400 (Сообщение удалено), выводим сообщение в консоль.
            if (exception.ErrorCode == 400)
            {
                Console.WriteLine("User deleted message");
            }
        }
    }

    private bool IsStartCommand(string messageText)
    {
        return messageText.ToLower() == CustomBotCommands.START;
    }

    private bool IsShowCommand(string messageText)
    {
        return messageText.ToLower() == CustomBotCommands.SHOW_CURRENCIES;
    }

    private async Task SendStartMessageAsync(long? chatId, CancellationToken cancellationToken)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Выбрать Валюту.", CustomCallbackData.SHOW_CURRENCIES_MENU)
            }
        });
        await _telegramBotClient.SendTextMessageAsync(chatId,
            "Привет!\n" + "Данный бот показывает текущий курс выбранной валюты.\n",
            replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
    }

    private async Task ShowCurrencySelectionAsync(long chatId, CancellationToken cancellationToken)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            //Строка 1
            new[]
            {
                //BTC
                InlineKeyboardButton.WithCallbackData("Bitcoin", CurrencyCode.BTC),
                //ETH
                InlineKeyboardButton.WithCallbackData("Ethereum", CurrencyCode.ETH)
            },
            //Строка 2
            new[]
            {
                //BNB
                InlineKeyboardButton.WithCallbackData("BNB", CurrencyCode.BNB),
                //DogeCoin
                InlineKeyboardButton.WithCallbackData("DogeCoin", CurrencyCode.DOGE)
            }
        });
        await _telegramBotClient.SendTextMessageAsync(chatId: chatId, text: "Выберите валюту:",
            replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
    }

    /// <summary>
    ///  Метод обрабатывает все коллбэки нажатия на инлай кнопки пользователем
    ///  И запускает выполнение нужного метода в зависимости от полученного сообщения
    /// </summary>
    /// <param name="update"> Информация о произошедшем событии </param>
    private async Task HandleCallbackQueryAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.CallbackQuery?.Message == null)
        {
            return;
        }

        var chatId = update.CallbackQuery.Message.Chat.Id;
        var callbackData = update.CallbackQuery.Data;
        var messageId = update.CallbackQuery.Message.MessageId;

        // Проверяем пользователь нажал на инлайн кнопку в ответ на стартовое сообщение
        if (callbackData == CustomCallbackData.SHOW_CURRENCIES_MENU)
        {
            await DeleteMessage(chatId, messageId, cancellationToken);
            await ShowCurrencySelectionAsync(chatId, cancellationToken);
            return;
        }

        // Проверяем коллбэк - это код валюты
        if (_currencyCodes.Contains(callbackData))
        {
            await DeleteMessage(chatId, messageId, cancellationToken);
            await SendCurrencyPriceAsync(chatId, callbackData, cancellationToken);
            return;
        }

        // Проверяем пользователь нажал на инлайн кнопку сменить валюту
        if (callbackData == CustomCallbackData.RETURN_TO_CURRENCIES_MENU)
        {
            await ShowCurrencySelectionAsync(chatId, cancellationToken);
        }
    }

    private async Task SendCurrencyPriceAsync(long chatId, string currencyCode, CancellationToken cancellationToken)
    {
        var price = await CoinMarket.GetPriceAsync(currencyCode);
        // Создаем инлайн кнопку с коллбэком для смены валюты
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Выбрать другую валюту.",
                    CustomCallbackData.RETURN_TO_CURRENCIES_MENU)
            }
        });
        // Отправляем сообщение с инлайн кнопкой
        await _telegramBotClient.SendTextMessageAsync(chatId,
            text: $"Валюта: {currencyCode}, стоимость: {Math.Round(price, 3)}$",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }
}