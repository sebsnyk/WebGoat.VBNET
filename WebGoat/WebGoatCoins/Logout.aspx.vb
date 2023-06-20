Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Partial Public Class Logout
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs)

        End Sub

        Protected Sub btnLogout_Click(sender As Object, e As EventArgs)
            FormsAuthentication.SignOut()
            Response.Redirect("/Default.aspx")
        End Sub
    End Class
End Namespace