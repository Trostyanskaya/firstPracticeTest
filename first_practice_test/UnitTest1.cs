using System.Diagnostics.CodeAnalysis;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace first_practice_test;
//1. структура теста: есть сетап, тирдаун, выведен отдельно метод авторизации
//2. вынесены в отдельные методы повторяющиеся куски кода
//3. использованы переходы по урлу если это излишне для уникальной проверки
//4. в ассертах написаны сообщения об ошибках и они понятны
//5. норм названия тестов, человекочитаемые и понятные проверяющему
//6. использовать уникальные локаторы для контролов если это было возможно
//7. использованы ожидания, явные/неявные
public class Tests
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
    public void AuthorizationTest() // тест входа
    {
        Authorize();
        Assert.That(driver.Title, Does.Contain("Новости"),
            "На странице новостей должен отображаться заголовок, содержащий текст 'Новости'"); // проверка что открыта нужная страница
    }

    [Test]
    public void NavigationMenuElementTest() // тест перехода в меню "Сообщества" через бургерМеню
    {
        Authorize();
        ClickReadyButton(By.CssSelector("[data-tid='SidebarMenuButton']"), "Меню");
        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='SidePage__container']"))); // ожидание появления боковой панели
        ClickReadyButton(By.CssSelector("[data-tid='SidePageBody'] [data-tid='Community']"), "Сообщества");
        wait.Until(ExpectedConditions.UrlToBe($"{BaseUrl}/communities"));
        var titlePageElement = driver.FindElement(By.CssSelector("[data-tid='Title']"));
        Assert.That(titlePageElement.Text, Does.Contain("Сообщества"), "После перехода в раздел 'Сообщества' заголовок страницы должен содержать текст 'Сообщества'"); // проверка заголовка
    }

    [Test]
    public void ExitTest() // тест выхода
    {
        Authorize();

        ClickReadyButton(By.CssSelector("[data-tid='SidebarMenuButton']"), "Меню");
        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='SidePage__container']")));
        ClickReadyButton(By.CssSelector("[data-tid='LogoutButton']"), "Выйти");
        var titlePageElement = driver.FindElement(By.CssSelector("[class='login-page']"));
        Assert.That(titlePageElement.Text, Does.Contain("Вы вышли из учетной записи"), "После выхода заголовок страницы должен содержать текст 'Вы вышли из учетной записи'"); // проверка заголовка
    }
    [Test]
    public void AddCommentTest() // тест добавления комментария
    {
        Authorize();
        driver.Navigate().GoToUrl($"{BaseUrl}/comments");
        ClickReadyButton(By.CssSelector("[data-tid='AddComment']"), "Кнопка 'Добавить комментарий'");
        var commentInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[data-tid='CommentInput']")));
        commentInput.SendKeys("comment");
        ClickReadyButton(By.CssSelector("[data-tid='SendComment']"), "Кнопка 'Отправить'");

        wait.Until(ExpectedConditions.TextToBePresentInElementLocated(By.CssSelector("[data-tid='CommentItem']"), "comment"));
        var commentItem = driver.FindElement(By.CssSelector("[data-tid='CommentItem']"));
        Assert.That(commentItem.Text, Does.Contain("comment"), "Страница должна содержать комментарий 'comment'");
    }

    [Test]
    public void CreateEventTest() // тест создания сообщества
    {
        Authorize();
        driver.Navigate().GoToUrl($"{BaseUrl}/communities");

        var addCommunity = wait.Until(
            ExpectedConditions.ElementToBeClickable(
                By.XPath("//section[@data-tid='PageHeader']//button[contains(text(), 'СОЗДАТЬ')]")
            )
        );
        Assert.IsTrue(addCommunity.Displayed, "Кнопка не отображается");
        Assert.IsTrue(addCommunity.Enabled, "Кнопка неактивна");
        StringAssert.Contains("СОЗДАТЬ", addCommunity.Text, "Текст кнопки неверный");
        addCommunity.Click();

        SendKeysToElement(By.CssSelector("[data-tid='Name']"), "community", "Название сообщества");
        ClickReadyButton(By.CssSelector("[data-tid='CreateButton']"), "Кнопка 'Создать сообщество'");
        ClickReadyButton(By.XPath("//button[contains(text(), 'Закрыть')]"), "Закрыть настройки");

        var titlePageElement = driver.FindElement(By.CssSelector("[data-tid='Title']"));
        Assert.That(titlePageElement.Text, Does.Contain("community"), "Страница должна содержать наше название 'community'");
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