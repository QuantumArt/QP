﻿using OpenQA.Selenium;
using SeleniumExtension.Support.PageObjects;

namespace QP8.PageObjects.Pages
{
    public class BasePage : Page
    {
        public BasePage(IWebDriver webDriver) 
            : base(webDriver)
        {
        }
    }
}
