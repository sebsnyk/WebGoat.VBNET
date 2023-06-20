Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Collections
Imports System.Collections.Specialized

Namespace OWASP.WebGoat.NET
    Partial Public Class HeaderInjection
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            If Not Request.QueryString("Cookie") Is Nothing Then
                Dim cookie As HttpCookie = New HttpCookie("UserAddedCookie")
                cookie.Value = Request.QueryString("Cookie")

                Response.Cookies.Add(cookie)
            ElseIf Not Request.QueryString("Header") Is Nothing Then
                Dim newHeader As NameValueCollection = New NameValueCollection()
                newHeader.Add("newHeader", Request.QueryString("Header"))
                Response.Headers.Add(newHeader)
            End If

            'Headers
            lblHeaders.Text = Request.Headers.ToString().Replace("&", "<br />")

            'Cookies
            Dim colCookies As ArrayList = New ArrayList()
            For i As Integer = 0 To Request.Cookies.Count - 1
                colCookies.Add(Request.Cookies(i))
            Next

            gvCookies.DataSource = colCookies
            gvCookies.DataBind()

            'possibly going to be used later for something interesting
        End Sub
    End Class
End Namespace