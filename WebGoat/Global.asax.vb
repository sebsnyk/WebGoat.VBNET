Imports System
Imports System.Collections.Generic
Imports System.Web
Imports System.Web.Security
Imports System.Web.SessionState
Imports System.Security.Principal
Imports OWASP.WebGoat.NET.App_Code
Imports log4net.Config
Imports System.Diagnostics

Namespace OWASP.WebGoat.NET
    Public Class Global
        Inherits System.Web.HttpApplication

        Protected Sub Application_Start(sender As Object, e As EventArgs)
            If Debugger.IsAttached Then
                BasicConfigurator.Configure()
            Else
                XmlConfigurator.Configure()
            End If

            Settings.Init(Server)
        End Sub

        Protected Sub Session_Start(sender As Object, e As EventArgs)
        End Sub

        Protected Sub Application_BeginRequest(sender As Object, e As EventArgs)
        End Sub

        Sub Application_PreSendRequestHeaders(sender As Object, e As EventArgs)
            Response.AddHeader("X-XSS-Protection", "0")
        End Sub

        Protected Sub Application_AuthenticateRequest(sender As Object, e As EventArgs)
            'get the role data out of the encrypted cookie and add to current context
            'TODO: get this out of a different cookie

            If HttpContext.Current.User IsNot Nothing Then
                If HttpContext.Current.User.Identity.IsAuthenticated Then
                    If TypeOf HttpContext.Current.User.Identity Is FormsIdentity Then
                        Dim id As FormsIdentity = CType(HttpContext.Current.User.Identity, FormsIdentity)
                        Dim ticket As FormsAuthenticationTicket = id.Ticket

                        ' Get the stored user-data, in this case, our roles
                        Dim userData As String = ticket.UserData
                        Dim roles() As String = userData.Split(","c)
                        HttpContext.Current.User = New GenericPrincipal(id, roles)
                    End If
                End If
            End If
        End Sub

        Protected Sub Application_Error(sender As Object, e As EventArgs)
        End Sub

        Protected Sub Session_End(sender As Object, e As EventArgs)
        End Sub

        Protected Sub Application_End(sender As Object, e As EventArgs)
        End Sub
    End Class
End Namespace