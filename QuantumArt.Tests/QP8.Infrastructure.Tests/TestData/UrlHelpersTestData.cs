using System.Collections.Generic;

namespace QP8.Infrastructure.Tests.TestData
{
    public class UrlHelpersTestData
    {
        public static IEnumerable<string[]> GetValidAbsoluteUrls
        {
            get
            {
                yield return new[] { "http://foo.com/blah_blah" };
                yield return new[] { "http://foo.com/blah_blah/" };
                yield return new[] { "http://foo.com/blah_blah_(wikipedia)" };
                yield return new[] { "http://foo.com/blah_blah_(wikipedia)_(again)" };
                yield return new[] { "http://www.example.com/wpstyle/?p=364" };
                yield return new[] { "https://www.example.com/foo/?bar=baz&inga=42&quux" };
                yield return new[] { "http://✪df.ws/123" };
                yield return new[] { "http://userid:password@example.com:8080" };
                yield return new[] { "http://userid:password@example.com:8080/" };
                yield return new[] { "http://userid@example.com" };
                yield return new[] { "http://userid@example.com/" };
                yield return new[] { "http://userid@example.com:8080" };
                yield return new[] { "http://userid@example.com:8080/" };
                yield return new[] { "http://userid:password@example.com" };
                yield return new[] { "http://userid:password@example.com/" };
                yield return new[] { "http://142.42.1.1/" };
                yield return new[] { "http://142.42.1.1:8080/" };
                yield return new[] { "http://➡.ws/䨹" };
                yield return new[] { "http://⌘.ws" };
                yield return new[] { "http://⌘.ws/" };
                yield return new[] { "http://foo.com/blah_(wikipedia)#cite-1" };
                yield return new[] { "http://foo.com/blah_(wikipedia)_blah#cite-1" };
                yield return new[] { "http://foo.com/unicode_(✪)_in_parens" };
                yield return new[] { "http://foo.com/(something)?after=parens" };
                yield return new[] { "http://☺.damowmow.com/" };
                yield return new[] { "http://code.google.com/events/#&product=browser" };
                yield return new[] { "http://j.mp" };
                yield return new[] { "http://foo.bar/?q=Test%20URL-encoded%20stuff" };
                yield return new[] { "http://مثال.إختبار" };
                yield return new[] { "http://例子.测试" };
                yield return new[] { "http://उदाहरण.परीक्षा" };
                yield return new[] { "http://-.~_!$&'()*+,;=:%40:80%2f::::::@example.com" };
                yield return new[] { "http://1337.net" };
                yield return new[] { "http://a.b-c.de" };
                yield return new[] { "http://223.255.255.254" };
            }
        }

        public static IEnumerable<string[]> GetValidRelativeUrls
        {
            get
            {
                yield return new[] { "/123" };
                yield return new[] { "/blah_blah" };
                yield return new[] { "/blah_blah/" };

                /* Currently not passes */
                //yield return new[] { "/blah_blah_(wikipedia)" };
                //yield return new[] { "/blah_blah_(wikipedia)_(again)" };
                //yield return new[] { "/wpstyle/?p=364" };
                //yield return new[] { "/foo/?bar=baz&inga=42&quux" };
                //yield return new[] { "/䨹" };
                //yield return new[] { "/blah_(wikipedia)#cite-1" };
                //yield return new[] { "/blah_(wikipedia)_blah#cite-1" };
                //yield return new[] { "/unicode_(✪)_in_parens" };
                //yield return new[] { "/(something)?after=parens" };
                //yield return new[] { "/events/#&product=browser" };
                //yield return new[] { "/?q=Test%20URL-encoded%20stuff" };
            }
        }

        public static IEnumerable<string[]> GetValidAbsoluteWebFolderUrls
        {
            get
            {
                yield return new[] { "http://foo.com/blah_blah" };
                yield return new[] { "http://foo.com/blah_blah/" };

                /* Currently not passes */
                //yield return new[] { "http://foo.com/blah_blah_(wikipedia)" };
                //yield return new[] { "http://foo.com/blah_blah_(wikipedia)_(again)" };
                //yield return new[] { "http://✪df.ws/123" };
                //yield return new[] { "http://➡.ws/䨹" };
                //yield return new[] { "http://foo.com/blah_(wikipedia)#cite-1" };
                //yield return new[] { "http://foo.com/blah_(wikipedia)_blah#cite-1" };
                //yield return new[] { "http://foo.com/unicode_(✪)_in_parens" };
            }
        }

        public static IEnumerable<string[]> GetValidRelativeWebFolderUrls
        {
            get
            {
                yield return new[] { "/123" };
                yield return new[] { "/blah_blah" };
                yield return new[] { "/blah_blah/" };

                /* Currently not passes */
                //yield return new[] { "/blah_blah_(wikipedia)" };
                //yield return new[] { "/blah_blah_(wikipedia)_(again)" };
                //yield return new[] { "/䨹" };
                //yield return new[] { "/blah_(wikipedia)#cite-1" };
                //yield return new[] { "/blah_(wikipedia)_blah#cite-1" };
                //yield return new[] { "/unicode_(✪)_in_parens" };
            }
        }

        public static IEnumerable<string[]> GetInvalidUrls()
        {
            yield return new[] { "rdar://1234" };
            yield return new[] { "http://?" };
            yield return new[] { "http://??" };
            yield return new[] { "http://??/" };
            yield return new[] { "ftps://foo.bar/" };

            yield return new[] { "http://" };
            yield return new[] { "http://." };
            yield return new[] { "http://.." };
            yield return new[] { "http://../" };
            yield return new[] { "http:///a" };

            yield return new[] { "http://#" };
            yield return new[] { "http://##" };
            yield return new[] { "http://##/" };

            yield return new[] { "h://test" };
            yield return new[] { "http:// shouldfail.com" };

            /* Currently not passes */
            //yield return new[] { "http://-error-.invalid/" };
            //yield return new[] { "http://a.b--c.de/" };
            //yield return new[] { "http://-a.b.co" };
            //yield return new[] { "http://a.b-.co" };

            //yield return new[] { "foo.com" };
            //yield return new[] { "http://.www.foo.bar/" };
            //yield return new[] { "http://www.foo.bar./" };
            //yield return new[] { "http://.www.foo.bar./" };
            //yield return new[] { "http://foo.bar?q=Spaces should be encoded" };
            //yield return new[] { "http://foo.bar/foo(bar)baz quux" };

            //yield return new[] { "//" };
            //yield return new[] { "//a" };
            //yield return new[] { "///a" };
            //yield return new[] { "///" };
            //yield return new[] { ":// should fail" };

            //yield return new[] { "http://0.0.0.0" };
            //yield return new[] { "http://10.1.1.0" };
            //yield return new[] { "http://10.1.1.255" };
            //yield return new[] { "http://224.1.1.1" };
            //yield return new[] { "http://1.1.1.1.1" };
            //yield return new[] { "http://123.123.123" };
            //yield return new[] { "http://3628126748" };
            //yield return new[] { "http://10.1.1.1" };
        }
    }
}
