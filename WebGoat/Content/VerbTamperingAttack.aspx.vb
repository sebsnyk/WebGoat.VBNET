Imports System
Imports System.Web
Imports System.Web.UI

Namespace OWASP.WebGoat.NET
    Public Partial Class VerbTamperingAttack
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            If Request.QueryString("message") IsNot Nothing Then
                VerbTampering.tamperedMessage = Request.QueryString("message")
            End If
        End Sub
    End Class
End Namespace