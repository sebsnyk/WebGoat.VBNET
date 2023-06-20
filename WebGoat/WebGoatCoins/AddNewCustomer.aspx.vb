Imports System
Imports System.Web.UI
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class AddNewCustomer
        Inherits Page

        Protected du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

        End Sub

        Protected Sub CreateCustomer(ByVal sender As Object, ByVal e As EventArgs)
            If Not du.IsAdminCustomerLogin(User.Identity.Name) Then
                InvalidUserNameOrPasswordMessage.Text = "ACCESS DENIED."
                InvalidUserNameOrPasswordMessage.Visible = True
                Return
            End If

            If String.IsNullOrEmpty(Username.Text) OrElse
                String.IsNullOrEmpty(Email.Text) OrElse
                String.IsNullOrEmpty(Password.Text) Then
                InvalidUserNameOrPasswordMessage.Text = "Fields Username, Email, Password should be filled."
                InvalidUserNameOrPasswordMessage.Visible = True
                Return
            End If

            Dim success As Boolean = du.CreateCustomer(
                Username.Text, Email.Text, Password.Text, IsAdmin.Checked, 1, "blue")
            If success Then
                InvalidUserNameOrPasswordMessage.Visible = False
            Else
                InvalidUserNameOrPasswordMessage.Text = "Error user creating."
                InvalidUserNameOrPasswordMessage.Visible = True
            End If

            Dim s As String = Page.User.Identity.Name
        End Sub
    End Class
End Namespace