<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="Quantumart.QP8.Constants" %>

<% if (!(bool)ViewData[ArticleViewDataKeys.CreateNew]) { %>
    <%: Html.Hidden("id") %>
<% } %>
<%: Html.Hidden("parentId") %>
<%: Html.Hidden("tabId") %>