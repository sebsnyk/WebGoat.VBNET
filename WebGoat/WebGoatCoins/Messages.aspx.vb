Imports System
Imports System.Data
Imports System.Web.UI
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class Messages
        Inherits Page

        Private du As IDbProvider = Settings.CurrentDbProvider
        Protected Sub Page_Load(sender As Object, e As EventArgs)
            If String.IsNullOrEmpty(TryCast(Session("messages"), String)) Then
                Dim ds As DataSet = du.GetMessages(User.Identity.Name)
                Dim messages As String = String.Empty
                For Each row As DataRow In ds.Tables(0).Rows
                    messages &= "<strong>" & Server.HtmlEncode(row("title").ToString()) & "</strong><br/>"
                    messages &= Server.HtmlEncode(row("text").ToString()) & "<br/><hr/>"
                Next

                Session("messages") = messages
            End If

            lblMessages.Text = CType(Session("messages"), String)
        End Sub
    End Class
End Namespace