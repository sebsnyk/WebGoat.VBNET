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

    Partial Public Class SQLInjection
        Inherits System.Web.UI.Page

        Private du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

        End Sub

        Protected Sub btnFind_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim name As String = txtName.Text
            Dim ds As DataSet = du.GetEmailByName(name)

            If ds IsNot Nothing Then
                grdEmail.DataSource = ds.Tables(0)
                grdEmail.DataBind()
            End If
        End Sub
    End Class
End Namespace