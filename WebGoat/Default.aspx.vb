Imports System
Imports System.Web
Imports System.Web.UI
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Partial Public Class Default
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub ButtonProceed_Click(ByVal sender As Object, ByVal e As EventArgs)
            Response.Redirect("RebuildDatabase.aspx")
        End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            If du.TestConnection() Then
                lblOutput.Text = String.Format("You appear to be connected to a valid {0} provider. " & "If you want to reconfigure or rebuild the database, click on the button below!", du.Name)
                Session("DBConfigured") = True

                Dim cookie As New HttpCookie("Server", Encoder.Encode(Server.MachineName))
                Response.Cookies.Add(cookie)
            Else
                lblOutput.Text = "Before proceeding, please ensure this instance of WebGoat.NET can connect to the database!"
            End If

            ViewState("Session") = Session.SessionID
        End Sub
    End Class
End Namespace