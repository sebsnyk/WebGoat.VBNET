Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls

Namespace OWASP.WebGoat.NET.resources.Master_Pages
    Partial Public Class SiteNew
        Inherits System.Web.UI.MasterPage

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            If Session("showSplash") Is Nothing Then
                Session("showSplash") = False
                Response.Redirect("~/Default.aspx")
            End If
        End Sub

        Protected Sub lbtGenerateTestData_Click(sender As Object, e As EventArgs)
            Response.Redirect("/RebuildDatabase.aspx")
        End Sub

        Public Sub GreyOutMenu()
            For Each item As RepeaterItem In rptrMenu.Items
                'nothing
            Next
        End Sub

    End Class
End Namespace