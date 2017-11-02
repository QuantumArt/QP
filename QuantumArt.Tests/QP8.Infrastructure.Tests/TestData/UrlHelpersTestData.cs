using System.Collections.Generic;

namespace QP8.Infrastructure.Tests.TestData
{
    public class UrlHelpersTestData
    {
        public static IEnumerable<object[]> GetValidAbsoluteUrls
        {
            get
            {
                yield return new object[] { "http://foo.com/blah_blah" };
                yield return new object[] { "http://foo.com/blah_blah/" };
                yield return new object[] { "http://foo.com/blah_blah_(wikipedia)" };
                yield return new object[] { "http://foo.com/blah_blah_(wikipedia)_(again)" };
                yield return new object[] { "http://www.example.com/wpstyle/?p=364" };
                yield return new object[] { "https://www.example.com/foo/?bar=baz&inga=42&quux" };
                yield return new object[] { "http://✪df.ws/123" };
                yield return new object[] { "http://userid:password@example.com:8080" };
                yield return new object[] { "http://userid:password@example.com:8080/" };
                yield return new object[] { "http://userid@example.com" };
                yield return new object[] { "http://userid@example.com/" };
                yield return new object[] { "http://userid@example.com:8080" };
                yield return new object[] { "http://userid@example.com:8080/" };
                yield return new object[] { "http://userid:password@example.com" };
                yield return new object[] { "http://userid:password@example.com/" };
                yield return new object[] { "http://142.42.1.1/" };
                yield return new object[] { "http://142.42.1.1:8080/" };
                yield return new object[] { "http://➡.ws/䨹" };
                yield return new object[] { "http://⌘.ws" };
                yield return new object[] { "http://⌘.ws/" };
                yield return new object[] { "http://foo.com/blah_(wikipedia)#cite-1" };
                yield return new object[] { "http://foo.com/blah_(wikipedia)_blah#cite-1" };
                yield return new object[] { "http://foo.com/unicode_(✪)_in_parens" };
                yield return new object[] { "http://foo.com/(something)?after=parens" };
                yield return new object[] { "http://☺.damowmow.com/" };
                yield return new object[] { "http://code.google.com/events/#&product=browser" };
                yield return new object[] { "http://j.mp" };
                yield return new object[] { "http://foo.bar/?q=Test%20URL-encoded%20stuff" };
                yield return new object[] { "http://مثال.إختبار" };
                yield return new object[] { "http://例子.测试" };
                yield return new object[] { "http://उदाहरण.परीक्षा" };
                yield return new object[] { "http://-.~_!$&'()*+,;=:%40:80%2f::::::@example.com" };
                yield return new object[] { "http://1337.net" };
                yield return new object[] { "http://a.b-c.de" };
                yield return new object[] { "http://223.255.255.254" };
            }
        }

        public static IEnumerable<object[]> GetValidRelativeUrls
        {
            get
            {
                yield return new object[] { "/123" };
                yield return new object[] { "/blah_blah" };
                yield return new object[] { "/blah_blah/" };

                /* Currently not passes */
                //yield return new object[] { "/blah_blah_(wikipedia)" };
                //yield return new object[] { "/blah_blah_(wikipedia)_(again)" };
                //yield return new object[] { "/wpstyle/?p=364" };
                //yield return new object[] { "/foo/?bar=baz&inga=42&quux" };
                //yield return new object[] { "/䨹" };
                //yield return new object[] { "/blah_(wikipedia)#cite-1" };
                //yield return new object[] { "/blah_(wikipedia)_blah#cite-1" };
                //yield return new object[] { "/unicode_(✪)_in_parens" };
                //yield return new object[] { "/(something)?after=parens" };
                //yield return new object[] { "/events/#&product=browser" };
                //yield return new object[] { "/?q=Test%20URL-encoded%20stuff" };
            }
        }

        public static IEnumerable<object[]> GetValidAbsoluteWebFolderUrls
        {
            get
            {
                yield return new object[] { "http://foo.com/blah_blah" };
                yield return new object[] { "http://foo.com/blah_blah/" };

                /* Currently not passes */
                //yield return new object[] { "http://foo.com/blah_blah_(wikipedia)" };
                //yield return new object[] { "http://foo.com/blah_blah_(wikipedia)_(again)" };
                //yield return new object[] { "http://✪df.ws/123" };
                //yield return new object[] { "http://➡.ws/䨹" };
                //yield return new object[] { "http://foo.com/blah_(wikipedia)#cite-1" };
                //yield return new object[] { "http://foo.com/blah_(wikipedia)_blah#cite-1" };
                //yield return new object[] { "http://foo.com/unicode_(✪)_in_parens" };
            }
        }

        public static IEnumerable<object[]> GetValidRelativeWebFolderUrls
        {
            get
            {
                yield return new object[] { "/123" };
                yield return new object[] { "/blah_blah" };
                yield return new object[] { "/blah_blah/" };

                /* Currently not passes */
                //yield return new object[] { "/blah_blah_(wikipedia)" };
                //yield return new object[] { "/blah_blah_(wikipedia)_(again)" };
                //yield return new object[] { "/䨹" };
                //yield return new object[] { "/blah_(wikipedia)#cite-1" };
                //yield return new object[] { "/blah_(wikipedia)_blah#cite-1" };
                //yield return new object[] { "/unicode_(✪)_in_parens" };
            }
        }

        public static IEnumerable<object[]> GetInvalidUrls()
        {
            yield return new object[] { "rdar://1234" };
            yield return new object[] { "http://?" };
            yield return new object[] { "http://??" };
            yield return new object[] { "http://??/" };
            yield return new object[] { "ftps://foo.bar/" };

            yield return new object[] { "http://" };
            yield return new object[] { "http://." };
            yield return new object[] { "http://.." };
            yield return new object[] { "http://../" };
            yield return new object[] { "http:///a" };

            yield return new object[] { "http://#" };
            yield return new object[] { "http://##" };
            yield return new object[] { "http://##/" };

            yield return new object[] { "h://test" };
            yield return new object[] { "http:// shouldfail.com" };

            /* Currently not passes */
            //yield return new object[] { "http://-error-.invalid/" };
            //yield return new object[] { "http://a.b--c.de/" };
            //yield return new object[] { "http://-a.b.co" };
            //yield return new object[] { "http://a.b-.co" };

            //yield return new object[] { "foo.com" };
            //yield return new object[] { "http://.www.foo.bar/" };
            //yield return new object[] { "http://www.foo.bar./" };
            //yield return new object[] { "http://.www.foo.bar./" };
            //yield return new object[] { "http://foo.bar?q=Spaces should be encoded" };
            //yield return new object[] { "http://foo.bar/foo(bar)baz quux" };

            //yield return new object[] { "//" };
            //yield return new object[] { "//a" };
            //yield return new object[] { "///a" };
            //yield return new object[] { "///" };
            //yield return new object[] { ":// should fail" };

            //yield return new object[] { "http://0.0.0.0" };
            //yield return new object[] { "http://10.1.1.0" };
            //yield return new object[] { "http://10.1.1.255" };
            //yield return new object[] { "http://224.1.1.1" };
            //yield return new object[] { "http://1.1.1.1.1" };
            //yield return new object[] { "http://123.123.123" };
            //yield return new object[] { "http://3628126748" };
            //yield return new object[] { "http://10.1.1.1" };
        }
    }
}
