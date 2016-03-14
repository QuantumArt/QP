<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="Quantumart.QP8.WebMvc.Extensions.Helpers" %>
<%@ Import Namespace="Quantumart.QP8.WebMvc.Resources.Views.LogOn" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Login page</title>
	<meta http-equiv="Content-Type" content="text/html;charset=UTF-8" />
	<%: Html.Telerik().StyleSheetRegistrar().StyleSheets(css =>
			css
				.Add("basic.css")
				.Add("page.css")
		)
	%>
</head>
<body>
	<div id="page" class="loginFormLayout">
		<div id="content" class="content">
		<% using (Html.BeginForm()) { %>
			<div class="formLayout">
				<dl class="row">
					<dt class="label">
						<label for="UserName"><%: IndexStrings.Label_UserName %></label>
					</dt>
					<dd class="field">
						<%: Html.TextBox("UserName", "", new { @class = "textbox" })%>
					</dd>
				</dl>
				<dl class="row">
					<dt class="label">
						<label for="Password"><%: IndexStrings.Label_Password %></label>
					</dt>
					<dd class="field">
						<%: Html.Password("Password", "", new { @class = "textbox" })%>
					</dd>
				</dl>
				<dl class="row">
					<dt class="label">
						<label for="CustomerCode"><%: IndexStrings.Label_CustomerCode %></label>
					</dt>
					<dd class="field">
						<%: Html.TextBox("CustomerCode", "", new { @class = "textbox" })%>
					</dd>
				</dl>
				<dl class="row">
					<dt class="label">
					</dt>
					<dd class="field">
						<input type="submit" id="Login" name="Login" value="<%: IndexStrings.Button_Login %>" />
						<br />
						<%: Html.ValidationSummary(IndexStrings.ErrorSummary_AuthenticationErrors, new { @style = "margin-top: 0.5em;" })%>
					</dd>
				</dl>
			</div>
		<% } %>
		</div>
</body>
</html>