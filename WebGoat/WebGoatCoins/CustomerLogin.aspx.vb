Imports System
Imports System.Web.Security
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code
Imports log4net
Imports System.Reflection

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Partial Public Class CustomerLogin
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider
        Private log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            PanelError.Visible = False

            Dim returnUrl As String = Request.QueryString("ReturnUrl")
            If returnUrl IsNot Nothing Then
                PanelError.Visible = True
            End If
        End Sub

        Protected Sub ButtonLogOn_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim email As String = txtUserName.Text
            Dim pwd As String = txtPassword.Text

            log.Info("User " & email & " attempted to log in with password " & pwd)

            If Not du.IsValidCustomerLogin(email, pwd) Then
                labelError.Text = "Incorrect username/password"
                PanelError.Visible = True
                Return
            End If

            ' put ticket into the cookie
            Dim ticket As New FormsAuthenticationTicket(
                            1, 'version
                            email, 'name
                            DateTime.Now, ' issueDate
                            DateTime.Now.AddDays(14), ' expireDate
                            True, ' isPersistent
                            "customer", ' userData (customer role)
                            FormsAuthentication.FormsCookiePath ' cookiePath
            )

            Dim encrypted_ticket As String = FormsAuthentication.Encrypt(ticket) ' encrypt the ticket

            ' put ticket into the cookie
            Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, encrypted_ticket)

            ' set expiration date
            If ticket.IsPersistent Then
                cookie.Expires = ticket.Expiration
            End If

            Response.Cookies.Add(cookie)

            Dim returnUrl As String = Request.QueryString("ReturnUrl")

            If returnUrl Is Nothing Then
                returnUrl = "/WebGoatCoins/MainPage.aspx"
            End If

            Response.Redirect(returnUrl)
        End Sub
    End Class
End Namespace
