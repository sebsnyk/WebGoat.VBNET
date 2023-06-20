Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET.resources.Master_Pages
    Partial Public Class Site
        Inherits System.Web.UI.MasterPage

        Protected Property IsAdmin As Boolean

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            If Session("showSplash") Is Nothing Then
                Session("showSplash") = False
                Response.Redirect("~/Default.aspx")
            End If

            If Page.User.Identity.IsAuthenticated AndAlso Settings.CurrentDbProvider IsNot Nothing Then
                IsAdmin = Settings.CurrentDbProvider.IsAdminCustomerLogin(Page.User.Identity.Name)
            End If

        End Sub

        Protected Sub lbtGenerateTestData_Click(ByVal sender As Object, ByVal e As EventArgs)
            Response.Redirect("/RebuildDatabase.aspx")
        End Sub
        Public Sub GreyOutMenu()
            For Each item As RepeaterItem In rptrMenu.Items
                'nothing
            Next
        End Sub
    End Class
End Namespace