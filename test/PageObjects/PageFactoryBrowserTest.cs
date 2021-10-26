using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SeleniumExtras.Environment;
using System;
using System.Collections.Generic;

namespace SeleniumExtras.PageObjects
{
    [TestFixture]
    public class PageFactoryBrowserTest : DriverTestFixture
    {
        //TODO: Move these to a standalone class when more tests rely on the server being up
        [OneTimeSetUp]
        public void RunBeforeAnyTest()
        {
            EnvironmentManager.Instance.WebServer.Start();
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            EnvironmentManager.Instance.CloseCurrentDriver();
            EnvironmentManager.Instance.WebServer.Stop();
        }

        [Test]
        public void LooksUpAgainAfterPageNavigation()
        {
            driver.Url = xhtmlTestPage;
            var page = new Page();

            PageFactory.InitElements(driver, page);

            driver.Navigate().Refresh();

            Assert.That(page.formElement.Displayed, Is.True);
        }

        [Test]
        public void ElementEqualityWorks()
        {
            driver.Url = xhtmlTestPage;
            var page = new PageFactoryTest.Page();

            PageFactory.InitElements(driver, page);

            var expectedElement = driver.FindElement(By.Name("someForm"));

            Assert.That(page.formElement, Is.EqualTo(expectedElement));
            Assert.That(expectedElement, Is.EqualTo(page.formElement));
            Assert.That(page.formElement.GetHashCode(), Is.EqualTo(expectedElement.GetHashCode()));
        }

        [Test]
        public void ElementIsILocatable()
        {
            driver.Url = xhtmlTestPage;
            var page = new PageFactoryTest.Page();

            PageFactory.InitElements(driver, page);

            var expectedElement = (ILocatable)driver.FindElement(By.Name("someForm"));

            var iLocatableElement = page.formElement as ILocatable;
            Assert.That(iLocatableElement, Is.Not.Null);
            Assert.That(iLocatableElement.Coordinates.LocationInViewport,
                Is.EqualTo(expectedElement.Coordinates.LocationInViewport));
        }

        [Test]
        public void UsesElementAsScriptArgument()
        {
            driver.Url = xhtmlTestPage;
            var page = new PageFactoryTest.Page();

            PageFactory.InitElements(driver, page);

            var tagName = (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].tagName", page.formElement);

            Assert.That(tagName.ToLower(), Is.EqualTo("form"));
        }

        [Test]
        public void ShouldAllowPageFactoryElementToBeUsedInInteractions()
        {
            driver.Url = javascriptPage;
            var page = new PageFactoryBrowserTest.HoverPage();
            PageFactory.InitElements(driver, page);

            Actions actions = new Actions(driver);
            actions.MoveToElement(page.MenuLink).Perform();

            IWebElement item = driver.FindElement(By.Id("item1"));
            Assert.That(item.Text, Is.EqualTo("Item 1"));
        }

        [Test]
        public void ShouldFindMultipleElements()
        {
            driver.Url = xhtmlTestPage;
            var page = new PageFactoryBrowserTest.LinksPage();
            PageFactory.InitElements(driver, page);
            Assert.That(page.AllLinks.Count, Is.EqualTo(12));
            Assert.That(page.AllLinks[0].Text.Trim(), Is.EqualTo("Open new window"));
        }

        [Test]
        public void ShouldFindElementUsingSequence()
        {
            driver.Url = xhtmlTestPage;
            var page = new PageFactoryBrowserTest.Page();
            PageFactory.InitElements(driver, page);
            Assert.That(page.NestedElement.Text.Trim(), Is.EqualTo("I'm a child"));
        }

        [Test]
        public void ShouldFindElementUsingAllFindBys()
        {
            driver.Url = xhtmlTestPage;
            var page = new PageFactoryBrowserTest.Page();
            PageFactory.InitElements(driver, page);
            Assert.That(page.ByAllElement.Displayed, Is.True);
        }

        [Test]
        public void MixingFindBySequenceAndFindByAllShouldThrow()
        {
            driver.Url = xhtmlTestPage;
            var page = new PageFactoryBrowserTest.InvalidAttributeCombinationPage();
            Assert.Throws<ArgumentException>(() => PageFactory.InitElements(driver, page), "Cannot specify FindsBySequence and FindsByAll on the same member");
        }

        [Test]
        public void FrameTest()
        {
            driver.Url = iframePage;
            var page = new PageFactoryBrowserTest.IFramePage();
            PageFactory.InitElements(driver, page);
            driver.SwitchTo().Frame(page.Frame);
        }

        [Test]
        public void AddMultipleFindsByAttributesShouldSearchByAnyOfThem()
        {
            driver.Url = xhtmlTestPage;
            var page = new Page();
            PageFactory.InitElements(driver, page);
            Assert.That(page.MultipleElement.GetAttribute("id"), Is.EqualTo("id1"));
        }

        #region Page classes for tests

        private class Page
        {
            [FindsBy(How = How.Name, Using = "someForm")]
            public IWebElement formElement;

            [FindsBySequence]
            [FindsBy(How = How.Id, Using = "parent", Priority = 0)]
            [FindsBy(How = How.Id, Using = "child", Priority = 1)]
            public IWebElement NestedElement;

            [FindsByAll]
            [FindsBy(How = How.TagName, Using = "form", Priority = 0)]
            [FindsBy(How = How.Name, Using = "someForm", Priority = 1)]
            public IWebElement ByAllElement;

            [FindsBy(How = How.Id, Using = "id-not-here")]
            [FindsBy(How = How.Id, Using = "id1")]
            [FindsBy(How = How.Id, Using = "id-not-here-either")]
            public IWebElement MultipleElement;
        }

        private class HoverPage
        {
            [FindsBy(How = How.Id, Using = "menu1")]
            public IWebElement MenuLink;
        }

        private class LinksPage
        {
            [FindsBy(How = How.TagName, Using = "a")]
            public IList<IWebElement> AllLinks;
        }

        private class InvalidAttributeCombinationPage
        {
            [FindsByAll]
            [FindsBySequence]
            [FindsBy(How = How.Id, Using = "parent", Priority = 0)]
            [FindsBy(How = How.Id, Using = "child", Priority = 1)]
            public IWebElement NotFound;
        }

        private class IFramePage
        {
            [FindsBy(How = How.Id, Using = "iframe1")]
            public IWebElement Frame;
        }

        #endregion
    }
}
