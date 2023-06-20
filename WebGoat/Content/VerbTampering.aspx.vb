Imports System
Imports System.Web
Imports System.Web.UI

Namespace OWASP.WebGoat.NET
    Public Partial Class VerbTampering
        Inherits System.Web.UI.Page

        'Probably best if eventually connected to DB
        Public Shared tamperedMessage As String = "This has not been tampered with yet..."

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            lblTampered.Text = tamperedMessage
        End Sub
    End Class
End Namespace