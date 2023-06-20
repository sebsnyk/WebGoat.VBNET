Imports System
Imports System.Collections.Generic
Imports System.Web

Namespace OWASP.WebGoat.NET
    Public Class CustomerLoginData
        Public email As String = String.Empty
        Public password As String = String.Empty
        Public isLoggedIn As Boolean = False
        Public message As String = String.Empty

        Public Sub New(email As String, password As String, isLoggedIn As Boolean)
            Me.email = email
            Me.password = password
            Me.isLoggedIn = isLoggedIn
        End Sub

        Public Property Message As String
            Get
                Return Me.message
            End Get
            Set(value As String)
                message = value
            End Set
        End Property

    End Class
End Namespace