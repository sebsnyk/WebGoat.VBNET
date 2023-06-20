Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Data
Imports OWASP.WebGoat.NET.App_Code.DB
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Partial Public Class ReflectedXSS
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            If Request("city") IsNot Nothing Then
                LoadCity(Request("city"))
            End If
        End Sub

        Private Sub LoadCity(city As String)
            Dim ds As DataSet = du.GetOffice(city)
            lblOutput.Text = "Here are the details for our " & city & " Office"
            dtlView.DataSource = ds.Tables(0)
            dtlView.DataBind()
        End Sub

        Private Sub FixedLoadCity(city As String)
            Dim ds As DataSet = du.GetOffice(city)
            lblOutput.Text = "Here are the details for our " & Server.HtmlEncode(city) & " Office"
            dtlView.DataSource = ds.Tables(0)
            dtlView.DataBind()
        End Sub

    End Class
End Namespace