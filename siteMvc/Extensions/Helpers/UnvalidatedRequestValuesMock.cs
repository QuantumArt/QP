﻿// using System.Collections.Specialized;
// //using System.Web;
//
// namespace Quantumart.QP8.WebMvc.Extensions.Helpers
// {
//     public class UnvalidatedRequestValuesMock : UnvalidatedRequestValuesBase
//     {
//         private readonly HttpRequestBase _request;
//
//         public UnvalidatedRequestValuesMock(HttpRequestBase request)
//         {
//             _request = request;
//         }
//
//         public override HttpCookieCollection Cookies => _request.Cookies;
//
//         public override HttpFileCollectionBase Files => _request.Files;
//
//         public override Dictionary<string, StringValues> Headers => _request.Headers;
//
//         public override Dictionary<string, StringValues> QueryString => _request.QueryString;
//
//         public override Dictionary<string, StringValues> Form => _request.Form;
//
//         public override string this[string field] => _request[field];
//     }
// }
