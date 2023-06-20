Imports System
Imports System.Web
Imports System.Web.Security

Namespace OWASP.WebGoat.NET.App_Code
    Public Class CookieManager
        Public Sub New()
        End Sub

        Public Shared Function SetCookie(ticket As FormsAuthenticationTicket, cookieId As String, cookieValue As String) As HttpCookie
            Dim encrypted_ticket As String = FormsAuthentication.Encrypt(ticket)

            Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, encrypted_ticket)

            If ticket.IsPersistent Then
                cookie.Expires = ticket.Expiration
            End If

            Return cookie
        End Function
    End Class
End Namespace