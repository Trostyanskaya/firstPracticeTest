using System.Diagnostics.CodeAnalysis;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace first_practice_test;

public class EventTest
{
    public WebDriver driver;
    public WebDriverWait wait;

    private const string BaseUrl = "https://staff-testing.testkontur.ru";
    private const string ValidUsername = "trostyanskaya.e.m@gymn1sam.ru";
    private const string ValidPassword = "12341234Liza!";

    [SetUp]
    public void Setup()
    {
        driver = new ChromeDriver();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
    }
    [TearDown]
    public void TearDown()
    {
        driver.Quit();
        driver.Dispose();
    }
    private void Authorize()
    {
        driver.Navigate().GoToUrl($"{BaseUrl}/");
        SendKeysToElement(By.Id("Username"), ValidUsername, "Логин");
        SendKeysToElement(By.Id("Password"), ValidPassword, "Пароль");
        ClickReadyButton(By.Name("button"), "Кнопка авторизации");
        wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[data-tid='Title']")));
    }
    [Test]
    public void EventCommentsTest() // тест не более 100 комментов
    {
        Authorize();
        driver.Navigate().GoToUrl($"{BaseUrl}/events/a9684773-f78f-42f5-92f8-a2970a0b00fb?tab=discussions&id=959fc812-41f8-4d53-a5e4-11847d3a4625");

        for (int i = 1; i <= 102; i++)
        {
            ClickReadyButton(By.CssSelector("[data-tid='AddComment']"), "Кнопка 'Комментировать...'");
            SendKeysToElement(By.CssSelector("[placeholder='Комментировать...']"), $"Комментарий №{i}", "comments");
            ClickReadyButton(By.CssSelector("[data-tid='SendComment']"), "Отправить");

            if (i > 100)
            {
                // Ищем комментарий с номером 101 на странице
                var comments = driver.FindElements(By.XPath($"//div[@data-tid='TextComment' and contains(text(), 'Комментарий №101')]"));

                if (comments.Count > 0)
                {
                    Assert.Fail($"ОШИБКА: Комментарий №101 отобразился на странице! Система позволила отправить больше 100 комментариев.");
                }
            }
        }
    }
    private void ClickReadyButton(By locator, string elementName = "Элемент")
    {
        var element = wait.Until(ExpectedConditions.ElementToBeClickable(locator));
        Assert.IsTrue(element.Displayed, $"{elementName} не отображается");
        Assert.IsTrue(element.Enabled, $"{elementName} неактивна");
        element.Click();
    }
    private void SendKeysToElement(By locator, string text, string elementName = "Поле ввода")
    {
        var element = wait.Until(ExpectedConditions.ElementToBeClickable(locator));
        element.Clear();
        element.SendKeys(text);
    }
}